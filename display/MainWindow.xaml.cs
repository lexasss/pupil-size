using System;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using Websocket.Client;

namespace PupilSizeDisplay;

public partial class MainWindow : Window
{
    private readonly PupilProcessor _ui;

    private readonly JsonSerializerOptions _jsonSerializerOptions = new() { PropertyNameCaseInsensitive = true };

    private WebsocketClient? _client;

    private Source _source = Source.Diameter;
    private int _pupilVisible = 0; // 0 (left) or 1 (right)

    public MainWindow()
    {
        InitializeComponent();

        _ui = new PupilProcessor(lvdChart, brdSize, tblSize, brdMax);

        CreateWebSocketClient();

        Application.Current.Exit += App_Exit;
    }

    private void CreateWebSocketClient()
    {
        var url = new Uri("ws://127.0.0.1:51688");

        _client?.Dispose();
        _client = new WebsocketClient(url)
        {
            IsReconnectionEnabled = false
        };

        _client.DisconnectionHappened.Subscribe(OnDisconnected);
        _client.ReconnectionHappened.Subscribe(OnConnected);
        _client.MessageReceived.Subscribe(OnMessage);

        _client.Start();
    }

    private void OnDisconnected(DisconnectionInfo info) => Dispatcher.Invoke(() =>
    {
        tblConnection.Text = "disconnected";
        elpCorrectionSign.Fill = Brushes.Red;

        System.Diagnostics.Debug.WriteLine("Disconnected");

        DispatchOnce.Do(3, CreateWebSocketClient);
    });

    private void OnConnected(ReconnectionInfo info) => Dispatcher.Invoke(() =>
    {
        tblConnection.Text = "connected";
        elpCorrectionSign.Fill = Brushes.Green;

        System.Diagnostics.Debug.WriteLine("Connected");
    });

    private void OnMessage(ResponseMessage msg)
    {
        Types.Pupil? pupil = msg.Text != null ? JsonSerializer.Deserialize<Types.Pupil>(msg.Text, _jsonSerializerOptions) : null;
        if (pupil != null)
        {
            try
            {
                Dispatcher.Invoke(() =>
                {
                    if (_pupilVisible == (1 - pupil.Id))
                        _ui.Add(pupil, _source);
                });
            }
            catch (TaskCanceledException)
            { }
        }
    }

    private void App_Exit(object sender, ExitEventArgs e)
    {
        _client?.Dispose();
    }

    // UI

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        _ui?.Clear();
        lsvSource.Focus();
    }

    private void Source_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        _source = (Source)Enum.Parse(typeof(Source), lsvSource.SelectedItem?.ToString() ?? "");
        _ui?.Clear();
    }

    private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        _pupilVisible = tbcTabs.SelectedIndex;
        _ui?.Clear();
    }
}
