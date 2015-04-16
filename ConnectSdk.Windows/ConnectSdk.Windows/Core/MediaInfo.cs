#region Copyright Notice
/*
 * ConnectSdk.Windows
 * MediaInfo.cs
 * 
 * Copyright (c) 2015 LG Electronics.
 * Created by Sorin S. Serban on 16-4-2015,
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
 #endregion
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