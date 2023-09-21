using System.Text.Json;

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

public class Tracker : BaseTracker
{
    public Tracker(string ip) : base(ip) { }

    // Internal

    private readonly JsonSerializerOptions _jsonSerializerOptions = new() { PropertyNameCaseInsensitive = true };

    protected override string Name => "Pupil Core";
    protected override int Port => 51688;

    protected override void ParseMessage(string message)
    {
        Pupil? pupil = JsonSerializer.Deserialize<Pupil>(message, _jsonSerializerOptions);
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
                OnSample(size);
            }
        }
    }
}