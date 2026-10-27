using System;
using System.Windows.Forms;
using System.Drawing;
using HikeManager.Services;
using HikeManager.Models;

namespace HikeManager.Views
{
    public class ParticipantsView : UserControl
    {
        private int _currentHikeId;
        private readonly ParticipantService _service;
        private DataGridView _grid;
        private Button _btnAdd, _btnEdit, _btnDelete;

        public ParticipantsView()
        {
            _service = new ParticipantService();
            SetupUI();
        }

        public void LoadHike(int hikeId)
        {
            _currentHikeId = hikeId;
            RefreshGrid();
        }

        private void SetupUI()
        {
            this.Dock = DockStyle.Fill;
            this.BackColor = Color.FromArgb(235, 245, 235);

            Panel topPanel = new Panel { Dock = DockStyle.Top, Height = 50 };

            _btnAdd = new Button { Text = "+ Додати", Location = new Point(10, 10), Width = 90 };
            _btnEdit = new Button { Text = "Редагувати", Location = new Point(110, 10), Width = 90 };
            _btnDelete = new Button { Text = "Видалити", Location = new Point(210, 10), Width = 90, ForeColor = Color.Red };

            _btnAdd.Click += BtnAdd_Click;
            _btnEdit.Click += BtnEdit_Click;
            _btnDelete.Click += BtnDelete_Click;

            topPanel.Controls.Add(_btnAdd);
            topPanel.Controls.Add(_btnEdit);
            topPanel.Controls.Add(_btnDelete);

            _grid = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AllowUserToAddRows = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = Color.White
            };

            this.Controls.Add(_grid);
            this.Controls.Add(topPanel);
        }

        private void RefreshGrid()
        {
            if (_currentHikeId == 0) return;
            var participants = _service.GetByHike(_currentHikeId);
            _grid.DataSource = null;
            _grid.DataSource = participants;

            if (_grid.Columns["Id"] != null) _grid.Columns["Id"].Visible = false;
            if (_grid.Columns["HikeId"] != null) _grid.Columns["HikeId"].Visible = false;
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            if (_currentHikeId == 0) return;
            using (var form = new ParticipantEditForm(_currentHikeId))
            {
                if (form.ShowDialog() == DialogResult.OK)
                {
                    RefreshGrid();
                }
            }
        }

        private void BtnEdit_Click(object sender, EventArgs e)
        {
            if (_grid.SelectedRows.Count == 0 || _currentHikeId == 0) return;

            var selectedParticipant = (Participant)_grid.SelectedRows[0].DataBoundItem;
            using (var form = new ParticipantEditForm(_currentHikeId, selectedParticipant))
            {
                if (form.ShowDialog() == DialogResult.OK)
                {
                    RefreshGrid();
                }
            }
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            if (_grid.SelectedRows.Count == 0) return;

            var result = MessageBox.Show("Ви впевнені, що хочете видалити цього учасника?", "Підтвердження", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.Yes)
            {
                var selectedParticipant = (Participant)_grid.SelectedRows[0].DataBoundItem;
                _service.Delete(selectedParticipant.Id);
                RefreshGrid();
            }
        }
    }
}