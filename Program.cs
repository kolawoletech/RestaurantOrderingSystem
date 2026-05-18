using System;
using System.Windows.Forms;
using RestaurantOrderingSystem.Forms;
using RestaurantOrderingSystem.Services;

namespace RestaurantOrderingSystem
{
    internal static class Program
    {
        // Flow:  Splash  ->  Login  ->  Dashboard  ->  (Sign-out loops back to Login)
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            var ctx = new ServiceContainer();

            using (var splash = new SplashForm())
            {
                splash.ShowDialog();
            }

            while (true)
            {
                using (var login = new LoginForm(ctx))
                {
                    if (login.ShowDialog() != DialogResult.OK) return; // user cancelled / closed
                }

                using (var dash = new DashboardForm(ctx))
                {
                    var result = dash.ShowDialog();
                    if (result != DialogResult.Retry) return; // anything other than "sign out" exits the app
                }
            }
        }
    }
}
