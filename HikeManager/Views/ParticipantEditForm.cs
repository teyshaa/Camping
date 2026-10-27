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

        private TextBox _txtFullName;
        private TextBox _txtPhone;
        private TextBox _txtNote;
        private Button _btnSave;
        private Button _btnCancel;

        public ParticipantEditForm(int hikeId, Participant participant = null)
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
            this.Size = new Size(400, 320);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = Color.FromArgb(235, 245, 235);

            Label lblFullName = new Label { Text = "ПІБ *", Location = new Point(20, 20), AutoSize = true, Font = new Font("Segoe UI", 9, FontStyle.Bold) };
            _txtFullName = new TextBox { Location = new Point(20, 45), Width = 340 };

            Label lblPhone = new Label { Text = "Телефон", Location = new Point(20, 85), AutoSize = true };
            _txtPhone = new TextBox { Location = new Point(20, 110), Width = 340 };

            Label lblNote = new Label { Text = "Примітка", Location = new Point(20, 150), AutoSize = true };
            _txtNote = new TextBox { Location = new Point(20, 175), Width = 340 };

            _btnSave = new Button
            {
                Text = "Зберегти",
                Location = new Point(160, 225),
                Width = 90,
                Height = 35,
                BackColor = Color.FromArgb(40, 75, 60),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            _btnSave.FlatAppearance.BorderSize = 0;
            _btnSave.Click += BtnSave_Click;

            _btnCancel = new Button { Text = "Скасувати", Location = new Point(260, 225), Width = 90, Height = 35 };
            _btnCancel.Click += (s, e) => this.DialogResult = DialogResult.Cancel;

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

        private void BtnSave_Click(object sender, EventArgs e)
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