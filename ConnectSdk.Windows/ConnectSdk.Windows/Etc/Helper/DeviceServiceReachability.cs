using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace ConnectSdk.Windows.Etc.Helper
{
    public class DeviceServiceReachability
    {
        private const int Timeout = 10000;
        private Task testThread;
        public string IpAddress { get; set; }
        public IDeviceServiceReachabilityListener Listener { get; set; }

        public DeviceServiceReachability(CancellationTokenSource cancelationTokenParam)
        {
            cancelationToken = cancelationTokenParam;
        }

        public DeviceServiceReachability(string ipAddress, CancellationTokenSource cancelationTokenParam)
        {
            IpAddress = ipAddress;
            cancelationToken = cancelationTokenParam;
        }

        public DeviceServiceReachability(string ipAddress, IDeviceServiceReachabilityListener listener, CancellationTokenSource cancelationTokenParam)
        {
            IpAddress = ipAddress;
            Listener = listener;
            cancelationToken = cancelationTokenParam;
        }


        public static DeviceServiceReachability GetReachability(string ipAddress, IDeviceServiceReachabilityListener listener)
        {
            return new DeviceServiceReachability(ipAddress, listener, cancelationToken);
        }


        public bool IsRunning()
        {
            return testThread != null && testThread.Status == TaskStatus.Running;
        }

        private static CancellationTokenSource cancelationToken;

        public void Start()
        {
            if (IsRunning())
                return;

            Task.Factory.StartNew(delegate
            {
                try
                {
                    while (true)
                    {
                        //todo: get a ping implementation from somewhere
                        //if (!ipAddress.IsReachable(TIMEOUT))
                        //    Unreachable();
                        testThread.Wait(Timeout);
                    }
                }
                catch (IOException)
                {
                    Unreachable();
                }
                catch
                {
                }
            }, cancelationToken);

        }

        public void Stop()
        {
            if (!IsRunning())
                return;
            cancelationToken.Cancel();
            testThread = null;
        }

        private void Unreachable()
        {
            Stop();

            if (Listener != null)
                Listener.OnLoseReachability(this);
        }
    }
}
