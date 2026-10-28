using System;
using System.Drawing;
using System.Windows.Forms;
using HikeManager.Models;
using HikeManager.Services;

namespace HikeManager.Views
{
    public class EquipmentEditForm : Form
    {
        private readonly EquipmentService _service;
        public EquipmentItem Item { get; private set; }

        private TextBox _txtName = null!;
        private NumericUpDown _numQuantity = null!;
        private Button _btnSave = null!;
        private Button _btnCancel = null!;

        public EquipmentEditForm(int hikeId, EquipmentItem? item = null)
        {
            _service = new EquipmentService();
            Item = item != null
                ? new EquipmentItem { Id = item.Id, HikeId = item.HikeId, Name = item.Name, Quantity = item.Quantity }
                : new EquipmentItem { HikeId = hikeId, Quantity = 1 };

            SetupUI();
            LoadData();
        }

        private void SetupUI()
        {
            Text = Item.Id == 0 ? "Додати спорядження" : "Редагувати спорядження";
            Size = new Size(380, 250);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            BackColor = Theme.BackgroundColor;
            Font = Theme.RegularFont;

            Label lblName = new Label
            {
                Text = "Назва спорядження *",
                Location = new Point(24, 18),
                AutoSize = true,
                Font = Theme.BoldFont,
                ForeColor = Theme.TextPrimary
            };

            _txtName = new TextBox
            {
                Location = new Point(24, 42),
                Width = 315,
                Font = Theme.RegularFont
            };

            Label lblQty = new Label
            {
                Text = "Кількість *",
                Location = new Point(24, 82),
                AutoSize = true,
                Font = Theme.BoldFont,
                ForeColor = Theme.TextPrimary
            };

            _numQuantity = new NumericUpDown
            {
                Location = new Point(24, 106),
                Width = 140,
                Font = Theme.RegularFont,
                Minimum = 1,
                Maximum = 1000,
                Value = Item.Quantity >= 1 ? Item.Quantity : 1
            };

            _btnSave = new Button
            {
                Text = "Зберегти",
                Location = new Point(130, 155),
                Width = 100,
                DialogResult = DialogResult.None
            };
            Theme.StylePrimaryButton(_btnSave);
            _btnSave.Click += BtnSave_Click;

            _btnCancel = new Button
            {
                Text = "Скасувати",
                Location = new Point(240, 155),
                Width = 100,
                DialogResult = DialogResult.Cancel
            };
            Theme.StyleSecondaryButton(_btnCancel);
            _btnCancel.Click += (s, e) => DialogResult = DialogResult.Cancel;

            AcceptButton = _btnSave;
            CancelButton = _btnCancel;

            Controls.Add(lblName);
            Controls.Add(_txtName);
            Controls.Add(lblQty);
            Controls.Add(_numQuantity);
            Controls.Add(_btnSave);
            Controls.Add(_btnCancel);
        }

        private void LoadData()
        {
            _txtName.Text = Item.Name;
            _numQuantity.Value = Item.Quantity >= 1 ? Item.Quantity : 1;
        }

        private void BtnSave_Click(object? sender, EventArgs e)
        {
            Item.Name = _txtName.Text.Trim();
            Item.Quantity = (int)_numQuantity.Value;

            try
            {
                _service.Save(Item);
                DialogResult = DialogResult.OK;
                Close();
            }
            catch (ValidationException ex)
            {
                MessageBox.Show(ex.Message, "Помилка валідації", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Помилка збереження: " + ex.Message, "Помилка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
