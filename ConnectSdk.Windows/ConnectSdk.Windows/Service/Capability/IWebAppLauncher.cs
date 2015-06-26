#region Copyright Notice
/*
 * ConnectSdk.Windows
 * IWebAppLauncher.cs
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
using Windows.Data.Json;
using ConnectSdk.Windows.Service.Capability.Listeners;
using ConnectSdk.Windows.Service.Command;
using ConnectSdk.Windows.Service.Sessions;

namespace ConnectSdk.Windows.Service.Capability
{
    public interface IWebAppLauncher : ICapabilityMethod
    {
        IWebAppLauncher GetWebAppLauncher();
        CapabilityPriorityLevel GetWebAppLauncherCapabilityLevel();

        void LaunchWebApp(string webAppId, ResponseListener listener);
        void LaunchWebApp(string webAppId, bool relaunchIfRunning, ResponseListener listener);
        void LaunchWebApp(string webAppId, JsonObject ps, ResponseListener listener);
        void LaunchWebApp(string webAppId, JsonObject ps, bool relaunchIfRunning, ResponseListener listener);

        void JoinWebApp(LaunchSession webAppLaunchSession, ResponseListener listener);
        void JoinWebApp(string webAppId, ResponseListener listener);
        void CloseWebApp(LaunchSession launchSession, ResponseListener listener);

        void PinWebApp(string webAppId, ResponseListener listener);
        void UnPinWebApp(string webAppId, ResponseListener listener);
        void IsWebAppPinned(string webAppId, ResponseListener listener);
        IServiceSubscription SubscribeIsWebAppPinned(string webAppId, ResponseListener listener);

    }
}