namespace ConnectSdk.Windows.Service.Capability
{
    public class TextInputControl : CapabilityMethods
    {
        public static string Any = "TextInputControl.Any";

        public static string Send = "TextInputControl.Send";
        public static string SendEnter = "TextInputControl.Enter";
        public static string SendDelete = "TextInputControl.Delete";
        public static string Subscribe = "TextInputControl.Subscribe";

        public static string[] Capabilities =
        {
            Send,
            SendEnter,
            SendDelete,
            Subscribe
        };
    }
}