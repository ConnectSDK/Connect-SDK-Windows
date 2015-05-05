using System.ComponentModel;
using System.Runtime.CompilerServices;
using Windows.UI.Core;
using Windows.UI.Xaml;
using ConnectSdk.Demo.Annotations;
using ConnectSdk.Windows.Core;
using ConnectSdk.Windows.Device;
using ConnectSdk.Windows.Etc.Helper;
using ConnectSdk.Windows.Service;
using ConnectSdk.Windows.Service.Capability;
using UpdateControls.Collections;
using Windows.ApplicationModel.Core;
using UpdateControls.Fields;

namespace ConnectSdk.Demo.Demo
{
    public partial class Model : INotifyPropertyChanged
    {
        private ConnectableDevice selectedDevice;
        private string textInput;
        private string logContent;

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

        public IndependentList<DeviceServiceViewModel> DiscoverredTvList { get; set; }

        public IndependentList<AppInfo> Apps { get; set; }

        public IndependentList<ChannelInfo> Channels { get; set; }

        public string LogContent
        {
            get { return logContent; }
            set { logContent = value; OnPropertyChanged(); }
        }

        public Model()
        {
            DiscoverredTvList = new IndependentList<DeviceServiceViewModel>();
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

    public class DeviceServiceViewModel
    {
        public ConnectableDevice Device { get; set; }
        public DeviceService Service { get; set; }
    }
}
