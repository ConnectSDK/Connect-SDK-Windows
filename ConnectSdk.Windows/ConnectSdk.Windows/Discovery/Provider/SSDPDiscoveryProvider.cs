using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Windows.Data.Json;
using ConnectSdk.Windows.Core.Upnp.Ssdp;
using ConnectSdk.Windows.Service.Config;

namespace ConnectSdk.Windows.Discovery.Provider
{
    public class SsdpDiscoveryProvider : IDiscoveryProvider
    {
        // ReSharper disable InconsistentNaming
        private const int RESCAN_INTERVAL = 10000;
        private const int RESCAN_ATTEMPTS = 3;
        private const int SSDP_TIMEOUT = RESCAN_INTERVAL*RESCAN_ATTEMPTS;
        // ReSharper restore InconsistentNaming

        private readonly Dictionary<string, ServiceDescription> services;
        private List<IDiscoveryProviderListener> serviceListeners;

        private readonly Dictionary<string, ServiceDescription> foundServices = new Dictionary<string, ServiceDescription>();
        private readonly Dictionary<string, ServiceDescription> discoveredServices = new Dictionary<string, ServiceDescription>();

        private readonly List<JsonObject> serviceFilters;

        private SSDPSocket mSsdpSocket;

        private readonly Regex uuidReg;

        public SsdpDiscoveryProvider()
        {
            uuidReg = new Regex("(?<=uuid:)(.+?)(?=(::)|$)");

            services = new Dictionary<string, ServiceDescription>();
            serviceListeners = new List<IDiscoveryProviderListener>();
            serviceFilters = new List<JsonObject>();
        }

        public SsdpDiscoveryProvider(bool needToStartSearch)
        {
            NeedToStartSearch = needToStartSearch;
        }

        public bool NeedToStartSearch { get; set; }

        private void MSsdpSocketOnNotifyReceivedChanged(object sender, string message)
        {
            HandleDatagramPacket(new ParsedDatagram(message));
        }

        private void MSsdpSocketOnMessageReceivedChanged(object sender, string message)
        {
            HandleDatagramPacket(new ParsedDatagram(message));
        }

        private void OpenSocket()
        {
            if (mSsdpSocket != null)
                return;
            mSsdpSocket = new SSDPSocket();

            mSsdpSocket.MessageReceivedChanged += MSsdpSocketOnMessageReceivedChanged;
            mSsdpSocket.NotifyReceivedChanged += MSsdpSocketOnNotifyReceivedChanged;
        }

        public void Start()
        {
            OpenSocket();
            SendSearch();
        }

        public void SendSearch()
        {
            var killPoint = DateTime.Now.Ticks/TimeSpan.TicksPerSecond - SSDP_TIMEOUT;

            var killKeys = (from key in foundServices.Keys let service = foundServices[key] where service.LastDetection < killPoint select key).ToList();

            foreach (var key in killKeys)
            {
                var service = foundServices[key];


                foreach (var listener in serviceListeners)
                {
                    listener.OnServiceRemoved(this, service);

                    foundServices.Remove(key);
                }
            }

            foreach (var message in serviceFilters.Select(searchTarget => new SSDPSearchMsg(searchTarget.GetNamedString("filter"))).Select(search => search.ToString()))
            {
                //Task task = new Task(() =>
                //{
                try
                {
                    if (mSsdpSocket != null)
                    {
                        // ReSharper disable once UnusedVariable
                        var result = mSsdpSocket.Send(message).Result;
                    }
                }
                catch (Exception e)
                {
                    throw e;
                }
                //});
                    
                //task.Start();
                //task.Wait(3000);
                //todo: add here a delay of 1 sec between calls
            }
            
        }


        public void Stop()
        {
        }

        public void Reset()
        {
            Stop();
            services.Clear();
            foundServices.Clear();
            discoveredServices.Clear();
        }

        public void AddDeviceFilter(JsonObject parameters)
        {
            if (!parameters.ContainsKey("filter"))
            {
            }
            else
            {
                serviceFilters.Add(parameters);
            }
        }

        public void RemoveDeviceFilter(JsonObject parameters)
        {
            var shouldRemove = false;
            var removalIndex = -1;

            var removalServiceId = parameters.GetNamedString("serviceId");

            for (var i = 0; i < serviceFilters.Count; i++)
            {
                var serviceFilter = serviceFilters[i];
                var serviceId = serviceFilter["serviceId"].GetString();

                if (serviceId.Equals(removalServiceId))
                {
                    shouldRemove = true;
                    removalIndex = i;
                    break;
                }
            }

            if (shouldRemove)
            {
                serviceFilters.RemoveAt(removalIndex);
            }
        }


