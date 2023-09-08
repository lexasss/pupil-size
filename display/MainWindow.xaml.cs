using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Websocket.Client;

namespace PupilSizeDisplay;

public enum Source { Diameter, Area }

public partial class MainWindow : Window
{
    private readonly WebsocketClient _client;
    private readonly Queue<Types.Pupil> _queue = new Queue<Types.Pupil>();

    private readonly JsonSerializerOptions _jsonSerializerOptions = new JsonSerializerOptions() { PropertyNameCaseInsensitive = true };

    private const int MAX_QUEUE_SIZE = 5;       // samples to average
    private const double DATA_SOURCE_FREQUENCY = 120; // Hz
    private const double DATA_UPDATE_FREQUENCY = DATA_SOURCE_FREQUENCY / MAX_QUEUE_SIZE;  // Hz

    private double _maxPupilSize = 0;
    private Source _source = Source.Diameter;

    public MainWindow()
    {
        InitializeComponent();

        var exitEvent = new ManualResetEvent(false);
        var url = new Uri("ws://127.0.0.1:51688");

        _client = new WebsocketClient(url)
        {
            ReconnectTimeout = TimeSpan.FromSeconds(10)
        };

        _client.ReconnectionHappened.Subscribe(info => {
            Debug.WriteLine($"Reconnection happened, type: {info.Type}");
            lvdChart.Reset(DATA_UPDATE_FREQUENCY / DATA_SOURCE_FREQUENCY / LiveData.PixelsPerPoint, 0);
        });

        _client.MessageReceived.Subscribe(msg =>
        {
            Types.Pupil? pupil = msg.Text != null ? JsonSerializer.Deserialize<Types.Pupil>(msg.Text, _jsonSerializerOptions) : null;
            if (pupil != null)
            {
                Debug.WriteLine(pupil);
                try
                {
                    Dispatcher.Invoke(() => HandlePupil(pupil));
                }
                catch (TaskCanceledException)
                { }
            }
        });

        _client.Start();

        Application.Current.Exit += Exit;
    }

    private void HandlePupil(Types.Pupil pupil)
    {
        var size = _source switch
        {
            Source.Diameter => pupil.Diameter3d,
            Source.Area => pupil.Diameter,
            _ => 0
        };
        tblSize.Text = size.ToString("F2");
        
        _queue.Enqueue(pupil);
        UpdateGraphAndBar();
    }

    private void UpdateGraphAndBar()
    {
        if (_queue.Count != MAX_QUEUE_SIZE)
            return;

        var mean = _queue.Average(pupil => _source switch
        {
            Source.Diameter => pupil.Diameter3d,
            Source.Area => pupil.Diameter,
            _ => 0
        });
        lvdChart.Add(Utils.Timestamp.Sec, mean);

        if (_maxPupilSize < mean)
            _maxPupilSize = mean;

        brdSize.Height = ActualHeight * mean / _maxPupilSize;

        _queue.Clear();
    }

    private void Exit(object sender, ExitEventArgs e)
    {
        _client.Dispose();
    }

    // UI

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        lvdChart.Reset(DATA_UPDATE_FREQUENCY / DATA_SOURCE_FREQUENCY / LiveData.PixelsPerPoint, 0);
        lsvSource.Focus();
    }

    private void Source_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        _source = (Source)Enum.Parse(typeof(Source), lsvSource.SelectedItem?.ToString() ?? "");

        if (lvdChart == null)
            return;

        lvdChart.Reset(DATA_UPDATE_FREQUENCY / DATA_SOURCE_FREQUENCY / LiveData.PixelsPerPoint, 0);
    }
}
