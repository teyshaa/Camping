using System;
using System.Drawing;
using System.Windows.Forms;
using HikeManager.Models;
using HikeManager.Services;

namespace HikeManager.Views
{
    public class ParticipantEditForm : Form
    {
        private readonly ParticipantService _service;
        public Participant Participant { get; private set; }

        private TextBox _txtFullName = null!;
        private TextBox _txtPhone = null!;
        private TextBox _txtNote = null!;
        private Button _btnSave = null!;
        private Button _btnCancel = null!;

        public ParticipantEditForm(int hikeId, Participant? participant = null)
        {
            _service = new ParticipantService();
            // Якщо редагуємо — беремо існуючого, якщо створюємо — нового з цим hikeId
            Participant = participant ?? new Participant { HikeId = hikeId };

            SetupUI();
            LoadData();
        }

        private void SetupUI()
        {
            this.Text = Participant.Id == 0 ? "Додати учасника" : "Редагувати учасника";
            this.Size = new Size(420, 340);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = Theme.BackgroundColor;
            this.Font = Theme.RegularFont;

            Label lblFullName = new Label { Text = "ПІБ *", Location = new Point(24, 18), AutoSize = true, Font = Theme.BoldFont, ForeColor = Theme.TextPrimary };
            _txtFullName = new TextBox { Location = new Point(24, 42), Width = 355, Font = Theme.RegularFont };

            Label lblPhone = new Label { Text = "Телефон (напр. +380501234567)", Location = new Point(24, 82), AutoSize = true, Font = Theme.BoldFont, ForeColor = Theme.TextPrimary };
            _txtPhone = new TextBox { Location = new Point(24, 106), Width = 355, Font = Theme.RegularFont };

            Label lblNote = new Label { Text = "Примітка", Location = new Point(24, 146), AutoSize = true, Font = Theme.BoldFont, ForeColor = Theme.TextPrimary };
            _txtNote = new TextBox { Location = new Point(24, 170), Width = 355, Font = Theme.RegularFont };

            _btnSave = new Button
            {
                Text = "Зберегти",
                Location = new Point(165, 230),
                Width = 100,
                DialogResult = DialogResult.None
            };
            Theme.StylePrimaryButton(_btnSave);
            _btnSave.Click += BtnSave_Click;

            _btnCancel = new Button
            {
                Text = "Скасувати",
                Location = new Point(275, 230),
                Width = 100,
                DialogResult = DialogResult.Cancel
            };
            Theme.StyleSecondaryButton(_btnCancel);
            _btnCancel.Click += (s, e) => this.DialogResult = DialogResult.Cancel;

            this.AcceptButton = _btnSave;
            this.CancelButton = _btnCancel;

            this.Controls.Add(lblFullName);
            this.Controls.Add(_txtFullName);
            this.Controls.Add(lblPhone);
            this.Controls.Add(_txtPhone);
            this.Controls.Add(lblNote);
            this.Controls.Add(_txtNote);
            this.Controls.Add(_btnSave);
            this.Controls.Add(_btnCancel);
        }

        private void LoadData()
        {
            _txtFullName.Text = Participant.FullName;
            _txtPhone.Text = Participant.Phone;
            _txtNote.Text = Participant.Note;
        }

        private void BtnSave_Click(object? sender, EventArgs e)
        {
            Participant.FullName = _txtFullName.Text;
            Participant.Phone = _txtPhone.Text;
            Participant.Note = _txtNote.Text;

            try
            {
                // Зберігаємо через сервіс (там спрацює наша валідація)
                _service.Save(Participant);
                this.DialogResult = DialogResult.OK;
            }
            catch (ValidationException ex)
            {
                // Помилки валідації показуємо українською
                MessageBox.Show(ex.Message, "Помилка вводу", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Сталася помилка при збереженні: " + ex.Message, "Помилка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}