using ConnectSdk.Windows.Service.Capability.Listeners;
using ConnectSdk.Windows.Service.Command;

namespace ConnectSdk.Windows.Service.Capability
{
    public interface IVolumeControl
    {
        IVolumeControl GetVolumeControl();
        CapabilityPriorityLevel GetVolumeControlCapabilityLevel();

        void VolumeUp(ResponseListener<object> listener);
        void VolumeDown(ResponseListener<object> listener);

        void SetVolume(float volume, ResponseListener<object> listener);
        void GetVolume(ResponseListener<float> listener);

        void SetMute(bool isMute, ResponseListener<object> listener);
        void GetMute(ResponseListener<bool> listener);

        IServiceSubscription<VolumeStatus> SubscribeVolume(ResponseListener<VolumeStatus> listener);
        IServiceSubscription<bool> SubscribeMute(ResponseListener<bool> listener);
    }
}