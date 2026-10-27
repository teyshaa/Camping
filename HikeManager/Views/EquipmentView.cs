using System;
using System.Drawing;
using System.Windows.Forms;
using HikeManager.Models;
using HikeManager.Services;

namespace HikeManager.Views
{
    public class EquipmentView : UserControl
    {
        private int _currentHikeId;
        private readonly EquipmentService _service;
        private DataGridView _grid;
        private Button _btnAdd, _btnEdit, _btnDelete;

        public EquipmentView()
        {
            _service = new EquipmentService();
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
            var items = _service.GetByHike(_currentHikeId);
            _grid.DataSource = null;
            _grid.DataSource = items;

            if (_grid.Columns["Id"] != null) _grid.Columns["Id"].Visible = false;
            if (_grid.Columns["HikeId"] != null) _grid.Columns["HikeId"].Visible = false;
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            if (_currentHikeId == 0) return;
            ShowEditDialog(new EquipmentItem { HikeId = _currentHikeId });
        }

        private void BtnEdit_Click(object sender, EventArgs e)
        {
            if (_grid.SelectedRows.Count == 0 || _currentHikeId == 0) return;
            var selectedItem = (EquipmentItem)_grid.SelectedRows[0].DataBoundItem;
            ShowEditDialog(selectedItem);
        }

        private void ShowEditDialog(EquipmentItem item)
        {
            using (Form dialog = new Form())
            {
                dialog.Text = item.Id == 0 ? "Додати спорядження" : "Редагувати спорядження";
                dialog.Size = new Size(350, 220);
                dialog.StartPosition = FormStartPosition.CenterParent;
                dialog.FormBorderStyle = FormBorderStyle.FixedDialog;
                dialog.MaximizeBox = false;
                dialog.MinimizeBox = false;
                dialog.BackColor = Color.FromArgb(235, 245, 235);

                Label lblName = new Label { Text = "Назва *", Location = new Point(20, 15), AutoSize = true };
                TextBox txtName = new TextBox { Text = item.Name, Location = new Point(20, 35), Width = 290 };

                Label lblQty = new Label { Text = "Кількість *", Location = new Point(20, 70), AutoSize = true };
                NumericUpDown numQty = new NumericUpDown { Value = item.Quantity < 1 ? 1 : item.Quantity, Minimum = 1, Maximum = 100, Location = new Point(20, 90), Width = 120 };

                Button btnSave = new Button { Text = "Зберегти", Location = new Point(120, 130), Width = 90, Height = 30, BackColor = Color.FromArgb(40, 75, 60), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
                btnSave.Click += (s, e) =>
                {
                    item.Name = txtName.Text;
                    item.Quantity = (int)numQty.Value;
                    try
                    {
                        _service.Save(item);
                        dialog.DialogResult = DialogResult.OK;
                    }
                    catch (ValidationException ex)
                    {
                        MessageBox.Show(ex.Message, "Помилка вводу", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                };

                Button btnCancel = new Button { Text = "Скасувати", Location = new Point(220, 130), Width = 90, Height = 30 };
                btnCancel.Click += (s, e) => dialog.DialogResult = DialogResult.Cancel;

                dialog.Controls.Add(lblName);
                dialog.Controls.Add(txtName);
                dialog.Controls.Add(lblQty);
                dialog.Controls.Add(numQty);
                dialog.Controls.Add(btnSave);
                dialog.Controls.Add(btnCancel);

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    RefreshGrid();
                }
            }
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            if (_grid.SelectedRows.Count == 0) return;

            var result = MessageBox.Show("Ви впевнені, що хочете видалити цей предмет спорядження?", "Підтвердження", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.Yes)
            {
                var selectedItem = (EquipmentItem)_grid.SelectedRows[0].DataBoundItem;
                _service.Delete(selectedItem.Id);
                RefreshGrid();
            }
        }
    }
}