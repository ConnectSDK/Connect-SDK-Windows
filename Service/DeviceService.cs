#region Copyright Notice
/*
 * ConnectSdk.Windows
 * deviceservice.cs
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
using System;
using System.Collections.Generic;
using System.Linq;
using Windows.Data.Json;
using ConnectSdk.Windows.Core;
using ConnectSdk.Windows.Discovery;
using ConnectSdk.Windows.Etc.Helper;
using ConnectSdk.Windows.Service.Capability;
using ConnectSdk.Windows.Service.Capability.Listeners;
using ConnectSdk.Windows.Service.Command;
using ConnectSdk.Windows.Service.Config;
using ConnectSdk.Windows.Service.Sessions;

namespace ConnectSdk.Windows.Service
{
    /// <summary>
    /// Overview
    /// From a high-level perspective, DeviceService completely abstracts the functionality of a particular service/protocol (webOS TV, Netcast TV, Chromecast, Roku, DIAL, etc).
    /// 
    /// In Depth
    /// DeviceService is an abstract class that is meant to be extended. You shouldn't ever use DeviceService directly, unless extending it to provide support for an additional service/protocol.
    /// Immediately after discovery of a DeviceService, DiscoveryManager will set the DeviceService's Listener to the ConnectableDevice that owns the DeviceService. You should not change the Listener unless you intend to manage the lifecycle of that service. The DeviceService will proxy all of its Listener method calls through the ConnectableDevice's ConnectableDeviceListener.
    /// 
    /// Connection & Pairing
    /// Your ConnectableDevice object will let you know if you need to connect or pair to any services.
    /// 
    /// Capabilities
    /// All DeviceService objects have a group of capabilities. These capabilities can be implemented by any object, and that object will be returned when you call the DeviceService's capability methods (launcher, mediaPlayer, volumeControl, etc).
    /// </summary>
    public class DeviceService : IDeviceServiceReachabilityListener, IServiceCommandProcessor
    {
        // ReSharper disable InconsistentNaming
        public static string KEY_CLASS = "class";
        public static string KEY_CONFIG = "config";
        public static string KEY_DESC = "description";

        protected ServiceConfig serviceConfig;

        protected DeviceServiceReachability mServiceReachability;
        protected bool connected = false;
        // ReSharper restore InconsistentNaming

        /// <summary>
        /// An array of capabilities supported by the DeviceService. This array may change based off a number of factors.
        /// DiscoveryManager's pairingLevel value
        /// - Connect SDK framework version
        /// - First screen device OS version
        /// - First screen device configuration (apps installed, settings, etc)
        /// - Physical region
        /// </summary>
        private List<string> capabilities;

        public List<ServiceCommand> Requests = new List<ServiceCommand>();

        private ServiceDescription serviceDescription;

        public ServiceConfig ServiceConfig
        {
            get { return serviceConfig; }
            set { serviceConfig = value; }
        }

        public IDeviceServiceListener Listener { get; set; }

        public List<string> Capabilities
        {
            get { return capabilities; }
            set { capabilities = value; }
        }

        public DeviceService(ServiceDescription serviceDescription, ServiceConfig serviceConfig)
        {
            SetServiceDescription(serviceDescription);
            ServiceConfig = serviceConfig;

            capabilities = new List<string>();

            UpdateCapabilities();
        }

        public DeviceService(ServiceConfig serviceConfig)
        {
            ServiceConfig = serviceConfig;

            capabilities = new List<string>();
            UpdateCapabilities();
        }

        public virtual void SetServiceDescription(ServiceDescription sd)
        {
            serviceDescription = sd;
        }

        public ServiceDescription GetServiceDescription()
        {
            return ServiceDescription;
        }

        public static DeviceService GetService(JsonObject json)
        {
            DeviceService newServiceClass = null;

            try
            {
                string className = json.GetNamedString(KEY_CLASS);

                if (className.Equals("DLNAService", StringComparison.OrdinalIgnoreCase))
                    return null;

                if (className.Equals("Chromecast", StringComparison.OrdinalIgnoreCase))
                    return null;

                var jsonConfig = json.GetNamedObject(KEY_CONFIG);
                ServiceConfig serviceConfig = null;
                if (jsonConfig != null)
                    serviceConfig = ServiceConfig.GetConfig(jsonConfig);

                var jsonDescription = json.GetNamedObject(KEY_DESC);
                ServiceDescription serviceDescription = null;
                if (jsonDescription != null)
                    serviceDescription = ServiceDescription.GetDescription(jsonDescription);

                if (serviceConfig == null || serviceDescription == null)
                    return null;


                //if (className.Equals("AirPlayService", StringComparison.OrdinalIgnoreCase))
                //{
                //    serviceDescription.ServiceId = AirPlayService.Id;
                //    newServiceClass =
                //        (AirPlayService)
                //            Activator.CreateInstance(typeof (AirPlayService),
                //                new object[] {serviceDescription, serviceConfig});
                //}
                if (className.Equals("NetcastTVService", StringComparison.OrdinalIgnoreCase))
                {
                    serviceDescription.ServiceId = NetcastTvService.Id;
                    newServiceClass =
                        (NetcastTvService)
                            Activator.CreateInstance(typeof(NetcastTvService),
                                new object[] { serviceDescription, serviceConfig });
                }
                return newServiceClass;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public static DeviceService GetService(Type clazz, ServiceConfig serviceConfig)
        {
            return Activator.CreateInstance(clazz, new object[] { serviceConfig }) as DeviceService;
        }

        public static DeviceService GetService(Type clazz, ServiceDescription serviceDescription,
            ServiceConfig serviceConfig)
        {
            return Activator.CreateInstance(clazz, new object[] { serviceDescription, serviceConfig }) as DeviceService;
        }

        public T GetApi<T>() where T: class, ICapabilityMethod
        {
            // if this class is of the type given return it, otherwise null
            // ReSharper disable once SuspiciousTypeConversion.Global
            //var tt = (T)this;
            return this as T;
        }

        public virtual CapabilityPriorityLevel GetPriorityLevel(CapabilityMethods clazz)
        {
            return CapabilityPriorityLevel.NotSupported;
        }

        public static DiscoveryFilter DiscoveryFilter()
        {
            return null;
        }

        public virtual void Connect()
        {

        }

        public virtual void Disconnect()
        {

        }

        public virtual bool IsConnected()
        {
            return true;
        }

        public virtual bool IsConnectable()
        {
            return false;
        }

        /** Explicitly cancels pairing in services that require pairing. In some services, this will hide a prompt that is displaying on the device. */
        public void CancelPairing()
        {

        }

        protected void ReportConnected(bool ready)
        {
            if (Listener == null)
                return;

            Listener.OnConnectionSuccess(this);
        }

        public virtual void SendPairingKey(string pairingKey)
        {

        }

        public virtual void Unsubscribe(UrlServiceSubscription subscription)
        {

        }

        public virtual void Unsubscribe(IServiceSubscription subscription)
        {

        }

        public virtual void SendCommand(ServiceCommand command)
        {

        }

        protected virtual void UpdateCapabilities()
        {

        }

        //public bool HasCapability(String capability)
        //{
        //    //Matcher m = CapabilityMethods.ANY_PATTERN.matcher(capability);

        //    //if (m.find()) {
        //    //    String match = m.group();
        //    //    for (String item : this.mCapabilities) {
        //    //        if (item.indexOf(match) != -1) {
        //    //            return true;
        //    //        }
        //    //    }

        //    //    return false;
        //    //}

        //    //return mCapabilities.contains(capability);

        //    throw new NotImplementedException();
        //    return false;
        //}

        public bool HasCapability(string capability)
        {
            return capabilities.Contains(capability);
        }

        public bool HasAnyCapability(List<string> caps)
        {
            return caps.Any(HasCapability);
        }

        public bool HasCapabilities(List<string> caps)
        {
            return caps.All(HasCapability);
        }

        //protected void AppendCapability(string capability)
        //{
        //    capabilities.Add(capability);
        //}

        //protected void AppendCapabilites(List<string> newItems)
        //{
        //    foreach (var capability in newItems)
        //        capabilities.Add(capability);
        //}


        public JsonObject ToJsonObject()
        {
            var jsonObj = new JsonObject();

            try
            {
                jsonObj.Add(KEY_CLASS, JsonValue.CreateStringValue(GetType().Name));
                jsonObj.Add("description", ServiceDescription.ToJsonObject());
                jsonObj.Add("config", ServiceConfig.ToJsonObject());
            }
            // ReSharper disable once EmptyGeneralCatchClause
            catch
            {

            }

            return jsonObj;
        }

        /** Name of the DeviceService (webOS, Chromecast, etc) */

        public string ServiceName
        {
            get { return ServiceDescription.ServiceId; }
        }

        public ServiceDescription ServiceDescription
        {
            get { return serviceDescription; }
        }

        public void CloseLaunchSession(LaunchSession launchSession, ResponseListener lst)
        {
            if (launchSession == null)
            {
                Util.PostError(lst, new ServiceCommandError(0, null));
                return;
            }

            var service = launchSession.Service;
            if (service == null)
            {
                Util.PostError(lst,
                    new ServiceCommandError(0, null));
                return;
            }

            switch (launchSession.SessionType)
            {
                case LaunchSessionType.App:
                    var launcher = service as ILauncher;
                    if (launcher != null)
                        launcher.CloseApp(launchSession, lst);
                    break;
                case LaunchSessionType.Media:
                    var player = service as IMediaPlayer;
                    if (player != null)
                        player.CloseMedia(launchSession, lst);
                    break;
                case LaunchSessionType.ExternalInputPicker:
                    var control = service as IExternalInputControl;
                    if (control != null)
                        control.CloseInputPicker(launchSession, lst);
                    break;
                case LaunchSessionType.WebApp:
                    var appLauncher = service as IWebAppLauncher;
                    if (appLauncher != null)
                        appLauncher.CloseWebApp(launchSession, lst);
                    break;
                default:
                    Util.PostError(lst,
                        new ServiceCommandError(0,
                            null));
                    break;
            }
        }

        public void AddCapability(string capability)
        {
            if (string.IsNullOrEmpty(capability) || capabilities.Contains(capability))
                return;

            capabilities.Add(capability);

            var added = new List<string> { capability };

            if (Listener != null)
                Listener.OnCapabilitiesUpdated(this, added, new List<string>());
        }

        public void AddCapabilities(List<string> caps)
        {
            if (capabilities == null)
                return;

            foreach (var capability in caps.Where(capability => !string.IsNullOrEmpty(capability) && !capabilities.Contains(capability)))
            {
                capabilities.Add(capability);
            }

            if (Listener != null)
                Listener.OnCapabilitiesUpdated(this, capabilities, new List<string>());
        }


        public void RemoveCapability(string capability)
        {
            if (capability == null)
                return;

            capabilities.Remove(capability);

            var removed = new List<string> { capability };

            if (Listener != null)
                Listener.OnCapabilitiesUpdated(this, new List<string>(), removed);
        }

        public void RemoveCapabilities(List<string> caps)
        {
            if (capabilities == null)
                return;

            foreach (var capability in caps)
            {
                capabilities.Remove(capability);
            }
            if (Listener != null)
                Listener.OnCapabilitiesUpdated(this, new List<string>(), capabilities);
        }


        public virtual void OnLoseReachability(DeviceServiceReachability reachability)
        {
        }

        protected void SetCapabilities(List<String> newCapabilities)
        {
            if (capabilities == null)
                capabilities = new List<string>();
            var oldCapabilities = capabilities;

            capabilities = newCapabilities;

            var lostCapabilities = oldCapabilities.Where(capability => !newCapabilities.Contains(capability)).ToList();
            var addedCapabilities = newCapabilities.Where(capability => !oldCapabilities.Contains(capability)).ToList();

            if (Listener != null)
            {
                Listener.OnCapabilitiesUpdated(this, addedCapabilities, lostCapabilities);
            }
        }
    }
}
