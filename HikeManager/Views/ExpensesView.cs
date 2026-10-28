
using System;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using HikeManager.Models;
using HikeManager.Services;

namespace HikeManager.Views
{
    public class ExpensesView : UserControl
    {
        private int _currentHikeId;

        private Func<int, Hike?>? _getHikeById;

        private readonly ExpenseService _expenseService = new();
        private readonly ParticipantService _participantService = new();

        private readonly DataGridView _table = new();
        private readonly Label _totalLabel = new();
        private readonly Label _perPersonLabel = new();

        public ExpensesView()
        {
            Dock = DockStyle.Fill;
            BackColor = Theme.BackgroundColor;

            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(20),
                ColumnCount = 1,
                RowCount = 3
            };

            layout.RowStyles.Add(
                new RowStyle(SizeType.Absolute, 60));

            layout.RowStyles.Add(
                new RowStyle(SizeType.Percent, 100));

            layout.RowStyles.Add(
                new RowStyle(SizeType.Absolute, 100));

            var toolbar = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                AutoScroll = true,
                WrapContents = false
            };

            var addButton = CreateButton("Додати");
            var editButton = CreateButton("Редагувати");
            var deleteButton = CreateButton("Видалити");
            var exportButton = CreateButton("Експортувати звіт (TXT)");

            addButton.Click += (s, e) => EditExpense(null);

            editButton.Click += (s, e) =>
            {
                if (_table.CurrentRow?.DataBoundItem is Expense expense)
                    EditExpense(expense);
                else
                    MessageBox.Show("Виберіть витрату.");
            };

            deleteButton.Click += (s, e) => DeleteExpense();

            exportButton.Click += (s, e) => ExportReport();

            toolbar.Controls.Add(addButton);
            toolbar.Controls.Add(editButton);
            toolbar.Controls.Add(deleteButton);
            toolbar.Controls.Add(exportButton);

            _table.Dock = DockStyle.Fill;
            _table.ReadOnly = true;
            _table.AllowUserToAddRows = false;
            _table.AllowUserToDeleteRows = false;
            _table.MultiSelect = false;

            _table.SelectionMode =
                DataGridViewSelectionMode.FullRowSelect;

            _table.AutoGenerateColumns = false;

            _table.AutoSizeColumnsMode =
                DataGridViewAutoSizeColumnsMode.Fill;

            _table.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Категорія",
                DataPropertyName = "Category"
            });

            _table.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Сума, грн",
                DataPropertyName = "Amount",
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    Format = "N2"
                }
            });

            _table.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Опис",
                DataPropertyName = "Description"
            });

            var summary = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false
            };

            _totalLabel.AutoSize = true;
            _totalLabel.Font =
                new Font("Segoe UI", 13, FontStyle.Bold);

            _perPersonLabel.AutoSize = true;
            _perPersonLabel.Font =
                new Font("Segoe UI", 11);

            summary.Controls.Add(_totalLabel);
            summary.Controls.Add(_perPersonLabel);

            layout.Controls.Add(toolbar, 0, 0);
            layout.Controls.Add(_table, 0, 1);
            layout.Controls.Add(summary, 0, 2);

            Controls.Add(layout);
        }

        private static Button CreateButton(string text)
        {
            return new Button
            {
                Text = text,
                AutoSize = true,
                MinimumSize = new Size(110, 38),
                Margin = new Padding(0, 0, 10, 0)
            };
        }

        // Підключення сервісу походів під час інтеграції
        public void SetHikeProvider(Func<int, Hike?> getHikeById)
        {
            _getHikeById = getHikeById
                ?? throw new ArgumentNullException(nameof(getHikeById));
        }

        // Завантаження фінансів конкретного походу
        public void LoadHike(int hikeId)
        {
            _currentHikeId = hikeId;
            RefreshData();
        }

        private void RefreshData()
        {
            if (_currentHikeId <= 0)
            {
                _table.DataSource = null;
                _totalLabel.Text = "Загальний бюджет: 0,00 грн";
                _perPersonLabel.Text = "На людину: немає учасників";
                return;
            }

            try
            {
                var expenses =
                    _expenseService.GetByHike(_currentHikeId);

                int count =
                    _participantService.CountByHike(_currentHikeId);

                var budget =
                    BudgetCalculator.Calculate(expenses, count);

                _table.DataSource = null;
                _table.DataSource = expenses;

                _totalLabel.Text =
                    $"Загальний бюджет: {budget.TotalBudget:N2} грн";

                _perPersonLabel.Text = budget.HasNoParticipants
                    ? "На людину: немає учасників"
                    : $"На людину: {budget.CostPerPerson:N2} грн";
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Не вдалося завантажити витрати: " + ex.Message,
                    "Помилка",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void EditExpense(Expense? expense)
        {
            if (_currentHikeId <= 0)
            {
                MessageBox.Show("Спочатку виберіть похід.");
                return;
            }

            using var form = new ExpenseEditForm(
                _currentHikeId,
                expense,
                _expenseService);

            if (form.ShowDialog(this) == DialogResult.OK)
                RefreshData();
        }

        private void DeleteExpense()
        {
            if (_table.CurrentRow?.DataBoundItem is not Expense expense)
            {
                MessageBox.Show(
                    "Виберіть витрату для видалення.");
                return;
            }

            var answer = MessageBox.Show(
                "Видалити вибрану витрату?",
                "Підтвердження",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (answer != DialogResult.Yes)
                return;

            try
            {
                _expenseService.Delete(expense.Id);
                RefreshData();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Не вдалося видалити витрату: " + ex.Message,
                    "Помилка",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        // Експорт повного TXT-звіту
        private void ExportReport()
        {
            if (_currentHikeId <= 0)
            {
                MessageBox.Show("Спочатку виберіть похід.");
                return;
            }

            if (_getHikeById == null)
            {
                MessageBox.Show(
                    "Експорт стане доступним після " +
                    "підключення модуля походів.",
                    "Інформація",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                return;
            }

            try
            {
                Hike? hike = _getHikeById(_currentHikeId);

                if (hike == null)
                {
                    MessageBox.Show("Похід не знайдено.");
                    return;
                }

                var exporter = new HikeReportExporter();
                string report = exporter.Generate(hike);

                using var dialog = new SaveFileDialog
                {
                    Title = "Зберегти звіт про похід",
                    Filter = "Текстові файли (*.txt)|*.txt",
                    DefaultExt = "txt",
                    AddExtension = true,
                    FileName = $"Звіт_похід_{hike.Id}.txt",
                    OverwritePrompt = true
                };

                if (dialog.ShowDialog(this) != DialogResult.OK)
                    return;

                File.WriteAllText(
                    dialog.FileName,
                    report,
                    Encoding.UTF8);

                MessageBox.Show(
                    "Звіт успішно збережено!",
                    "Успіх",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Помилка експорту: " + ex.Message,
                    "Помилка",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }
    }
}


