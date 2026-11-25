using System.Windows;
using AquaGuard.Core;
using Microsoft.Data.Sqlite;

namespace AquaGuard;

public partial class HabitatHistoryWindow : Window
{
    private const int PageSize = 100;
    private readonly Hub hub;
    private readonly Habitat habitat;
    private readonly DateTimeOffset through = DateTimeOffset.UtcNow;
    private int offset;

    internal HabitatHistoryWindow(Hub hub, Habitat habitat)
    {
        this.hub = hub;
        this.habitat = habitat;
        InitializeComponent();
        Title = Heading.Text = $"{habitat.Name} history";
        ShowPage(0);
    }

    private void ShowNewer(object sender, RoutedEventArgs e) => ShowPage(offset - PageSize);
    private void ShowOlder(object sender, RoutedEventArgs e) => ShowPage(offset + PageSize);

    private void ShowPage(int pageOffset)
    {
        try
        {
            var rows = hub.ReadHabitatHistory(habitat.HabitatId, through, pageOffset, PageSize + 1);
            offset = pageOffset;
            Records.ItemsSource = rows.Take(PageSize);
            Newer.IsEnabled = offset > 0;
            Older.IsEnabled = rows.Count > PageSize;
            Status.Text = rows.Count == 0
                ? "No history records are available for this habitat."
                : $"Records {offset + 1}–{offset + Math.Min(PageSize, rows.Count)}, newest first.";
        }
        catch (SqliteException)
        {
            Status.Text = "Could not read habitat history from SQLite.";
            Records.ItemsSource = null;
            Newer.IsEnabled = Older.IsEnabled = false;
        }
    }
}
