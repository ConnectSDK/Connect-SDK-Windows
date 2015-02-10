using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Data.Json;
using Windows.Networking.Sockets;
using ConnectSdk.Windows.Core;
using MyRemote.ConnectSDK.Core;
using MyRemote.ConnectSDK.Etc.Helper;
using MyRemote.ConnectSDK.Service.Capability;
using MyRemote.ConnectSDK.Service.Capability.Listeners;
using MyRemote.ConnectSDK.Service.Command;
using MyRemote.ConnectSDK.Service.Config;
using MyRemote.ConnectSDK.Service.Sessions;

namespace MyRemote.ConnectSDK.Service
{
    /**
     * ###Overview
     * From a high-level perspective, DeviceService completely abstracts the functionality of a particular service/protocol (webOS TV, Netcast TV, Chromecast, Roku, DIAL, etc).
     *
     * ###In Depth
     * DeviceService is an abstract class that is meant to be extended. You shouldn't ever use DeviceService directly, unless extending it to provide support for an additional service/protocol.
     *
     * Immediately after discovery of a DeviceService, DiscoveryManager will set the DeviceService's Listener to the ConnectableDevice that owns the DeviceService. You should not change the Listener unless you intend to manage the lifecycle of that service. The DeviceService will proxy all of its Listener method calls through the ConnectableDevice's ConnectableDeviceListener.
     *
     * ####Connection & Pairing
     * Your ConnectableDevice object will let you know if you need to connect or pair to any services.
     *
     * ####Capabilities
     * All DeviceService objects have a group of capabilities. These capabilities can be implemented by any object, and that object will be returned when you call the DeviceService's capability methods (launcher, mediaPlayer, volumeControl, etc).
     */

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


        /**
	 * An array of capabilities supported by the DeviceService. This array may change based off a number of factors.
	 * - DiscoveryManager's pairingLevel value
	 * - Connect SDK framework version
	 * - First screen device OS version
	 * - First screen device configuration (apps installed, settings, etc)
	 * - Physical region
	 */
        private List<string> mCapabilities;

        protected IDeviceServiceListener listener;

        public List<ServiceCommand> requests = new List<ServiceCommand>();

        public DeviceService(ServiceDescription serviceDescription, ServiceConfig serviceConfig)
        {
            this.ServiceDescription = serviceDescription;
            this.ServiceConfig = serviceConfig;

            mCapabilities = new List<string>();

            //setCapabilities();
        }

