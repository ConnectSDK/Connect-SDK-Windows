using ConnectSdk.Windows.Service.Capability.Listeners;

namespace ConnectSdk.Windows.Service.Capability
{
    public interface IPlayListControl
    {
        IPlayListControl GetPlaylistControl();
        CapabilityPriorityLevel GetPlaylistControlCapabilityLevel();

        /// <summary>
        /// Jump to previous track in the playlist
        /// </summary>
        /// <param name="listener"></param>
        void Previous(ResponseListener listener);

        /// <summary>
        /// Jump to next track in the playlist
        /// </summary>
        /// <param name="listener"></param>
        void Next(ResponseListener listener);

        /// <summary>
        /// This method is used for playlist only and it allows to switch to another track by it's position
        /// </summary>
        /// <param name="index"></param>
        /// <param name="listener"></param>
        void JumpToTrack(long index, ResponseListener listener);

        /// <summary>
        /// Set order of playing tracks
        /// </summary>
        /// <param name="playMode"></param>
        /// <param name="listener"></param>
        void SetPlayMode(PlayMode playMode, ResponseListener listener);

    }
}