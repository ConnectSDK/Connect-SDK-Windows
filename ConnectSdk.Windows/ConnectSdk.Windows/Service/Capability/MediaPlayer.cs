namespace ConnectSdk.Windows.Service.Capability
{
    public class MediaPlayer : CapabilityMethods
    {
        public static string Any = "MediaPlayer.Any";

        public static string DisplayImage = "MediaPlayer.Display.Image";
        public static string DisplayVideo = "MediaPlayer.Display.Video";
        public static string DisplayAudio = "MediaPlayer.Display.Audio";
        public static string Close = "MediaPlayer.Close";
        public static string MetaDataTitle = "MediaControl.MetaData.Title";
        public static string MetaDataDescription = "MediaControl.MetaData.Description";
        public static string MetaDataThumbnail = "MediaControl.MetaData.Thumbnail";
        public static string MetaDataMimeType = "MediaControl.MetaData.MimeType";

        public static string[] Capabilities =
        {
            DisplayImage,
            DisplayVideo,
            DisplayAudio,
            Close,
            MetaDataTitle,
            MetaDataDescription,
            MetaDataThumbnail,
            MetaDataMimeType
        };
    }

 


}
