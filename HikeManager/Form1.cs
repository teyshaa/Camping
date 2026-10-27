using System;
using System.Drawing;
using System.Windows.Forms;
using HikeManager.Views;

namespace HikeManager
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            SetupUI();
        }

        private void SetupUI()
        {
            // 1. Налаштування вікна
            this.Size = new Size(1000, 650);
            this.Text = "HikeManager — Головне меню";
            this.StartPosition = FormStartPosition.CenterScreen;

            // 2. Бокове меню зліва
            Panel sideMenu = new Panel();
            sideMenu.Dock = DockStyle.Left;
            sideMenu.Width = 200;
            sideMenu.BackColor = Color.FromArgb(40, 75, 60);

            Button btnRoutes = CreateMenuButton("Маршрути");
            Button btnHikes = CreateMenuButton("Походи");
            Button btnHistory = CreateMenuButton("Історія");

            sideMenu.Controls.Add(btnHistory);
            sideMenu.Controls.Add(btnHikes);
            sideMenu.Controls.Add(btnRoutes);

            // 3. Світла робоча зона праворуч
            Panel mainArea = new Panel();
            mainArea.Dock = DockStyle.Fill;
            mainArea.BackColor = Color.FromArgb(235, 245, 235);

           

            this.Controls.Add(mainArea);
            this.Controls.Add(sideMenu);
        }

        private Button CreateMenuButton(string text)
        {
            Button btn = new Button();
            btn.Text = text;
            btn.Dock = DockStyle.Top;
            btn.Height = 55;
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 0;
            btn.ForeColor = Color.White;
            btn.Font = new Font("Segoe UI", 11, FontStyle.Bold);
            btn.Cursor = Cursors.Hand;
            return btn;
        }
    }
}