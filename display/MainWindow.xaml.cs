using PupilSizeDisplay.Trackers;
using System;
using System.Net;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace PupilSizeDisplay;

public partial class MainWindow : Window
{
    private enum TrackerType { PupilLabs, EtuDriver, SmartEye }

    private readonly DataProcessor _dataProc;

    private ITracker? _tracker;

    public MainWindow()
    {
        InitializeComponent();

        var settings = Display.Properties.Settings.Default;
        cmbTrackerType.SelectedIndex = settings.DeviceIndex;
        txbIP.Text = settings.DeviceIP;

        _dataProc = new DataProcessor(lvdChart, brdSize, brdLevel);
        DataContext = _dataProc;

        var trackerType = (TrackerType)cmbTrackerType.SelectedIndex;
        CreateTracker(trackerType);
    }

    private bool CreateTracker(TrackerType trackerType)
    {
        _tracker?.Dispose();
        _tracker = null;

        _dataProc.Clear();

        var ip = txbIP.Text ?? "";
        if (!IPAddress.TryParse(ip, out _))
        {
            MessageBox.Show($"'{ip}' is not a valid IP", Title, MessageBoxButton.OK, MessageBoxImage.Error);
            return false;
        }

        _tracker = trackerType switch
        {
            TrackerType.EtuDriver => new EtuDriver(ip),
            TrackerType.PupilLabs => new PupilLabs(ip),
            TrackerType.SmartEye => new SmartEye(ip),
            _ => throw new Exception($"Unsuppported tracker type: {trackerType}")
        };

        _tracker.Sample += (s, e) =>
        {
            try
            {
                Dispatcher.Invoke(() =>
                {
                    _dataProc.Add(e);
                });
            }
            catch (TaskCanceledException)
            { }
        };
        _tracker.DeviceStateChanged += (s, e) => Dispatcher.Invoke(() =>
        {
            if (e.Name != null)
            {
                lblDeviceName.Content = e.Name;
            }
            if (e.State != null)
            {
                var isConnected = e.State?.HasFlag(DeviceState.Connected) == true;
                tblConnection.Text = isConnected ? "connected" : "disconnected";
                elpCorrectionSign.Fill = isConnected ? Brushes.Green : Brushes.Red;
            }
        });

        return true;
    }

    // UI

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        _dataProc?.Clear();
        lsvSource.Focus();
    }

    private void Source_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (!IsInitialized)
            return;

        var source = (DataSource)Enum.Parse(typeof(DataSource), lsvSource.SelectedItem?.ToString() ?? "");

        if (!Enum.IsDefined(typeof(DataSource), source))
            throw new Exception("Unsupported data source");

        if (_tracker != null)
            _tracker.Source = source;
        _dataProc?.Clear();
    }

    private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (!IsInitialized)
            return;

        var eye = (Eye)tbcTabs.SelectedIndex;

        if (!Enum.IsDefined(typeof(Eye), eye))
            throw new Exception("Unsupported eye");

        if (_tracker != null)
            _tracker.Eye = eye;
        _dataProc?.Clear();
    }

    private void SelectDevice_Click(object sender, RoutedEventArgs e)
    {
        var trackerType = (TrackerType)cmbTrackerType.SelectedIndex;
        if (CreateTracker(trackerType))
        {
            var settings = Display.Properties.Settings.Default;
            settings.DeviceIndex = cmbTrackerType.SelectedIndex;
            settings.DeviceIP = txbIP.Text;
            settings.Save();
        }
    }
}