        public DeviceService(ServiceConfig serviceConfig)
        {
            this.ServiceConfig = serviceConfig;

            mCapabilities = new List<string>();

            //setCapabilities();
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

        public static DeviceService getService(JsonObject json)
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
                    serviceConfig = Config.ServiceConfig.GetConfig(jsonConfig);

                JsonObject jsonDescription = json.GetNamedObject(KEY_DESC);
                ServiceDescription serviceDescription = null;
                if (jsonDescription != null)
                    serviceDescription = Config.ServiceDescription.GetDescription(jsonDescription);

                if (serviceConfig == null || serviceDescription == null)
                    return null;


                if (className.Equals("AirPlayService", StringComparison.OrdinalIgnoreCase))
                {
                    serviceDescription.ServiceId = AirPlayService.Id;
                    newServiceClass =
                        (AirPlayService)
                            Activator.CreateInstance(typeof (AirPlayService),
                                new object[] {serviceDescription, serviceConfig});
                }
                if (className.Equals("NetcastTVService", StringComparison.OrdinalIgnoreCase))
                {
                    serviceDescription.ServiceId = NetcastTVService.ID;
                    newServiceClass =
                        (NetcastTVService)
                            Activator.CreateInstance(typeof (NetcastTVService),
                                new object[] {serviceDescription, serviceConfig});
                }
                return newServiceClass;
            }
            catch (Exception e)
            {
                throw e;
            }
            return null;
        }

        public static DeviceService getService(Type clazz, ServiceConfig serviceConfig)
        {
            return Activator.CreateInstance(clazz, new object[] {serviceConfig}) as DeviceService;
        }

        public static DeviceService getService(Type clazz, ServiceDescription serviceDescription,
            ServiceConfig serviceConfig)
        {
            return Activator.CreateInstance(clazz, new object[] {serviceDescription, serviceConfig}) as DeviceService;
        }


        public static JsonObject discoveryParameters()
        {
            return null;
        }

        // @endcond

        /**
	 * Will attempt to connect to the DeviceService. The failure/success will be reported back to the DeviceServiceListener. If the connection attempt reveals that pairing is required, the DeviceServiceListener will also be notified in that event.
	 */

        public virtual void connect()
        {

        }

        /**
	 * Will attempt to disconnect from the DeviceService. The failure/success will be reported back to the DeviceServiceListener.
	 */

        public virtual void disconnect()
        {

        }

        /** Whether the DeviceService is currently connected */

        public virtual bool isConnected()
        {
            return true;
        }

        public virtual bool isConnectable()
        {
            return false;
        }

        protected void reportConnected(bool ready)
        {
            //Util.runOnUI(new Runnable() {
            //    @Override
            //    public void run() {
            //        if (listener != null)
            //            listener.onConnectionSuccess(DeviceService.this);
            //    }
            //});
        }

        /**
	 * Will attempt to pair with the DeviceService with the provided pairingData. The failure/success will be reported back to the DeviceServiceListener.
	 *
	 * @param pairingKey Data to be used for pairing. The type of this parameter will vary depending on what type of pairing is required, but is likely to be a string (pin code, pairing key, etc).
	 */

        public virtual void sendPairingKey(string pairingKey)
        {

        }

        // @cond INTERNAL

        public virtual void Unsubscribe(UrlServiceSubscription subscription)
        {

        }

        public virtual void SendCommand(ServiceCommand command)
        {

        }



        /**
	 * Test to see if the capabilities array contains a given capability. See the individual Capability classes for acceptable capability values.
	 *
	 * It is possible to append a wildcard search term `.Any` to the end of the search term. This method will return true for capabilities that match the term up to the wildcard.
	 *
	 * Example: `Launcher.App.Any`
	 *
	 * @param capability Capability to test against
	 */

        public bool hasCapability(string capability)
        {
            //Matcher m = CapabilityMethods.ANY_PATTERN.matcher(capability);

            //if (m.find())
            //{
            //    string match = m.group();
            //    foreach (var item in mCapabilities)
            //    {

            //        if (item.IndexOf(match) != -1)
            //        {
            //            return true;
            //        }
            //    }

            //    return false;
            //}

            return mCapabilities.Contains(capability);
        }

        /**
	 * Test to see if the capabilities array contains at least one capability in a given set of capabilities. See the individual Capability classes for acceptable capability values.
	 *
	 * See hasCapability: for a description of the wildcard feature provided by this method.
	 *
	 * @param capabilities Set of capabilities to test against
	 */

        public bool hasAnyCapability(List<string> capabilities)
        {
            foreach (var capability in capabilities)
            {
                if (hasCapability(capability))
                    return true;
            }

            return false;
        }



        /**
     * Test to see if the capabilities array contains a given set of capabilities. See the individual Capability classes for acceptable capability values.
     *
     * See hasCapability: for a description of the wildcard feature provided by this method.
     *
     * @param capabilities Set of capabilities to test against
     */

        public bool hasCapabilities(List<string> capabilities)
        {
            bool hasCaps = true;

            foreach (var capability in capabilities)
            {
                if (!hasCapability(capability))
                {
                    hasCaps = false;
                    break;
                }
            }

            return hasCaps;
        }

        protected void appendCapability(string capability)
        {
            mCapabilities.Add(capability);
        }

        protected void appendCapabilites(List<string> newItems)
        {
            foreach (var capability in newItems)
                mCapabilities.Add(capability);
        }

        public virtual void SetServiceDescription(ServiceDescription serviceDescription)
        {
            this.serviceDescription = serviceDescription;
        }

        public ServiceDescription GetServiceDescription()
        {
            return serviceDescription;
        }

        public JsonObject toJSONObject()
        {
            JsonObject jsonObj = new JsonObject();

            try
            {
                jsonObj.Add(KEY_CLASS, JsonValue.CreateStringValue(this.GetType().Name));
                jsonObj.Add("description", ServiceDescription.ToJsonObject());
                jsonObj.Add("config", ServiceConfig.ToJsonObject());
            }
            catch (Exception e)
            {

            }

            return jsonObj;
        }

        /** Name of the DeviceService (webOS, Chromecast, etc) */

        public string ServiceName
        {
            get { return ServiceDescription.ServiceId; }
        }

        // @cond INTERNAL
        /**
	 * Create a LaunchSession from a serialized JSON object.
	 * May return null if the session was not the one that created the session.
	 * 
	 * Intended for internal use.
	 */

        public virtual LaunchSession decodeLaunchSession(string type, JsonObject sessionObj)
        {
            return null;
        }

        public IDeviceServiceListener getListener()
        {
            return Listener;
        }

        public void setListener(IDeviceServiceListener listener)
        {
            this.Listener = listener;
        }

        /**
	 * Closes the session on the first screen device. Depending on the sessionType, the associated service will have different ways of handling the close functionality.
	 *
	 * @param launchSession LaunchSession to close
	 * @param success (optional) listener to be called on success/failure
	 */

        public void closeLaunchSession(LaunchSession launchSession, ResponseListener listener)
        {
            if (launchSession == null)
            {
                Util.PostError(listener, new ServiceCommandError(0, "You must provide a valid LaunchSession", null));
                return;
            }

            DeviceService service = launchSession.Service;
            if (service == null)
            {
                Util.PostError(listener,
                    new ServiceCommandError(0, "There is no service attached to this launch session", null));
                return;
            }

            switch (launchSession.SessionType)
            {
                case LaunchSessionType.App:
                    if (service is ILauncher)
                        ((ILauncher) service).CloseApp(launchSession, listener);
                    break;
                case LaunchSessionType.Media:
                    if (service is IMediaPlayer)
                        ((IMediaPlayer) service).CloseMedia(launchSession, listener);
                    break;
                case LaunchSessionType.ExternalInputPicker:
                    if (service is IExternalInputControl)
                        ((IExternalInputControl) service).CloseInputPicker(launchSession, listener);
                    break;
                case LaunchSessionType.WebApp:
                    if (service is IWebAppLauncher)
                        ((IWebAppLauncher) service).CloseWebApp(launchSession, listener);
                    break;
                case LaunchSessionType.Unknown:
                default:
                    Util.PostError(listener,
                        new ServiceCommandError(0, "This DeviceService does not know ho to close this LaunchSession",
                            null));
                    break;
            }
        }

        public void addCapability(string capability)
        {
            if (capability == null || capability.Length == 0 || this.mCapabilities.Contains(capability))
                return;

            this.mCapabilities.Add(capability);

            List<string> added = new List<string>();
            added.Add(capability);

            if (Listener != null)
                Listener.OnCapabilitiesUpdated(this, added, new List<string>());
        }

        public void addCapabilities(List<string> capabilities)
        {
            if (capabilities == null)
                return;

            foreach (var capability in capabilities)
            {
                if (capability == null || capability.Length == 0 || mCapabilities.Contains(capability))
                    continue;

                mCapabilities.Add(capability);
            }

            //    Util.runOnUI(new Runnable()
            //    {

            //        @Override
            //    public void run() {

            if (Listener != null)
                Listener.OnCapabilitiesUpdated(this, capabilities, new List<string>());
        }


        public void removeCapability(string capability)
        {
            if (capability == null)
                return;

            this.mCapabilities.Remove(capability);

            List<string> removed = new List<string>();
            removed.Add(capability);

            if (Listener != null)
                Listener.OnCapabilitiesUpdated(this, new List<string>(), removed);
        }

        public void removeCapabilities(List<string> capabilities)
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

        protected void setCapabilities(List<String> newCapabilities)
        {
            List<String> oldCapabilities = mCapabilities;

            mCapabilities = newCapabilities;

            List<String> _lostCapabilities = new List<String>();

            foreach (String capability in oldCapabilities)
            {
                if (!newCapabilities.Contains(capability))
                    _lostCapabilities.Add(capability);
            }

            List<String> _addedCapabilities = new List<String>();

            foreach (String capability in newCapabilities)
            {
                if (!oldCapabilities.Contains(capability))
                    _addedCapabilities.Add(capability);
            }

            List<String> lostCapabilities = _lostCapabilities;
            List<String> addedCapabilities = _addedCapabilities;

            if (this.listener != null)
            {
                listener.OnCapabilitiesUpdated(this, addedCapabilities, lostCapabilities);
            }
        }
    }
}
