namespace PupilSizeDisplay.Types;

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
