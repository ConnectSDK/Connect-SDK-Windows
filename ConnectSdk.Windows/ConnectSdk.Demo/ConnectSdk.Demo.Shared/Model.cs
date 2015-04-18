using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using ConnectSdk.Demo.Annotations;
using ConnectSdk.Windows.Core;
using ConnectSdk.Windows.Device;
using ConnectSdk.Windows.Service.Capability;
using UpdateControls.Collections;
using UpdateControls.Fields;

namespace ConnectSdk.Demo.Demo
{
    public partial class Model : INotifyPropertyChanged
    {
        private IndependentList<TvDefinition> knownTvs;

        private string textInput;

        private ConnectableDevice selectedDevice;
        private IndependentList<AppInfo> apps;
        private IndependentList<ChannelInfo> channels;

        public IndependentList<TvDefinition> KnownTvs
        {
            get { return knownTvs; }
            set
            {
                if (Equals(value, knownTvs)) return;
                knownTvs = value;
            }
        }

        public TvDefinition SelectedTv { get; set; }

        public ConnectableDevice SelectedDevice
        {
            get { return selectedDevice; }
            set { selectedDevice = value;}
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


        //public bool TouchEnabled
        //{
        //    get { return touchEnabled; }
        //    set
        //    {
        //        touchEnabled = value;

        //        if (sdkConnector == null) sdkConnector = new SdkConnector(SelectedDevice);
        //        sdkConnector.EnableMouse("", true);
        //    }
        //}


        //public long UniqueId { get; set; }

        public IndependentList<ConnectableDevice> DiscoverredTvList { get; set; }

        public IndependentList<AppInfo> Apps
        {
            get
            {
                return apps;
            }
            set { apps = value; }
        }

        public IndependentList<ChannelInfo> Channels
        {
            get { return channels; }
            set { channels = value; }
        }

        public Model()
        {
            //ExecuteCommand.Enabled = true;
            DiscoverredTvList = new IndependentList<ConnectableDevice>();
            this.apps = new IndependentList<AppInfo>();
            this.channels = new IndependentList<ChannelInfo>();
        }


        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        public virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
