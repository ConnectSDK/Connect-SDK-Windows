using ConnectSdk.Windows.Service.Capability.Listeners;
using ConnectSdk.Windows.Service.Sessions;

namespace ConnectSdk.Windows.Service.Capability
{
    public interface IMediaPlayer
    {
        IMediaPlayer GetMediaPlayer();
        CapabilityPriorityLevel GetMediaPlayerCapabilityLevel();

        void DisplayImage(string url, string mimeType, string title, string description, string iconSrc,
            ResponseListener<MediaLaunchObject> listener);

        void PlayMedia(string url, string mimeType, string title, string description, string iconSrc, bool shouldLoop,
            ResponseListener<MediaLaunchObject> listener);

        void CloseMedia<T>(LaunchSession<T> launchSession, ResponseListener<object> listener) where T: ResponseListener<object>;
    }
}