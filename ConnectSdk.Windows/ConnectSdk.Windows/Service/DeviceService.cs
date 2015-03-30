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
    public class DeviceService<T> : IDeviceServiceReachabilityListener, IServiceCommandProcessor<T> where T: ResponseListener<object>
    {
        // ReSharper disable StaticFieldInGenericType
        public static string KeyClass = "class";
        public static string KeyConfig = "config";
        public static string KeyDesc = "description";

        // ReSharper disable once InconsistentNaming
        protected ServiceConfig serviceConfig;

        protected DeviceServiceReachability ServiceReachability;
        protected bool Connected = false;

        /// <summary>
        /// An array of capabilities supported by the DeviceService. This array may change based off a number of factors.
        /// DiscoveryManager's pairingLevel value
        /// - Connect SDK framework version
        /// - First screen device OS version
        /// - First screen device configuration (apps installed, settings, etc)
        /// - Physical region
        /// </summary>
        private List<string> capabilities;

        public List<ServiceCommand<T>> Requests = new List<ServiceCommand<T>>();

        public ServiceDescription ServiceDescription { get; set; }

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
            ServiceDescription = serviceDescription;
            ServiceConfig = serviceConfig;

            capabilities = new List<string>();

            // ReSharper disable once DoNotCallOverridableMethodsInConstructor
            UpdateCapabilities();
        }

        public DeviceService(ServiceConfig serviceConfig)
        {
            ServiceConfig = serviceConfig;

            capabilities = new List<string>();

            // ReSharper disable once DoNotCallOverridableMethodsInConstructor
            UpdateCapabilities();
        }

        public static DeviceService<T> GetService(JsonObject json)
        {
            DeviceService<T> newServiceClass = null;

            try
            {
                string className = json.GetNamedString(KeyClass);

                if (className.Equals("DLNAService", StringComparison.OrdinalIgnoreCase))
                    return null;

                if (className.Equals("Chromecast", StringComparison.OrdinalIgnoreCase))
                    return null;

                var jsonConfig = json.GetNamedObject(KeyConfig);
                ServiceConfig serviceConfig = null;
                if (jsonConfig != null)
                    serviceConfig = ServiceConfig.GetConfig(jsonConfig);

                var jsonDescription = json.GetNamedObject(KeyDesc);
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

        public static DeviceService<T> GetService(Type clazz, ServiceConfig serviceConfig)
        {
            return Activator.CreateInstance(clazz, new object[] { serviceConfig }) as DeviceService<T>;
        }

        public static DeviceService<T> GetService(Type clazz, ServiceDescription serviceDescription,
            ServiceConfig serviceConfig)
        {
            return Activator.CreateInstance(clazz, new object[] { serviceDescription, serviceConfig }) as DeviceService<T>;
        }

        public TL GetApi<TL>(TL clazz) where TL : CapabilityMethods
        {
            // if this class is of the type given return it, otherwise null
            // ReSharper disable once SuspiciousTypeConversion.Global
            var tt = this as TL;
            return tt;
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

        public virtual void Unsubscribe(UrlServiceSubscription<T> subscription)
        {

        }

        public virtual void Unsubscribe(IServiceSubscription<T> subscription)
        {

        }

        public virtual void SendCommand(ServiceCommand<T> command)
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
                jsonObj.Add(KeyClass, JsonValue.CreateStringValue(GetType().Name));
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

        public void CloseLaunchSession(LaunchSession launchSession, ResponseListener<object> lst)
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
                    // TODO: check this, there is no implementation of IWebAppLauncher yet
                    // ReSharper disable once SuspiciousTypeConversion.Global
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
