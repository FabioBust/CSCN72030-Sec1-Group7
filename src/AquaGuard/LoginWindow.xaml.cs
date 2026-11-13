using System.Windows;

namespace AquaGuard
{
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
        }

        private void SelectStaff(object sender, RoutedEventArgs e)
        {
            StaffCodePanel.Visibility = Visibility.Visible;
            StaffCode.Focus();
        }

        private void SelectVisitor(object sender, RoutedEventArgs e)
        {
            NavigateTo(new VisitorWindow());
        }

        private void NavigateTo(Window destination)
        {
            Application.Current.MainWindow = destination;
            destination.Show();
            Close();
        }
    }
}
