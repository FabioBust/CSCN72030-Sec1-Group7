using System.IO;
using System.Windows;
using System.Windows.Threading;
using AquaGuard.Core;
using AquaGuard.Data;

namespace AquaGuard
{
    public partial class MainWindow : Window
    {
        private readonly SensorMonitor monitor = new();
        private readonly DispatcherTimer refresh = new() { Interval = TimeSpan.FromSeconds(1) };

        public MainWindow()
        {
            InitializeComponent();
            LoadSources(Environment.GetEnvironmentVariable("AQUAGUARD_SENSOR_DATA_DIRECTORY"));
            refresh.Tick += (_, _) => ShowReadings();
            Loaded += (_, _) =>
            {
                ShowReadings();
                refresh.Start();
            };
            Closed += (_, _) => refresh.Stop();
        }

        private void LoadSources(string? root)
        {
            if (!Directory.Exists(root))
            {
                return;
            }

            try
            {
                foreach (string habitat in Directory.GetDirectories(root))
                {
                    foreach (string file in Directory.GetFiles(habitat, "*.txt"))
                    {
                        monitor.AddSource(Path.GetFileName(habitat), Path.GetFileNameWithoutExtension(file),
                            new FileDataProvider(file));
                    }
                }
            }
            catch (IOException)
            {
                SensorStatus.Text = "Some sensor sources could not be loaded.";
            }
            catch (UnauthorizedAccessException)
            {
                SensorStatus.Text = "Some sensor sources could not be loaded.";
            }
        }

        private void ShowReadings()
        {
            var readings = monitor.Poll();
            SensorReadings.ItemsSource = readings;
            if (readings.Count == 0 && string.IsNullOrEmpty(SensorStatus.Text))
            {
                SensorStatus.Text = "No Data";
            }
        }
    }
}
