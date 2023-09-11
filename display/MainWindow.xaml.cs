using System;
using System.Collections.Generic;
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

    private readonly LiveData[] _graphs;
    private readonly Border[] _bars;
    private readonly TextBlock[] _values;

    private readonly Queue<Types.Pupil>[] _queue = { new Queue<Types.Pupil>(), new Queue<Types.Pupil>() };

    private readonly JsonSerializerOptions _jsonSerializerOptions = new JsonSerializerOptions() { PropertyNameCaseInsensitive = true };

    private const int MAX_QUEUE_SIZE = 5;       // samples to average
    private const double DATA_SOURCE_FREQUENCY = 120; // Hz
    private const double DATA_UPDATE_FREQUENCY = DATA_SOURCE_FREQUENCY / MAX_QUEUE_SIZE;  // Hz

    private double[] _maxPupilSize = {0, 0};
    private Source _source = Source.Diameter;
    private int _pupilVisible = 0;

    public MainWindow()
    {
        InitializeComponent();

        _graphs = new LiveData[2] { lvdChartLeft, lvdChartRight };
        _bars = new Border[2] { brdSizeLeft, brdSizeRight };
        _values = new TextBlock[2] { tblSizeLeft, tblSizeRight };

        var exitEvent = new ManualResetEvent(false);
        var url = new Uri("ws://127.0.0.1:51688");

        _client = new WebsocketClient(url)
        {
            ReconnectTimeout = TimeSpan.FromSeconds(10)
        };

        _client.ReconnectionHappened.Subscribe(info => Dispatcher.Invoke(() => {
            System.Diagnostics.Debug.WriteLine($"Reconnection happened, type: {info.Type}");
            //ResetGraphs();
        }));

        _client.MessageReceived.Subscribe(msg =>
        {
            Types.Pupil? pupil = msg.Text != null ? JsonSerializer.Deserialize<Types.Pupil>(msg.Text, _jsonSerializerOptions) : null;
            if (pupil != null)
            {
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

    private void ResetGraphs()
    {
        if (_graphs != null)
            _graphs[_pupilVisible].Reset(DATA_UPDATE_FREQUENCY / DATA_SOURCE_FREQUENCY / LiveData.PixelsPerPoint, 0);
    }

    private void HandlePupil(Types.Pupil pupil)
    {
        var id = 1 - pupil.Id;

        if (_pupilVisible != id)
            return;

        var size = _source switch
        {
            Source.Diameter => pupil.Diameter3d,
            Source.Area => pupil.Diameter,
            _ => 0
        };

        if (size < 0)
            return;

        _values[id].Text = size.ToString("F2");
        _queue[id].Enqueue(pupil);

        UpdateGraphAndBar(id);
    }

    private void UpdateGraphAndBar(int id)
    {
        if (_queue[id].Count != MAX_QUEUE_SIZE)
            return;

        var mean = _queue[id].Average(pupil => _source switch
        {
            Source.Diameter => pupil.Diameter3d,
            Source.Area => pupil.Diameter,
            _ => 0
        });
        _graphs[id].Add(Utils.Timestamp.Sec, mean);

        if (_maxPupilSize[id] < mean)
            _maxPupilSize[id] = mean;

        _bars[id].Height = (_bars[id].Parent as Grid)!.ActualHeight * mean / _maxPupilSize[id];

        _queue[id].Clear();
    }

    private void Exit(object sender, ExitEventArgs e)
    {
        _client.Dispose();
    }

    // UI

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        ResetGraphs();
        lsvSource.Focus();
    }

    private void Source_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        _source = (Source)Enum.Parse(typeof(Source), lsvSource.SelectedItem?.ToString() ?? "");
        ResetGraphs();
    }

    private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        _pupilVisible = tbcTabs.SelectedIndex;
        ResetGraphs();
    }
}
