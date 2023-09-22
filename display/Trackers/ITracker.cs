using System;

namespace PupilSizeDisplay.Trackers;

public enum Eye : int
{
    Left = 0,
    Right = 1,
}

public enum DataSource { Diameter, Area }


[Flags]
public enum DeviceState
{
    None = 0,
    Connected = 1,
    Calibrated = 2,
    Tracking = 4,
    Blocked = 8,     // for example, it's software is showing a dialog
}

public struct DeviceInfo
{
    public string? Name;
    public DeviceState? State;
}

public interface ITracker : IDisposable
{
    public event EventHandler<DeviceInfo>? DeviceStateChanged;
    public event EventHandler<double>? Sample;
    public Eye Eye { get; set; }
    public DataSource Source { get; set; }

}
