
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using System.Collections.Generic;
using AquaGuard.Core;
using AquaGuard.Data;
using AquaGuard.Devices;
using AquaGuard.Devices.Equipment;
using Microsoft.Data.Sqlite;

namespace AquaGuard;

public partial class MainWindow : Window
{
    private readonly DispatcherTimer refresh = new()
    {
        Interval = TimeSpan.FromSeconds(1)
    };

    private readonly Hub hub;
    private readonly DataLogger logger;
    private readonly Action returnToLogin;

    private IReadOnlyList<SensorDisplay> latestReadings = [];

    internal MainWindow(
        AuthenticationService authentication,
        Hub hub,
        DataLogger logger,
        string? sourceError,
        Action returnToLogin)
    {
        authentication.RequireStaff();

        this.hub = hub;
        this.returnToLogin = returnToLogin;
        this.logger = logger;

        InitializeComponent();

        SensorStatus.Text = sourceError ??
            (hub.Habitats.Count == 0 ? "No Data" : "");

        HabitatSelection.ItemsSource = hub.Habitats;

        HabitatSelection.SelectedIndex =
            hub.Habitats.Count == 0 ? -1 : 0;

        UpdateSelection();

        refresh.Tick += (_, _) => ShowReadings();

        Loaded += (_, _) =>
        {
            ShowReadings();
            refresh.Start();
        };

        Closed += (_, _) => refresh.Stop();
    }

    private Habitat? SelectedHabitat =>
        HabitatSelection.SelectedItem as Habitat;

    // SENSOR MONITORING

    private void ShowReadings()
    {
        latestReadings = hub.PollHabitatSensors();

        ShowSelectedReadings();

        SensorLoggingStatus.Text =
            hub.SensorLoggingFailed
                ? "Sensor history could not be saved to SQLite."
                : "";

        AlarmStatus.Text = string.Join(" ", new[]
        {
            hub.ThresholdConfigurationFailed
                ? "Some threshold settings could not be loaded."
                : "",

            hub.AlarmLoggingFailed
                ? "Alarm history is waiting for SQLite storage."
                : ""

        }.Where(message => message.Length > 0));
    }

    private void ShowSelectedReadings()
    {
        SensorReadings.ItemsSource = latestReadings
            .Where(r => r.HabitatId ==
                SelectedHabitat?.HabitatId)
            .ToList();

        var alarms = hub.ActiveAlerts
            .Where(alert => alert.HabitatId ==
                SelectedHabitat?.HabitatId)
            .Select(alert =>
                $"{alert.HabitatId} / {alert.DeviceId} — " +
                $"{alert.Severity} at " +
                $"{alert.Timestamp:yyyy-MM-dd HH:mm:ss} UTC")
            .ToList();

        ActiveAlarms.ItemsSource =
            alarms.Count == 0
                ? ["No active alarms."]
                : alarms;
    }

    // HABITAT SELECTION

    private void SelectHabitat(
        object sender, RoutedEventArgs e)
    {
        UpdateSelection();
    }

    private void UpdateSelection()
    {
        ShowSelectedReadings();

        HistoryButton.IsEnabled =
            SelectedHabitat is not null;

        // Heater and chiller

        var heater = SelectedHabitat?.Equipment
            .OfType<HeaterChiller>()
            .SingleOrDefault();

        HeaterTarget.Text =
            heater?.TargetTemperature?.ToString("0.0") ?? "";

        HeaterTarget.IsEnabled = heater is not null;

        // Food dispenser

        var feeder = SelectedHabitat?.Equipment
            .OfType<FoodDispenser>()
            .SingleOrDefault();

        FeederToggle.IsEnabled = feeder is not null;

        FeederToggle.Content =
            feeder?.IsEnabled == true
                ? "_Disable food dispenser"
                : "_Enable food dispenser";

        // Circulation pump

        var pump = SelectedHabitat?.Equipment
            .OfType<CirculationPump>()
            .SingleOrDefault();

        PumpToggle.IsEnabled =
            PumpSpeed.IsEnabled =
            PumpApply.IsEnabled =
                pump is not null;

        PumpToggle.Content =
            pump?.IsOn == true
                ? "Turn pump off"
                : "Turn pump on";

        PumpSpeed.Text =
            pump?.SpeedPercentage
                .ToString(CultureInfo.CurrentCulture) ?? "";

        // Filter system

        var filter = SelectedHabitat?.Equipment
            .OfType<FilterSystem>()
            .SingleOrDefault();

        FilterToggle.IsEnabled = filter is not null;

        FilterToggle.Content =
            filter?.IsOn == true
                ? "Turn filter off"
                : "Turn filter on";

        // Aerator

        var aerator = SelectedHabitat?.Equipment
            .OfType<Aerator>()
            .SingleOrDefault();

        AeratorToggle.IsEnabled = aerator is not null;

        AeratorToggle.Content =
            aerator?.IsOn == true
                ? "Turn aerator off"
                : "Turn aerator on";

        AeratorOutput.IsEnabled =
            AeratorApply.IsEnabled =
                aerator is not null;

        AeratorOutput.Text =
            aerator?.OutputPercentage
                .ToString(CultureInfo.CurrentCulture) ?? "";

        CommandFeedback.Text = "";

        // Update threshold configuration

        UpdateThresholdSensors();
    }

