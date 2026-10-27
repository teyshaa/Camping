using System.Drawing;
using System.Windows.Forms;
using HikeManager.Services;

namespace HikeManager.Views
{
    public class ExpensesView : UserControl
    {
        private int _currentHikeId;

        public ExpensesView()
        {
            Dock = DockStyle.Fill;
            BackColor = Theme.BackgroundColor;

            Label lbl = new Label
            {
                Text = "Витрати та бюджет походу (розробляється Учасником 4 відповідно до IExpenseService)",
                Font = Theme.SubtitleFont,
                ForeColor = Theme.TextMuted,
                Location = new Point(30, 30),
                AutoSize = true
            };
            Controls.Add(lbl);
        }

        public void LoadHike(int hikeId)
        {
            _currentHikeId = hikeId;
            // Буде реалізовано Учасником 4
        }
    }
}
