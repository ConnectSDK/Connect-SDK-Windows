#region Copyright Notice
/*
 * ConnectSdk.Windows
 * ILauncher.cs
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
using System;
using ConnectSdk.Windows.Core;
using ConnectSdk.Windows.Service.Capability.Listeners;
using ConnectSdk.Windows.Service.Command;
using ConnectSdk.Windows.Service.Sessions;

namespace ConnectSdk.Windows.Service.Capability
{
    public interface ILauncher
    {
        ILauncher GetLauncher();
        CapabilityPriorityLevel GetLauncherCapabilityLevel();

        void LaunchAppWithInfo(AppInfo appInfo, ResponseListener listener);
        void LaunchAppWithInfo(AppInfo appInfo, Object ps, ResponseListener listener);
        void LaunchApp(string appId, ResponseListener listener);

        void CloseApp(LaunchSession launchSession, ResponseListener listener);

        void GetAppList(ResponseListener listener);

        void GetRunningApp(ResponseListener listener);
        IServiceSubscription SubscribeRunningApp(ResponseListener listener);

        void GetAppState(LaunchSession launchSession, ResponseListener listener);
        IServiceSubscription SubscribeAppState(LaunchSession launchSession, ResponseListener listener);

        void LaunchBrowser(string url, ResponseListener listener);
        void LaunchYouTube(string contentId, ResponseListener listener);
        void LaunchNetflix(string contentId, ResponseListener listener);
        void LaunchHulu(string contentId, ResponseListener listener);
        void LaunchAppStore(string appId, ResponseListener listener);
    }
}