using System;

namespace PupilSizeDisplay.Trackers
{
    public class SmartEye : ITracker
    {
        public Eye Eye { get; set; }
        public DataSource Source { get; set; }

        public event EventHandler<DeviceInfo>? DeviceStateChanged;
        public event EventHandler<double>? Sample;

        public SmartEye(string ip)
        {
            _dataSource.Data += DataSource_Data;
            _dataSource.Closed += DataSource_Closed;

            _parser.Sample += Parser_Sample;

            DispatchOnce.Do(0.1, () => {
                DeviceStateChanged?.Invoke(this, new DeviceInfo() { Name = "Smart Eye Pro", State = DeviceState.Connected });
                _parser.Reset();
                _dataSource.Start(ip, 5002.ToString());
            });
        }

        public void Dispose()
        {
            _dataSource.Stop();
            GC.SuppressFinalize(this);
        }

        // Internal

        readonly SEClient.DataSource _dataSource = new();
        readonly SEClient.Parser _parser = new();

        private void DataSource_Closed(object? sender, EventArgs e)
        {
            DeviceStateChanged?.Invoke(this, new DeviceInfo() { State = DeviceState.None });
        }

        private void DataSource_Data(object? sender, string e)
        {
            _parser.Feed(e);
        }

        private void Parser_Sample(object? sender, SEClient.Sample e)
        {
            Sample?.Invoke(this, e.EyeFeature[(int)Eye].Size.Diameter * 1000);
        }
    }
}
