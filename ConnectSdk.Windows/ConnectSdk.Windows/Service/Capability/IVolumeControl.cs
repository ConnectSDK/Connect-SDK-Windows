using MyRemote.ConnectSDK.Service.Capability.Listeners;
using MyRemote.ConnectSDK.Service.Command;

namespace MyRemote.ConnectSDK.Service.Capability
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