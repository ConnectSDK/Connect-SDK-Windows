using System;
using System.Collections.Generic;
using System.Linq;
using Windows.Data.Json;
using ConnectSdk.Windows.Core;
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

        protected ServiceDescription serviceDescription;
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
        private List<string> mCapabilities;

        private IDeviceServiceListener listener;

        public List<ServiceCommand> Requests = new List<ServiceCommand>();

        public DeviceService(ServiceDescription serviceDescription, ServiceConfig serviceConfig)
        {
            ServiceDescription = serviceDescription;
            ServiceConfig = serviceConfig;

            mCapabilities = new List<string>();
        }

        public DeviceService(ServiceConfig serviceConfig)
        {
            ServiceConfig = serviceConfig;

            mCapabilities = new List<string>();
        }

        public ServiceDescription ServiceDescription
        {
            get { return serviceDescription; }
            set { serviceDescription = value; }
        }

        public ServiceConfig ServiceConfig
        {
            get { return serviceConfig; }
            set { serviceConfig = value; }
        }

        public IDeviceServiceListener Listener
        {
            get { return listener; }
            set { listener = value; }
        }

        public List<string> Capabilities
        {
            get { return mCapabilities; }
            set { mCapabilities = value; }
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

                JsonObject jsonConfig = json.GetNamedObject(KEY_CONFIG);
                ServiceConfig serviceConfig = null;
                if (jsonConfig != null)
                    serviceConfig = ServiceConfig.GetConfig(jsonConfig);

                JsonObject jsonDescription = json.GetNamedObject(KEY_DESC);
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
                            Activator.CreateInstance(typeof (NetcastTvService),
                                new object[] {serviceDescription, serviceConfig});
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
            return Activator.CreateInstance(clazz, new object[] {serviceConfig}) as DeviceService;
        }

        public static DeviceService GetService(Type clazz, ServiceDescription serviceDescription,
            ServiceConfig serviceConfig)
        {
            return Activator.CreateInstance(clazz, new object[] {serviceDescription, serviceConfig}) as DeviceService;
        }


        public static JsonObject DiscoveryParameters()
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

        protected void ReportConnected(bool ready)
        {
            //Util.runOnUI(new Runnable() {
            //    @Override
            //    public void run() {
            //        if (listener != null)
            //            listener.onConnectionSuccess(DeviceService.this);
            //    }
            //});
        }

        public virtual void SendPairingKey(string pairingKey)
        {

        }

        public virtual void Unsubscribe(UrlServiceSubscription subscription)
        {

        }

        public virtual void SendCommand(ServiceCommand command)
        {

        }

        public bool HasCapability(string capability)
        {
            return mCapabilities.Contains(capability);
        }

        public bool HasAnyCapability(List<string> capabilities)
        {
            return capabilities.Any(HasCapability);
        }

        public bool HasCapabilities(List<string> capabilities)
        {
            return capabilities.All(HasCapability);
        }

        protected void AppendCapability(string capability)
        {
            mCapabilities.Add(capability);
        }

        protected void AppendCapabilites(List<string> newItems)
        {
            foreach (var capability in newItems)
                mCapabilities.Add(capability);
        }

        public virtual void SetServiceDescription(ServiceDescription serviceDescriptionParam)
        {
            serviceDescription = serviceDescriptionParam;
        }

        public ServiceDescription GetServiceDescription()
        {
            return serviceDescription;
        }

        public JsonObject ToJsonObject()
        {
            var jsonObj = new JsonObject();

            try
            {
                jsonObj.Add(KEY_CLASS, JsonValue.CreateStringValue(GetType().Name));
                jsonObj.Add("description", ServiceDescription.ToJsonObject());
                jsonObj.Add("config", ServiceConfig.ToJsonObject());
            }
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

        public void CloseLaunchSession(LaunchSession launchSession, ResponseListener listener)
        {
            if (launchSession == null)
            {
                Util.PostError(listener, new ServiceCommandError(0, "You must provide a valid LaunchSession", null));
                return;
            }

            var service = launchSession.Service;
            if (service == null)
            {
                Util.PostError(listener,
                    new ServiceCommandError(0, "There is no service attached to this launch session", null));
                return;
            }

            switch (launchSession.SessionType)
            {
                case LaunchSessionType.App:
                    var launcher = service as ILauncher;
                    if (launcher != null)
                        launcher.CloseApp(launchSession, listener);
                    break;
                case LaunchSessionType.Media:
                    var player = service as IMediaPlayer;
                    if (player != null)
                        player.CloseMedia(launchSession, listener);
                    break;
                case LaunchSessionType.ExternalInputPicker:
                    var control = service as IExternalInputControl;
                    if (control != null)
                        control.CloseInputPicker(launchSession, listener);
                    break;
                case LaunchSessionType.WebApp:
                    // TODO: check this, there is no implementation of IWebAppLauncher yet
                    if (service is IWebAppLauncher)
                        ((IWebAppLauncher)service).CloseWebApp(launchSession, listener);
                    break;
                default:
                    Util.PostError(listener,
                        new ServiceCommandError(0, "This DeviceService does not know ho to close this LaunchSession",
                            null));
                    break;
            }
        }

        public void AddCapability(string capability)
        {
            if (string.IsNullOrEmpty(capability) || mCapabilities.Contains(capability))
                return;

            mCapabilities.Add(capability);

            var added = new List<string> {capability};

            if (Listener != null)
                Listener.OnCapabilitiesUpdated(this, added, new List<string>());
        }

        public void AddCapabilities(List<string> capabilities)
        {
            if (capabilities == null)
                return;

            foreach (var capability in capabilities.Where(capability => !string.IsNullOrEmpty(capability) && !mCapabilities.Contains(capability)))
            {
                mCapabilities.Add(capability);
            }

            if (Listener != null)
                Listener.OnCapabilitiesUpdated(this, capabilities, new List<string>());
        }


        public void RemoveCapability(string capability)
        {
            if (capability == null)
                return;

            mCapabilities.Remove(capability);

            var removed = new List<string> {capability};

            if (Listener != null)
                Listener.OnCapabilitiesUpdated(this, new List<string>(), removed);
        }

        public void RemoveCapabilities(List<string> capabilities)
        {
            if (capabilities == null)
                return;

            foreach (var capability in capabilities)
            {
                mCapabilities.Remove(capability);
            }
            if (Listener != null)
                Listener.OnCapabilitiesUpdated(this,
                    new List<string>(),
                    capabilities);
        }


        public virtual void OnLoseReachability(DeviceServiceReachability reachability)
        {
        }

        protected void SetCapabilities(List<String> newCapabilities)
        {
            var oldCapabilities = mCapabilities;

            mCapabilities = newCapabilities;

            var lostCapabilities = oldCapabilities.Where(capability => !newCapabilities.Contains(capability)).ToList();
            var addedCapabilities = newCapabilities.Where(capability => !oldCapabilities.Contains(capability)).ToList();

            if (listener != null)
            {
                listener.OnCapabilitiesUpdated(this, addedCapabilities, lostCapabilities);
            }
        }
    }
}
