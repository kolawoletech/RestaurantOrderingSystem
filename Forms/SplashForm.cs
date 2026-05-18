using System;
using System.Drawing;
using System.Windows.Forms;

namespace RestaurantOrderingSystem.Forms
{
    // First window shown on launch. Displays branding for a few seconds, then
    // signals completion so Program.cs can move on to the LoginForm.
    public partial class SplashForm : Form
    {
        private readonly Timer _timer;

        public SplashForm()
        {
            InitializeComponent();
            _timer = new Timer { Interval = 2000 };
            _timer.Tick += (s, e) =>
            {
                _timer.Stop();
                DialogResult = DialogResult.OK;
                Close();
            };
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            _timer.Start();
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            _timer?.Stop();
            _timer?.Dispose();
            base.OnFormClosed(e);
        }
    }
}
