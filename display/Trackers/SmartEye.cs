using System;

namespace PupilSizeDisplay.Trackers
{
    public class SmartEye : ITracker
    {
        public Eye Eye { get; set; }
        public DataSource Source { get; set; }

        public event EventHandler<DeviceInfo>? DeviceStateChanged;
        public event EventHandler<double>? Sample;

        public SmartEye(string ip, bool useTcp = true)
        {
            _useTcp = useTcp;

            if (_useTcp)
            {
                _tcpClient = new SEClient.Tcp.Client();
                _tcpClient.Sample += (s, e) =>
                {
                    if (Eye == Eye.Left && e.LeftPupilDiameter is not null)
                    {
                        Sample?.Invoke(this, (double)e.LeftPupilDiameter * 1000);
                    }
                    else if (Eye == Eye.Right && e.RightPupilDiameter is not null)
                    {
                        Sample?.Invoke(this, (double)e.RightPupilDiameter * 1000);
                    }
                };
                _tcpClient.Connected += (s, e) => DeviceStateChanged?.Invoke(this, new DeviceInfo() { Name = "Smart Eye Pro", State = DeviceState.Connected });
                _tcpClient.Disconnected += (s, e) => DeviceStateChanged?.Invoke(this, new DeviceInfo() { State = DeviceState.None });

                DispatchOnce.Do(0.1, () =>
                {
                    _tcpClient.Connect(ip, 5002);
                });
            }
            else
            {
                _cmdDataSource = new();
                _cmdDataSource.Data += CmdDataSource_Data;
                _cmdDataSource.Closed += CmdDataSource_Closed;

                _cmdParser = new();
                _cmdParser.Sample += CmdParser_Sample;

                DispatchOnce.Do(0.1, () =>
                {
                    DeviceStateChanged?.Invoke(this, new DeviceInfo() { Name = "Smart Eye Pro", State = DeviceState.Connected });
                    _cmdParser.Reset();
                    _cmdDataSource.Start(ip, 5002.ToString());
                });
            }
        }

        public void Dispose()
        {
            _tcpClient?.Dispose();
            _cmdDataSource?.Stop();

            GC.SuppressFinalize(this);
        }

        // Internal

        readonly bool _useTcp;
        readonly SEClient.Tcp.Client? _tcpClient;
        readonly SEClient.Cmd.DataSource? _cmdDataSource;
        readonly SEClient.Cmd.Parser? _cmdParser;

        private void CmdDataSource_Closed(object? sender, EventArgs e)
        {
            DeviceStateChanged?.Invoke(this, new DeviceInfo() { State = DeviceState.None });
        }

        private void CmdDataSource_Data(object? sender, string e)
        {
            _cmdParser?.Feed(e);
        }

        private void CmdParser_Sample(object? sender, SEClient.Cmd.Sample e)
        {
            Sample?.Invoke(this, e.EyeFeature[(int)Eye].Size.Diameter * 1000);
        }
    }
}
