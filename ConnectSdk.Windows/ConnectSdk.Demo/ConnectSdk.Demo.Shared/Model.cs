using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using ConnectSdk.Demo.Annotations;
using ConnectSdk.Windows.Core;
using ConnectSdk.Windows.Device;
using UpdateControls.Collections;

namespace ConnectSdk.Demo.Demo
{
    public class Model : INotifyPropertyChanged
    {
        private IndependentList<TvDefinition> knownTvs;
        private Command deleteCommand;
        private Command executeCommand;
        private string textInput;
        private bool touchEnabled;

        private SdkConnector sdkConnector;

        public string IpAddress;
        public string Port;

        public IndependentList<TvDefinition> KnownTvs
        {
            get { return knownTvs; }
            set
            {
                if (Equals(value, knownTvs)) return;
                knownTvs = value;
            }
        }

        public Command DeleteCommand
        {
            get { return deleteCommand ?? (deleteCommand = new Command(Delete)); }
            set { deleteCommand = value; }
        }

        public Command ExecuteCommand
        {
            get { return executeCommand ?? (executeCommand = new Command(Execute)); }
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
                SendText(textInput);
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
            }
        }


        public long UniqueId { get; set; }

        public IndependentList<ConnectableDevice> DiscoverredTvList { get; set; }

        public List<AppInfo> Channels { get; set; }

        private void Delete(object param)
        {
            knownTvs.Clear();

        }

        private async void CloseApp()
        {
            var msgd = new MessageDialog("The TV is powering-off. This can take a few moments. This application will now close.", "Info");
            await msgd.ShowAsync();
            Application.Current.Exit();
        }

        private void Execute(object param)
        {
            if (sdkConnector == null) sdkConnector = new SdkConnector(SelectedDevice);

            sdkConnector.MakeCommand(param as string);

            var s = param as string;
            if (s != null && s.Equals("1"))
            {
                CloseApp();
            }
        }

        public Model()
        {
            ExecuteCommand.Enabled = true;
            DiscoverredTvList = new IndependentList<ConnectableDevice>();
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

        private void SendText(string text)
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
