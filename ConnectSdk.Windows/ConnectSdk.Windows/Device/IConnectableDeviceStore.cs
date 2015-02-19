using Windows.Data.Json;
using ConnectSdk.Windows.Service.Config;

namespace ConnectSdk.Windows.Device
{
    /// <summary>
    ///  ConnectableDeviceStore is a interface which can be implemented to save key information about ConnectableDevices that have been connected to.  Any class which implements this interface can be used as DiscoveryManager's deviceStore.
    /// 
    ///  A default implementation, DefaultConnectableDeviceStore, will be used by DiscoveryManager if no other ConnectableDeviceStore is provided to DiscoveryManager when startDiscovery is called.
    /// 
    ///  ###Privacy Considerations
    ///  If you chose to implement ConnectableDeviceStore, it is important to keep your users' privacy in mind.
    ///  - There should be UI elements in your app to
    ///    + completely disable ConnectableDeviceStore
    ///    + purge all data from ConnectableDeviceStore (removeAll)
    ///  - Your ConnectableDeviceStore implementation should
    ///    + avoid tracking too much data (indefinitely storing all discovered devices)
    ///   + periodically remove ConnectableDevices from the ConnectableDeviceStore if they haven't been used/connected in X amount of time
    /// </summary>
    public interface IConnectableDeviceStore
    {
        /// <summary>
        /// Add a ConnectableDevice to the ConnectableDeviceStore. If the ConnectableDevice is already stored, it's record will be updated.
        /// </summary>
        /// <param name="device">ConnectableDevice to add to the ConnectableDeviceStore</param>
        void AddDevice(ConnectableDevice device);

        /// <summary>
        /// Removes a ConnectableDevice's record from the ConnectableDeviceStore.
        /// </summary>
        /// <param name="device">ConnectableDevice to remove from the ConnectableDeviceStore</param>
        void RemoveDevice(ConnectableDevice device);

        /// <summary>
        /// Updates a ConnectableDevice's record in the ConnectableDeviceStore.
        /// </summary>
        /// <param name="device">ConnectableDevice to update in the ConnectableDeviceStore</param>
        void UpdateDevice(ConnectableDevice device);

        /// <summary>
        /// A JSONObject of all ConnectableDevices in the ConnectableDeviceStore. To gt a strongly-typed ConnectableDevice object, use the `getDevice(string);` method.
        /// </summary>
        JsonObject GetStoredDevices();

        /// <summary>
        /// Gets a ConnectableDevice object for a provided id.  The id may be for the ConnectableDevice object or any of the DeviceServices.
        /// </summary>
        /// <param name="uuid">Unique ID for a ConnectableDevice or any of its DeviceService objects</param>
        /// <returns>ConnectableDevice object if a matching uuit was found, otherwise will return null</returns>
        ConnectableDevice GetDevice(string uuid);

        /// <summary>
        /// Gets a ServiceConfig object for a provided UUID.  This is used by DiscoveryManager to retain crucial service information between sessions (pairing code, etc).
        /// </summary>
        /// <param name="uuid">Unique ID for the service</param>
        /// <returns>ServiceConfig object if matching UUID was found, otherwise will return null</returns>
        ServiceConfig GetServiceConfig(string uuid);

        /// <summary>
        ///  Clears out the ConnectableDeviceStore, removing all records.
        /// </summary>
        void RemoveAll();
    }
}
