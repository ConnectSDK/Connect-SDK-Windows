#region Copyright Notice
/*
 * ConnectSdk.Windows
 * SSDPDiscoveryProvider.cs
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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ConnectSdk.Windows.Core.Upnp.Ssdp;
using ConnectSdk.Windows.Discovery.Provider.ssdp;
using ConnectSdk.Windows.Service.Config;

namespace ConnectSdk.Windows.Discovery.Provider
{
    public class SsdpDiscoveryProvider : IDiscoveryProvider
    {
        // ReSharper disable InconsistentNaming
        private const int RESCAN_INTERVAL = 10000;
        private const int RESCAN_ATTEMPTS = 3;
        private const int SSDP_TIMEOUT = RESCAN_INTERVAL * RESCAN_ATTEMPTS;
        // ReSharper restore InconsistentNaming

        //private readonly Dictionary<string, ServiceDescription> services;
        private readonly List<IDiscoveryProviderListener> serviceListeners;

        private readonly ConcurrentDictionary<string, ServiceDescription> foundServices = new ConcurrentDictionary<string, ServiceDescription>();
        private readonly ConcurrentDictionary<string, ServiceDescription> discoveredServices = new ConcurrentDictionary<string, ServiceDescription>();

        private readonly List<DiscoveryFilter> serviceFilters;
        private bool isRunning;
        private SsdpSocket ssdpSocket;

        private readonly Regex uuidReg;

        public SsdpDiscoveryProvider()
        {
            uuidReg = new Regex("(?<=uuid:)(.+?)(?=(::)|$)");

            serviceListeners = new List<IDiscoveryProviderListener>();
            serviceFilters = new List<DiscoveryFilter>();
        }

        private void OpenSocket()
        {
            if (ssdpSocket != null && ssdpSocket.IsConnected)
                return;
            ssdpSocket = new SsdpSocket();

            ssdpSocket.MessageReceivedChanged += SsdpSocketOnMessageReceivedChanged;
            ssdpSocket.NotifyReceivedChanged += SsdpSocketOnNotifyReceivedChanged;
        }

        private void SsdpSocketOnNotifyReceivedChanged(object sender, string message)
        {
            HandleDatagramPacket(new ParsedDatagram(message));
        }

        private void SsdpSocketOnMessageReceivedChanged(object sender, string message)
        {
            HandleDatagramPacket(new ParsedDatagram(message));
        }



        public void Start()
        {
            if (isRunning)
                return;
            isRunning = true;

            OpenSocket();

            for (var i = 0; i < RESCAN_ATTEMPTS; i++)
            {
                var task = new Task(SendSearch);
                task.Start();
                task.Wait(RESCAN_INTERVAL);
            }

        }

        public void SendSearch()
        {
            var killPoint = DateTime.Now.Ticks/TimeSpan.TicksPerSecond - SSDP_TIMEOUT;

            var killKeys =
                (from key in foundServices.Keys
                    let service = foundServices[key]
                    where service.LastDetection < killPoint
                    select key).ToList();

            foreach (var key in killKeys)
            {
                var service = foundServices[key];


                if (service != null)
                {
                    NotifyListenersOfLostService(service);
                }

                ServiceDescription fsrv = null;
                if (foundServices.ContainsKey(key))
                    foundServices.TryRemove(key, out fsrv);
            }
            Rescan();
        }


        public void Stop()
        {
            if (ssdpSocket != null)
            {
                ssdpSocket.Close();
                ssdpSocket = null;
            }
        }

        public void Restart()
        {
            Stop();
            Start();
        }

        public void Reset()
        {
            Stop();
            foundServices.Clear();
            discoveredServices.Clear();
        }

        public void Rescan()
        {
            foreach (var searchTarget in serviceFilters)
            {
                var message = new SsdpSearchMsg(searchTarget.ServiceFilter).ToString();
                if (ssdpSocket != null)
                {
                    // ReSharper disable once UnusedVariable
                    var result = ssdpSocket.Send(message).Result;
                }
            }
        }

        public void AddDeviceFilter(DiscoveryFilter  filter)
        {
            if (filter.ServiceFilter == null)
            {
            }
            else
            {
                serviceFilters.Add(filter);
            }
        }

        public void RemoveDeviceFilter(DiscoveryFilter parameters)
        {
            var shouldRemove = false;
            var removalIndex = -1;

            var removalServiceId = parameters.ServiceId;

            for (var i = 0; i < serviceFilters.Count; i++)
            {
                var serviceFilter = serviceFilters[i];
                var serviceId = serviceFilter.ServiceId;

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
            if (pd == null || pd.DataPacket.Length == 0)
                return;

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
                    ServiceDescription fsrv = null;
                    foundServices.TryRemove(uuid, out fsrv);
                    NotifyListenersOfLostService(service);
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
                    foundService = new ServiceDescription { Uuid = uuid, ServiceFilter = serviceFilter };


                    var u = new Uri(location);
                    foundService.IpAddress = u.DnsSafeHost;//pd.dp.IpAddress.getHostAddress();
                    foundService.Port = u.Port;
                    
                    if (!discoveredServices.ContainsKey(uuid))
                        discoveredServices.TryAdd(uuid, foundService);

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

                        foundServices.TryAdd(uuid, service);

                        NotifyListenersOfNewService(service);
                    }
                }
            }
            ServiceDescription srv;
            discoveredServices.TryRemove(uuid, out srv);
        }

        public string ServiceIdForFilter(string filter)
        {
            const string serviceId = "";

            foreach (var serviceFilter in serviceFilters)
            {
                try
                {
                    string ssdpFilter = serviceFilter.ServiceFilter;
                    if (ssdpFilter.Equals(filter))
                    {
                        return serviceFilter.ServiceId;
                    }
                }
                // ReSharper disable once EmptyGeneralCatchClause
                catch
                {
                }
            }

            return serviceId;
        }


        public bool ContainsServicesWithFilter(Core.Upnp.Device device, string filter)
        {
            //  TODO  Implement this method.  Not sure why needs to happen since there are now required services.
            return true;
        }

        private void NotifyListenersOfNewService(ServiceDescription service)
        {
            var serviceIds = ServiceIdsForFilter(service.ServiceFilter);

            foreach (var serviceId in serviceIds)
            {
                var newService = service.Clone();

                newService.ServiceId = serviceId;

                foreach (var listener in serviceListeners)
                {
                    listener.OnServiceAdded(this, newService);
                }
            }
        }

        private void NotifyListenersOfLostService(ServiceDescription service)
        {
            var serviceIds = ServiceIdsForFilter(service.ServiceFilter);

            foreach (var serviceId in serviceIds)
            {
                var newService = service.Clone();
                newService.ServiceId = serviceId;

                foreach (var listener in serviceListeners)
                {
                    listener.OnServiceRemoved(this, newService);
                }
            }

        }

        public List<String> ServiceIdsForFilter(String filter)
        {
            return 
                (from serviceFilter in serviceFilters 
                 let ssdpFilter = serviceFilter.ServiceFilter 
                 where ssdpFilter.Equals(filter) 
                 select serviceFilter.ServiceId 
                 into 
                    serviceId 
                    where serviceId != null 
                    select serviceId
                 ).ToList();
        }

        public bool IsSearchingForFilter(String filter)
        {
            return serviceFilters.Select(serviceFilter => serviceFilter.ServiceFilter).Any(ssdpFilter => ssdpFilter.Equals(filter));
        }

/*
        private bool ContainsServicesWithFilter(SsdpDevice device, String filter)
        {
            //  TODO  Implement this method.  Not sure why needs to happen since there are now required services.
            return true;
        }
*/

        public void AddListener(IDiscoveryProviderListener listener)
        {
            serviceListeners.Add(listener);
        }

        public void RemoveListener(IDiscoveryProviderListener listener)
        {
            serviceListeners.Remove(listener);
        }
    }
}
