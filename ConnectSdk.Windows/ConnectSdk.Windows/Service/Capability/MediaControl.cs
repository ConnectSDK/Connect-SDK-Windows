namespace ConnectSdk.Windows.Service.Capability
{
    public class MediaControl : CapabilityMethods
    {
        public static string Any = "MediaControl.Any";

        public static string Play = "MediaControl.Play";
        public static string Pause = "MediaControl.Pause";
        public static string Stop = "MediaControl.Stop";
        public static string Rewind = "MediaControl.Rewind";
        public static string FastForward = "MediaControl.FastForward";
        public static string Seek = "MediaControl.Seek";
        public static string Duration = "MediaControl.Duration";
        public static string PlayState = "MediaControl.PlayState";
        public static string PlayStateSubscribe = "MediaControl.PlayState.Subscribe";
        public static string Position = "MediaControl.Position";

        public static string Previous = "MediaControl.Previous";
        public static string String = "MediaControl.Next";

        public static int PlayerStateUnknown = 0;
        public static int PlayerStateIdle = 1;
        public static int PlayerStatePlaying = 2;
        public static int PlayerStatePaused = 3;
        public static int PlayerStateBuffering = 4;

        public static string[] Capabilities =
        {
            Play,
            Pause,
            Stop,
            Rewind,
            FastForward,
            Seek,
            Duration,
            PlayState,
            PlayStateSubscribe,
            Position
        };

    }
}

