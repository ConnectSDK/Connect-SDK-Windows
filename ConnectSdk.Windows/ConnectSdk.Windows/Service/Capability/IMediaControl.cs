using MyRemote.ConnectSDK.Service.Capability.Listeners;
using MyRemote.ConnectSDK.Service.Command;

namespace MyRemote.ConnectSDK.Service.Capability
{
    public interface IMediaControl
    {
        IMediaControl GetMediaControl();
        CapabilityPriorityLevel GetMediaControlCapabilityLevel();

        void Play(ResponseListener listener);
        void Pause(ResponseListener listener);
        void Stop(ResponseListener listener);
        void Rewind(ResponseListener listener);
        void FastForward(ResponseListener listener);

        void Seek(long position, ResponseListener listener);
        void GetDuration(ResponseListener listener);
        void GetPosition(ResponseListener listener);

        void GetPlayState(ResponseListener listener);
        IServiceSubscription SubscribePlayState(ResponseListener listener);
    }
}