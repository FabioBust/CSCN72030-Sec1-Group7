using System.Windows;
using AquaGuard.Core;

namespace AquaGuard
{
    public partial class LoginWindow : Window
    {
        private readonly AuthenticationService authentication = new(
            Environment.GetEnvironmentVariable("AQUAGUARD_STAFF_CODE_SHA256"));

        public LoginWindow()
        {
            InitializeComponent();
        }

        private void SelectStaff(object sender, RoutedEventArgs e)
        {
            StaffCodePanel.Visibility = Visibility.Visible;
            StaffCode.Focus();
        }

        private void SubmitStaffCode(object sender, RoutedEventArgs e)
        {
            bool valid = authentication.ValidateStaffCode(StaffCode.Password);
            StaffCode.Clear();
            if (valid)
            {
                NavigateTo(new MainWindow());
                return;
            }

            LoginError.Text = authentication.IsConfigured
                ? "Invalid staff code. Try again."
                : "Staff access has not been configured.";
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
