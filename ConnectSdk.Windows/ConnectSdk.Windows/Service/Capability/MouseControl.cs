namespace MyRemote.ConnectSDK.Service.Capability
{
    public class MouseControl : CapabilityMethods
    {
        public static string Any = "MouseControl.Any";

        public static string Connect = "MouseControl.Connect";
        public static string Disconnect = "MouseControl.Disconnect";
        public static string Click = "MouseControl.Click";
        public static string Move = "MouseControl.Move";
        public static string Scroll = "MouseControl.Scroll";

        public static string[] Capabilities =
        {
            Connect,
            Disconnect,
            Click,
            Move,
            Scroll
        };
    }
}