using System;
using System.Drawing;
using System.Windows.Forms;
using HikeManager.Models;
using HikeManager.Services;

namespace HikeManager.Views
{
    public class ParticipantsView : UserControl
    {
        private int _currentHikeId;
        private readonly IParticipantService _service;
        private DataGridView _grid = null!;
        private Label _lblCounter = null!;
        private Button _btnAdd = null!;
        private Button _btnEdit = null!;
        private Button _btnDelete = null!;

        public ParticipantsView(IParticipantService? service = null)
        {
            _service = service ?? new ParticipantService();
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

            // Головна панель з відступами
            Panel container = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(20, 15, 20, 15),
                BackColor = Theme.BackgroundColor
            };

            // Верхня панель дій
            Panel topPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 60,
                BackColor = Theme.BackgroundColor
            };

            Label lblTitle = new Label
            {
                Text = "👥 Учасники походу",
                Font = Theme.SubtitleFont,
                ForeColor = Theme.PrimaryColor,
                AutoSize = true,
                Location = new Point(0, 5)
            };

            _lblCounter = new Label
            {
                Text = "Всього зареєстровано: 0",
                Font = Theme.SmallFont,
                ForeColor = Theme.TextMuted,
                AutoSize = true,
                Location = new Point(2, 34)
            };

            topPanel.Controls.Add(lblTitle);
            topPanel.Controls.Add(_lblCounter);

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

            _btnAdd = new Button { Text = "+ Додати учасника", Width = 150 };
            Theme.StylePrimaryButton(_btnAdd);
            _btnAdd.Click += BtnAdd_Click;

            _btnEdit = new Button { Text = "Редагувати", Width = 110, Enabled = false };
            Theme.StyleSecondaryButton(_btnEdit);
            _btnEdit.Click += BtnEdit_Click;

            _btnDelete = new Button { Text = "Видалити", Width = 100, Enabled = false };
            Theme.StyleDangerButton(_btnDelete);
            _btnDelete.Click += BtnDelete_Click;

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

            var participants = _service.GetByHike(_currentHikeId);
            _grid.DataSource = null;
            _grid.DataSource = participants;

            int count = _service.CountByHike(_currentHikeId);
            _lblCounter.Text = $"Всього зареєстровано: {count} {(count == 1 ? "учасник" : count >= 2 && count <= 4 ? "учасники" : "учасників")}";

            if (_grid.Columns["Id"] != null) _grid.Columns["Id"].Visible = false;
            if (_grid.Columns["HikeId"] != null) _grid.Columns["HikeId"].Visible = false;

            if (_grid.Columns["FullName"] != null)
            {
                _grid.Columns["FullName"].HeaderText = "ПІБ учасника";
                _grid.Columns["FullName"].FillWeight = 40;
            }
            if (_grid.Columns["Phone"] != null)
            {
                _grid.Columns["Phone"].HeaderText = "Номер телефону";
                _grid.Columns["Phone"].FillWeight = 30;
            }
            if (_grid.Columns["Note"] != null)
            {
                _grid.Columns["Note"].HeaderText = "Примітка / особливості";
                _grid.Columns["Note"].FillWeight = 30;
            }

            UpdateButtonsState();
        }

        private void BtnAdd_Click(object? sender, EventArgs e)
        {
            if (_currentHikeId <= 0) return;

            using (var form = new ParticipantEditForm(_currentHikeId))
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

            var selectedParticipant = (Participant)_grid.SelectedRows[0].DataBoundItem;
            using (var form = new ParticipantEditForm(_currentHikeId, selectedParticipant))
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

            var selected = (Participant)_grid.SelectedRows[0].DataBoundItem;
            var result = MessageBox.Show(
                $"Ви дійсно бажаєте видалити учасника «{selected.FullName}» зі списку походу?",
                "Підтвердження видалення",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                _service.Delete(selected.Id);
                RefreshGrid();
            }
        }
    }
}
