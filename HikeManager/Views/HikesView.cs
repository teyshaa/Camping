using System;
using System.Drawing;
using System.Windows.Forms;
using HikeManager.Models;
using HikeManager.Services;

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
        private readonly DataGridView _grid;
        private readonly Button? _btnCreate;
        private readonly Button _btnDetails;
        private readonly Button _btnEdit;
        private readonly Button? _btnRestore;
        private readonly Button _btnDelete;

        public HikesView(bool showHistory = false, IHikeService? hikeService = null)
        {
            ShowHistory = showHistory;
            _hikeService = hikeService ?? new HikeService();

            Dock = DockStyle.Fill;
            BackColor = Theme.BackgroundColor;
            Font = Theme.RegularFont;

            // Головна панель-контейнер із відступами
            Panel container = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(30, 25, 30, 25),
                BackColor = Theme.BackgroundColor
            };

            // Верхня панель: дворядковий лейаут (заголовок зверху, панель дій під ним)
            Panel topPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 112,
                BackColor = Theme.BackgroundColor
        private DataGridView _grid;
        private HikeService _hikeService;

        public HikesView(bool showHistory = false)
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
                Text = showHistory ? "Історія походів" : "Походи",
                Font = Theme.TitleFont,
                ForeColor = Theme.PrimaryColor,
                AutoSize = true,
                Location = new Point(0, 0)
            };

            Label lblSubtitle = new Label
            {
                Text = showHistory
                    ? "Архів видалених гірських походів"
                    : "Перегляд, планування та керування поточними гірськими походами",
                Font = Theme.SmallFont,
                ForeColor = Theme.TextMuted,
                AutoSize = true,
                Location = new Point(2, 38)
            };

            // Панель інструментів / кнопок дій (власний рядок під заголовком)
            FlowLayoutPanel actionPanel = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.LeftToRight,
                Dock = DockStyle.Bottom,
                Height = 44,
                WrapContents = false,
                AutoScroll = false,
                BackColor = Theme.BackgroundColor,
                Margin = new Padding(0),
                Padding = new Padding(0)
            };

            if (!showHistory)
            {
                _btnCreate = new Button
                {
                    Text = "+ Створити похід",
                    AutoSize = true,
                    AutoSizeMode = AutoSizeMode.GrowOnly,
                    MinimumSize = new Size(150, 38),
                    Padding = new Padding(14, 0, 14, 0)
                };
                Theme.StylePrimaryButton(_btnCreate);
                _btnCreate.Click += BtnCreate_Click;
                actionPanel.Controls.Add(_btnCreate);
            }

            _btnDetails = new Button
            {
                Text = "Деталі",
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowOnly,
                MinimumSize = new Size(90, 38),
                Padding = new Padding(12, 0, 12, 0),
                Enabled = false
            };
            Theme.StyleSecondaryButton(_btnDetails);
            _btnDetails.Click += BtnDetails_Click;
            actionPanel.Controls.Add(_btnDetails);

            _btnEdit = new Button
            {
                Text = "Редагувати",
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowOnly,
                MinimumSize = new Size(115, 38),
                Padding = new Padding(12, 0, 12, 0),
                Enabled = false
            };
            Theme.StyleSecondaryButton(_btnEdit);
            _btnEdit.Click += BtnEdit_Click;
            actionPanel.Controls.Add(_btnEdit);

            if (showHistory)
            {
                _btnRestore = new Button
                {
                    Text = "Відновити",
                    AutoSize = true,
                    AutoSizeMode = AutoSizeMode.GrowOnly,
                    MinimumSize = new Size(115, 38),
                    Padding = new Padding(12, 0, 12, 0),
                    Enabled = false
                };
                Theme.StyleSecondaryButton(_btnRestore);
                _btnRestore.Click += BtnRestore_Click;
                actionPanel.Controls.Add(_btnRestore);
            }

            _btnDelete = new Button
            {
                Text = showHistory ? "Видалити назавжди" : "Видалити",
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowOnly,
                MinimumSize = new Size(105, 38),
                Padding = new Padding(12, 0, 12, 0),
                Enabled = false
            };
            Theme.StyleDangerButton(_btnDelete);
            _btnDelete.Click += BtnDelete_Click;
            actionPanel.Controls.Add(_btnDelete);

            Button btnRefresh = new Button
            {
                Text = "Оновити",
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowOnly,
                MinimumSize = new Size(95, 38),
                Padding = new Padding(12, 0, 12, 0)
                Text = showHistory
                    ? "Історія походів"
                    : "Активні та заплановані походи",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = Color.FromArgb(40, 75, 60),
                Location = new Point(20, 15),
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

                detailsForm.ShowDialog(this);
            };
            Theme.StyleSecondaryButton(btnRefresh);
            btnRefresh.Click += (s, e) => LoadData();
            actionPanel.Controls.Add(btnRefresh);

            topPanel.Controls.Add(lblTitle);
            topPanel.Controls.Add(lblSubtitle);
            topPanel.Controls.Add(actionPanel);

            // Картка для таблиці
            Panel gridCard = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Theme.CardBackground,
                Padding = new Padding(12)
            };

            _grid = new DataGridView
            {
                Dock = DockStyle.Fill
            };
            Theme.StyleDataGridView(_grid);
            SetupGridColumns();

            _grid.SelectionChanged += (s, e) => UpdateActionButtonsState();
            _grid.CellDoubleClick += (s, e) =>
            {
                if (e.RowIndex >= 0) BtnDetails_Click(s, e);
            };

            gridCard.Controls.Add(_grid);

            container.Controls.Add(gridCard);
            container.Controls.Add(topPanel);

            Controls.Add(container);

            LoadData();
        }

        private void SetupGridColumns()
        {
            _grid.AutoGenerateColumns = false;
            _grid.Columns.Clear();

            _grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Id",
                HeaderText = "ID",
                Visible = false
            });

            _grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "RouteId",
                HeaderText = "RouteId",
                Visible = false
            });

            _grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Name",
                HeaderText = "Назва походу",
                FillWeight = 32
            });

            _grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "RouteName",
                HeaderText = "Маршрут",
                FillWeight = 28
            });

            _grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "StartDate",
                HeaderText = "Дата початку",
                DefaultCellStyle = { Format = "dd.MM.yyyy" },
                FillWeight = 16
            });

            _grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "EndDate",
                HeaderText = "Дата завершення",
                DefaultCellStyle = { Format = "dd.MM.yyyy" },
                FillWeight = 16
            });

            _grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "DurationDays",
                HeaderText = "Тривалість (дн.)",
                FillWeight = 12
            });
        }

        public void LoadData()
        {
            try
            {
                var data = ShowHistory ? _hikeService.GetHistory() : _hikeService.GetActive();
                _grid.DataSource = null;
                _grid.DataSource = data;
                UpdateActionButtonsState();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Не вдалося завантажити походи: " + ex.Message, "Помилка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private Hike? GetSelectedHike()
        {
            if (_grid.SelectedRows.Count == 0) return null;
            return _grid.SelectedRows[0].DataBoundItem as Hike;
        }

        private void UpdateActionButtonsState()
        {
            bool hasSelection = _grid.SelectedRows.Count > 0;
            _btnDetails.Enabled = hasSelection;
            _btnEdit.Enabled = hasSelection;
            if (_btnRestore != null) _btnRestore.Enabled = hasSelection;
            _btnDelete.Enabled = hasSelection;
        }

        private void BtnCreate_Click(object? sender, EventArgs e)
        {
            using (var form = new HikeEditForm(null, _hikeService))
            {
                if (form.ShowDialog(FindForm()) == DialogResult.OK)
                {
                    LoadData();
                }
            }
        }

        private void BtnEdit_Click(object? sender, EventArgs e)
        {
            var selected = GetSelectedHike();
            if (selected == null) return;

            using (var form = new HikeEditForm(selected, _hikeService))
            {
                if (form.ShowDialog(FindForm()) == DialogResult.OK)
                {
                    LoadData();
                }
            }
        }

        private void BtnDetails_Click(object? sender, EventArgs e)
        {
            var selected = GetSelectedHike();
            if (selected == null) return;

            using (var form = new HikeDetailsForm(selected))
            {
                form.ShowDialog(FindForm());
                LoadData();
            }
        }

        private void BtnRestore_Click(object? sender, EventArgs e)
        {
            var selected = GetSelectedHike();
            if (selected == null) return;

            var result = MessageBox.Show(
                $"Відновити похід «{selected.Name}»?\nВін знову відображатиметься у вкладці «Походи».",
                "Відновлення походу",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                try
                {
                    _hikeService.RestoreHike(selected.Id);
                    LoadData();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Помилка: " + ex.Message, "Помилка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void BtnDelete_Click(object? sender, EventArgs e)
        {
            var selected = GetSelectedHike();
            if (selected == null) return;

            if (!ShowHistory)
            {
                var result = MessageBox.Show(
                    $"Видалити похід «{selected.Name}»?\nВін буде переміщений у вкладку «Історія».",
                    "Видалення походу",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    try
                    {
                        _hikeService.Delete(selected.Id);
                        LoadData();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Помилка: " + ex.Message, "Помилка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            else
            {
                var result = MessageBox.Show(
                    $"Ви впевнені, що хочете остаточно видалити похід «{selected.Name}» з бази даних?\nУсі пов'язані дані (учасники, спорядження, витрати) також будуть безповоротно видалені.",
                    "Остаточне видалення",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning);

                if (result == DialogResult.Yes)
                {
                    try
                    {
                        _hikeService.HardDelete(selected.Id);
                        LoadData();
                    }
                    catch (ValidationException valEx)
                    {
                        MessageBox.Show(valEx.Message, "Неможливо видалити", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Помилка видалення: " + ex.Message, "Помилка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            topPanel.Controls.Add(title);
            topPanel.Controls.Add(btnCreate);
            topPanel.Controls.Add(btnEdit);
            topPanel.Controls.Add(btnDetails);
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