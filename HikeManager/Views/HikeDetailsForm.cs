using System;
using System.Drawing;
using System.Windows.Forms;
using HikeManager.Models;
using HikeManager.Services;

namespace HikeManager.Views
{
    public class HikeDetailsForm : Form
    {
        private readonly Hike          _hike;
        private readonly IRouteService _routeService;
        public HikeDetailsForm(Hike hike, IRouteService? routeService = null)
        {
            _hike         = hike ?? throw new ArgumentNullException(nameof(hike));
            _routeService = routeService ?? new RouteService();

            Text            = $"Деталі: {_hike.Name}";
            Size            = new Size(760, 600);
            MinimumSize     = new Size(680, 520);
            StartPosition   = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.Sizable;
            MaximizeBox     = true;
            MinimizeBox     = false;
            BackColor       = Theme.BackgroundColor;
            Font            = Theme.RegularFont;

            Panel outer = new Panel
            {
                Dock      = DockStyle.Fill,
                Padding   = new Padding(24, 20, 24, 20),
                BackColor = Theme.BackgroundColor
            };

            Panel headerCard = BuildHeaderCard();
            TabControl tabs = BuildTabControl();

            Button btnClose = new Button { Text = "Закрити", Width = 110, Anchor = AnchorStyles.Bottom | AnchorStyles.Right };
            Theme.StyleSecondaryButton(btnClose);
            btnClose.Click += (s, e) => Close();

            Panel bottomPanel = new Panel
            {
                Dock      = DockStyle.Bottom,
                Height    = 52,
                BackColor = Theme.BackgroundColor
            };
            btnClose.Location = new Point(bottomPanel.Width - 120, 8);
            btnClose.Anchor   = AnchorStyles.Right | AnchorStyles.Top;
            bottomPanel.Controls.Add(btnClose);

            outer.Controls.Add(tabs);          
            outer.Controls.Add(headerCard);    
            outer.Controls.Add(bottomPanel);   

            Controls.Add(outer);
        }

        private Panel BuildHeaderCard()
        {
            Route? route = null;
            try { route = _routeService.GetById(_hike.RouteId); }
            catch {  }

            string difficulty = route?.Difficulty ?? "—";
            string location   = route?.Location   ?? "—";
            string lengthKm   = route?.LengthKm != null
                ? $"{route.LengthKm:F1} км"
                : "—";

            Panel card = new Panel
            {
                Dock      = DockStyle.Top,
                Height    = 150,
                BackColor = Theme.CardBackground,
                Padding   = new Padding(20, 16, 20, 12),
                Margin    = new Padding(0, 0, 0, 16)
            };

            // Назва походу — великий заголовок
            Label lblName = new Label
            {
                Text      = _hike.Name,
                Font      = Theme.SubtitleFont,
                ForeColor = Theme.PrimaryColor,
                AutoSize  = true,
                Location  = new Point(20, 14)
            };
            card.Controls.Add(lblName);

            int col1X = 20;
            int col2X = 370;
            int startY = 65;

            AddInfoRow(card, "📅  Дата:",
                $"{_hike.StartDate:dd.MM.yyyy}  —  {_hike.EndDate:dd.MM.yyyy}",
                col1X, startY);

            AddInfoRow(card, "🗺  Маршрут:",
                _hike.RouteName ?? "—",
                col1X, startY + 30);

            AddInfoRow(card, "⛰  Складність:",
                difficulty,
                col2X, startY);

            AddInfoRow(card, "🕐  Тривалість:",
                $"{_hike.DurationDays} дн.",
                col2X, startY + 30);

            Panel divider = new Panel
            {
                BackColor = Theme.BorderColor,
                Location  = new Point(0, card.Height - 1),
                Size      = new Size(card.Width, 1),
                Anchor    = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
            };
            card.Controls.Add(divider);

            return card;
        }

        private TabControl BuildTabControl()
        {
            var tabs = new TabControl
            {
                Dock      = DockStyle.Fill,
                Font      = Theme.HeaderFont,
                Padding   = new Point(16, 8)
            };

            tabs.Controls.Add(BuildParticipantsTab());
            tabs.Controls.Add(BuildEquipmentTab());
            tabs.Controls.Add(BuildFinancesTab());

            return tabs;
        }

        // ── Вкладка «Учасники» 
        private TabPage BuildParticipantsTab()
        {
            var page = new TabPage("👥  Учасники") { BackColor = Theme.BackgroundColor };
            var view = new ParticipantsView();
            view.LoadHike(_hike.Id);
            page.Controls.Add(view);
            return page;
        }

        // ── Вкладка «Спорядження» 
        private TabPage BuildEquipmentTab()
        {
            var page = new TabPage("🎒  Спорядження") { BackColor = Theme.BackgroundColor };
            var view = new EquipmentView();
            view.LoadHike(_hike.Id);
            page.Controls.Add(view);
            return page;
        }

        // ── Вкладка «Фінанси» 
        private TabPage BuildFinancesTab()
        {
            var page = new TabPage("💰  Фінанси") { BackColor = Theme.BackgroundColor };
            var expensesView = new ExpensesView();
            expensesView.SetHikeProvider(id => id == _hike.Id ? _hike : null);
            expensesView.LoadHike(_hike.Id);
            page.Controls.Add(expensesView);
            return page;
        }

        private static Panel BuildPlaceholder(string title, string description)
        {
            Panel wrapper = new Panel { Dock = DockStyle.Fill, BackColor = Theme.CardBackground };
            Label lblTitle = new Label
            {
                Text      = title,
                Font      = Theme.SubtitleFont,
                ForeColor = Theme.TextMuted,
                AutoSize  = true,
                TextAlign = ContentAlignment.MiddleCenter
            };

            // Опис
            Label lblDesc = new Label
            {
                Text      = description,
                Font      = Theme.RegularFont,
                ForeColor = Theme.TextMuted,
                AutoSize  = false,
                Width     = 400,
                Height    = 60,
                TextAlign = ContentAlignment.MiddleCenter
            };

            FlowLayoutPanel flow = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.TopDown,
                WrapContents  = false,
                AutoSize      = true,
                AutoSizeMode  = AutoSizeMode.GrowAndShrink
            };

            flow.Controls.Add(lblTitle);
            flow.Controls.Add(lblDesc);

            wrapper.Resize += (s, e) =>
            {
                flow.Location = new Point(
                    Math.Max(0, (wrapper.Width  - flow.Width)  / 2),
                    Math.Max(0, (wrapper.Height - flow.Height) / 2));
            };

            wrapper.Controls.Add(flow);
            return wrapper;
        }

        private static void AddInfoRow(Panel parent, string label, string value, int x, int y)
        {
            parent.Controls.Add(new Label
            {
                Text      = label,
                Font      = Theme.BoldFont,
                ForeColor = Theme.TextMuted,
                AutoSize  = true,
                Location  = new Point(x, y)
            });

            parent.Controls.Add(new Label
            {
                Text      = value,
                Font      = Theme.RegularFont,
                ForeColor = Theme.TextPrimary,
                AutoSize  = true,
                Location  = new Point(x + 120, y)
            });
        }
    }
}
