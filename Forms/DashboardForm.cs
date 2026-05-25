using System;
using System.Drawing;
using System.Windows.Forms;
using RestaurantOrderingSystem.Models;
using RestaurantOrderingSystem.Services;

namespace RestaurantOrderingSystem.Forms
{
    // Main hub after sign-in. Shows role-appropriate navigation tiles
    // and hosts a MenuStrip for keyboard-friendly actions.
    public sealed class DashboardForm : Form
    {
        private readonly ServiceContainer _ctx;

        public DashboardForm(ServiceContainer ctx)
        {
            _ctx = ctx ?? throw new ArgumentNullException(nameof(ctx));
            BuildUi();
        }

        private void BuildUi()
        {
            Text = "Dashboard — Restaurant Ordering System";
            ClientSize = new Size(900, 560);
            BackColor = Color.FromArgb(247, 248, 252);
            Font = new Font("Segoe UI", 10F);
            StartPosition = FormStartPosition.CenterScreen;
            MinimumSize = new Size(820, 520);

            // ---------- Menu strip ----------
            var menu = new MenuStrip();
            var fileMenu = new ToolStripMenuItem("&File");
            var miSignOut = new ToolStripMenuItem("Sign &out", null, (s, e) => SignOut());
            var miExit = new ToolStripMenuItem("E&xit", null, (s, e) => Application.Exit());
            fileMenu.DropDownItems.AddRange(new ToolStripItem[] { miSignOut, new ToolStripSeparator(), miExit });

            var goMenu = new ToolStripMenuItem("&Go");
            if (_ctx.CurrentUser is Admin)
                goMenu.DropDownItems.Add(new ToolStripMenuItem("&Menu management", null, (s, e) => OpenMenuManagement()));
            goMenu.DropDownItems.Add(new ToolStripMenuItem("&New order", null, (s, e) => OpenOrdering()));
            goMenu.DropDownItems.Add(new ToolStripMenuItem("&Kitchen queue", null, (s, e) => OpenKitchen()));

            var helpMenu = new ToolStripMenuItem("&Help");
            helpMenu.DropDownItems.Add(new ToolStripMenuItem("&About", null, (s, e) =>
                MessageBox.Show(this, "Restaurant Ordering System v1.0\nWinForms tutorial project.", "About",
                                MessageBoxButtons.OK, MessageBoxIcon.Information)));

            menu.Items.AddRange(new ToolStripItem[] { fileMenu, goMenu, helpMenu });
            MainMenuStrip = menu;

            // ---------- Header ----------
            var header = new Panel
            {
                Dock = DockStyle.Top,
                Height = 96,
                BackColor = Color.FromArgb(28, 33, 48)
            };
            var lblHello = new Label
            {
                Text = "Welcome, " + _ctx.CurrentUser.FullName,
                ForeColor = Color.White,
                Font = new Font("Segoe UI Semibold", 18F),
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleLeft,
                Bounds = new Rectangle(24, 18, 700, 36)
            };
            var lblRole = new Label
            {
                Text = "Signed in as " + _ctx.CurrentUser.RoleName,
                ForeColor = Color.LightGray,
                AutoSize = false,
                Bounds = new Rectangle(24, 56, 700, 22)
            };
            header.Controls.Add(lblHello);
            header.Controls.Add(lblRole);

            // ---------- Content tiles ----------
            var content = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(24),
                BackColor = Color.FromArgb(247, 248, 252),
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = true
            };

            if (_ctx.CurrentUser is Admin)
                content.Controls.Add(BuildTile("Menu management",
                    "Add, update or remove meals on the menu.",
                    Color.FromArgb(46, 110, 234),
                    OpenMenuManagement));

            content.Controls.Add(BuildTile("New order",
                "Build a customer order and print the receipt.",
                Color.FromArgb(20, 161, 110),
                OpenOrdering));

            content.Controls.Add(BuildTile("Kitchen queue",
                "Track order status: Pending → Preparing → Ready → Completed.",
                Color.FromArgb(240, 150, 30),
                OpenKitchen));

            content.Controls.Add(BuildTile("Sign out",
                "End the current session and return to login.",
                Color.FromArgb(180, 60, 60),
                SignOut));

            Controls.Add(content);
            Controls.Add(header);
            Controls.Add(menu);
        }

        private Panel BuildTile(string title, string description, Color accent, Action onClick)
        {
            var tile = new Panel
            {
                Width = 260,
                Height = 150,
                Margin = new Padding(0, 0, 16, 16),
                BackColor = Color.White,
                Cursor = Cursors.Hand
            };
            var bar = new Panel { Dock = DockStyle.Left, Width = 6, BackColor = accent };
            var lblTitle = new Label
            {
                Text = title,
                Font = new Font("Segoe UI Semibold", 13F),
                ForeColor = Color.FromArgb(28, 33, 48),
                Bounds = new Rectangle(20, 20, 220, 26),
                AutoSize = false
            };
            var lblDesc = new Label
            {
                Text = description,
                ForeColor = Color.Gray,
                Bounds = new Rectangle(20, 54, 220, 70),
                AutoSize = false
            };

            void invoke(object s, EventArgs e) { onClick(); }
            tile.Click += invoke;
            bar.Click += invoke;
            lblTitle.Click += invoke;
            lblDesc.Click += invoke;

            tile.Controls.Add(lblDesc);
            tile.Controls.Add(lblTitle);
            tile.Controls.Add(bar);
            return tile;
        }

        private void OpenMenuManagement()
        {
            using (var f = new MenuManagementForm(_ctx))
            {
                f.ShowDialog(this);
            }
        }

        private void OpenOrdering()
        {
            using (var f = new OrderingForm(_ctx))
            {
                f.ShowDialog(this);
            }
        }

        private void OpenKitchen()
        {
            using (var f = new KitchenForm(_ctx))
            {
                f.ShowDialog(this);
            }
        }

        private void SignOut()
        {
            _ctx.CurrentUser = null;
            DialogResult = DialogResult.Retry; // signal Program.cs to loop back to login
            Close();
        }
    }
}
