#region Copyright Notice
/*
 * ConnectSdk.Windows
 * MediaControl.cs
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
namespace ConnectSdk.Windows.Service.Capability
{
    public class MediaControl : CapabilityMethods
    {
        public static string Any = "MediaControl.Any";

        public static string Play = "MediaControl.Play";
        public static string Pause = "MediaControl.Pause";
        public static string Stop = "MediaControl.Stop";
        public static string Rewind = "MediaControl.Rewind";
        public static string FastForward = "MediaControl.FastForward";
        public static string Seek = "MediaControl.Seek";
        public static string Duration = "MediaControl.Duration";
        public static string PlayState = "MediaControl.PlayState";
        public static string PlayStateSubscribe = "MediaControl.PlayState.Subscribe";
        public static string Position = "MediaControl.Position";

        public static string Previous = "MediaControl.Previous";
        public static string Next = "MediaControl.Next";

        public static int PlayerStateUnknown = 0;
        public static int PlayerStateIdle = 1;
        public static int PlayerStatePlaying = 2;
        public static int PlayerStatePaused = 3;
        public static int PlayerStateBuffering = 4;

        public static string[] Capabilities =
        {
            Play,
            Pause,
            Stop,
            Rewind,
            FastForward,
            Seek,
            Duration,
            PlayState,
            PlayStateSubscribe,
            Position
        };

    }
}

