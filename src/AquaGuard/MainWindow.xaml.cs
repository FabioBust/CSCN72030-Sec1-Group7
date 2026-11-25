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
        SensorLoggingStatus.Text = hub.SensorLoggingFailed ? "Sensor history could not be saved to SQLite." : "";
        AlarmStatus.Text = string.Join(" ", new[]
        {
            hub.ThresholdConfigurationFailed ? "Some threshold settings could not be loaded." : "",
            hub.AlarmLoggingFailed ? "Alarm history is waiting for SQLite storage." : ""
        }.Where(message => message.Length > 0));
    }

    private void ShowSelectedReadings()
    {
        SensorReadings.ItemsSource = latestReadings.Where(r => r.HabitatId == SelectedHabitat?.HabitatId).ToList();
        var alarms = hub.ActiveAlerts.Where(alert => alert.HabitatId == SelectedHabitat?.HabitatId)
            .Select(alert => $"{alert.HabitatId} / {alert.DeviceId} — {alert.Severity} at {alert.Timestamp:yyyy-MM-dd HH:mm:ss} UTC")
            .ToList();
        ActiveAlarms.ItemsSource = alarms.Count == 0 ? ["No active alarms."] : alarms;
    }

    private void SelectHabitat(object sender, RoutedEventArgs e) => UpdateSelection();

    private void UpdateSelection()
    {
        ShowSelectedReadings();
        HistoryButton.IsEnabled = SelectedHabitat is not null;
        var heater = SelectedHabitat?.Equipment.OfType<HeaterChiller>().SingleOrDefault();
        HeaterTarget.Text = heater?.TargetTemperature?.ToString("0.0") ?? "";
        HeaterTarget.IsEnabled = heater is not null;
        var feeder = SelectedHabitat?.Equipment.OfType<FoodDispenser>().SingleOrDefault();
        FeederToggle.IsEnabled = feeder is not null;
        FeederToggle.Content = feeder?.IsEnabled == true ? "_Disable food dispenser" : "_Enable food dispenser";
        var pump = SelectedHabitat?.Equipment.OfType<CirculationPump>().SingleOrDefault();
        PumpToggle.IsEnabled = PumpSpeed.IsEnabled = PumpApply.IsEnabled = pump is not null;
        PumpToggle.Content = pump?.IsOn == true ? "Turn pump off" : "Turn pump on";
        PumpSpeed.Text = pump?.SpeedPercentage.ToString(CultureInfo.CurrentCulture) ?? "";
        var filter = SelectedHabitat?.Equipment.OfType<FilterSystem>().SingleOrDefault();
        FilterToggle.IsEnabled = filter is not null;
        FilterToggle.Content = filter?.IsOn == true ? "Turn filter off" : "Turn filter on";
        var aerator = SelectedHabitat?.Equipment.OfType<Aerator>().SingleOrDefault();
        AeratorOutput.IsEnabled = AeratorApply.IsEnabled = aerator is not null;
        AeratorOutput.Text = aerator?.OutputPercentage.ToString(CultureInfo.CurrentCulture) ?? "";
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

        if (!TryApplyLoggedChange(feeder.Name, () => hub.SetEquipmentPower(habitat.HabitatId, feeder.DeviceId, !feeder.IsEnabled)))
            return;
        FeederToggle.Content = feeder.IsEnabled ? "_Disable food dispenser" : "_Enable food dispenser";
        CommandFeedback.Text = $"{feeder.Name}: {(feeder.IsEnabled ? "enabled" : "disabled")}.";
    }

    private void BackToLogin(object sender, RoutedEventArgs e)
    {
        returnToLogin();
        Close();
    }

    private void ShowHistory(object sender, RoutedEventArgs e)
    {
        if (SelectedHabitat is { } habitat)
            new HabitatHistoryWindow(hub, habitat) { Owner = this }.ShowDialog();
    }

    private void TogglePump(object sender, RoutedEventArgs e)
    {
        if (SelectedHabitat is not { } habitat ||
            habitat.Equipment.OfType<CirculationPump>().SingleOrDefault() is not { } pump)
            return;

        if (!TryApplyLoggedChange(pump.Name, () => hub.SetEquipmentPower(habitat.HabitatId, pump.DeviceId, !pump.IsOn)))
            return;
        PumpToggle.Content = pump.IsOn ? "Turn pump off" : "Turn pump on";
        CommandFeedback.Text = $"{pump.Name}: {(pump.IsOn ? "on" : "off")}.";
    }

    private void ToggleFilter(object sender, RoutedEventArgs e)
    {
        if (SelectedHabitat is not { } habitat ||
            habitat.Equipment.OfType<FilterSystem>().SingleOrDefault() is not { } filter)
            return;

        if (!TryApplyLoggedChange(filter.Name, () => hub.SetEquipmentPower(habitat.HabitatId, filter.DeviceId, !filter.IsOn)))
            return;
        FilterToggle.Content = filter.IsOn ? "Turn filter off" : "Turn filter on";
        CommandFeedback.Text = $"{filter.Name}: {(filter.IsOn ? "on" : "off")}.";
    }

    private void ApplyPumpSpeed(object sender, RoutedEventArgs e) =>
        ApplyPercentage(SelectedHabitat?.Equipment.OfType<CirculationPump>().SingleOrDefault(), "speed", PumpSpeed.Text);

    private void ApplyAeratorOutput(object sender, RoutedEventArgs e) =>
        ApplyPercentage(SelectedHabitat?.Equipment.OfType<Aerator>().SingleOrDefault(), "output", AeratorOutput.Text);

    private void ApplyPercentage(AquaGuard.Devices.IDevice? device, string setting, string input)
    {
        if (device is null)
            return;

        if (!int.TryParse(input, NumberStyles.Integer, CultureInfo.CurrentCulture, out int percentage)
            || percentage is < 0 or > 100)
        {
            CommandFeedback.Text = $"Invalid {device.Name} {setting} “{input}”. Enter a whole number from 0 to 100.";
            return;
        }

        if (TryApplyLoggedChange(device.Name, () => hub.SetEquipmentPercentage(device.HabitatId, device.DeviceId, percentage)))
            CommandFeedback.Text = $"{device.Name} {setting}: {percentage}%.";
    }

    private bool TryApplyLoggedChange(string deviceName, Action apply)
    {
        try
        {
            apply();
            return true;
        }
        catch (SqliteException)
        {
            CommandFeedback.Text = $"Could not log the {deviceName} change. The previous setting was kept.";
            return false;
        }
    }
}
