using System.Drawing;
using System.Windows.Forms;

namespace RestaurantOrderingSystem.Forms
{
    // Read-only modal that displays a formatted receipt in a monospaced font.
    public sealed class ReceiptPreviewForm : Form
    {
        public ReceiptPreviewForm(string receiptText)
        {
            Text = "Receipt";
            ClientSize = new Size(560, 600);
            StartPosition = FormStartPosition.CenterParent;
            BackColor = Color.White;

            var txt = new TextBox
            {
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Vertical,
                Font = new Font("Consolas", 10F),
                Dock = DockStyle.Fill,
                Text = receiptText
            };

            var btnClose = new Button
            {
                Text = "Close",
                Dock = DockStyle.Bottom,
                Height = 40,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(46, 110, 234),
                ForeColor = Color.White
            };
            btnClose.FlatAppearance.BorderSize = 0;
            btnClose.Click += (s, e) => Close();

            Controls.Add(txt);
            Controls.Add(btnClose);
        }
    }
}
