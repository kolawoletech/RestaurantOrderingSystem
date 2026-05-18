using System;
using System.Drawing;
using System.Windows.Forms;
using RestaurantOrderingSystem.Models;
using RestaurantOrderingSystem.Services;

namespace RestaurantOrderingSystem.Forms
{
    // Code-only form: builds itself entirely in this single file.
    // Easier to study than a Designer-generated form.
    public sealed class LoginForm : Form
    {
        private readonly ServiceContainer _ctx;

        public Employee AuthenticatedUser { get; private set; }

        private TextBox txtUsername;
        private TextBox txtPassword;
        private Button  btnLogin;
        private Button  btnExit;
        private Label   lblStatus;
        private Label   lblHint;

        public LoginForm(ServiceContainer ctx)
        {
            _ctx = ctx ?? throw new ArgumentNullException(nameof(ctx));
            BuildUi();
        }

        private void BuildUi()
        {
            Text = "Sign in — Restaurant Ordering System";
            ClientSize = new Size(420, 380);
            BackColor = Color.FromArgb(247, 248, 252);
            Font = new Font("Segoe UI", 10F);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            StartPosition = FormStartPosition.CenterScreen;

            var lblTitle = new Label
            {
                Text = "Welcome back",
                Font = new Font("Segoe UI Semibold", 18F),
                ForeColor = Color.FromArgb(28, 33, 48),
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleLeft,
                Bounds = new Rectangle(40, 30, 340, 36)
            };

            var lblSubtitle = new Label
            {
                Text = "Sign in to continue.",
                ForeColor = Color.Gray,
                AutoSize = false,
                Bounds = new Rectangle(40, 70, 340, 20)
            };

            var lblUser = new Label { Text = "Username", Bounds = new Rectangle(40, 110, 200, 18), ForeColor = Color.DimGray };
            txtUsername = new TextBox { Bounds = new Rectangle(40, 130, 340, 28), Font = new Font("Segoe UI", 11F) };

            var lblPwd = new Label { Text = "Password", Bounds = new Rectangle(40, 170, 200, 18), ForeColor = Color.DimGray };
            txtPassword = new TextBox { Bounds = new Rectangle(40, 190, 340, 28), Font = new Font("Segoe UI", 11F), UseSystemPasswordChar = true };

            btnLogin = new Button
            {
                Text = "Sign in",
                Bounds = new Rectangle(40, 235, 340, 40),
                BackColor = Color.FromArgb(46, 110, 234),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI Semibold", 11F),
                Cursor = Cursors.Hand
            };
            btnLogin.FlatAppearance.BorderSize = 0;
            btnLogin.Click += BtnLogin_Click;

            btnExit = new Button
            {
                Text = "Exit",
                Bounds = new Rectangle(40, 285, 340, 32),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.Transparent,
                ForeColor = Color.DimGray
            };
            btnExit.FlatAppearance.BorderColor = Color.LightGray;
            btnExit.Click += (s, e) => Application.Exit();

            lblStatus = new Label
            {
                Bounds = new Rectangle(40, 322, 340, 20),
                ForeColor = Color.Firebrick,
                TextAlign = ContentAlignment.MiddleCenter
            };

            lblHint = new Label
            {
                Text = "Demo: admin/admin123  •  cashier/cashier123",
                Bounds = new Rectangle(40, 345, 340, 20),
                ForeColor = Color.Silver,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI", 8F)
            };

            Controls.AddRange(new Control[] { lblTitle, lblSubtitle, lblUser, txtUsername, lblPwd, txtPassword, btnLogin, btnExit, lblStatus, lblHint });
            AcceptButton = btnLogin;
        }

        private void BtnLogin_Click(object sender, EventArgs e)
        {
            lblStatus.Text = string.Empty;
            try
            {
                var user = _ctx.Auth.Authenticate(txtUsername.Text, txtPassword.Text);
                if (user == null)
                {
                    lblStatus.Text = "Invalid username or password.";
                    return;
                }
                _ctx.CurrentUser = user;
                AuthenticatedUser = user;
                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                lblStatus.Text = ex.Message;
            }
        }
    }
}
