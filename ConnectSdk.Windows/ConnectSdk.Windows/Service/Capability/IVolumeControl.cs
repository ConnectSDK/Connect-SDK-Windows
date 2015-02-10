using ConnectSdk.Windows.Service.Capability.Listeners;
using ConnectSdk.Windows.Service.Command;

namespace ConnectSdk.Windows.Service.Capability
{
    public interface IVolumeControl
    {
        IVolumeControl GetVolumeControl();
        CapabilityPriorityLevel GetVolumeControlCapabilityLevel();

        void VolumeUp(ResponseListener listener);
        void VolumeDown(ResponseListener listener);

        void SetVolume(float volume, ResponseListener listener);
        void GetVolume(ResponseListener listener);

        void SetMute(bool isMute, ResponseListener listener);
        void GetMute(ResponseListener listener);

        IServiceSubscription SubscribeVolume(ResponseListener listener);
        IServiceSubscription SubscribeMute(ResponseListener listener);
    }
}