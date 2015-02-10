using ConnectSdk.Windows.Service.Command;
using ConnectSdk.Windows.Service.Config;

namespace ConnectSdk.Windows.Discovery
{
    /// <summary>
    /// The DiscoveryProviderListener is mechanism for passing service information to the DiscoveryManager. You likely will not be using the DiscoveryProviderListener class directly, as DiscoveryManager acts as a listener to all of the DiscoveryProviders.
    /// </summary>
    public interface IDiscoveryProviderListener
    {
        /// <summary>
        /// This method is called when the DiscoveryProvider discovers a service that matches one of its DeviceService filters. The ServiceDescription is created and passed to the listener (which should be the DiscoveryManager). The ServiceDescription is used to create a DeviceService, which is then attached to a ConnectableDevice object. 
        /// </summary>
        /// <param name="provider">DiscoveryProvider that found the service</param>
        /// <param name="serviceDescription">ServiceDescription of the service that was found</param>
        void OnServiceAdded(IDiscoveryProvider provider, ServiceDescription serviceDescription);

        /// <summary>
        /// This method is called when the DiscoveryProvider's internal mechanism loses reference to a service that matches one of its DeviceService filters. 
        /// </summary>
        /// <param name="provider">DiscoveryProvider that lost the service</param>
        /// <param name="serviceDescription">ServiceDescription of the service that was lost</param>
        void OnServiceRemoved(IDiscoveryProvider provider, ServiceDescription serviceDescription);

        /// <summary>
        /// This method is called on any error/failure within the DiscoveryProvider.
        /// </summary>
        /// <param name="provider">DiscoveryProvider that failed</param>
        /// <param name="error">ServiceCommandError providing a information about the failure</param>
        void OnServiceDiscoveryFailed(IDiscoveryProvider provider, ServiceCommandError error);
    } 
}
