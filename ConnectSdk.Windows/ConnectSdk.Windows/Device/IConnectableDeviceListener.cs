using System.Collections.Generic;
using ConnectSdk.Windows.Service;
using ConnectSdk.Windows.Service.Command;

namespace ConnectSdk.Windows.Device
{
    /// <summary>
    /// ConnectableDeviceListener allows for a class to receive messages about ConnectableDevice connection, disconnect, and update events.
    ///
    /// It also serves as a proxy for message handling when connecting and pairing with each of a ConnectableDevice's DeviceServices. Each of the DeviceService proxy methods are optional and would only be useful in a few use cases.
    /// - providing your own UI for the pairing process.
    /// - interacting directly and exclusively with a single type of DeviceService
    /// </summary>
    public interface IConnectableDeviceListener
    {
        /// <summary>
        /// A ConnectableDevice sends out a ready message when all of its connectable DeviceServices have been connected and are ready to receive commands.
        /// </summary>
        /// <param name="device">ConnectableDevice that is ready for commands</param>
        void OnDeviceReady(ConnectableDevice device);

        /// <summary>
        /// When all of a ConnectableDevice's DeviceServices have become disconnected, the disconnected message is sent.
        /// </summary>
        /// <param name="device">ConnectableDevice that has been disconnected.</param>
        void OnDeviceDisconnected(ConnectableDevice device);

        /// <summary>
        /// DeviceService listener proxy method.
        /// This method is called when a DeviceService tries to connect and finds out that it requires pairing information from the user.
        /// </summary>
        /// <param name="device">ConnectableDevice containing the DeviceService</param>
        /// <param name="service">DeviceService that requires pairing</param>
        /// <param name="pairingType">DeviceServicePairingType that the DeviceService requires</param>
        void OnPairingRequired(ConnectableDevice device, DeviceService service, PairingType pairingType);

        /// <summary>
        /// When a ConnectableDevice finds & loses DeviceServices, that ConnectableDevice will experience a change in its collective capabilities list. When such a change occurs, this message will be sent with arrays of capabilities that were added & removed.
        /// This message will allow you to decide when to stop/start interacting with a ConnectableDevice, based off of its supported capabilities.
        /// </summary>
        /// <param name="device">ConnectableDevice that has experienced a change in capabilities</param>
        /// <param name="added">Capabilities that are new to the ConnectableDevice</param>
        /// <param name="removed">Capabilities that the ConnectableDevice has lost</param>
        void OnCapabilityUpdated(ConnectableDevice device, List<string> added, List<string> removed);

        /// <summary>
        /// This method is called when the connection to the ConnectableDevice has failed.
        /// </summary>
        /// <param name="device">ConnectableDevice that has failed to connect</param>
        /// <param name="error">ServiceCommandError with a description of the failure</param>
        void OnConnectionFailed(ConnectableDevice device, ServiceCommandError error);

    }
}
