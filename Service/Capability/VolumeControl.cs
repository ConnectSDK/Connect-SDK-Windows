#region Copyright Notice
/*
 * ConnectSdk.Windows
 * VolumeControl.cs
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
    public class VolumeControl : CapabilityMethods
    {
        public static string Any = "VolumeControl.Any";

        public static string VolumeGet = "VolumeControl.Get";
        public static string VolumeSet = "VolumeControl.Set";
        public static string VolumeUpDown = "VolumeControl.UpDown";
        public static string VolumeSubscribe = "VolumeControl.Subscribe";
        public static string MuteGet = "VolumeControl.Mute.Get";
        public static string MuteSet = "VolumeControl.Mute.Set";
        public static string MuteSubscribe = "VolumeControl.Mute.Subscribe";

        public static string[] Capabilities =
        {
            VolumeGet,
            VolumeSet,
            VolumeUpDown,
            VolumeSubscribe,
            MuteGet,
            MuteSet,
            MuteSubscribe
        };
    }
}