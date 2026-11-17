using System.Windows;

namespace AquaGuard
{
    public partial class VisitorWindow : Window
    {
        public VisitorWindow()
        {
            InitializeComponent();
        }

        private void BackToLogin(object sender, RoutedEventArgs e)
        {
            var login = new LoginWindow();
            Application.Current.MainWindow = login;
            login.Show();
            Close();
        }
    }
}
