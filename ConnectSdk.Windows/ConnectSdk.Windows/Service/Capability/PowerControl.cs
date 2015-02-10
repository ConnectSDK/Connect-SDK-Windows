namespace MyRemote.ConnectSDK.Service.Capability
{
    public class PowerControl : CapabilityMethods
    {
        public static string Any = "PowerControl.Any";

        public static string Off = "PowerControl.Off";
        public static string On = "PowerControl.On";

        public static string[] Capabilities =
        {
            Off,
            On
        };
    }
}