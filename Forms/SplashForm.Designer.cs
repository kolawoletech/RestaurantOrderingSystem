using System.Drawing;
using System.Windows.Forms;

namespace RestaurantOrderingSystem.Forms
{
    partial class SplashForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private Label lblTitle;
        private Label lblSubtitle;
        private Label lblVersion;

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();

            this.lblTitle = new Label
            {
                Text = "Restaurant Ordering System",
                Font = new Font("Segoe UI", 22F, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Top,
                Height = 120
            };

            this.lblSubtitle = new Label
            {
                Text = "Loading…",
                Font = new Font("Segoe UI", 12F, FontStyle.Regular),
                ForeColor = Color.Gainsboro,
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Fill
            };

            this.lblVersion = new Label
            {
                Text = "v1.0.0  •  Phase 2 build",
                Font = new Font("Segoe UI", 9F),
                ForeColor = Color.LightGray,
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Bottom,
                Height = 32
            };

            this.AutoScaleMode = AutoScaleMode.Font;
            this.BackColor = Color.FromArgb(28, 33, 48);
            this.ClientSize = new Size(600, 350);
            this.Controls.Add(this.lblSubtitle);
            this.Controls.Add(this.lblTitle);
            this.Controls.Add(this.lblVersion);
            this.FormBorderStyle = FormBorderStyle.None;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Text = "Restaurant Ordering System";
        }
    }
}
