using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using HikeManager.Models;
using HikeManager.Services;

namespace HikeManager.Views
{
    public class EquipmentView : UserControl
    {
        private int _currentHikeId;
        private readonly EquipmentService _service;
        private DataGridView _grid = null!;
        private Label _lblSummary = null!;
        private Button _btnAdd = null!;
        private Button _btnEdit = null!;
        private Button _btnDelete = null!;
        private Button _btnBasicKit = null!;

        public EquipmentView(EquipmentService? service = null)
        {
            _service = service ?? new EquipmentService();
            SetupUI();
        }

        public void LoadHike(int hikeId)
        {
            _currentHikeId = hikeId;
            RefreshGrid();
        }

        private void SetupUI()
        {
            Dock = DockStyle.Fill;
            BackColor = Theme.BackgroundColor;
            Font = Theme.RegularFont;

            Panel container = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(20, 15, 20, 15),
                BackColor = Theme.BackgroundColor
            };

            // Верхня панель
            Panel topPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 60,
                BackColor = Theme.BackgroundColor
            };

            Label lblTitle = new Label
            {
                Text = "🎒 Спорядження для походу",
                Font = Theme.SubtitleFont,
                ForeColor = Theme.PrimaryColor,
                AutoSize = true,
                Location = new Point(0, 5)
            };

            _lblSummary = new Label
            {
                Text = "Всього позицій: 0 | Загальна кількість: 0 шт.",
                Font = Theme.SmallFont,
                ForeColor = Theme.TextMuted,
                AutoSize = true,
                Location = new Point(2, 34)
            };

            topPanel.Controls.Add(lblTitle);
            topPanel.Controls.Add(_lblSummary);

            // Кнопки праворуч
            FlowLayoutPanel actionPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Right,
                FlowDirection = FlowDirection.LeftToRight,
                AutoSize = true,
                WrapContents = false,
                Height = 45,
                Padding = new Padding(0, 5, 0, 0)
            };

            _btnBasicKit = new Button { Text = "+ Базовий набір", Width = 140 };
            Theme.StyleSecondaryButton(_btnBasicKit);
            _btnBasicKit.Click += BtnBasicKit_Click;

            _btnAdd = new Button { Text = "+ Додати", Width = 110 };
            Theme.StylePrimaryButton(_btnAdd);
            _btnAdd.Click += BtnAdd_Click;

            _btnEdit = new Button { Text = "Редагувати", Width = 110, Enabled = false };
            Theme.StyleSecondaryButton(_btnEdit);
            _btnEdit.Click += BtnEdit_Click;

            _btnDelete = new Button { Text = "Видалити", Width = 100, Enabled = false };
            Theme.StyleDangerButton(_btnDelete);
            _btnDelete.Click += BtnDelete_Click;

            actionPanel.Controls.Add(_btnBasicKit);
            actionPanel.Controls.Add(_btnAdd);
            actionPanel.Controls.Add(_btnEdit);
            actionPanel.Controls.Add(_btnDelete);

            topPanel.Controls.Add(actionPanel);

            // Таблиця DataGridView
            _grid = new DataGridView
            {
                Dock = DockStyle.Fill
            };
            Theme.StyleDataGridView(_grid);

            _grid.SelectionChanged += (s, e) => UpdateButtonsState();
            _grid.CellDoubleClick += (s, e) =>
            {
                if (e.RowIndex >= 0) BtnEdit_Click(s, e);
            };

            container.Controls.Add(_grid);
            container.Controls.Add(topPanel);
            Controls.Add(container);
        }

        private void UpdateButtonsState()
        {
            bool hasSelection = _grid.SelectedRows.Count > 0;
            _btnEdit.Enabled = hasSelection;
            _btnDelete.Enabled = hasSelection;
        }

        public void RefreshGrid()
        {
            if (_currentHikeId <= 0) return;

            var items = _service.GetByHike(_currentHikeId);
            _grid.DataSource = null;
            _grid.DataSource = items;

            int totalPositions = items.Count;
            int totalQuantity = items.Sum(i => i.Quantity);
            _lblSummary.Text = $"Всього позицій: {totalPositions} | Загальна кількість одиниць: {totalQuantity} шт.";

            if (_grid.Columns["Id"] != null) _grid.Columns["Id"].Visible = false;
            if (_grid.Columns["HikeId"] != null) _grid.Columns["HikeId"].Visible = false;

            if (_grid.Columns["Name"] != null)
            {
                _grid.Columns["Name"].HeaderText = "Назва спорядження";
                _grid.Columns["Name"].FillWeight = 70;
            }
            if (_grid.Columns["Quantity"] != null)
            {
                _grid.Columns["Quantity"].HeaderText = "Кількість (шт.)";
                _grid.Columns["Quantity"].FillWeight = 30;
                _grid.Columns["Quantity"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            }

            UpdateButtonsState();
        }

        private void BtnAdd_Click(object? sender, EventArgs e)
        {
            if (_currentHikeId <= 0) return;

            using (var form = new EquipmentEditForm(_currentHikeId))
            {
                if (form.ShowDialog(this) == DialogResult.OK)
                {
                    RefreshGrid();
                }
            }
        }

        private void BtnEdit_Click(object? sender, EventArgs e)
        {
            if (_grid.SelectedRows.Count == 0 || _currentHikeId <= 0) return;

            var selected = (EquipmentItem)_grid.SelectedRows[0].DataBoundItem;
            using (var form = new EquipmentEditForm(_currentHikeId, selected))
            {
                if (form.ShowDialog(this) == DialogResult.OK)
                {
                    RefreshGrid();
                }
            }
        }

        private void BtnDelete_Click(object? sender, EventArgs e)
        {
            if (_grid.SelectedRows.Count == 0) return;

            var selected = (EquipmentItem)_grid.SelectedRows[0].DataBoundItem;
            var result = MessageBox.Show(
                $"Ви дійсно бажаєте видалити «{selected.Name}» зі списку спорядження?",
                "Підтвердження видалення",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                _service.Delete(selected.Id);
                RefreshGrid();
            }
        }

        private void BtnBasicKit_Click(object? sender, EventArgs e)
        {
            if (_currentHikeId <= 0) return;

            int added = _service.AddBasicKit(_currentHikeId);
            RefreshGrid();

            if (added > 0)
            {
                MessageBox.Show(
                    $"Додано {added} нових позицій базового туристичного спорядження!",
                    "Базовий набір",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show(
                    "Усі позиції з базового набору вже присутні у списку спорядження цього походу.",
                    "Базовий набір",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
        }
    }
}
