namespace ConnectSdk.Windows.Service.Capability
{
    public class ExternalInputControl : CapabilityMethods
    {
        public static string Any = "ExternalInputControl.Any";

        public static string PickerLaunch = "ExternalInputControl.Picker.Launch";
        public static string PickerClose = "ExternalInputControl.Picker.Close";
        public static string List = "ExternalInputControl.List";
        public static string Set = "ExternalInputControl.Set";

        public static string[] Capabilities =
        {
            PickerLaunch,
            PickerClose,
            List,
            Set
        };
    }
}