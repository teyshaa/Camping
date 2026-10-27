using System.Drawing;
using System.Windows.Forms;
using HikeManager.Services;

namespace HikeManager.Views
{
    public class EquipmentView : UserControl
    {
        private int _currentHikeId;

        public EquipmentView()
        {
            Dock = DockStyle.Fill;
            BackColor = Theme.BackgroundColor;

            Label lbl = new Label
            {
                Text = "Спорядження для походу (розробляється Учасником 3 відповідно до IEquipmentService)",
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
            // Буде реалізовано Учасником 3
        }
    }
}
