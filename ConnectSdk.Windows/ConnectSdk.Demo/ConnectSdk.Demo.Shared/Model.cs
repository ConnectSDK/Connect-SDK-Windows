#if WINDOWS_PHONE_APP
using Windows.Phone.Devices.Notification;
#endif
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Xml.Serialization;
using Windows.UI.Popups;
using ConnectSdk.Demo.Annotations;
using ConnectSdk.Windows.Core;
using ConnectSdk.Windows.Device;
using MyRemote.Tablet.Model;
using MyRemote.Tablet.Tablet;

namespace ConnectSdk.Demo.Demo
{
    public class Model : INotifyPropertyChanged
    {
        private List<TvDefinition> knownTvs;
        private Command addCommand;
        private Command deleteCommand;
        private Command executeCommand;
        private string textInput;
        private bool touchEnabled;
        private bool textEnabled;

        private SdkConnector sdkConnector;

        public string IpAddress;
        public string Port;
        private ObservableCollection<ConnectableDevice> discoverredTvList;

        public List<TvDefinition> KnownTvs
        {
            get { return knownTvs; }
            set
            {
                if (Equals(value, knownTvs)) return;
                knownTvs = value;
            }
        }

        [XmlIgnore]
        public Command DeleteCommand
        {
            get
            {
                if (deleteCommand == null)
                    deleteCommand = new Command(Delete);
                return deleteCommand;
            }
            set { deleteCommand = value; }
        }

        [XmlIgnore]
        public Command ExecuteCommand
        {
            get
            {
                if (executeCommand == null)
                    executeCommand = new Command(Execute);
                return executeCommand;
            }
            set { executeCommand = value; }
        }

        public TvDefinition SelectedTv { get; set; }
        public ConnectableDevice SelectedDevice { get; set; }

        public string TextInput
        {
            get { return textInput; }
            set
            {
                if (Equals(value, textInput)) return;
                textInput = value;
                SendText(textInput, textEnabled);
                textEnabled = true;
            }
        }


        public bool TouchEnabled
        {
            get { return touchEnabled; }
            set
            {
                touchEnabled = value;

                if (sdkConnector == null) sdkConnector = new SdkConnector(SelectedDevice);
                sdkConnector.EnableMouse("", true);

                //connector.EnableMouse(this.SelectedTv.BaseUrl, touchEnabled);
            }
        }


        public bool TextEnabled
        {
            get { return touchEnabled; }
            set
            {
                textEnabled = value;
               
            }
        }
        public long UniqueId { get; set; }

        public ObservableCollection<ConnectableDevice> DiscoverredTvList
        {
            get { return discoverredTvList; }
            set { discoverredTvList = value; OnPropertyChanged();}
        }

        public List<AppInfo> Channels { get; set; }

        private void Delete(object param)
        {
            knownTvs.Clear();
            
        }

        private async void CloseApp()
        {
            MessageDialog msgd = new MessageDialog("The TV is powering-off. This can take a few moments. This application will now close.", "Info");
            var res = await msgd.ShowAsync();
            App.Current.Exit();
        }

        private void Execute(object param)
        {
            if (sdkConnector == null) sdkConnector = new SdkConnector(SelectedDevice);

            try
            {
#if WINDOWS_PHONE_APP
                if (VibrationOn)
                {
                    VibrationDevice vd = VibrationDevice.GetDefault();
                    vd.Vibrate(TimeSpan.FromSeconds(0.1));

                }
#endif
                sdkConnector.MakeCommand(param as string);

                if ((param as string).Equals("1"))
                {
                    //MessageDialog msgd = new MessageDialog("The TV is powering-off. This can take a few moments. This application will now close.", "Info");
                    //msgd.ShowAsync();
                    //App.Current.Exit();    
                    CloseApp();
                }
            }
            catch (Exception e)
            {
            }

        }

        public Model()
        {
            //var savedData = AppSettings.Current.KnownTvs;
            //if (!string.IsNullOrEmpty(savedData))
            //{
            //    TvListToSave savedModel = DeserializeObject<TvListToSave>(savedData);
            //    knownTvs = new IndependentList<TvDefinition>(savedModel.List);

            //}
            //else knownTvs = new IndependentList<TvDefinition>();

            ExecuteCommand.Enabled = true;
            DiscoverredTvList = new ObservableCollection<ConnectableDevice>();
        }

 
        public void Move(double x, double y)
        {
            if (sdkConnector == null) sdkConnector = new SdkConnector(SelectedDevice);
            sdkConnector.Move(x, y);
        }

        public void Tap()
        {
            if (sdkConnector == null) sdkConnector = new SdkConnector(SelectedDevice);
            sdkConnector.Tap();
        }

        public void ScrollUp()
        {
            if (sdkConnector == null) sdkConnector = new SdkConnector(SelectedDevice);
            sdkConnector.Scroll(ScrollDirection.Up);
        }

        public void ScrollDown()
        {
            if (sdkConnector == null) sdkConnector = new SdkConnector(SelectedDevice);
            sdkConnector.Scroll(ScrollDirection.Down);
        }

        private void SendText(string text, bool textEnabled)
        {

            if (sdkConnector == null) sdkConnector = new SdkConnector(SelectedDevice);
            sdkConnector.SendText(text);

        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
