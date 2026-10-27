using System.Drawing;
using System.Windows.Forms;

namespace HikeManager
{
    public static class Theme
    {
        // Фірмові кольори проєкту
        public static readonly Color PrimaryColor = Color.FromArgb(40, 75, 60);         // Темно-зелений
        public static readonly Color BackgroundColor = Color.FromArgb(235, 245, 235);   // Світло-зелений фон
        public static readonly Color SidebarHover = Color.FromArgb(52, 96, 78);         // Підсвічування кнопки меню
        public static readonly Color SidebarActive = Color.FromArgb(30, 58, 46);        // Активна кнопка меню
        public static readonly Color CardBackground = Color.White;                      // Фон карток та таблиць
        public static readonly Color TextPrimary = Color.FromArgb(35, 45, 40);          // Основний текст
        public static readonly Color TextMuted = Color.FromArgb(100, 115, 105);         // Другорядний текст
        public static readonly Color BorderColor = Color.FromArgb(205, 220, 210);       // Лінії та рамки
        public static readonly Color DangerColor = Color.FromArgb(185, 45, 45);         // Кнопка видалення / помилка
        public static readonly Color DangerHover = Color.FromArgb(160, 35, 35);

        // Типографіка (Segoe UI)
        public static readonly Font TitleFont = new Font("Segoe UI", 20, FontStyle.Bold);
        public static readonly Font SubtitleFont = new Font("Segoe UI", 14, FontStyle.Bold);
        public static readonly Font HeaderFont = new Font("Segoe UI", 11, FontStyle.Bold);
        public static readonly Font RegularFont = new Font("Segoe UI", 10, FontStyle.Regular);
        public static readonly Font BoldFont = new Font("Segoe UI", 10, FontStyle.Bold);
        public static readonly Font SmallFont = new Font("Segoe UI", 9, FontStyle.Regular);

        public static void StylePrimaryButton(Button btn)
        {
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 0;
            btn.BackColor = PrimaryColor;
            btn.ForeColor = Color.White;
            btn.Font = HeaderFont;
            btn.Cursor = Cursors.Hand;
            btn.Height = 38;
        }

        public static void StyleDangerButton(Button btn)
        {
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 0;
            btn.BackColor = DangerColor;
            btn.ForeColor = Color.White;
            btn.Font = HeaderFont;
            btn.Cursor = Cursors.Hand;
            btn.Height = 38;
        }

        public static void StyleSecondaryButton(Button btn)
        {
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderColor = BorderColor;
            btn.FlatAppearance.BorderSize = 1;
            btn.BackColor = Color.White;
            btn.ForeColor = TextPrimary;
            btn.Font = BoldFont;
            btn.Cursor = Cursors.Hand;
            btn.Height = 38;
        }

        public static void StyleDataGridView(DataGridView grid)
        {
            grid.BackgroundColor = CardBackground;
            grid.BorderStyle = BorderStyle.None;
            grid.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            grid.GridColor = Color.FromArgb(230, 235, 230);
            grid.EnableHeadersVisualStyles = false;
            grid.RowHeadersVisible = false;
            grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            grid.MultiSelect = false;
            grid.ReadOnly = true;
            grid.AllowUserToAddRows = false;
            grid.AllowUserToDeleteRows = false;
            grid.AllowUserToResizeRows = false;
            grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            grid.RowTemplate.Height = 36;
            grid.Font = RegularFont;

            // Заголовок
            grid.ColumnHeadersDefaultCellStyle.BackColor = PrimaryColor;
            grid.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            grid.ColumnHeadersDefaultCellStyle.Font = HeaderFont;
            grid.ColumnHeadersDefaultCellStyle.Padding = new Padding(6);
            grid.ColumnHeadersHeight = 40;

            // Рядки
            grid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(215, 235, 225);
            grid.DefaultCellStyle.SelectionForeColor = TextPrimary;
            grid.DefaultCellStyle.Padding = new Padding(4);
            grid.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 252, 248);
        }
    }
}
