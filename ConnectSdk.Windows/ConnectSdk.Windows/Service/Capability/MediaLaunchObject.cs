#region Copyright Notice
/*
 * ConnectSdk.Windows
 * medialaunchobject.cs
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
using ConnectSdk.Windows.Service.Sessions;

namespace ConnectSdk.Windows.Service.Capability
{
    /// <summary>
    /// Helper class used with the MediaPlayer.LaunchListener to return the current media playback.
    /// </summary>
    public class MediaLaunchObject
    {
        public MediaLaunchObject(LaunchSession launchSession, IMediaControl mediaControl)
        {
            LaunchSession = launchSession;
            MediaControl = mediaControl;
        }

        public MediaLaunchObject(LaunchSession launchSession, IMediaControl mediaControl, IPlayListControl playlistControl)
        {
            LaunchSession = launchSession;
            MediaControl = mediaControl;
            PlaylistControl = playlistControl;
        }

        public LaunchSession LaunchSession { get; set; }

        public IMediaControl MediaControl { get; set; }

        public IPlayListControl PlaylistControl { get; set; }
    }
}