        public bool IsEmpty()
        {
            return serviceFilters.Count == 0;
        }

        private void HandleDatagramPacket(ParsedDatagram pd)
        {

            var serviceFilter = pd.Data[pd.PacketType.Equals(SSDP.SlNotify) ? SSDP.Nt : SSDP.St];

            if (serviceFilter == null || SSDP.SlMsearch.Equals(pd.PacketType) || !IsSearchingForFilter(serviceFilter))
                return;

            var usnKey = pd.Data[SSDP.Usn];

            if (string.IsNullOrEmpty(usnKey))
                return;

            var m = uuidReg.Match(usnKey);


            if (!m.Success)
                return;

            var uuid = m.Value;

            if (pd.Data.ContainsKey(SSDP.Nts) && SSDP.NtsByebye.Equals(pd.Data[SSDP.Nts]))
            {
                var service = foundServices[uuid];

                if (service != null)
                {
                    foreach (var listener in serviceListeners)
                    {
                        listener.OnServiceRemoved(this, service);
                    }
                }
            }
            else
            {
                var location = pd.Data[SSDP.Location];

                if (string.IsNullOrEmpty(location))
                    return;

                var foundService = foundServices.ContainsKey(uuid) ? foundServices[uuid] : null;
                var discoverdService = discoveredServices.ContainsKey(uuid) ? discoveredServices[uuid] : null;

                var isNew = foundService == null && discoverdService == null;

                if (isNew)
                {
                    foundService = new ServiceDescription {Uuid = uuid, ServiceFilter = serviceFilter};


                    var u = new Uri(location);
                    foundService.IpAddress = u.DnsSafeHost;//pd.dp.IpAddress.getHostAddress();
                    foundService.Port = u.Port;

                    discoveredServices.Add(uuid, foundService);

                    GetLocationData(location, uuid, serviceFilter);
                }

                if (foundService != null)
                    foundService.LastDetection = DateTime.Now.Ticks;
            }

        }

        public void GetLocationData(string location, string uuid, string serviceFilter)
        {
            var device = Core.Upnp.Device.CreateInstanceFromXml(location, serviceFilter);

            if (device != null)
            {
                if (true)
                {
                    device.Uuid = uuid;
                    var hasServices = ContainsServicesWithFilter(device, serviceFilter);

                    if (hasServices)
                    {
                        var service = discoveredServices[uuid];
                        service.ServiceId = ServiceIdForFilter(serviceFilter);
                        service.ServiceFilter = serviceFilter;
                        service.FriendlyName = device.FriendlyName ?? "LG Smart TV";
                        service.ModelName = device.ModelName;
                        service.ModelNumber = device.ModelNumber;
                        service.ModelDescription = device.ModelDescription;
                        service.Manufacturer = device.Manufacturer;
                        service.ApplicationUrl = device.ApplicationUrl;
                        service.ServiceList = device.ServiceList;
                        service.ResponseHeaders = device.Headers;
                        service.LocationXml = device.LocationXml;

                        foundServices.Add(uuid, service);

                        foreach (var listener in serviceListeners)
                        {
                            listener.OnServiceAdded(this, service);
                        }
                    }
                }
            }

            discoveredServices.Remove(uuid);
        }

        public string ServiceIdForFilter(string filter)
        {
            const string serviceId = "";

            foreach (var serviceFilter in serviceFilters)
            {
                try
                {
                    string ssdpFilter = serviceFilter.GetNamedString("filter");
                    if (ssdpFilter.Equals(filter))
                    {
                        return serviceFilter.GetNamedString("serviceId");
                    }
                }
                // ReSharper disable once EmptyGeneralCatchClause
                catch
                {
                }
            }

            return serviceId;
        }

        public bool IsSearchingForFilter(string filter)
        {
            foreach (var serviceFilter in serviceFilters)
            {
                try
                {
                    var ssdpFilter = serviceFilter.GetNamedString("filter");

                    if (ssdpFilter.Equals(filter))
                        return true;
                }
                // ReSharper disable once EmptyGeneralCatchClause
                catch
                {
                }
            }

            return false;
        }

        public bool ContainsServicesWithFilter(Core.Upnp.Device device, string filter)
        {
            //  TODO  Implement this method.  Not sure why needs to happen since there are now required services.
            return true;
        }


        public void AddListener(IDiscoveryProviderListener listener)
        {
            if (serviceListeners == null)
                serviceListeners = new List<IDiscoveryProviderListener>();
            serviceListeners.Add(listener);
        }

        public void RemoveListener(IDiscoveryProviderListener listener)
        {
            serviceListeners.Remove(listener);
        }
    }
}
