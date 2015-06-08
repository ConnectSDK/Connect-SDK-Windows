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
        private string duration;
        private string position;
        private long totalDuration;
        private long currentPosition;
        private double volume;
        private bool canChangeVolume;

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

        public string Duration
        {
            get { return duration; }
            set { duration = value; OnPropertyChanged();}
        }

        public string Position
        {
            get { return position; }
            set { position = value; OnPropertyChanged(); }
        }

        public long TotalDuration
        {
            get { return totalDuration; }
            set { totalDuration = value; OnPropertyChanged(); }
        }

        public long CurrentPosition
        {
            get { return currentPosition; }
            set { currentPosition = value; OnPropertyChanged(); }
        }

        public double Volume
        {
            get { return volume; }
            set { volume = value; OnPropertyChanged(); }
        }

        public bool CanChangeVolume
        {
            get { return canChangeVolume; }
            set { canChangeVolume = value; OnPropertyChanged(); }
        }

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