    // HEATER CONTROL

    private void ApplyTarget(
        object sender, RoutedEventArgs e)
    {
        if (SelectedHabitat is not { } habitat ||
            habitat.Equipment.OfType<HeaterChiller>()
                .SingleOrDefault() is not { } heater)
        {
            return;
        }

        string input = HeaterTarget.Text;

        const NumberStyles styles =
            NumberStyles.AllowLeadingSign |
            NumberStyles.AllowDecimalPoint |
            NumberStyles.AllowLeadingWhite |
            NumberStyles.AllowTrailingWhite;

        if (!decimal.TryParse(
            input,
            styles,
            CultureInfo.CurrentCulture,
            out decimal target))
        {
            ShowInvalidTarget(input);
            return;
        }

        try
        {
            hub.SetHeaterTarget(
                habitat.HabitatId,
                heater.DeviceId,
                target,
                logger);

            CommandFeedback.Text =
                $"{heater.Name} target: {target:0.0} °C.";
        }
        catch (ArgumentOutOfRangeException)
        {
            ShowInvalidTarget(input);
        }
        catch (SqliteException)
        {
            CommandFeedback.Text =
                $"Could not log the {heater.Name} change. " +
                "The previous setting was kept.";
        }
    }

    private void ShowInvalidTarget(string input)
    {
        CommandFeedback.Text =
            $"Invalid heater/chiller target “{input}”. " +
            "Enter 15.0–32.0 °C in 0.5 °C increments.";
    }

    // FOOD DISPENSER CONTROL

    private void ToggleFeeder(
        object sender, RoutedEventArgs e)
    {
        if (SelectedHabitat is not { } habitat ||
            habitat.Equipment.OfType<FoodDispenser>()
                .SingleOrDefault() is not { } feeder)
        {
            return;
        }

        if (!TryApplyLoggedChange(
            feeder.Name,
            () => hub.SetEquipmentPower(
                habitat.HabitatId,
                feeder.DeviceId,
                !feeder.IsEnabled)))
        {
            return;
        }

        FeederToggle.Content =
            feeder.IsEnabled
                ? "_Disable food dispenser"
                : "_Enable food dispenser";

        CommandFeedback.Text =
            $"{feeder.Name}: " +
            $"{(feeder.IsEnabled ? "enabled" : "disabled")}.";
    }

    // NAVIGATION

    private void BackToLogin(
        object sender, RoutedEventArgs e)
    {
        returnToLogin();
        Close();
    }

    private void ShowHistory(
        object sender, RoutedEventArgs e)
    {
        if (SelectedHabitat is { } habitat)
        {
            new HabitatHistoryWindow(hub, habitat)
            {
                Owner = this
            }.ShowDialog();
        }
    }

    // CIRCULATION PUMP CONTROL

    private void TogglePump(
        object sender, RoutedEventArgs e)
    {
        if (SelectedHabitat is not { } habitat ||
            habitat.Equipment.OfType<CirculationPump>()
                .SingleOrDefault() is not { } pump)
        {
            return;
        }

