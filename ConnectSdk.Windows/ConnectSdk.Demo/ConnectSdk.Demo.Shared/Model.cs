using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using ConnectSdk.Demo.Annotations;
using ConnectSdk.Windows.Core;
using ConnectSdk.Windows.Device;
using ConnectSdk.Windows.Service;
using ConnectSdk.Windows.Service.Capability;
using UpdateControls.Collections;

namespace ConnectSdk.Demo.Demo
{
    public partial class Model : INotifyPropertyChanged
    {
        private ConnectableDevice selectedDevice;
        private string textInput;
        private IndependentList<ConnectableDevice> discoverredDevices;

        public ConnectableDevice SelectedDevice
        {
            get { return selectedDevice; }
            set
            {
                selectedDevice = value;
                SetControls();
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

        public IndependentList<ConnectableDevice> DiscoverredDevices
        {
            get { return discoverredDevices; }
            set
            {
                discoverredDevices = value;
            }
        }

        public IndependentList<AppInfo> Apps { get; set; }

        public IndependentList<ChannelInfo> Channels { get; set; }

        public Model()
        {
            DiscoverredDevices = new IndependentList<ConnectableDevice>();
            Apps = new IndependentList<AppInfo>();
            Channels = new IndependentList<ChannelInfo>();
            selectedDevice = new ConnectableDevice();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        public virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }

        public void AddDevice(ConnectableDevice device)
        {
            discoverredDevices.Add(device);
        }
    }
}
