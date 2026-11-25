using System.IO;
using System.Windows;
using AquaGuard.Core;
using AquaGuard.Data;

namespace AquaGuard;

public partial class LoginWindow : Window
{
    private readonly AuthenticationService authentication;
    private readonly Hub hub;
    private readonly DataLogger logger;
    private readonly string? sourceError;

    public LoginWindow()
    {
        InitializeComponent();
        authentication = new AuthenticationService(
            Environment.GetEnvironmentVariable("AQUAGUARD_STAFF_CODE_SHA256"));
        logger = new DataLogger(Environment.GetEnvironmentVariable("AQUAGUARD_DATABASE_PATH")
            ?? Path.Combine(Environment.CurrentDirectory, "aquaguard.db"));
        hub = new Hub(authentication, new DataTransitBus(), [], handleFeeding: null, logger: logger);
        string? root = Environment.GetEnvironmentVariable("AQUAGUARD_SENSOR_DATA_DIRECTORY");
        try
        {
            if (Directory.Exists(root))
                HabitatFileSources.Load(hub, root, logger);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            sourceError = "Some sensor sources could not be loaded.";
        }
    }

    private void SelectStaff(object sender, RoutedEventArgs e)
    {
        StaffCodePanel.Visibility = Visibility.Visible;
        StaffCode.Focus();
    }

    private void SubmitStaffCode(object sender, RoutedEventArgs e)
    {
        bool valid = authentication.TrySignIn(StaffCode.Password);
        StaffCode.Clear();
        if (valid)
        {
            NavigateTo(new MainWindow(authentication, hub, logger, sourceError, ReturnToLogin));
            return;
        }

        LoginError.Text = authentication.IsConfigured
            ? "Invalid staff code. Try again."
            : "Staff access has not been configured.";
        StaffCode.Focus();
    }

    private void SelectVisitor(object sender, RoutedEventArgs e) =>
        NavigateTo(new VisitorWindow(hub, ReturnToLogin));

    private void NavigateTo(Window destination)
    {
        Application.Current.MainWindow = destination;
        destination.Show();
        Hide();
        destination.Closed += (_, _) =>
        {
            if (!IsVisible)
                Close();
        };
    }

    private void ReturnToLogin()
    {
        authentication.TrySignIn(null);
        LoginError.Text = "";
        StaffCodePanel.Visibility = Visibility.Collapsed;
        Application.Current.MainWindow = this;
        Show();
    }
}
