using System.Drawing;
using System.Windows.Forms;
using HikeManager.Services;

namespace HikeManager.Views
{
    public class ParticipantsView : UserControl
    {
        private int _currentHikeId;

        public ParticipantsView()
        {
            Dock = DockStyle.Fill;
            BackColor = Theme.BackgroundColor;

            Label lbl = new Label
            {
                Text = "Учасники походу (розробляється Учасником 3 відповідно до IParticipantService)",
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
