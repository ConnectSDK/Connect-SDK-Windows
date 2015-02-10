namespace ConnectSdk.Windows.Service.Capability
{
    public class VolumeControl : CapabilityMethods
    {
        public static string Any = "VolumeControl.Any";

        public static string VolumeGet = "VolumeControl.Get";
        public static string VolumeSet = "VolumeControl.Set";
        public static string VolumeUpDown = "VolumeControl.UpDown";
        public static string VolumeSubscribe = "VolumeControl.Subscribe";
        public static string MuteGet = "VolumeControl.Mute.Get";
        public static string MuteSet = "VolumeControl.Mute.Set";
        public static string MuteSubscribe = "VolumeControl.Mute.Subscribe";

        public static string[] Capabilities =
        {
            VolumeGet,
            VolumeSet,
            VolumeUpDown,
            VolumeSubscribe,
            MuteGet,
            MuteSet,
            MuteSubscribe
        };
    }
}