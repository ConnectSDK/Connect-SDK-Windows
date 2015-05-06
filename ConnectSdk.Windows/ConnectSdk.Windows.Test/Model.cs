using System.Collections.Generic;
using ConnectSdk.Windows.Core;
using ConnectSdk.Windows.Device;
using ConnectSdk.Windows.Service;
using ConnectSdk.Windows.Service.Capability;

namespace ConnectSdk.Windows.Test
{
    public class Model
    {
        private ConnectableDevice selectedDevice;
        private string textInput;

        public ConnectableDevice SelectedDevice
        {
            get { return selectedDevice; }
            set
            {
                selectedDevice = value; 
            }
        }

        public string TextInput
        {
            get { return textInput; }
            set
            {
                if (Equals(value, textInput)) return;
                textInput = value;
                selectedDevice.GetControl<ITextInputControl>().SendText(textInput);
            }
        }

        public List<DeviceServiceViewModel> DiscoverredDeviceServices { get; set; }

        public List<AppInfo> Apps { get; set; }

        public List<ChannelInfo> Channels { get; set; }

        public Model()
        {
            DiscoverredDeviceServices = new List<DeviceServiceViewModel>();
            Apps = new List<AppInfo>();
            Channels = new List<ChannelInfo>();
            selectedDevice = new ConnectableDevice();
        }
    }

    public class DeviceServiceViewModel
    {
        public ConnectableDevice Device { get; set; }
        public DeviceService Service { get; set; }
    }
}
