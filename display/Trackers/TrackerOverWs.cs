using System;
using System.Windows;
using Websocket.Client;

namespace PupilSizeDisplay.Trackers;

public abstract class TrackerOverWs : ITracker
{
    public event EventHandler<DeviceInfo>? DeviceStateChanged;
    public event EventHandler<double>? Sample;
    public Eye Eye { get; set; }
    public DataSource Source { get; set; }

    public TrackerOverWs(string ip = "127.0.0.1")
    {
        _ip = ip;

        CreateWebSocketClient();

        Application.Current.Exit += App_Exit;
    }

    public void Dispose()
    {
        _isDisposed = true;
        _reconnectTimer?.Dispose();
        _client?.Dispose();
        GC.SuppressFinalize(this);
    }

    // Internal

    protected abstract string Name { get; }
    protected abstract int Port { get; }
    protected abstract void ParseMessage(string message);

    private readonly string _ip;

    private WebsocketClient? _client;
    private DispatchOnce? _reconnectTimer;

    private bool _isDisposed = false;

    protected virtual void OnDeviceStateChanged(DeviceInfo e)
    {
        DeviceStateChanged?.Invoke(this, e);
    }

    protected virtual void OnSample(double e)
    {
        Sample?.Invoke(this, e);
    }

    private void CreateWebSocketClient()
    {
        _reconnectTimer = null;

        var url = new Uri($"ws://{_ip}:{Port}");

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

    private void App_Exit(object sender, ExitEventArgs e)
    {
        _client?.Dispose();
    }

    private void OnDisconnected(DisconnectionInfo info)
    {
        info.CancelReconnection = true;

        System.Diagnostics.Debug.WriteLine($"Connection closed: {info.Type}");
        if (!string.IsNullOrEmpty(info.CloseStatusDescription))
            System.Diagnostics.Debug.WriteLine(info.CloseStatusDescription);
        if (info.Exception != null)
            System.Diagnostics.Debug.WriteLine($"   - {info.Exception.Message}");

        DeviceStateChanged?.Invoke(this, new DeviceInfo() { State = DeviceState.None });

        if (!_isDisposed)
        {
            _reconnectTimer = DispatchOnce.Do(3, CreateWebSocketClient);
        }
    }

    private void OnConnected(ReconnectionInfo info)
    {
        System.Diagnostics.Debug.WriteLine($"Connect established: {info.Type}");
        DeviceStateChanged?.Invoke(this, new DeviceInfo() { Name = Name, State = DeviceState.Connected | DeviceState.Tracking });
    }

    private void OnMessage(ResponseMessage msg)
    {
        if (msg.Text != null)
        {
            ParseMessage(msg.Text);
        }
    }
}
