using System;
using System.Collections.Generic;

namespace ConnectSdk.Windows.Core
{
    public class MediaInfo 
    {
        public string Url { get; set; }
        public string MimeType { get; set; }
        public string Description { get; set; }
        public string Title { get; set; }
        public long Duration { get; set; }
        public List<ImageInfo> AllImages { get; set; }

        public MediaInfo(String url, String mimeType, String title, String description)
        {
            Url = url;
            MimeType = mimeType;
            Title = title;
            Description = description;
        }

        public MediaInfo(String url, String mimeType, String title, String description, List<ImageInfo> allImages)
        {
            Url = url;
            MimeType = mimeType;
            Title = title;
            Description = description;
            AllImages = allImages;
        }

        public void AddImages(List<ImageInfo> images) 
        {
            AllImages.AddRange(images);
        }
    }
}