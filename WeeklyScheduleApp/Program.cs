using WeeklyScheduleApp.Data;
using WeeklyScheduleApp.Views;

namespace WeeklyScheduleApp
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();

            // Veritabanını ve seed datayı oluştur
            using (var db = new AppDbContext())
            {
                db.Database.EnsureCreated();
            }

            // Giriş ekranını aç
            using (var loginForm = new LoginForm())
            {
                if (loginForm.ShowDialog() == DialogResult.OK && loginForm.LoggedInUser != null)
                {
                    var currentUser = loginForm.LoggedInUser;

                    // Ana formu açıyoruz (MainForm adında formunuzun olduğunu varsayıyoruz)
                    Application.Run(new MainForm(currentUser));
                }
            }
        }
    }
}