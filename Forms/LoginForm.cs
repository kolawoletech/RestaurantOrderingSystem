using System;
using System.Drawing;
using System.Windows.Forms;
using RestaurantOrderingSystem.Exceptions;   // LESSON: Lesson 12 — using directive imports the Exceptions namespace
using RestaurantOrderingSystem.Models;
using RestaurantOrderingSystem.Services;

namespace RestaurantOrderingSystem.Forms
{
    // =======================================================================
    //  LoginForm.cs  (the "code-behind" half)
    //  ---------------------------------------------------------------------
    //  LESSON NOTE (Week 4, Lesson 12 — Namespaces & partial classes):
    //
    //  This file used to build every control by hand inside BuildUi().
    //  We converted it to a Designer-driven form so we can use the Visual
    //  Studio Toolbox: drag a Label/TextBox/Button onto the form and VS
    //  writes the boilerplate inside LoginForm.Designer.cs for us.
    //
    //  Both files declare `partial class LoginForm` — the C# compiler glues
    //  them into ONE class.  This file keeps only:
    //     • the constructor
    //     • event handlers (BtnLogin_Click, BtnExit_Click)
    //     • any properties the rest of the app uses (AuthenticatedUser)
    //
    //  All control creation lives in the auto-generated Designer file.
    // =======================================================================
    public sealed partial class LoginForm : Form
    {
        private readonly ServiceContainer _ctx;

        public Employee AuthenticatedUser { get; private set; }

        public LoginForm(ServiceContainer ctx)
        {
            _ctx = ctx ?? throw new ArgumentNullException(nameof(ctx));
            InitializeComponent();   // ← lives in LoginForm.Designer.cs
        }

        private void BtnLogin_Click(object sender, EventArgs e)
        {
            lblStatus.Text = string.Empty;
            // LESSON: Lesson 11 (§2.4.1) — order catch clauses from MOST
            // SPECIFIC to LEAST specific. InvalidLoginException is a subclass
            // of Exception, so it must come first; otherwise the general
            // catch (Exception) would swallow it.
            try
            {
                var user = _ctx.Auth.Authenticate(txtUsername.Text, txtPassword.Text);
                _ctx.CurrentUser = user;
                AuthenticatedUser = user;
                DialogResult = DialogResult.OK;
                Close();
            }
            catch (InvalidLoginException ex)
            {
                // We know exactly what went wrong — bad credentials.
                lblStatus.Text = ex.Message;
                txtPassword.Clear();
                txtPassword.Focus();
            }
            catch (Exception ex)
            {
                // Anything else (e.g. a future repository error) lands here.
                lblStatus.Text = "Unexpected error: " + ex.Message;
            }
        }

        private void BtnExit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
    }
}
