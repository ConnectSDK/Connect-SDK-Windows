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
    public partial class Model
    {
        private Command powerCommand;
        private Command inputCommand;
        private Command qMenuCommand;
        private Command ratioCommand;
        private Command helpCommand;
        private Command oneCommand;
        private Command twoCommand;
        private Command threeCommand;
        private Command fourCommand;
        private Command fiveCommand;
        private Command sixCommand;
        private Command sevenCommand;
        private Command eightCommand;
        private Command nineCommand;
        private Command guideCommand;
        private Command zeroCommand;
        private Command qViewCommand;

        private Command volumeUpCommand;
        private Command volumeDownCommand;
        private Command channelUpCommand;
        private Command channelDownCommand;
        private Command favoritesCommand;
        private Command infoCommand;
        private Command muteCommand;

        private Command recentCommand;
        private Command smartCommand;
        private Command myAppsCommand;
        private Command upCommand;
        private Command leftCommand;
        private Command okCommand;
        private Command rightCommand;
        private Command downCommand;
        private Command backCommand;
        private Command liveCommand;
        private Command exitCommand;

        private Command redCommand;
        private Command greenCommand;
        private Command yellowCommand;
        private Command blueCommand;

        private Command textCommand;
        private Command toptCommand;
        private Command appCommand;
        private Command stopCommand;
        private Command playCommand;
        private Command pauseCommand;
        private Command rewCommand;
        private Command ffCommand;
        private Command recCommand;
        private Command subCommand;
        private Command aDCommand;
        private Command tVradCommand;

        public Command PowerCommand
        {
            get { return deleteCommand ?? (deleteCommand = new Command(Delete)); ; }
            set { powerCommand = value; }
        }

        public Command InputCommand
        {
            get { return deleteCommand ?? (deleteCommand = new Command(Delete)); ; }
            set { inputCommand = value; }
        }

        public Command QMenuCommand
        {
            get { return deleteCommand ?? (deleteCommand = new Command(Delete)); ; }
            set { qMenuCommand = value; }
        }

        public Command RatioCommand
        {
            get { return deleteCommand ?? (deleteCommand = new Command(Delete)); ; }
            set { ratioCommand = value; }
        }

        public Command HelpCommand
        {
            get { return deleteCommand ?? (deleteCommand = new Command(Delete)); ; }
            set { helpCommand = value; }
        }

        public Command OneCommand
        {
            get { return deleteCommand ?? (deleteCommand = new Command(Delete)); ; }
            set { oneCommand = value; }
        }

        public Command TwoCommand
        {
            get { return deleteCommand ?? (deleteCommand = new Command(Delete)); ; }
            set { twoCommand = value; }
        }

        public Command ThreeCommand
        {
            get { return deleteCommand ?? (deleteCommand = new Command(Delete)); ; }
            set { threeCommand = value; }
        }

        public Command FourCommand
        {
            get { return deleteCommand ?? (deleteCommand = new Command(Delete)); ; }
            set { fourCommand = value; }
        }

        public Command FiveCommand
        {
            get { return deleteCommand ?? (deleteCommand = new Command(Delete)); ; }
            set { fiveCommand = value; }
        }

        public Command SixCommand
        {
            get { return deleteCommand ?? (deleteCommand = new Command(Delete)); ; }
            set { sixCommand = value; }
        }

        public Command SevenCommand
        {
            get { return deleteCommand ?? (deleteCommand = new Command(Delete)); ; }
            set { sevenCommand = value; }
        }

        public Command EightCommand
        {
            get { return deleteCommand ?? (deleteCommand = new Command(Delete)); ; }
            set { eightCommand = value; }
        }

        public Command NineCommand
        {
            get { return deleteCommand ?? (deleteCommand = new Command(Delete)); ; }
            set { nineCommand = value; }
        }

        public Command GuideCommand
        {
            get { return deleteCommand ?? (deleteCommand = new Command(Delete)); ; }
            set { guideCommand = value; }
        }

        public Command ZeroCommand
        {
            get { return deleteCommand ?? (deleteCommand = new Command(Delete)); ; }
            set { zeroCommand = value; }
        }

        public Command QViewCommand
        {
            get { return deleteCommand ?? (deleteCommand = new Command(Delete)); ; }
            set { qViewCommand = value; }
        }

        public Command VolumeUpCommand
        {
            get { return deleteCommand ?? (deleteCommand = new Command(Delete)); ; }
            set { volumeUpCommand = value; }
        }

        public Command VolumeDownCommand
        {
            get { return deleteCommand ?? (deleteCommand = new Command(Delete)); ; }
            set { volumeDownCommand = value; }
        }

        public Command ChannelUpCommand
        {
            get { return deleteCommand ?? (deleteCommand = new Command(Delete)); ; }
            set { channelUpCommand = value; }
        }

        public Command ChannelDownCommand
        {
            get { return deleteCommand ?? (deleteCommand = new Command(Delete)); ; }
            set { channelDownCommand = value; }
        }

        public Command FavoritesCommand
        {
            get { return deleteCommand ?? (deleteCommand = new Command(Delete)); ; }
            set { favoritesCommand = value; }
        }

        public Command InfoCommand
        {
            get { return deleteCommand ?? (deleteCommand = new Command(Delete)); ; }
            set { infoCommand = value; }
        }

        public Command MuteCommand
        {
            get { return deleteCommand ?? (deleteCommand = new Command(Delete)); ; }
            set { muteCommand = value; }
        }

        public Command RecentCommand
        {
            get { return deleteCommand ?? (deleteCommand = new Command(Delete)); ; }
            set { recentCommand = value; }
        }

        public Command SmartCommand
        {
            get { return deleteCommand ?? (deleteCommand = new Command(Delete)); ; }
            set { smartCommand = value; }
        }

        public Command MyAppsCommand
        {
            get { return deleteCommand ?? (deleteCommand = new Command(Delete)); ; }
            set { myAppsCommand = value; }
        }

        public Command UpCommand
        {
            get { return deleteCommand ?? (deleteCommand = new Command(Delete)); ; }
            set { upCommand = value; }
        }

        public Command LeftCommand
        {
            get { return deleteCommand ?? (deleteCommand = new Command(Delete)); ; }
            set { leftCommand = value; }
        }

        public Command OkCommand
        {
            get { return deleteCommand ?? (deleteCommand = new Command(Delete)); ; }
            set { okCommand = value; }
        }

        public Command RightCommand
        {
            get { return deleteCommand ?? (deleteCommand = new Command(Delete)); ; }
            set { rightCommand = value; }
        }

        public Command DownCommand
        {
            get { return deleteCommand ?? (deleteCommand = new Command(Delete)); ; }
            set { downCommand = value; }
        }

        public Command BackCommand
        {
            get { return deleteCommand ?? (deleteCommand = new Command(Delete)); ; }
            set { backCommand = value; }
        }

        public Command LiveCommand
        {
            get { return deleteCommand ?? (deleteCommand = new Command(Delete)); ; }
            set { liveCommand = value; }
        }

        public Command ExitCommand
        {
            get { return deleteCommand ?? (deleteCommand = new Command(Delete)); ; }
            set { exitCommand = value; }
        }

        public Command RedCommand
        {
            get { return deleteCommand ?? (deleteCommand = new Command(Delete)); ; }
            set { redCommand = value; }
        }

        public Command GreenCommand
        {
            get { return deleteCommand ?? (deleteCommand = new Command(Delete)); ; }
            set { greenCommand = value; }
        }

        public Command YellowCommand
        {
            get { return deleteCommand ?? (deleteCommand = new Command(Delete)); ; }
            set { yellowCommand = value; }
        }

        public Command BlueCommand
        {
            get { return deleteCommand ?? (deleteCommand = new Command(Delete)); ; }
            set { blueCommand = value; }
        }

        public Command TextCommand
        {
            get { return deleteCommand ?? (deleteCommand = new Command(Delete)); ; }
            set { textCommand = value; }
        }

        public Command ToptCommand
        {
            get { return deleteCommand ?? (deleteCommand = new Command(Delete)); ; }
            set { toptCommand = value; }
        }

        public Command AppCommand
        {
            get { return deleteCommand ?? (deleteCommand = new Command(Delete)); ; }
            set { appCommand = value; }
        }

        public Command StopCommand
        {
            get { return deleteCommand ?? (deleteCommand = new Command(Delete)); ; }
            set { stopCommand = value; }
        }

        public Command PlayCommand
        {
            get { return deleteCommand ?? (deleteCommand = new Command(Delete)); ; }
            set { playCommand = value; }
        }

        public Command PauseCommand
        {
            get { return deleteCommand ?? (deleteCommand = new Command(Delete)); ; }
            set { pauseCommand = value; }
        }

        public Command RewCommand
        {
            get { return deleteCommand ?? (deleteCommand = new Command(Delete)); ; }
            set { rewCommand = value; }
        }

        public Command FfCommand
        {
            get { return deleteCommand ?? (deleteCommand = new Command(Delete)); ; }
            set { ffCommand = value; }
        }

        public Command RecCommand
        {
            get { return deleteCommand ?? (deleteCommand = new Command(Delete)); ; }
            set { recCommand = value; }
        }

        public Command SubCommand
        {
            get { return deleteCommand ?? (deleteCommand = new Command(Delete)); ; }
            set { subCommand = value; }
        }

        public Command ADCommand
        {
            get { return deleteCommand ?? (deleteCommand = new Command(Delete)); ; }
            set { aDCommand = value; }
        }

        public Command TVradCommand
        {
            get { return deleteCommand ?? (deleteCommand = new Command(Delete)); ; }
            set { tVradCommand = value; }
        }
    }

    public partial class Model : INotifyPropertyChanged
    {
        private IndependentList<TvDefinition> knownTvs;
        private Command deleteCommand;
        private Command executeCommand;


        private string textInput;
        private bool touchEnabled;

        private SdkConnector sdkConnector;

        public string IpAddress;
        public string Port;
        private ConnectableDevice selectedDevice;

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

        public ConnectableDevice SelectedDevice
        {
            get { return selectedDevice; }
            set { selectedDevice = value; OnPropertyChanged("CanEnterKey");}
        }

        public Visibility CanEnterKey
        {
            get { return this.SelectedDevice != null ? Visibility.Visible : Visibility.Collapsed; }
        }

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

        public IndependentList<AppInfo> Apps { get; set; }
        public IndependentList<ChannelInfo> Channels { get; set; }

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
