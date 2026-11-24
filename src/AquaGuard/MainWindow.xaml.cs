using System.Globalization;
using System.Windows;
using System.Windows.Threading;
using AquaGuard.Core;
using AquaGuard.Data;
using AquaGuard.Devices.Equipment;
using Microsoft.Data.Sqlite;

namespace AquaGuard;

public partial class MainWindow : Window
{
    private readonly DispatcherTimer refresh = new() { Interval = TimeSpan.FromSeconds(1) };
    private readonly Hub hub;
    private readonly DataLogger logger;
    private readonly Action returnToLogin;
    private IReadOnlyList<SensorDisplay> latestReadings = [];

    internal MainWindow(AuthenticationService authentication, Hub hub, DataLogger logger, string? sourceError, Action returnToLogin)
    {
        authentication.RequireStaff();
        this.hub = hub;
        this.returnToLogin = returnToLogin;
        this.logger = logger;
        InitializeComponent();
        SensorStatus.Text = sourceError ?? (hub.Habitats.Count == 0 ? "No Data" : "");
        HabitatSelection.ItemsSource = hub.Habitats;
        HabitatSelection.SelectedIndex = hub.Habitats.Count == 0 ? -1 : 0;
        UpdateSelection();
        refresh.Tick += (_, _) => ShowReadings();
        Loaded += (_, _) =>
        {
            ShowReadings();
            refresh.Start();
        };
        Closed += (_, _) => refresh.Stop();
    }

    private Habitat? SelectedHabitat => HabitatSelection.SelectedItem as Habitat;

    private void ShowReadings()
    {
        latestReadings = hub.PollHabitatSensors();
        ShowSelectedReadings();
    }

    private void ShowSelectedReadings() =>
        SensorReadings.ItemsSource = latestReadings.Where(r => r.HabitatId == SelectedHabitat?.HabitatId).ToList();

    private void SelectHabitat(object sender, RoutedEventArgs e) => UpdateSelection();

    private void UpdateSelection()
    {
        ShowSelectedReadings();
        var heater = SelectedHabitat?.Equipment.OfType<HeaterChiller>().SingleOrDefault();
        HeaterTarget.Text = heater?.TargetTemperature?.ToString("0.0") ?? "";
        HeaterTarget.IsEnabled = heater is not null;
        var feeder = SelectedHabitat?.Equipment.OfType<FoodDispenser>().SingleOrDefault();
        FeederToggle.IsEnabled = feeder is not null;
        FeederToggle.Content = feeder?.IsEnabled == true ? "_Disable food dispenser" : "_Enable food dispenser";
        CommandFeedback.Text = "";
    }

    private void ApplyTarget(object sender, RoutedEventArgs e)
    {
        if (SelectedHabitat is not { } habitat ||
            habitat.Equipment.OfType<HeaterChiller>().SingleOrDefault() is not { } heater)
            return;

        string input = HeaterTarget.Text;
        const NumberStyles styles = NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint
            | NumberStyles.AllowLeadingWhite | NumberStyles.AllowTrailingWhite;
        if (!decimal.TryParse(input, styles, CultureInfo.CurrentCulture, out decimal target))
        {
            ShowInvalidTarget(input);
            return;
        }

        try
        {
            hub.SetHeaterTarget(habitat.HabitatId, heater.DeviceId, target, logger);
            CommandFeedback.Text = $"{heater.Name} target: {target:0.0} °C.";
        }
        catch (ArgumentOutOfRangeException)
        {
            ShowInvalidTarget(input);
        }
        catch (SqliteException)
        {
            CommandFeedback.Text = $"Could not log the {heater.Name} change. The previous setting was kept.";
        }
    }

    private void ShowInvalidTarget(string input) =>
        CommandFeedback.Text = $"Invalid heater/chiller target “{input}”. Enter 15.0–32.0 °C in 0.5 °C increments.";

    private void ToggleFeeder(object sender, RoutedEventArgs e)
    {
        if (SelectedHabitat is not { } habitat ||
            habitat.Equipment.OfType<FoodDispenser>().SingleOrDefault() is not { } feeder)
            return;

        hub.SetEquipmentPower(habitat.HabitatId, feeder.DeviceId, !feeder.IsEnabled);
        FeederToggle.Content = feeder.IsEnabled ? "_Disable food dispenser" : "_Enable food dispenser";
        CommandFeedback.Text = $"{feeder.Name}: {(feeder.IsEnabled ? "enabled" : "disabled")}.";
    }

    private void BackToLogin(object sender, RoutedEventArgs e)
    {
        returnToLogin();
        Close();
    }
}
