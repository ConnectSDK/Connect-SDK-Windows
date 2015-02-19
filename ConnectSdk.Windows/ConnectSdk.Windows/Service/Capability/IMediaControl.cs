using ConnectSdk.Windows.Service.Capability.Listeners;
using ConnectSdk.Windows.Service.Command;

namespace ConnectSdk.Windows.Service.Capability
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

        void Next(ResponseListener listener);
        void Previous(ResponseListener listener);
    }
}