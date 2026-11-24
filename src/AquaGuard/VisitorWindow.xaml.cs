using System.Windows;
using System.Windows.Threading;
using AquaGuard.Core;
using Microsoft.Data.Sqlite;

namespace AquaGuard;

public partial class VisitorWindow : Window
{
    private readonly Hub hub;
    private readonly Action returnToLogin;
    private readonly DispatcherTimer refresh = new() { Interval = TimeSpan.FromSeconds(1) };

    internal VisitorWindow(Hub hub, Action returnToLogin)
    {
        this.hub = hub;
        this.returnToLogin = returnToLogin;
        InitializeComponent();
        HabitatSelection.ItemsSource = hub.Habitats;
        HabitatSelection.SelectedIndex = hub.Habitats.Count == 0 ? -1 : 0;
        UpdateEligibility();
        refresh.Tick += (_, _) => UpdateEligibility(updateMessage: false);
        Loaded += (_, _) => refresh.Start();
        Closed += (_, _) => refresh.Stop();
    }

    private void SelectHabitat(object sender, RoutedEventArgs e) => UpdateEligibility();

    private void UpdateEligibility(bool updateMessage = true)
    {
        bool eligible;
        try
        {
            eligible = HabitatSelection.SelectedItem is Habitat habitat && hub.CanFeed(habitat.HabitatId);
        }
        catch (SqliteException)
        {
            FeedButton.IsEnabled = false;
            FeedingStatus.Text = "Feeding history is unavailable.";
            return;
        }
        if (updateMessage || FeedButton.IsEnabled != eligible)
            FeedingStatus.Text = eligible ? "Feeding is available." : "Feeding is unavailable.";
        FeedButton.IsEnabled = eligible;
    }

    private void Feed(object sender, RoutedEventArgs e)
    {
        if (HabitatSelection.SelectedItem is not Habitat habitat)
            return;

        try
        {
            bool success = hub.RequestVisitorFeeding(habitat.HabitatId, 2m);
            UpdateEligibility();
            FeedingStatus.Text = success ? "One simulated portion dispensed." : "Feeding is unavailable.";
        }
        catch (SqliteException)
        {
            FeedButton.IsEnabled = false;
            FeedingStatus.Text = "Could not record the feeding. No portion was dispensed.";
        }
    }

    private void BackToLogin(object sender, RoutedEventArgs e)
    {
        returnToLogin();
        Close();
    }
}
