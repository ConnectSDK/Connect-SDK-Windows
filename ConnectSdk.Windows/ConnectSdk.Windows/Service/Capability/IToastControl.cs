#region Copyright Notice
/*
 * ConnectSdk.Windows
 * IToastControl.cs
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
using Windows.Data.Json;
using ConnectSdk.Windows.Core;
using ConnectSdk.Windows.Service.Capability.Listeners;

namespace ConnectSdk.Windows.Service.Capability
{
    public interface IToastControl
    {
        IToastControl GetToastControl();
        CapabilityPriorityLevel GetToastControlCapabilityLevel();

        void ShowToast(string message, ResponseListener listener);
        void ShowToast(string message, string iconData, string iconExtension, ResponseListener listener);

        void ShowClickableToastForApp(string message, AppInfo appInfo, JsonObject ps, ResponseListener listener);

        void ShowClickableToastForApp(string message, AppInfo appInfo, JsonObject ps, string iconData,string iconExtension, ResponseListener listener);

        void ShowClickableToastForUrl(string message, string url, ResponseListener listener);

        void ShowClickableToastForUrl(string message, string url, string iconData, string iconExtension, ResponseListener listener);
    }
}