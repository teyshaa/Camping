using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using HikeManager.Views;

namespace HikeManager
{
    public partial class Form1 : Form
    {
        private Panel _mainArea = null!;
        private readonly List<Button> _menuButtons = new();
        private Button? _activeButton;

        public Form1()
        {
            InitializeComponent();
            SetupUI();
        }

        private void SetupUI()
        {
            // 1. Налаштування головної форми
            Text = "HikeManager — Керування гірськими походами";
            Size = new Size(1150, 720);
            MinimumSize = new Size(950, 600);
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = Theme.BackgroundColor;

            // 2. Ліве навігаційне меню
            Panel sideMenu = new Panel
            {
                Dock = DockStyle.Left,
                Width = 230,
                BackColor = Theme.PrimaryColor
            };

            // Логотип та бренд застосунку
            Panel logoPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 85,
                Padding = new Padding(20, 18, 10, 10)
            };

            Label logoTitle = new Label
            {
                Text = "HikeManager",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = true,
                Location = new Point(16, 16)
            };

            Label logoSubtitle = new Label
            {
                Text = "Планування та логістика",
                Font = new Font("Segoe UI", 8.5f, FontStyle.Regular),
                ForeColor = Color.FromArgb(180, 210, 195),
                AutoSize = true,
                Location = new Point(18, 46)
            };

            logoPanel.Controls.Add(logoTitle);
            logoPanel.Controls.Add(logoSubtitle);

            // Кнопки навігації
            Button btnRoutes = CreateMenuButton("Маршрути", () => ShowView(new RoutesView()));
            Button btnHikes = CreateMenuButton("Походи", () => ShowView(new HikesView(showHistory: false)));
            Button btnHistory = CreateMenuButton("Історія", () => ShowView(new HikesView(showHistory: true)));

            // Порядок додавання для DockStyle.Top
            sideMenu.Controls.Add(btnHistory);
            sideMenu.Controls.Add(btnHikes);
            sideMenu.Controls.Add(btnRoutes);
            sideMenu.Controls.Add(logoPanel);

            // 3. Основна робоча панель (контейнер для UserControl)
            _mainArea = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Theme.BackgroundColor
            };

            Controls.Add(_mainArea);
            Controls.Add(sideMenu);

            // 4. Початковий екран — Маршрути
            btnRoutes.PerformClick();
        }

        private Button CreateMenuButton(string text, Action onClickAction)
        {
            Button btn = new Button
            {
                Text = text,
                Dock = DockStyle.Top,
                Height = 52,
                FlatStyle = FlatStyle.Flat,
                FlatAppearance =
                {
                    BorderSize = 0,
                    MouseOverBackColor = Theme.SidebarHover,
                    MouseDownBackColor = Theme.SidebarActive
                },
                ForeColor = Color.White,
                Font = Theme.HeaderFont,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(20, 0, 0, 0),
                Cursor = Cursors.Hand,
                BackColor = Theme.PrimaryColor
            };

            btn.Click += (s, e) =>
            {
                SetActiveButton(btn);
                onClickAction();
            };

            _menuButtons.Add(btn);
            return btn;
        }

        private void SetActiveButton(Button button)
        {
            _activeButton = button;
            foreach (var btn in _menuButtons)
            {
                if (btn == button)
                {
                    btn.BackColor = Theme.SidebarActive;
                    btn.Font = new Font(Theme.HeaderFont.FontFamily, Theme.HeaderFont.Size, FontStyle.Bold);
                }
                else
                {
                    btn.BackColor = Theme.PrimaryColor;
                    btn.Font = Theme.HeaderFont;
                }
            }
        }

        public void ShowView(UserControl view)
        {
            _mainArea.SuspendLayout();

            while (_mainArea.Controls.Count > 0)
            {
                var ctrl = _mainArea.Controls[0];
                _mainArea.Controls.RemoveAt(0);
                ctrl.Dispose();
            }

            view.Dock = DockStyle.Fill;
            _mainArea.Controls.Add(view);
            _mainArea.ResumeLayout(true);
        }
    }
}