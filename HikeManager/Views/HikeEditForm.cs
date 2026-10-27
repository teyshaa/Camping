using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using HikeManager.Models;
using HikeManager.Services;

namespace HikeManager.Views
{
    /// <summary>
    /// Форма для створення нового та редагування існуючого походу.
    /// Зберігає дані через HikeService; перехоплює ValidationException.
    /// Жодного SQL у View — усі дані надходять через сервісний шар.
    /// </summary>
    public class HikeEditForm : Form
    {
        // ── Поля ──────────────────────────────────────────────────────────
        private readonly IHikeService  _hikeService;
        private readonly IRouteService _routeService;
        private readonly Hike          _hike;
        private readonly bool          _isEditMode;

        private readonly TextBox        _txtName;
        private readonly DateTimePicker _dtpStartDate;
        private readonly ComboBox       _cboRoute;
        private readonly Button         _btnSave;
        private readonly ErrorProvider  _errorProvider;

        // ── Конструктор ───────────────────────────────────────────────────
        public HikeEditForm(Hike?          existingHike = null,
                            IHikeService?  hikeService  = null,
                            IRouteService? routeService = null)
        {
            _hikeService  = hikeService  ?? new HikeService();
            _routeService = routeService ?? new RouteService();
            _isEditMode   = existingHike != null;

            // Копія даних, щоб не мутувати оригінал до збереження
            _hike = _isEditMode
                ? new Hike
                  {
                      Id        = existingHike!.Id,
                      Name      = existingHike.Name,
                      StartDate = existingHike.StartDate,
                      RouteId   = existingHike.RouteId
                  }
                : new Hike { StartDate = DateTime.Today };

            _errorProvider = new ErrorProvider { BlinkStyle = ErrorBlinkStyle.NeverBlink };

            // ── Параметри форми ───────────────────────────────────────────
            Text            = _isEditMode ? "Редагування походу" : "Новий похід";
            Size            = new Size(500, 360);
            StartPosition   = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox     = false;
            MinimizeBox     = false;
            BackColor       = Color.FromArgb(235, 245, 235);
            Font            = new Font("Segoe UI", 10f, FontStyle.Regular);

            // ── Картка з полями ───────────────────────────────────────────
            Panel card = new Panel
            {
                BackColor = Theme.CardBackground,
                Location  = new Point(20, 20),
                Size      = new Size(445, 290),
                Padding   = new Padding(24)
            };

            Label lblFormTitle = new Label
            {
                Text      = Text,
                Font      = new Font("Segoe UI", 16f, FontStyle.Bold),
                ForeColor = Color.FromArgb(40, 75, 60),
                AutoSize  = true,
                Location  = new Point(20, 16)
            };
            card.Controls.Add(lblFormTitle);

            int      y     = 62;
            const int IX   = 175;   // X початку поля вводу
            const int IW   = 240;   // ширина поля вводу
            const int RH   = 48;    // висота рядка

            // ── Поле: Назва ───────────────────────────────────────────────
            card.Controls.Add(MakeLabel("Назва походу *:", 20, y));
            _txtName = new TextBox { Location = new Point(IX, y - 2), Width = IW, Text = _hike.Name };
            card.Controls.Add(_txtName);
            y += RH;

            // ── Поле: Дата початку ────────────────────────────────────────
            card.Controls.Add(MakeLabel("Дата початку *:", 20, y));
            _dtpStartDate = new DateTimePicker
            {
                Location     = new Point(IX, y - 2),
                Width        = IW,
                Format       = DateTimePickerFormat.Custom,
                CustomFormat = "dd.MM.yyyy",
                Value        = _hike.StartDate
            };
            card.Controls.Add(_dtpStartDate);
            y += RH;

            // ── Поле: Маршрут (ComboBox) ──────────────────────────────────
            card.Controls.Add(MakeLabel("Маршрут *:", 20, y));
            _cboRoute = new ComboBox
            {
                Location      = new Point(IX, y - 2),
                Width         = IW,
                DropDownStyle = ComboBoxStyle.DropDownList,
                DisplayMember = "Name",   // відображається назва маршруту
                ValueMember   = "Id"      // значення — Id маршруту
            };
            card.Controls.Add(_cboRoute);
            y += RH + 10;

            // «Зберегти» — кнопка головної дії
            _btnSave = new Button
            {
                Text         = "Зберегти",
                Location     = new Point(IX, y),
                Width        = 115,
                DialogResult = DialogResult.None
            };
            _btnSave.FlatStyle                 = FlatStyle.Flat;
            _btnSave.FlatAppearance.BorderSize = 0;
            _btnSave.BackColor                 = Color.FromArgb(40, 75, 60);
            _btnSave.ForeColor                 = Color.White;
            _btnSave.Font                      = new Font("Segoe UI", 10f, FontStyle.Bold);
            _btnSave.Cursor                    = Cursors.Hand;
            _btnSave.Height                    = 38;
            _btnSave.Click += BtnSave_Click;

            // «Скасувати» — другорядна кнопка
            Button btnCancel = new Button
            {
                Text         = "Скасувати",
                Location     = new Point(IX + 125, y),
                Width        = 115,
                DialogResult = DialogResult.Cancel
            };
            btnCancel.FlatStyle                  = FlatStyle.Flat;
            btnCancel.FlatAppearance.BorderColor = Color.Gray;
            btnCancel.FlatAppearance.BorderSize  = 1;
            btnCancel.BackColor                  = Color.White;
            btnCancel.ForeColor                  = Color.Black;
            btnCancel.Font                       = new Font("Segoe UI", 10f, FontStyle.Bold);
            btnCancel.Cursor                     = Cursors.Hand;
            btnCancel.Height                     = 38;

            card.Controls.Add(_btnSave);
            card.Controls.Add(btnCancel);
            Controls.Add(card);

            AcceptButton = _btnSave;
            CancelButton = btnCancel;

            // Завантажуємо маршрути після побудови UI
            LoadRoutes();
        }

