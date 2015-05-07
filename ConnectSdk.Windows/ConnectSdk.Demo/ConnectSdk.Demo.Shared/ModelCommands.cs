using ConnectSdk.Windows.Service.Capability;
using ConnectSdk.Windows.Service.Sessions;

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
        private LaunchSession launchSession;

        public Command PowerCommand
        {
            get { return powerCommand ?? (powerCommand = new Command(PowerExecute) { Enabled = selectedDevice.HasCapability(PowerControl.Off) }); }
            set { powerCommand = value; }
        }

        public Command InputCommand
        {
            get { return inputCommand ?? (inputCommand = new Command(InputExecute) { Enabled = selectedDevice.HasCapability(ExternalInputControl.PickerLaunch) }); }
            set { inputCommand = value; }
        }

        public Command QMenuCommand
        {
            get
            {
                return qMenuCommand ??
                       (qMenuCommand =
                           new Command(QMenuExecute)
                           {
                               Enabled = false
                           });
            }
            set { qMenuCommand = value; }
        }

        public Command RatioCommand
        {
            get { return ratioCommand ?? (ratioCommand = new Command(RatioExecute) { Enabled = false }); }
            set { ratioCommand = value; }
        }

        public Command HelpCommand
        {
            get { return helpCommand ?? (helpCommand = new Command(HelpExecute) { Enabled = false }); }
            set { helpCommand = value; }
        }

        public Command OneCommand
        {
            get { return oneCommand ?? (oneCommand = new Command(OneExecute) { Enabled = selectedDevice.HasCapability(TextInputControl.Send) }); }
            set { oneCommand = value; }
        }

        public Command TwoCommand
        {
            get { return twoCommand ?? (twoCommand = new Command(TwoExecute) { Enabled = selectedDevice.HasCapability(TextInputControl.Send) }); }
            set { twoCommand = value; }
        }

        public Command ThreeCommand
        {
            get { return threeCommand ?? (threeCommand = new Command(ThreeExecute) { Enabled = selectedDevice.HasCapability(TextInputControl.Send) }); }
            set { threeCommand = value; }
        }

        public Command FourCommand
        {
            get { return fourCommand ?? (fourCommand = new Command(FourExecute) { Enabled = selectedDevice.HasCapability(TextInputControl.Send) }); }
            set { fourCommand = value; }
        }

        public Command FiveCommand
        {
            get { return fiveCommand ?? (fiveCommand = new Command(FiveExecute) { Enabled = selectedDevice.HasCapability(TextInputControl.Send) }); }
            set { fiveCommand = value; }
        }

        public Command SixCommand
        {
            get { return sixCommand ?? (sixCommand = new Command(SixExecute) { Enabled = selectedDevice.HasCapability(TextInputControl.Send) }); }
            set { sixCommand = value; }
        }

        public Command SevenCommand
        {
            get { return sevenCommand ?? (sevenCommand = new Command(SevenExecute) { Enabled = selectedDevice.HasCapability(TextInputControl.Send) }); }
            set { sevenCommand = value; }
        }

        public Command EightCommand
        {
            get { return eightCommand ?? (eightCommand = new Command(EightExecute) { Enabled = selectedDevice.HasCapability(TextInputControl.Send) }); }
            set { eightCommand = value; }
        }

        public Command NineCommand
        {
            get { return nineCommand ?? (nineCommand = new Command(NineExecute) { Enabled = selectedDevice.HasCapability(TextInputControl.Send) }); }
            set { nineCommand = value; }
        }

        public Command GuideCommand
        {
            get { return guideCommand ?? (guideCommand = new Command(GuideExecute) { Enabled = false }); }
            set { guideCommand = value; }
        }

        public Command ZeroCommand
        {
            get { return zeroCommand ?? (zeroCommand = new Command(ZeroExecute) { Enabled = selectedDevice.HasCapability(TextInputControl.Send) }); }
            set { zeroCommand = value; }
        }

        public Command QViewCommand
        {
            get { return qViewCommand ?? (qViewCommand = new Command(QViewExecute) { Enabled = false }); }
            set { qViewCommand = value; }
        }

        public Command VolumeUpCommand
        {
            get { return volumeUpCommand ?? (volumeUpCommand = new Command(VolumeUpExecute) { Enabled = selectedDevice.HasCapability(VolumeControl.VolumeUpDown) }); }
            set { volumeUpCommand = value; }
        }

        public Command VolumeDownCommand
        {
            get { return volumeDownCommand ?? (volumeDownCommand = new Command(VolumeDownExecute) { Enabled = selectedDevice.HasCapability(VolumeControl.VolumeUpDown) }); }
            set { volumeDownCommand = value; }
        }

        public Command ChannelUpCommand
        {
            get { return channelUpCommand ?? (channelUpCommand = new Command(ChannelUpExecute) { Enabled = selectedDevice.HasCapability(TvControl.ChannelUp) }); }
            set { channelUpCommand = value; }
        }

        public Command ChannelDownCommand
        {
            get { return channelDownCommand ?? (channelDownCommand = new Command(ChannelDownExecute) { Enabled = selectedDevice.HasCapability(TvControl.ChannelDown) }); }
            set { channelDownCommand = value; }
        }

        public Command FavoritesCommand
        {
            get { return favoritesCommand ?? (favoritesCommand = new Command(FavoritesExecute) { Enabled = false }); }
            set { favoritesCommand = value; }
        }

        public Command InfoCommand
        {
            get { return infoCommand ?? (infoCommand = new Command(InfoExecute) { Enabled = false }); }
            set { infoCommand = value; }
        }

        public Command MuteCommand
        { 
            get { return muteCommand ?? (muteCommand = new Command(MuteExecute) { Enabled = selectedDevice.HasCapability(VolumeControl.MuteSet) }); }
            set { muteCommand = value; }
        }

        public Command RecentCommand
        {
            get { return recentCommand ?? (recentCommand = new Command(RecentExecute) { Enabled = false }); }
            set { recentCommand = value; }
        }

        public Command SmartCommand
        {
            get { return smartCommand ?? (smartCommand = new Command(SmartExecute) { Enabled = selectedDevice.HasCapability(KeyControl.Home) }); }
            set { smartCommand = value; }
        }

        public Command MyAppsCommand
        {
            get { return myAppsCommand ?? (myAppsCommand = new Command(MyAppsExecute) {Enabled = false}); }
            set { myAppsCommand = value; }
        }

        public Command UpCommand
        {
            get { return upCommand ?? (upCommand = new Command(UpExecute) { Enabled = selectedDevice.HasCapability(KeyControl.Up) }); }
            set { upCommand = value; }
        }

        public Command LeftCommand
        {
            get { return leftCommand ?? (leftCommand = new Command(LeftExecute) { Enabled = selectedDevice.HasCapability(KeyControl.Left) }); }
            set { leftCommand = value; }
        }

        public Command OkCommand
        {
            get { return okCommand ?? (okCommand = new Command(OkExecute) { Enabled = selectedDevice.HasCapability(KeyControl.Ok) }); }
            set { okCommand = value; }
        }

        public Command RightCommand
        {
            get { return rightCommand ?? (rightCommand = new Command(RightExecute) { Enabled = selectedDevice.HasCapability(KeyControl.Right) }); }
            set { rightCommand = value; }
        }

        public Command DownCommand
        {
            get { return downCommand ?? (downCommand = new Command(DownExecute) { Enabled = selectedDevice.HasCapability(KeyControl.Down) }); }
            set { downCommand = value; }
        }

        public Command BackCommand
        {
            get { return backCommand ?? (backCommand = new Command(BackExecute) { Enabled = selectedDevice.HasCapability(KeyControl.Back) }); }
            set { backCommand = value; }
        }

        public Command LiveCommand
        {
            get { return liveCommand ?? (liveCommand = new Command(LiveExecute) { Enabled = false }); }
            set { liveCommand = value; }
        }

        public Command ExitCommand
        {
            get { return exitCommand ?? (exitCommand = new Command(ExitExecute) { Enabled = selectedDevice.HasCapability(KeyControl.Back) }); }
            set { exitCommand = value; }
        }

        public Command RedCommand
        {
            get { return redCommand ?? (redCommand = new Command(RedExecute) { Enabled = false }); }
            set { redCommand = value; }
        }

        public Command GreenCommand
        {
            get { return greenCommand ?? (greenCommand = new Command(GreenExecute) { Enabled = false }); }
            set { greenCommand = value; }
        }

        public Command YellowCommand
        {
            get { return yellowCommand ?? (yellowCommand = new Command(YellowExecute) { Enabled = false }); }
            set { yellowCommand = value; }
        }

        public Command BlueCommand
        {
            get { return blueCommand ?? (blueCommand = new Command(BlueExecute) { Enabled = false }); }
            set { blueCommand = value; }
        }

        public Command TextCommand
        {
            get { return textCommand ?? (textCommand = new Command(TextExecute) { Enabled = false }); }
            set { textCommand = value; }
        }

        public Command ToptCommand
        {
            get { return toptCommand ?? (toptCommand = new Command(TOptExecute) { Enabled = false }); }
            set { toptCommand = value; }
        }

        public Command AppCommand
        {
            get { return appCommand ?? (appCommand = new Command(AppExecute) { Enabled = false }); }
            set { appCommand = value; }
        }

        public Command StopCommand
        {
            get { return stopCommand ?? (stopCommand = new Command(StopExecute) { Enabled = selectedDevice.HasCapability(MediaControl.Stop) }); }
            set { stopCommand = value; }
        }

        public Command PlayCommand
        {
            get { return playCommand ?? (playCommand = new Command(PlayExecute) { Enabled = selectedDevice.HasCapability(MediaControl.Play) }); }
            set { playCommand = value; }
        }

        public Command PauseCommand
        {
            get { return pauseCommand ?? (pauseCommand = new Command(PauseExecute) { Enabled = selectedDevice.HasCapability(MediaControl.Pause) }); }
            set { pauseCommand = value; }
        }

        public Command RewCommand
        {
            get { return rewCommand ?? (rewCommand = new Command(RewExecute) { Enabled = selectedDevice.HasCapability(MediaControl.Rewind) }); }
            set { rewCommand = value; }
        }

        public Command FfCommand
        {
            get { return ffCommand ?? (ffCommand = new Command(FfExecute) { Enabled = selectedDevice.HasCapability(MediaControl.FastForward) }); }
            set { ffCommand = value; }
        }

        public Command RecCommand
        {
            get { return recCommand ?? (recCommand = new Command(RecExecute) { Enabled = false }); }
            set { recCommand = value; }
        }

        public Command SubCommand
        {
            get { return subCommand ?? (subCommand = new Command(SubExecute) { Enabled = false }); }
            set { subCommand = value; }
        }

        public Command AdCommand
        {
            get { return aDCommand ?? (aDCommand = new Command(AdExecute) { Enabled = false }); }
            set { aDCommand = value; }
        }

        public Command TvRadCommand
        {
            get { return tVradCommand ?? (tVradCommand = new Command(TvRadExecute) { Enabled = false }); }
            set { tVradCommand = value; }
        }
    }
}
