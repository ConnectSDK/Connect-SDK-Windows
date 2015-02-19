namespace ConnectSdk.Windows.Service.Capability
{
    public class MediaPlayer : CapabilityMethods
    {
        public static string Any = "MediaPlayer.Any";

        public static string DisplayImage = "MediaPlayer.Display.Image";
        public static string PlayVideo = "MediaPlayer.Play.Video";
        public static string PlayAudio = "MediaPlayer.Play.Audio";
        public static string PlayPlaylist = "MediaPlayer.Play.Playlist";
        public static string Close = "MediaPlayer.Close";
        public static string MetaDataTitle = "MediaControl.MetaData.Title";
        public static string MetaDataDescription = "MediaControl.MetaData.Description";
        public static string MetaDataThumbnail = "MediaControl.MetaData.Thumbnail";
        public static string MetaDataMimeType = "MediaControl.MetaData.MimeType";


        public static string MediaInfoGet = "MediaPlayer.MediaInfo.Get";
        public static string MediaInfoSubscribe = "MediaPlayer.MediaInfo.Subscribe";

        public static string[] Capabilities =
        {
            DisplayImage,
            PlayVideo,
            PlayAudio,
            Close,
            MetaDataTitle,
            MetaDataDescription,
            MetaDataThumbnail,
            MetaDataMimeType,
            MediaInfoGet,
            MediaInfoSubscribe
        };
    }
}
