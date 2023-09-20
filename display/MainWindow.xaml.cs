using PupilSizeDisplay.Trackers;
using PupilSizeDisplay.Trackers.PupilLabs;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace PupilSizeDisplay;

public partial class MainWindow : Window
{
    private readonly PupilLabs _tracker;
    private readonly DataProcessor _dataProc;

    public MainWindow()
    {
        InitializeComponent();

        _dataProc = new DataProcessor(lvdChart, brdSize, brdLevel);
        DataContext = _dataProc;

        _tracker = new PupilLabs();
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
        _tracker.Connected += (s, e) => Dispatcher.Invoke(() =>
        {
            tblConnection.Text = "connected";
            elpCorrectionSign.Fill = Brushes.Green;
        });
        _tracker.Disconnected += (s, e) => Dispatcher.Invoke(() =>
        {
            tblConnection.Text = "disconnected";
            elpCorrectionSign.Fill = Brushes.Red;
        });
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

        _tracker.Eye = eye;
        _dataProc?.Clear();
    }
}
