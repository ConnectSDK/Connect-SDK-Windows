#region Copyright Notice
/*
 * ConnectSdk.Windows
 * TvControl.cs
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
    public class TvControl : CapabilityMethods
    {
        public static string Any = "TVControl.Any";

        public static string ChannelGet = "TVControl.Channel.Get";
        public static string ChannelSet = "TVControl.Channel.Set";
        public static string ChannelUp = "TVControl.Channel.Up";
        public static string ChannelDown = "TVControl.Channel.Down";
        public static string ChannelList = "TVControl.Channel.List";
        public static string ChannelSubscribe = "TVControl.Channel.Subscribe";
        public static string ProgramGet = "TVControl.Program.Get";
        public static string ProgramList = "TVControl.Program.List";
        public static string ProgramSubscribe = "TVControl.Program.Subscribe";
        public static string ProgramListSubscribe = "TVControl.Program.List.Subscribe";
        public static string Get_3D = "TVControl.3D.Get";
        public static string Set_3D = "TVControl.3D.Set";
        public static string Subscribe_3D = "TVControl.3D.Subscribe";

        public static string[] Capabilities =
        {
            ChannelGet,
            ChannelSet,
            ChannelUp,
            ChannelDown,
            ChannelList,
            ChannelSubscribe,
            ProgramGet,
            ProgramList,
            ProgramSubscribe,
            ProgramListSubscribe,
            Get_3D,
            Set_3D,
            Subscribe_3D
        };
    }
}