        // ── Завантаження маршрутів у ComboBox ────────────────────────────
        private void LoadRoutes()
        {
            try
            {
                List<Route> routes = _routeService.GetAll();

                if (routes.Count == 0)
                {
                    // Немає маршрутів — блокуємо збереження та пояснюємо чому
                    _btnSave.Enabled = false;
                    _cboRoute.Enabled = false;

                    MessageBox.Show(
                        "Спочатку додайте маршрут у розділі «Маршрути», а потім створюйте похід.",
                        "Маршрути відсутні",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                    return;
                }

                _cboRoute.DataSource    = null;
                _cboRoute.DataSource    = routes;
                _cboRoute.DisplayMember = "Name";
                _cboRoute.ValueMember   = "Id";

                // Режим редагування — відновлюємо раніше збережений маршрут
                if (_isEditMode && _hike.RouteId > 0)
                {
                    _cboRoute.SelectedValue = _hike.RouteId;

                    // Якщо маршрут було видалено з БД — залишаємо перший у списку
                    if (_cboRoute.SelectedValue == null ||
                        (int)_cboRoute.SelectedValue != _hike.RouteId)
                    {
                        _cboRoute.SelectedIndex = 0;
                    }
                }
                else if (_cboRoute.Items.Count > 0)
                {
                    _cboRoute.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Не вдалося завантажити маршрути: " + ex.Message,
                    "Помилка",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        // ── Збереження ────────────────────────────────────────────────────
        private void BtnSave_Click(object? sender, EventArgs e)
        {
            _errorProvider.Clear();

            // Клієнтська перевірка обов'язкових полів до звернення до сервісу
            bool hasError = false;

            if (string.IsNullOrWhiteSpace(_txtName.Text))
            {
                _errorProvider.SetError(_txtName, "Введіть назву походу");
                hasError = true;
            }

            if (_cboRoute.SelectedValue == null)
            {
                _errorProvider.SetError(_cboRoute, "Оберіть маршрут");
                hasError = true;
            }

            if (hasError) return;

            // Заповнюємо об'єкт даними з форми
            _hike.Name      = _txtName.Text.Trim();
            _hike.StartDate = _dtpStartDate.Value.Date;
            _hike.RouteId   = (int)_cboRoute.SelectedValue!;

            try
            {
                if (_isEditMode)
                    _hikeService.Update(_hike);
                else
                    _hikeService.Create(_hike);

                DialogResult = DialogResult.OK;
                Close();
            }
            catch (ValidationException valEx)
            {
                // Помилки валідації з сервісу — показуємо українською
                MessageBox.Show(
                    valEx.Message,
                    "Помилка валідації",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Помилка збереження: " + ex.Message,
                    "Помилка",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        // ── Допоміжний метод: підписний лейбл поля ───────────────────────
        private static Label MakeLabel(string text, int x, int y) =>
            new Label
            {
                Text      = text,
                Location  = new Point(x, y + 4),
                AutoSize  = true,
                Font      = Theme.BoldFont,
                ForeColor = Theme.TextPrimary
            };
    }
}
