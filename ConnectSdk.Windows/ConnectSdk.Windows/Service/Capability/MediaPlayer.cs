#region Copyright Notice
/*
 * ConnectSdk.Windows
 * MediaPlayer.cs
 * 
 * Copyright (c) 2015 LG Electronics.
 * Created by Sorin S. Serban on 20-2-2015,
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
