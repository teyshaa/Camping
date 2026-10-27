using System.Drawing;
using System.Windows.Forms;

namespace HikeManager.Views
{
    public class HikesView : UserControl
    {
        public bool ShowHistory { get; }

        public HikesView(bool showHistory = false)
        {
            ShowHistory = showHistory;
            Dock = DockStyle.Fill;
            BackColor = Theme.BackgroundColor;

            Panel card = new Panel
            {
                Dock = DockStyle.Fill,
                Margin = new Padding(30),
                BackColor = Theme.BackgroundColor
            };

            Label title = new Label
            {
                Text = showHistory ? "Історія проведених походів" : "Активні та заплановані походи",
                Font = Theme.TitleFont,
                ForeColor = Theme.PrimaryColor,
                Location = new Point(30, 30),
                AutoSize = true
            };

            Label subtitle = new Label
            {
                Text = showHistory
                    ? "Розділ історії розробляється Учасником 2 відповідно до контракту IHikeService."
                    : "Розділ планування походів розробляється Учасником 2 відповідно до контракту IHikeService.",
                Font = Theme.RegularFont,
                ForeColor = Theme.TextMuted,
                Location = new Point(32, 75),
                AutoSize = true
            };

            card.Controls.Add(title);
            card.Controls.Add(subtitle);
            Controls.Add(card);
        }
    }
}
