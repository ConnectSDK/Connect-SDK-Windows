using MyRemote.ConnectSDK.Service.Capability.Listeners;
using MyRemote.ConnectSDK.Service.Sessions;

namespace MyRemote.ConnectSDK.Service.Capability
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