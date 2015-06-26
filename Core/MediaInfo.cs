#region Copyright Notice
/*
 * ConnectSdk.Windows
 * MediaInfo.cs
 * 
 * Copyright (c) 2015, https://github.com/sdaemon
 * Created by Sorin S. Serban on 22-4-2015,
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
        /// <summary>
        /// Gets or sets the URL address of a media file.
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// Gets or sets the type of a media file.
        /// </summary>
        public string MimeType { get; set; }

        /// <summary>
        /// Gets or sets the description for a media.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the title for a media file.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the duration of a media file.
        /// </summary>
        public long Duration { get; set; }

        /// <summary>
        /// Gets or sets the list of ImageInfo objects for images representing a media (ex. icon, poster). Where first ([0]) is icon image, and second ([1]) is poster image
        /// </summary>
        public List<ImageInfo> AllImages { get; set; }

        /// <summary>
        /// Default constructor method.
        /// </summary>
        /// <param name="url"></param>
        /// <param name="mimeType"></param>
        /// <param name="title"></param>
        /// <param name="description"></param>
        public MediaInfo(String url, String mimeType, String title, String description)
        {
            Url = url;
            MimeType = mimeType;
            Title = title;
            Description = description;
        }

        /// <summary>
        /// Default constructor method.
        /// </summary>
        /// <param name="url"></param>
        /// <param name="mimeType"></param>
        /// <param name="title"></param>
        /// <param name="description"></param>
        /// <param name="allImages"> list of imageInfo objects where [0] is icon, [1] is poster</param>
        public MediaInfo(String url, String mimeType, String title, String description, List<ImageInfo> allImages)
        {
            Url = url;
            MimeType = mimeType;
            Title = title;
            Description = description;
            AllImages = allImages;
        }

        /// <summary>
        /// Stores ImageInfo objects.
        /// </summary>
        /// <param name="images"></param>
        public void AddImages(List<ImageInfo> images) 
        {
            AllImages.AddRange(images);
        }
    }
}