        if (!TryApplyLoggedChange(
            pump.Name,
            () => hub.SetEquipmentPower(
                habitat.HabitatId,
                pump.DeviceId,
                !pump.IsOn)))
        {
            return;
        }

        PumpToggle.Content =
            pump.IsOn
                ? "Turn pump off"
                : "Turn pump on";

        CommandFeedback.Text =
            $"{pump.Name}: {(pump.IsOn ? "on" : "off")}.";
    }

    private void ApplyPumpSpeed(
        object sender, RoutedEventArgs e)
    {
        ApplyPercentage(
            SelectedHabitat?.Equipment
                .OfType<CirculationPump>()
                .SingleOrDefault(),
            "speed",
            PumpSpeed.Text);
    }

    // FILTER SYSTEM CONTROL

    private void ToggleFilter(
        object sender, RoutedEventArgs e)
    {
        if (SelectedHabitat is not { } habitat ||
            habitat.Equipment.OfType<FilterSystem>()
                .SingleOrDefault() is not { } filter)
        {
            return;
        }

        if (!TryApplyLoggedChange(
            filter.Name,
            () => hub.SetEquipmentPower(
                habitat.HabitatId,
                filter.DeviceId,
                !filter.IsOn)))
        {
            return;
        }

        FilterToggle.Content =
            filter.IsOn
                ? "Turn filter off"
                : "Turn filter on";

        CommandFeedback.Text =
            $"{filter.Name}: {(filter.IsOn ? "on" : "off")}.";
    }

    // AERATOR CONTROL

    private void ToggleAerator(
        object sender, RoutedEventArgs e)
    {
        if (SelectedHabitat is not { } habitat ||
            habitat.Equipment.OfType<Aerator>()
                .SingleOrDefault() is not { } aerator)
        {
            return;
        }

        if (!TryApplyLoggedChange(
            aerator.Name,
            () => hub.SetEquipmentPower(
                habitat.HabitatId,
                aerator.DeviceId,
                !aerator.IsOn)))
        {
            return;
        }

        AeratorToggle.Content =
            aerator.IsOn
                ? "Turn aerator off"
                : "Turn aerator on";

        AeratorOutput.Text =
            aerator.OutputPercentage
                .ToString(CultureInfo.CurrentCulture);

        CommandFeedback.Text =
            $"{aerator.Name}: {(aerator.IsOn ? "on" : "off")}.";
    }

    private void ApplyAeratorOutput(
        object sender, RoutedEventArgs e)
    {
        var aerator = SelectedHabitat?.Equipment
            .OfType<Aerator>()
            .SingleOrDefault();

        ApplyPercentage(
            aerator,
            "output",
            AeratorOutput.Text);

        if (aerator is not null)
        {
            AeratorToggle.Content =
                aerator.IsOn
                    ? "Turn aerator off"
                    : "Turn aerator on";

            AeratorOutput.Text =
                aerator.OutputPercentage
                    .ToString(CultureInfo.CurrentCulture);
        }
    }

    // EQUIPMENT PERCENTAGE VALIDATION

    private void ApplyPercentage(
        IDevice? device,
        string setting,
        string input)
    {
        if (device is null)
        {
            return;
        }

        if (!int.TryParse(
                input,
                NumberStyles.Integer,
                CultureInfo.CurrentCulture,
                out int percentage)
            || percentage is < 0 or > 100)
        {
            CommandFeedback.Text =
                $"Invalid {device.Name} {setting} “{input}”. " +
                "Enter a whole number from 0 to 100.";

            return;
        }

        if (TryApplyLoggedChange(
            device.Name,
            () => hub.SetEquipmentPercentage(
                device.HabitatId,
                device.DeviceId,
                percentage)))
        {
            CommandFeedback.Text =
                $"{device.Name} {setting}: {percentage}%.";
        }
    }

    // SQLITE EQUIPMENT LOGGING

    private bool TryApplyLoggedChange(
        string deviceName,
        Action apply)
    {
        try
        {
            apply();
            return true;
        }
        catch (SqliteException)
        {
            CommandFeedback.Text =
                $"Could not log the {deviceName} change. " +
                "The previous setting was kept.";

            return false;
        }
    }

