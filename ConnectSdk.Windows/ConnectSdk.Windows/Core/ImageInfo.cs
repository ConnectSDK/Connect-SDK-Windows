#region Copyright Notice
/*
 * ConnectSdk.Windows
 * ImageInfo.cs
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

namespace ConnectSdk.Windows.Core
{
    /// <summary>
    /// Normalized reference object for information about an image file. This object can be used to represent a media file (ex. icon, poster)
    /// </summary>
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