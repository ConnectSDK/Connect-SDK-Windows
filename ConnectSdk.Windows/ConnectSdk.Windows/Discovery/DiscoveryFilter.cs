using System;

namespace ConnectSdk.Windows.Discovery
{
    public class DiscoveryFilter
    {
        public string ServiceId { get; set; }
        public string ServiceFilter { get; set; }

        public DiscoveryFilter(String serviceId, String serviceFilter)
        {
            ServiceId = serviceId;
            ServiceFilter = serviceFilter;
        }
    }
}