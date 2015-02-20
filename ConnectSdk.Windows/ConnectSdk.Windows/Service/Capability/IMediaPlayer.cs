using ConnectSdk.Windows.Service.Capability.Listeners;
using ConnectSdk.Windows.Service.Sessions;

namespace ConnectSdk.Windows.Service.Capability
{
    public interface IMediaPlayer
    {
        IMediaPlayer GetMediaPlayer();
        CapabilityPriorityLevel GetMediaPlayerCapabilityLevel();

        void DisplayImage(string url, string mimeType, string title, string description, string iconSrc,
            ResponseListener listener);

        void PlayMedia(string url, string mimeType, string title, string description, string iconSrc, bool shouldLoop,
            ResponseListener listener);

        void CloseMedia(LaunchSession launchSession, ResponseListener listener);
    }
}