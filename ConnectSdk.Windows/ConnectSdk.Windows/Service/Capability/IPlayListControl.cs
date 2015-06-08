#region Copyright Notice
/*
 * ConnectSdk.Windows
 * IPlayListControl.cs
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
using ConnectSdk.Windows.Service.Capability.Listeners;

namespace ConnectSdk.Windows.Service.Capability
{
    public interface IPlayListControl : ICapabilityMethod
    {
        IPlayListControl GetPlaylistControl();
        CapabilityPriorityLevel GetPlaylistControlCapabilityLevel();

        /// <summary>
        /// Jump to previous track in the playlist
        /// </summary>
        /// <param name="listener"></param>
        void Previous(ResponseListener listener);

        /// <summary>
        /// Jump to next track in the playlist
        /// </summary>
        /// <param name="listener"></param>
        void Next(ResponseListener listener);

        /// <summary>
        /// This method is used for playlist only and it allows to switch to another track by it's position
        /// </summary>
        /// <param name="index"></param>
        /// <param name="listener"></param>
        void JumpToTrack(long index, ResponseListener listener);

        /// <summary>
        /// Set order of playing tracks
        /// </summary>
        /// <param name="playMode"></param>
        /// <param name="listener"></param>
        void SetPlayMode(PlayMode playMode, ResponseListener listener);

    }
}