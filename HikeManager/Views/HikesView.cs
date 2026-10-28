
using System;
using System.Drawing;
using System.Windows.Forms;
using HikeManager.Services;
using HikeManager.Models;

namespace HikeManager.Views
{
    public class HikesView : UserControl
    {
        public bool ShowHistory { get; }

        private DataGridView _grid;
        private HikeService _hikeService;

        public HikesView(bool showHistory = false)
        {
            ShowHistory = showHistory;
            _hikeService = new HikeService();

            Dock = DockStyle.Fill;
            BackColor = Color.FromArgb(235, 245, 235);

            // Верхня панель
            Panel topPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 60
            };

            Label title = new Label
            {
                Text = showHistory
                    ? "Історія походів"
                    : "Активні та заплановані походи",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = Color.FromArgb(40, 75, 60),
                Location = new Point(20, 15),
                AutoSize = true,
                UseCompatibleTextRendering = true
            };

            Button btnCreate =
                CreateButton("🌲 Створити", 400);

            Button btnEdit =
                CreateSecondaryButton("Редагувати", 520);

            Button btnDetails =
                CreateSecondaryButton("Деталі", 640);

            // Відкриття деталей вибраного походу
            btnDetails.Click += (s, e) =>
            {
                if (_grid.CurrentRow?.DataBoundItem
                    is not Hike selectedHike)
                {
                    MessageBox.Show(
                        "Спочатку виберіть похід.");
                    return;
                }

                using var detailsForm =
                    new HikeDetailsForm(selectedHike);

                detailsForm.ShowDialog(this);
            };

            topPanel.Controls.Add(title);
            topPanel.Controls.Add(btnCreate);
            topPanel.Controls.Add(btnEdit);
            topPanel.Controls.Add(btnDetails);

            // Таблиця походів
            _grid = new DataGridView
            {
                Dock = DockStyle.Fill,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                EnableHeadersVisualStyles = false,
                RowHeadersVisible = false,
                ReadOnly = true,
                SelectionMode =
                    DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode =
                    DataGridViewAutoSizeColumnsMode.Fill,
                Font = new Font("Segoe UI", 10)
            };

            _grid.ColumnHeadersDefaultCellStyle.BackColor =
                Color.FromArgb(40, 75, 60);

            _grid.ColumnHeadersDefaultCellStyle.ForeColor =
                Color.White;

            _grid.ColumnHeadersHeight = 35;

            Controls.Add(_grid);
            Controls.Add(topPanel);

            LoadData();
        }

        private Button CreateButton(string text, int x)
        {
            return new Button
            {
                Text = text,
                Location = new Point(x, 15),
                Size = new Size(110, 30),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(40, 75, 60),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10),
                FlatAppearance = { BorderSize = 0 }
            };
        }

        private Button CreateSecondaryButton(
            string text, int x)
        {
            return new Button
            {
                Text = text,
                Location = new Point(x, 15),
                Size = new Size(110, 30),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.White,
                ForeColor = Color.Black,
                Font = new Font("Segoe UI", 10),
                FlatAppearance =
                {
                    BorderColor = Color.Gray
                }
            };
        }

        private void LoadData()
        {
            var data = ShowHistory
                ? _hikeService.GetHistory()
                : _hikeService.GetActive();

            _grid.DataSource = data;

            if (_grid.Columns["Id"] != null)
                _grid.Columns["Id"].Visible = false;

            if (_grid.Columns["RouteId"] != null)
                _grid.Columns["RouteId"].Visible = false;

            if (_grid.Columns["EndDate"] != null)
                _grid.Columns["EndDate"].Visible = false;

            if (_grid.Columns["Name"] != null)
                _grid.Columns["Name"].HeaderText =
                    "Назва походу";

            if (_grid.Columns["StartDate"] != null)
                _grid.Columns["StartDate"].HeaderText =
                    "Дата початку";

            if (_grid.Columns["RouteName"] != null)
                _grid.Columns["RouteName"].HeaderText =
                    "Маршрут";

            if (_grid.Columns["DurationDays"] != null)
                _grid.Columns["DurationDays"].HeaderText =
                    "Тривалість (днів)";
        }
    }
}
