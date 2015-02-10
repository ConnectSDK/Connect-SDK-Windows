namespace MyRemote.ConnectSDK.Service.Capability
{
    public class TvControl : CapabilityMethods
    {
        public static string Any = "TVControl.Any";

        public static string ChannelGet = "TVControl.Channel.Get";
        public static string ChannelSet = "TVControl.Channel.Set";
        public static string ChannelUp = "TVControl.Channel.Up";
        public static string ChannelDown = "TVControl.Channel.Down";
        public static string ChannelList = "TVControl.Channel.List";
        public static string ChannelSubscribe = "TVControl.Channel.Subscribe";
        public static string ProgramGet = "TVControl.Program.Get";
        public static string ProgramList = "TVControl.Program.List";
        public static string ProgramSubscribe = "TVControl.Program.Subscribe";
        public static string ProgramListSubscribe = "TVControl.Program.List.Subscribe";
        public static string Get_3D = "TVControl.3D.Get";
        public static string Set_3D = "TVControl.3D.Set";
        public static string Subscribe_3D = "TVControl.3D.Subscribe";

        public static string[] Capabilities =
        {
            ChannelGet,
            ChannelSet,
            ChannelUp,
            ChannelDown,
            ChannelList,
            ChannelSubscribe,
            ProgramGet,
            ProgramList,
            ProgramSubscribe,
            ProgramListSubscribe,
            Get_3D,
            Set_3D,
            Subscribe_3D
        };
    }
}