using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using HikeManager.Models;
using HikeManager.Services;

namespace HikeManager.Views
{
    public class RoutesView : UserControl
    {
        private readonly IRouteService _routeService;
        private readonly DataGridView _grid;
        private readonly Button _btnEdit;
        private readonly Button _btnDelete;

        public RoutesView(IRouteService? routeService = null)
        {
            _routeService = routeService ?? new RouteService();

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

            // Верхня панель дій
            Panel topPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 65,
                BackColor = Theme.BackgroundColor
            };

            Label lblTitle = new Label
            {
                Text = "Каталог маршрутів",
                Font = Theme.TitleFont,
                ForeColor = Theme.PrimaryColor,
                AutoSize = true,
                Location = new Point(0, 5)
            };

            Label lblSubtitle = new Label
            {
                Text = "Перегляд, додавання, редагування та видалення гірських маршрутів",
                Font = Theme.SmallFont,
                ForeColor = Theme.TextMuted,
                AutoSize = true,
                Location = new Point(2, 42)
            };

            // Кнопки дій праворуч
            FlowLayoutPanel actionPanel = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.RightToLeft,
                Dock = DockStyle.Right,
                Width = 460,
                Height = 55
            };

            Button btnAdd = new Button
            {
                Text = "+ Додати маршрут",
                Width = 160
            };
            Theme.StylePrimaryButton(btnAdd);
            btnAdd.Click += BtnAdd_Click;

            _btnEdit = new Button
            {
                Text = "Редагувати",
                Width = 110,
                Enabled = false
            };
            Theme.StyleSecondaryButton(_btnEdit);
            _btnEdit.Click += BtnEdit_Click;

            _btnDelete = new Button
            {
                Text = "Видалити",
                Width = 100,
                Enabled = false
            };
            Theme.StyleDangerButton(_btnDelete);
            _btnDelete.Click += BtnDelete_Click;

            Button btnRefresh = new Button
            {
                Text = "Оновити",
                Width = 80
            };
            Theme.StyleSecondaryButton(btnRefresh);
            btnRefresh.Click += (s, e) => LoadRoutes();

            actionPanel.Controls.Add(btnAdd);
            actionPanel.Controls.Add(_btnDelete);
            actionPanel.Controls.Add(_btnEdit);
            actionPanel.Controls.Add(btnRefresh);

            topPanel.Controls.Add(lblTitle);
            topPanel.Controls.Add(lblSubtitle);
            topPanel.Controls.Add(actionPanel);

            // Таблиця
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

            _grid.SelectionChanged += Grid_SelectionChanged;
            _grid.CellDoubleClick += (s, e) =>
            {
                if (e.RowIndex >= 0) BtnEdit_Click(s, e);
            };

            gridCard.Controls.Add(_grid);

            container.Controls.Add(gridCard);
            container.Controls.Add(topPanel);

            Controls.Add(container);

            LoadRoutes();
        }

        private void SetupGridColumns()
        {
            _grid.AutoGenerateColumns = false;
            _grid.Columns.Clear();

            _grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Id",
                HeaderText = "ID",
                Width = 60,
                Visible = false
            });

            _grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Name",
                HeaderText = "Назва маршруту",
                FillWeight = 35
            });

            _grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Location",
                HeaderText = "Локація / Регіон",
                FillWeight = 30
            });

            _grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Difficulty",
                HeaderText = "Складність",
                FillWeight = 15
            });

            _grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "DurationDays",
                HeaderText = "Тривалість (днів)",
                FillWeight = 15
            });

            _grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "LengthKm",
                HeaderText = "Довжина (км)",
                FillWeight = 15
            });
        }

        public void LoadRoutes()
        {
            try
            {
                var routes = _routeService.GetAll();
                _grid.DataSource = null;
                _grid.DataSource = routes;

                UpdateActionButtonsState();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Не вдалося завантажити маршрути: " + ex.Message, "Помилка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private Route? GetSelectedRoute()
        {
            if (_grid.SelectedRows.Count == 0) return null;
            return _grid.SelectedRows[0].DataBoundItem as Route;
        }

        private void Grid_SelectionChanged(object? sender, EventArgs e)
        {
            UpdateActionButtonsState();
        }

        private void UpdateActionButtonsState()
        {
            bool hasSelection = _grid.SelectedRows.Count > 0;
            _btnEdit.Enabled = hasSelection;
            _btnDelete.Enabled = hasSelection;
        }

        private void BtnAdd_Click(object? sender, EventArgs e)
        {
            using (var form = new RouteEditForm())
            {
                if (form.ShowDialog(FindForm()) == DialogResult.OK)
                {
                    try
                    {
                        _routeService.Add(form.RouteData);
                        LoadRoutes();
                    }
                    catch (ValidationException valEx)
                    {
                        MessageBox.Show(valEx.Message, "Помилка валідації", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Помилка збереження: " + ex.Message, "Помилка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void BtnEdit_Click(object? sender, EventArgs e)
        {
            var selected = GetSelectedRoute();
            if (selected == null) return;

            using (var form = new RouteEditForm(selected))
            {
                if (form.ShowDialog(FindForm()) == DialogResult.OK)
                {
                    try
                    {
                        _routeService.Update(form.RouteData);
                        LoadRoutes();
                    }
                    catch (ValidationException valEx)
                    {
                        MessageBox.Show(valEx.Message, "Помилка валідації", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Помилка оновлення: " + ex.Message, "Помилка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void BtnDelete_Click(object? sender, EventArgs e)
        {
            var selected = GetSelectedRoute();
            if (selected == null) return;

            var result = MessageBox.Show(
                $"Ви впевнені, що хочете видалити маршрут «{selected.Name}»?",
                "Підтвердження видалення",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                try
                {
                    _routeService.Delete(selected.Id);
                    LoadRoutes();
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
    }
}
