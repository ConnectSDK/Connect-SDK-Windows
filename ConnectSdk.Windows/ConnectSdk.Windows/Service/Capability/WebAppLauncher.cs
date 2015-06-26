#region Copyright Notice
/*
 * ConnectSdk.Windows
 * WebAppLauncher.cs
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
    public class WebAppLauncher : CapabilityMethods
    {
        public static string Any = "WebAppLauncher.Any";

        public static string Launch = "WebAppLauncher.Launch";
        public static string LaunchParams = "WebAppLauncher.Launch.Params";
        public static string MessageSend = "WebAppLauncher.Message.Send";
        public static string MessageReceive = "WebAppLauncher.Message.Receive";
        public static string MessageSendJson = "WebAppLauncher.Message.Send.JSON";
        public static string MessageReceiveJson = "WebAppLauncher.Message.Receive.JSON";

        public static string Connect = "WebAppLauncher.Connect";
        public static string Disconnect = "WebAppLauncher.Disconnect";
        public static string Join = "WebAppLauncher.Join";
        public static string Close = "WebAppLauncher.Close";
        public static string Pin = "WebAppLauncher.Pin";

        public static string[] Capabilities =
        {
            Launch,
            LaunchParams,
            MessageSend,
            MessageReceive,
            MessageSendJson,
            MessageReceiveJson,
            Connect,
            Disconnect,
            Join,
            Close,
            Pin
        };
    }
}