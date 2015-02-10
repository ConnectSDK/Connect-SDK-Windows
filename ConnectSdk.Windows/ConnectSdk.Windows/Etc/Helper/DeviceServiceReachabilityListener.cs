namespace ConnectSdk.Windows.Etc.Helper
{
    public interface IDeviceServiceReachabilityListener
    {
        void OnLoseReachability(DeviceServiceReachability reachability);
    }
}