using System;

namespace PupilSizeDisplay.Trackers;

public enum Eye : int
{
    Left = 0,
    Right = 1,
}

public enum DataSource { Diameter, Area }

public interface ITracker
{
    public event EventHandler Connected;
    public event EventHandler Disconnected;
    public event EventHandler<double> Sample;
    public Eye Eye { get; set; }
    public DataSource Source { get; set; }
}
