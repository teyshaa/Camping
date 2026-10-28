
using System;
using System.Drawing;
using System.Windows.Forms;
using HikeManager.Models;
using HikeManager.Services;

namespace HikeManager.Views
{
    public class ExpenseEditForm : Form
    {
        private readonly int _hikeId;
        private readonly Expense? _expense;
        private readonly ExpenseService _service;

        private readonly ComboBox _category = new();
        private readonly TextBox _amount = new();
        private readonly TextBox _description = new();

        public ExpenseEditForm(
            int hikeId,
            Expense? expense,
            ExpenseService service)
        {
            _hikeId = hikeId;
            _expense = expense;
            _service = service;

            Text = expense == null
                ? "Додати витрату"
                : "Редагувати витрату";

            Size = new Size(430, 350);
            MinimumSize = new Size(430, 350);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;

            Font = new Font("Segoe UI", 10);

            var panel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(20),
                ColumnCount = 1,
                RowCount = 7
            };

            panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
            panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
            panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
            panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
            panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
            panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
            panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 55));

            _category.Dock = DockStyle.Fill;
            _category.DropDownStyle = ComboBoxStyle.DropDownList;
            _category.Items.AddRange(ExpenseService.Categories);

            _amount.Dock = DockStyle.Fill;
            _amount.PlaceholderText = "Наприклад: 150,50";

            _description.Dock = DockStyle.Fill;

            panel.Controls.Add(new Label
            {
                Text = "Категорія:",
                AutoSize = true
            }, 0, 0);

            panel.Controls.Add(_category, 0, 1);

            panel.Controls.Add(new Label
            {
                Text = "Сума, грн:",
                AutoSize = true
            }, 0, 2);

            panel.Controls.Add(_amount, 0, 3);

            panel.Controls.Add(new Label
            {
                Text = "Опис (необов'язково):",
                AutoSize = true
            }, 0, 4);

            panel.Controls.Add(_description, 0, 5);

            var saveButton = new Button
            {
                Text = "Зберегти",
                Width = 130,
                Height = 38
            };

            saveButton.Click += SaveExpense;

            var cancelButton = new Button
            {
                Text = "Скасувати",
                Width = 130,
                Height = 38,
                DialogResult = DialogResult.Cancel
            };

            var buttons = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.RightToLeft
            };

            buttons.Controls.Add(cancelButton);
            buttons.Controls.Add(saveButton);

            panel.Controls.Add(buttons, 0, 6);
            Controls.Add(panel);

            AcceptButton = saveButton;
            CancelButton = cancelButton;

            if (expense != null)
            {
                _category.SelectedItem = expense.Category;
                _amount.Text = expense.Amount.ToString("0.00");
                _description.Text = expense.Description ?? "";
            }
            else
            {
                _category.SelectedIndex = 0;
            }
        }

        private void SaveExpense(object? sender, EventArgs e)
        {
            try
            {
                decimal amount = ExpenseService.ParseAmount(
                    _amount.Text);

                var expense = new Expense
                {
                    Id = _expense?.Id ?? 0,
                    HikeId = _hikeId,
                    Category = _category.SelectedItem?.ToString() ?? "",
                    Amount = amount,
                    Description = _description.Text.Trim()
                };

                if (_expense == null)
                    _service.Insert(expense);
                else
                    _service.Update(expense);

                DialogResult = DialogResult.OK;
                Close();
            }
            catch (ValidationException ex)
            {
                MessageBox.Show(
                    ex.Message,
                    "Помилка введення",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Не вдалося зберегти витрату: " + ex.Message,
                    "Помилка",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }
    }
}

