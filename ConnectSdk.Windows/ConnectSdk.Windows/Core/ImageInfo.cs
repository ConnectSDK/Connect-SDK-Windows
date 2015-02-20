using System;

namespace ConnectSdk.Windows.Core
{
    public class ImageInfo
    {
        public string Url { get; set; }
        public ImageType Type { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }

        public ImageInfo(String url)
        {
            Url = url;
        }

        public ImageInfo(String url, ImageType type, int width, int height)
        {
            Url = url;
            Type = type;
            Width = width;
            Height = height;
        }


        public enum ImageType
        {
            Thumb,
            VideoPoster,
            AlbumArt,
            Unknown
        }
    }
}