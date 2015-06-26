#region Copyright Notice
/*
 * ConnectSdk.Windows
 * Launcher.cs
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
    public class Launcher : CapabilityMethods
    {
        public static string Any = "Launcher.Any";

        public static string Application = "Launcher.Application";
        public static string ApplicationParams = "Launcher.Application.Params";
        public static string ApplicationClose = "Launcher.Application.Close";
        public static string ApplicationList = "Launcher.Application.List";
        public static string Browser = "Launcher.Browser";
        public static string BrowserParams = "Launcher.Browser.Params";
        public static string Hulu = "Launcher.Hulu";
        public static string HuluParams = "Launcher.Hulu.Params";
        public static string Netflix = "Launcher.Netflix";
        public static string NetflixParams = "Launcher.Netflix.Params";
        public static string YouTube = "Launcher.YouTube";
        public static string YouTubeParams = "Launcher.YouTube.Params";
        public static string AppStore = "Launcher.AppStore";
        public static string AppStoreParams = "Launcher.AppStore.Params";
        public static string AppState = "Launcher.AppState";
        public static string AppStateSubscribe = "Launcher.AppState.Subscribe";
        public static string RunningApp = "Launcher.RunningApp";
        public static string RunningAppSubscribe = "Launcher.RunningApp.Subscribe";

        public static string[] Capabilities =
        {
            Application,
            ApplicationParams,
            ApplicationClose,
            ApplicationList,
            Browser,
            BrowserParams,
            Hulu,
            HuluParams,
            Netflix,
            NetflixParams,
            YouTube,
            YouTubeParams,
            AppStore,
            AppStoreParams,
            AppState,
            AppStateSubscribe,
            RunningApp,
            RunningAppSubscribe
        };
    }
}