#region Copyright Notice
/*
 * ConnectSdk.Windows
 * MouseControl.cs
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
    public class MouseControl : CapabilityMethods
    {
        public static string Any = "MouseControl.Any";

        public static string Connect = "MouseControl.Connect";
        public static string Disconnect = "MouseControl.Disconnect";
        public static string Click = "MouseControl.Click";
        public static string Move = "MouseControl.Move";
        public static string Scroll = "MouseControl.Scroll";

        public static string[] Capabilities =
        {
            Connect,
            Disconnect,
            Click,
            Move,
            Scroll
        };
    }
}