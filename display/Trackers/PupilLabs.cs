using System;
using System.Text.Json;
using System.Windows;
using Websocket.Client;

namespace PupilSizeDisplay.Trackers.PupilLabs;

// Data types transferred over the net

public record class Vector2D(double X, double Y);

public record class Vector3D(double X, double Y, double Z);

public record class Circle(Vector3D Center, Vector3D Normal, double Radius);

public record class Ellipse(Vector2D Center, Vector2D Axes, double Angle);

public record class Sphere(Vector3D Center, double Radius);

public record class Pupil(
    Circle Circle3d,
    double Confidence,
    double Diameter3d,
    Ellipse Ellipse,
    Vector2D NormPos,
    double Diameter,
    Sphere Sphere,
    Ellipse ProjectedSphere,
    double Theta,
    double Phi,
    int Id
);

public class PupilLabs : ITracker
{
    public event EventHandler<double>? Sample;
    public event EventHandler? Connected;
    public event EventHandler? Disconnected;

    public Eye Eye { get; set; } = Eye.Left;
    public DataSource Source { get; set; } = DataSource.Diameter;

    public PupilLabs()
    {
        CreateWebSocketClient();
        Application.Current.Exit += App_Exit;
    }

    // Internal

    private readonly JsonSerializerOptions _jsonSerializerOptions = new() { PropertyNameCaseInsensitive = true };

    private WebsocketClient? _client;

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

    private void App_Exit(object sender, ExitEventArgs e)
    {
        _client?.Dispose();
    }

    private void OnDisconnected(DisconnectionInfo info)
    {
        System.Diagnostics.Debug.WriteLine("Disconnected");
        Disconnected?.Invoke(this, new EventArgs());

        DispatchOnce.Do(3, CreateWebSocketClient);
    }

    private void OnConnected(ReconnectionInfo info)
    {

        System.Diagnostics.Debug.WriteLine($"Connected: {info}");
        Connected?.Invoke(this, new EventArgs());
    }

    private void OnMessage(ResponseMessage msg)
    {
        Pupil? pupil = msg.Text != null ? JsonSerializer.Deserialize<Pupil>(msg.Text, _jsonSerializerOptions) : null;
        if (pupil != null && (int)Eye == (1 - pupil.Id))
        {
            var size = Source switch
            {
                DataSource.Diameter => pupil.Diameter3d,
                DataSource.Area => pupil.Diameter,
                _ => 0
            };

            if (size >= 0)
            {
                Sample?.Invoke(this, size);
            }
        }
    }
}