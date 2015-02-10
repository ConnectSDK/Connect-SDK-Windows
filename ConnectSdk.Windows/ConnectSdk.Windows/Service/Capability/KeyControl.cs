namespace ConnectSdk.Windows.Service.Capability
{
    public class KeyControl : CapabilityMethods
    {
        public static string Any = "KeyControl.Any";

        public static string Up = "KeyControl.Up";
        public static string Down = "KeyControl.Down";
        public static string Left = "KeyControl.Left";
        public static string Right = "KeyControl.Right";
        public static string Ok = "KeyControl.OK";
        public static string Back = "KeyControl.Back";
        public static string Home = "KeyControl.Home";
        public static string SendKey = "KeyControl.SendKey";

        public static string[] Capabilities =
        {
            Up,
            Down,
            Left,
            Right,
            Ok,
            Back,
            Home
        };
    }
}




