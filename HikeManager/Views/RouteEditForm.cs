using System;
using System.Drawing;
using System.Windows.Forms;
using HikeManager.Models;

namespace HikeManager.Views
{
    public class RouteEditForm : Form
    {
        private readonly TextBox _txtName;
        private readonly TextBox _txtLocation;
        private readonly ComboBox _cboDifficulty;
        private readonly NumericUpDown _numDurationDays;
        private readonly NumericUpDown _numLengthKm;
        private readonly ErrorProvider _errorProvider;

        public Route RouteData { get; private set; }

        public RouteEditForm(Route? existingRoute = null)
        {
            RouteData = existingRoute != null
                ? new Route
                {
                    Id = existingRoute.Id,
                    Name = existingRoute.Name,
                    Location = existingRoute.Location,
                    Difficulty = existingRoute.Difficulty,
                    DurationDays = existingRoute.DurationDays,
                    LengthKm = existingRoute.LengthKm
                }
                : new Route
                {
                    Difficulty = "Легкий",
                    DurationDays = 1
                };

            _errorProvider = new ErrorProvider { BlinkStyle = ErrorBlinkStyle.NeverBlink };

            // Налаштування форми
            Text = existingRoute == null ? "Додавання нового маршруту" : "Редагування маршруту";
            Size = new Size(520, 480);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            BackColor = Theme.BackgroundColor;
            Font = Theme.RegularFont;

            // Головна картка
            Panel card = new Panel
            {
                BackColor = Theme.CardBackground,
                Location = new Point(20, 20),
                Size = new Size(465, 395),
                Padding = new Padding(24)
            };

            Label lblTitle = new Label
            {
                Text = Text,
                Font = Theme.SubtitleFont,
                ForeColor = Theme.PrimaryColor,
                AutoSize = true,
                Location = new Point(20, 16)
            };
            card.Controls.Add(lblTitle);

            int currentY = 60;
            const int labelWidth = 140;
            const int inputWidth = 260;
            const int rowHeight = 44;

            // Назва
            Label lblName = CreateFieldLabel("Назва маршруту *:", 20, currentY);
            _txtName = new TextBox
            {
                Location = new Point(labelWidth + 30, currentY),
                Width = inputWidth,
                Text = RouteData.Name
            };
            card.Controls.Add(lblName);
            card.Controls.Add(_txtName);
            currentY += rowHeight;

            // Локація
            Label lblLocation = CreateFieldLabel("Локація / Регіон:", 20, currentY);
            _txtLocation = new TextBox
            {
                Location = new Point(labelWidth + 30, currentY),
                Width = inputWidth,
                Text = RouteData.Location ?? string.Empty
            };
            card.Controls.Add(lblLocation);
            card.Controls.Add(_txtLocation);
            currentY += rowHeight;

            // Складність
            Label lblDifficulty = CreateFieldLabel("Складність *:", 20, currentY);
            _cboDifficulty = new ComboBox
            {
                Location = new Point(labelWidth + 30, currentY),
                Width = inputWidth,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            _cboDifficulty.Items.AddRange(new object[] { "Легкий", "Середній", "Складний" });
            _cboDifficulty.SelectedItem = string.IsNullOrWhiteSpace(RouteData.Difficulty) ? "Легкий" : RouteData.Difficulty;
            card.Controls.Add(lblDifficulty);
            card.Controls.Add(_cboDifficulty);
            currentY += rowHeight;

            // Тривалість (дні)
            Label lblDuration = CreateFieldLabel("Тривалість (днів) *:", 20, currentY);
            _numDurationDays = new NumericUpDown
            {
                Location = new Point(labelWidth + 30, currentY),
                Width = inputWidth,
                Minimum = 1,
                Maximum = 365,
                Value = Math.Max(1, RouteData.DurationDays)
            };
            card.Controls.Add(lblDuration);
            card.Controls.Add(_numDurationDays);
            currentY += rowHeight;

            // Довжина (км)
            Label lblLength = CreateFieldLabel("Довжина (км):", 20, currentY);
            _numLengthKm = new NumericUpDown
            {
                Location = new Point(labelWidth + 30, currentY),
                Width = inputWidth,
                Minimum = 0,
                Maximum = 5000,
                DecimalPlaces = 1,
                Increment = 0.5m,
                Value = RouteData.LengthKm.HasValue ? (decimal)RouteData.LengthKm.Value : 0
            };
            card.Controls.Add(lblLength);
            card.Controls.Add(_numLengthKm);
            currentY += rowHeight + 10;

            // Кнопки
            Button btnSave = new Button
            {
                Text = "Зберегти",
                Location = new Point(labelWidth + 30, currentY),
                Width = 125,
                DialogResult = DialogResult.None
            };
            Theme.StylePrimaryButton(btnSave);
            btnSave.Click += BtnSave_Click;

            Button btnCancel = new Button
            {
                Text = "Скасувати",
                Location = new Point(labelWidth + 165, currentY),
                Width = 125,
                DialogResult = DialogResult.Cancel
            };
            Theme.StyleSecondaryButton(btnCancel);

            card.Controls.Add(btnSave);
            card.Controls.Add(btnCancel);

            Controls.Add(card);

            AcceptButton = btnSave;
            CancelButton = btnCancel;
        }

        private static Label CreateFieldLabel(string text, int x, int y)
        {
            return new Label
            {
                Text = text,
                Location = new Point(x, y + 4),
                AutoSize = true,
                Font = Theme.BoldFont,
                ForeColor = Theme.TextPrimary
            };
        }

        private void BtnSave_Click(object? sender, EventArgs e)
        {
            _errorProvider.Clear();
            bool hasError = false;

            if (string.IsNullOrWhiteSpace(_txtName.Text))
            {
                _errorProvider.SetError(_txtName, "Будь ласка, введіть назву маршруту");
                hasError = true;
            }

            if (_cboDifficulty.SelectedIndex < 0)
            {
                _errorProvider.SetError(_cboDifficulty, "Оберіть рівень складності");
                hasError = true;
            }

            if (_numDurationDays.Value < 1)
            {
                _errorProvider.SetError(_numDurationDays, "Тривалість повинна бути не менше 1 дня");
                hasError = true;
            }

            if (hasError)
            {
                return;
            }

            RouteData.Name = _txtName.Text.Trim();
            RouteData.Location = string.IsNullOrWhiteSpace(_txtLocation.Text) ? null : _txtLocation.Text.Trim();
            RouteData.Difficulty = _cboDifficulty.SelectedItem?.ToString() ?? "Легкий";
            RouteData.DurationDays = (int)_numDurationDays.Value;
            RouteData.LengthKm = _numLengthKm.Value > 0 ? (double)_numLengthKm.Value : null;

            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
