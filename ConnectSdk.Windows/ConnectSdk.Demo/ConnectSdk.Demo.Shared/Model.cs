using System.ComponentModel;
using System.Runtime.CompilerServices;
using Windows.UI.Xaml;
using ConnectSdk.Demo.Annotations;
using ConnectSdk.Windows.Core;
using ConnectSdk.Windows.Device;
using ConnectSdk.Windows.Service.Capability;
using UpdateControls.Collections;

namespace ConnectSdk.Demo.Demo
{
    public partial class Model : INotifyPropertyChanged
    {
        private ConnectableDevice selectedDevice;
        private string textInput;

        public ConnectableDevice SelectedDevice
        {
            get { return selectedDevice; }
            set
            {
                selectedDevice = value; 
                OnPropertyChanged("IsNetcastVisible"); 
                OnPropertyChanged("ConnectButtonText");
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

        public IndependentList<ConnectableDevice> DiscoverredTvList { get; set; }

        public IndependentList<AppInfo> Apps { get; set; }

        public IndependentList<ChannelInfo> Channels { get; set; }

        public Visibility IsValidDeviceSelected
        {
            get
            {
                if (selectedDevice.DeviceType == null) 
                    return Visibility.Collapsed; 
                return Visibility.Visible;
            }
        }

        public Visibility IsNetcastVisible
        {
            get
            {
                if (selectedDevice == null) return Visibility.Collapsed;
                if (selectedDevice.DeviceType == "NetCast")
                    return Visibility.Visible;
                return Visibility.Collapsed;
            }
        }

        public string ConnectButtonText
        {
            get
            {
                if (selectedDevice == null || selectedDevice.DeviceType == null) return "";
                if (selectedDevice.DeviceType == "NetCast")
                    return "Send Pairing Key";
                return "Connect";
            }
        }

        public Model()
        {
            DiscoverredTvList = new IndependentList<ConnectableDevice>();
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
    }
}
