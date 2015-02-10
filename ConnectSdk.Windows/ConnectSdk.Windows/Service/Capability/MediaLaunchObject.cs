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

        public LaunchSession LaunchSession { get; set; }

        public IMediaControl MediaControl { get; set; }
    }
}