    // SENSOR THRESHOLD CONFIGURATION

    private void UpdateThresholdSensors()
    {
        if (ThresholdSensorSelection == null)
        {
            return;
        }

        ThresholdSensorSelection.ItemsSource =
            SelectedHabitat?.Sensors;

        ThresholdSensorSelection.SelectedIndex =
            SelectedHabitat?.Sensors.Any() == true
                ? 0
                : -1;

        LoadSensorThresholds();
    }

    private void SelectThresholdSensor(
        object sender, SelectionChangedEventArgs e)
    {
        LoadSensorThresholds();
    }

    private void LoadSensorThresholds()
    {
        // Prevent errors during window initialization

        if (ThresholdSensorSelection == null ||
            ApplyThresholdButton == null ||
            WarningLowInput == null ||
            NormalLowInput == null ||
            NormalHighInput == null ||
            WarningHighInput == null ||
            ThresholdFeedback == null)
        {
            return;
        }

        ApplyThresholdButton.IsEnabled = false;

        WarningLowInput.Text = "";
        NormalLowInput.Text = "";
        NormalHighInput.Text = "";
        WarningHighInput.Text = "";
        ThresholdFeedback.Text = "";

        if (SelectedHabitat is not { } habitat ||
            ThresholdSensorSelection.SelectedItem
                is not IDevice sensor)
        {
            return;
        }

        var thresholds = hub.GetSensorThresholds(
            habitat.HabitatId,
            sensor.DeviceId);

        ApplyThresholdButton.IsEnabled = true;

        if (thresholds is null)
        {
            ThresholdFeedback.Text =
                "No thresholds configured for this sensor.";

            return;
        }

        WarningLowInput.Text =
            thresholds.WarningLow.ToString(
                CultureInfo.CurrentCulture);

        NormalLowInput.Text =
            thresholds.NormalLow.ToString(
                CultureInfo.CurrentCulture);

        NormalHighInput.Text =
            thresholds.NormalHigh.ToString(
                CultureInfo.CurrentCulture);

        WarningHighInput.Text =
            thresholds.WarningHigh.ToString(
                CultureInfo.CurrentCulture);
    }

    private void ApplyThresholds(
        object sender, RoutedEventArgs e)
    {
        if (SelectedHabitat is not { } habitat ||
            ThresholdSensorSelection.SelectedItem
                is not IDevice sensor)
        {
            return;
        }

        // Validate numeric input

        if (!double.TryParse(
                WarningLowInput.Text,
                out double warningLow) ||

            !double.TryParse(
                NormalLowInput.Text,
                out double normalLow) ||

            !double.TryParse(
                NormalHighInput.Text,
                out double normalHigh) ||

            !double.TryParse(
                WarningHighInput.Text,
                out double warningHigh))
        {
            ThresholdFeedback.Text =
                "Please enter valid numbers in all four fields.";

            return;
        }

        // Reject NaN and infinity

        if (!double.IsFinite(warningLow) ||
            !double.IsFinite(normalLow) ||
            !double.IsFinite(normalHigh) ||
            !double.IsFinite(warningHigh))
        {
            ThresholdFeedback.Text =
                "Threshold values must be finite numbers.";

            return;
        }

        // Create the threshold configuration

        var thresholds = new SensorThresholds
        {
            WarningLow = warningLow,
            NormalLow = normalLow,
            NormalHigh = normalHigh,
            WarningHigh = warningHigh
        };

        try
        {
            hub.SetSensorThresholds(
                habitat.HabitatId,
                sensor.DeviceId,
                thresholds);

            ThresholdFeedback.Text =
                $"Thresholds updated for {sensor.Name}.";
        }
        catch (ArgumentException)
        {
            ThresholdFeedback.Text =
                "Invalid thresholds. " +
                "Values must be in ascending order.";
        }
        catch (UnauthorizedAccessException)
        {
            ThresholdFeedback.Text =
                "Only staff can change sensor thresholds.";
        }
        catch (InvalidOperationException ex)
        {
            ThresholdFeedback.Text = ex.Message;
        }
        catch (KeyNotFoundException ex)
        {
            ThresholdFeedback.Text = ex.Message;
        }
    }
}
