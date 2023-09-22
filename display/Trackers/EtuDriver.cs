using System.Text.Json;

namespace PupilSizeDisplay.Trackers;

public class EtuDriver : TrackerOverWs
{
    #region Data types transferred over the net

    public record class Message
    {
        public string Type { get; init; }
        public static Message? Create(string json)
        {
            Message? msg = JsonSerializer.Deserialize<Message>(json, _jsonSerializerOptions);
            if (msg == null)
                return null;
            else if (msg.Type == "device")
                return JsonSerializer.Deserialize<DeviceMessage>(json!, _jsonSerializerOptions);
            else if (msg.Type == "state")
                return JsonSerializer.Deserialize<StateMessage>(json!, _jsonSerializerOptions);
            else if (msg.Type == "sample")
                return JsonSerializer.Deserialize<SampleMessage>(json!, _jsonSerializerOptions);
            else
            {
                System.Diagnostics.Debug.WriteLine($"Unknown message: {json}");
                return null;
            }
        }
        public Message(string type)
        {
            Type = type;
        }

        // Internal

        private static readonly JsonSerializerOptions _jsonSerializerOptions = new() { PropertyNameCaseInsensitive = true };
    }

    public record class DeviceMessage : Message
    {
        public string Name { get; init; }

        public DeviceMessage(string type, string name) : base(type)
        {
            Name = name;
        }
    }

    public record class StateMessage : Message
    {
        public DeviceState Value { get; init; }

        public StateMessage(string type, DeviceState value) : base(type)
        {
            Value = value;
        }
    }

    public record class EyeCoordinates(double XL, double YL, double XR, double YR);

    public record class SampleMessage : Message
    {
        public long Ts { get; init; }
        public long X { get; init; }
        public long Y { get; init; }
        public double P { get; init; }
        public EyeCoordinates EC { get; init; }

        public SampleMessage(string type, long ts, long x, long y, double p, EyeCoordinates ec) : base(type)
        {
            Ts = ts;
            X = x;
            Y = y;
            P = p;
            EC = ec;
        }
    }

    #endregion

    public EtuDriver(string ip) : base(ip) { }

    // Internal

    protected override string Name => _deviceName;
    protected override int Port => 8086;

    private string _deviceName = "unknown";

    protected override void ParseMessage(string message)
    {
        var msgObj = Message.Create(message);
        if (msgObj is SampleMessage sample)
        {
            var size = Source switch
            {
                DataSource.Diameter => sample.P,
                _ => 0
            };
            OnSample(size);
        }
        else if (msgObj is DeviceMessage device)
        {
            _deviceName = device.Name;
            OnDeviceStateChanged(new DeviceInfo() { Name = _deviceName });
        }
        else if (msgObj is StateMessage state)
        {
            OnDeviceStateChanged(new DeviceInfo() { State = state.Value });
        }
    }
}