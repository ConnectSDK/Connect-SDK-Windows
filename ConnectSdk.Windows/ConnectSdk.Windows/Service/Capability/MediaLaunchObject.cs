using ConnectSdk.Windows.Service.Sessions;

namespace ConnectSdk.Windows.Service.Capability
{
    /// <summary>
    /// Helper class used with the MediaPlayer.LaunchListener to return the current media playback.
    /// </summary>
    public class MediaLaunchObject
    {
        public MediaLaunchObject(LaunchSession launchSession, IMediaControl mediaControl)
        {
            LaunchSession = launchSession;
            MediaControl = mediaControl;
        }

        public MediaLaunchObject(LaunchSession launchSession, IMediaControl mediaControl, IPlayListControl playlistControl)
        {
            LaunchSession = launchSession;
            MediaControl = mediaControl;
            PlaylistControl = playlistControl;
        }

        public LaunchSession LaunchSession { get; set; }

        public IMediaControl MediaControl { get; set; }

        public IPlayListControl PlaylistControl { get; set; }
    }
}