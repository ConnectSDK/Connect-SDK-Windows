using ConnectSdk.Windows.Service.Capability.Listeners;
using ConnectSdk.Windows.Service.Command;

namespace ConnectSdk.Windows.Service.Capability
{
    public interface IMediaControl
    {
        IMediaControl GetMediaControl();
        CapabilityPriorityLevel GetMediaControlCapabilityLevel();

        void Play(ResponseListener<object> listener);
        void Pause(ResponseListener<object> listener);
        void Stop(ResponseListener<object> listener);
        void Rewind(ResponseListener<object> listener);
        void FastForward(ResponseListener<object> listener);

        void Seek(long position, ResponseListener<object> listener);
        void GetDuration(ResponseListener<long> listener);
        void GetPosition(ResponseListener<long> listener);

        void GetPlayState(ResponseListener<PlayStateStatus> listener);
        IServiceSubscription<PlayStateStatus> SubscribePlayState(ResponseListener<PlayStateStatus> listener);

        void Next(ResponseListener<object> listener);
        void Previous(ResponseListener<object> listener);
    }
}