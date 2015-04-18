#region Copyright Notice
/*
 * ConnectSdk.Windows
 * KeyControl.cs
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
    public class KeyControl : CapabilityMethods
    {
        public static string Any = "KeyControl.Any";

        public static string Up = "KeyControl.Up";
        public static string Down = "KeyControl.Down";
        public static string Left = "KeyControl.Left";
        public static string Right = "KeyControl.Right";
        public static string Ok = "KeyControl.OK";
        public static string Back = "KeyControl.Back";
        public static string Home = "KeyControl.Home";
        public static string SendKey = "KeyControl.SendKey";

        public static string[] Capabilities =
        {
            Up,
            Down,
            Left,
            Right,
            Ok,
            Back,
            Home
        };
    }
}




