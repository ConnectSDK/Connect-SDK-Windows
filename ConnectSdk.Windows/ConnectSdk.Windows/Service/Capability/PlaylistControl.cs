namespace ConnectSdk.Windows.Service.Capability
{
    public class PlaylistControl : CapabilityMethods
    {
        public static string Any = "PlaylistControl.Any";
        public static string JumpToTrack = "PlaylistControl.JumpToTrack";
        public static string SetPlayMode = "PlaylistControl.SetPlayMode";
        public static string Previous = "PlaylistControl.Previous";
        public static string Next = "PlaylistControl.Next";

        public static string[] Capabilities =
        {
            Previous,
            Next,
            JumpToTrack,
            SetPlayMode,
            JumpToTrack
        };

    }
}