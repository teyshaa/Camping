using System;
using System.Drawing;
using System.Windows.Forms;
using HikeManager.Models;
using HikeManager.Services;

namespace HikeManager.Views
{
    public class HikesView : UserControl
    {
        public bool ShowHistory { get; }
        private readonly IHikeService _hikeService;
        private DataGridView _grid = null!;
        private Label _lblSubtitle = null!;
        private Button _btnCreate = null!;
        private Button _btnDetails = null!;
        private Button _btnEdit = null!;
        private Button _btnDelete = null!;

        public HikesView(bool showHistory = false, IHikeService? hikeService = null)
        {
            ShowHistory = showHistory;
            _hikeService = hikeService ?? new HikeService();

            SetupUI();
            LoadData();
        }

        private void SetupUI()
        {
            Dock = DockStyle.Fill;
            BackColor = Theme.BackgroundColor;
            Font = Theme.RegularFont;

            Panel container = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(25, 20, 25, 20),
                BackColor = Theme.BackgroundColor
            };

            // Верхня панель
            Panel topPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 65,
                BackColor = Theme.BackgroundColor
            };

            Label lblTitle = new Label
            {
                Text = ShowHistory ? "Історія проведених походів" : "Активні та заплановані походи",
                Font = Theme.TitleFont,
                ForeColor = Theme.PrimaryColor,
                AutoSize = true,
                Location = new Point(0, 5)
            };

            _lblSubtitle = new Label
            {
                Text = ShowHistory
                    ? "Архів завершених туристичних походів, склад груп та витрати"
                    : "Поточні походи: керування групами, спорядженням та розрахунком витрат",
                Font = Theme.SmallFont,
                ForeColor = Theme.TextMuted,
                AutoSize = true,
                Location = new Point(2, 40)
            };

            topPanel.Controls.Add(lblTitle);
            topPanel.Controls.Add(_lblSubtitle);

            // Панель кнопок праворуч
            FlowLayoutPanel actionPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Right,
                FlowDirection = FlowDirection.LeftToRight,
                AutoSize = true,
                WrapContents = false,
                Height = 45,
                Padding = new Padding(0, 8, 0, 0)
            };

            _btnCreate = new Button { Text = "+ Новий похід", Width = 135 };
            Theme.StylePrimaryButton(_btnCreate);
            _btnCreate.Click += BtnCreate_Click;

            _btnDetails = new Button { Text = "📋 Деталі (вкладки)", Width = 150, Enabled = false };
            Theme.StyleSecondaryButton(_btnDetails);
            _btnDetails.Click += BtnDetails_Click;

            _btnEdit = new Button { Text = "Редагувати", Width = 110, Enabled = false };
            Theme.StyleSecondaryButton(_btnEdit);
            _btnEdit.Click += BtnEdit_Click;

            _btnDelete = new Button { Text = "Видалити", Width = 100, Enabled = false };
            Theme.StyleDangerButton(_btnDelete);
            _btnDelete.Click += BtnDelete_Click;

            actionPanel.Controls.Add(_btnCreate);
            actionPanel.Controls.Add(_btnDetails);
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
                if (e.RowIndex >= 0) BtnDetails_Click(s, e);
            };

            container.Controls.Add(_grid);
            container.Controls.Add(topPanel);
            Controls.Add(container);
        }

        private void UpdateButtonsState()
        {
            bool hasSelection = _grid.SelectedRows.Count > 0;
            _btnDetails.Enabled = hasSelection;
            _btnEdit.Enabled = hasSelection;
            _btnDelete.Enabled = hasSelection;
        }

        public void LoadData()
        {
            var data = ShowHistory ? _hikeService.GetHistory() : _hikeService.GetActive();
            _grid.DataSource = null;
            _grid.DataSource = data;

            if (_grid.Columns["Id"] != null) _grid.Columns["Id"].Visible = false;
            if (_grid.Columns["RouteId"] != null) _grid.Columns["RouteId"].Visible = false;

            if (_grid.Columns["Name"] != null)
            {
                _grid.Columns["Name"].HeaderText = "Назва походу";
                _grid.Columns["Name"].FillWeight = 30;
            }
            if (_grid.Columns["RouteName"] != null)
            {
                _grid.Columns["RouteName"].HeaderText = "Маршрут";
                _grid.Columns["RouteName"].FillWeight = 30;
            }
            if (_grid.Columns["StartDate"] != null)
            {
                _grid.Columns["StartDate"].HeaderText = "Дата початку";
                _grid.Columns["StartDate"].DefaultCellStyle.Format = "dd.MM.yyyy";
                _grid.Columns["StartDate"].FillWeight = 15;
            }
            if (_grid.Columns["DurationDays"] != null)
            {
                _grid.Columns["DurationDays"].HeaderText = "Тривалість (дн.)";
                _grid.Columns["DurationDays"].FillWeight = 12;
                _grid.Columns["DurationDays"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            }
            if (_grid.Columns["EndDate"] != null)
            {
                _grid.Columns["EndDate"].HeaderText = "Завершення";
                _grid.Columns["EndDate"].DefaultCellStyle.Format = "dd.MM.yyyy";
                _grid.Columns["EndDate"].FillWeight = 15;
            }

            UpdateButtonsState();
        }

        private void BtnCreate_Click(object? sender, EventArgs e)
        {
            using (var form = new HikeEditForm())
            {
                if (form.ShowDialog(this) == DialogResult.OK)
                {
                    LoadData();
                }
            }
        }

        private void BtnDetails_Click(object? sender, EventArgs e)
        {
            if (_grid.SelectedRows.Count == 0) return;

            var selected = (Hike)_grid.SelectedRows[0].DataBoundItem;
            using (var form = new HikeDetailsForm(selected))
            {
                form.ShowDialog(this);
                LoadData();
            }
        }

        private void BtnEdit_Click(object? sender, EventArgs e)
        {
            if (_grid.SelectedRows.Count == 0) return;

            var selected = (Hike)_grid.SelectedRows[0].DataBoundItem;
            using (var form = new HikeEditForm(selected))
            {
                if (form.ShowDialog(this) == DialogResult.OK)
                {
                    LoadData();
                }
            }
        }

        private void BtnDelete_Click(object? sender, EventArgs e)
        {
            if (_grid.SelectedRows.Count == 0) return;

            var selected = (Hike)_grid.SelectedRows[0].DataBoundItem;
            var result = MessageBox.Show(
                $"Ви дійсно бажаєте видалити похід «{selected.Name}»?\n\nУвага: пов'язані учасники, спорядження та витрати також будуть видалені!",
                "Підтвердження видалення",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (result == DialogResult.Yes)
            {
                _hikeService.Delete(selected.Id);
                LoadData();
            }
        }
    }
}
