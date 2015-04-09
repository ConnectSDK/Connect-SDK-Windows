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
            get { return powerCommand ?? (powerCommand = new Command(PowerExecute)); }
            set { powerCommand = value; }
        }

        public Command InputCommand
        {
            get { return inputCommand ?? (inputCommand = new Command(InputExecute)); }
            set { inputCommand = value; }
        }

        public Command QMenuCommand
        {
            get { return qMenuCommand ?? (qMenuCommand = new Command(QMenuExecute)); }
            set { qMenuCommand = value; }
        }

        public Command RatioCommand
        {
            get { return ratioCommand ?? (ratioCommand = new Command(RatioExecute)); }
            set { ratioCommand = value; }
        }

        public Command HelpCommand
        {
            get { return helpCommand ?? (helpCommand = new Command(HelpExecute)); }
            set { helpCommand = value; }
        }

        public Command OneCommand
        {
            get { return oneCommand ?? (oneCommand = new Command(OneExecute)); }
            set { oneCommand = value; }
        }

        public Command TwoCommand
        {
            get { return twoCommand ?? (twoCommand = new Command(TwoExecute)); }
            set { twoCommand = value; }
        }

        public Command ThreeCommand
        {
            get { return threeCommand ?? (threeCommand = new Command(ThreeExecute)); }
            set { threeCommand = value; }
        }

        public Command FourCommand
        {
            get { return fourCommand ?? (fourCommand = new Command(FourExecute)); }
            set { fourCommand = value; }
        }

        public Command FiveCommand
        {
            get { return fiveCommand ?? (fiveCommand = new Command(FiveExecute)); }
            set { fiveCommand = value; }
        }

        public Command SixCommand
        {
            get { return sixCommand ?? (sixCommand = new Command(SixExecute)); }
            set { sixCommand = value; }
        }

        public Command SevenCommand
        {
            get { return sevenCommand ?? (sevenCommand = new Command(SevenExecute)); }
            set { sevenCommand = value; }
        }

        public Command EightCommand
        {
            get { return eightCommand ?? (eightCommand = new Command(EightExecute)); }
            set { eightCommand = value; }
        }

        public Command NineCommand
        {
            get { return nineCommand ?? (nineCommand = new Command(NineExecute)); }
            set { nineCommand = value; }
        }

        public Command GuideCommand
        {
            get { return guideCommand ?? (guideCommand = new Command(GuideExecute)); }
            set { guideCommand = value; }
        }

        public Command ZeroCommand
        {
            get { return zeroCommand ?? (zeroCommand = new Command(ZeroExecute)); }
            set { zeroCommand = value; }
        }

        public Command QViewCommand
        {
            get { return qViewCommand ?? (qViewCommand = new Command(QViewExecute)); }
            set { qViewCommand = value; }
        }

        public Command VolumeUpCommand
        {
            get { return volumeUpCommand ?? (volumeUpCommand = new Command(VolumeUpExecute)); }
            set { volumeUpCommand = value; }
        }

        public Command VolumeDownCommand
        {
            get { return volumeDownCommand ?? (volumeDownCommand = new Command(VolumeDownExecute)); }
            set { volumeDownCommand = value; }
        }

        public Command ChannelUpCommand
        {
            get { return channelUpCommand ?? (channelUpCommand = new Command(ChannelUpExecute)); }
            set { channelUpCommand = value; }
        }

        public Command ChannelDownCommand
        {
            get { return channelDownCommand ?? (channelDownCommand = new Command(ChannelDownExecute)); }
            set { channelDownCommand = value; }
        }

        public Command FavoritesCommand
        {
            get { return favoritesCommand ?? (favoritesCommand = new Command(FavoritesExecute)); }
            set { favoritesCommand = value; }
        }

        public Command InfoCommand
        {
            get { return infoCommand ?? (infoCommand = new Command(InfoExecute)); }
            set { infoCommand = value; }
        }

        public Command MuteCommand
        {
            get { return muteCommand ?? (muteCommand = new Command(MuteExecute)); }
            set { muteCommand = value; }
        }

        public Command RecentCommand
        {
            get { return recentCommand ?? (recentCommand = new Command(RecentExecute)); }
            set { recentCommand = value; }
        }

        public Command SmartCommand
        {
            get { return smartCommand ?? (smartCommand = new Command(SmartExecute)); }
            set { smartCommand = value; }
        }

        public Command MyAppsCommand
        {
            get { return myAppsCommand ?? (myAppsCommand = new Command(MyAppsExecute)); }
            set { myAppsCommand = value; }
        }

        public Command UpCommand
        {
            get { return upCommand ?? (upCommand = new Command(UpExecute)); }
            set { upCommand = value; }
        }

        public Command LeftCommand
        {
            get { return leftCommand ?? (leftCommand = new Command(LeftExecute)); }
            set { leftCommand = value; }
        }

        public Command OkCommand
        {
            get { return okCommand ?? (okCommand = new Command(OkExecute)); }
            set { okCommand = value; }
        }

        public Command RightCommand
        {
            get { return rightCommand ?? (rightCommand = new Command(RightExecute)); }
            set { rightCommand = value; }
        }

        public Command DownCommand
        {
            get { return downCommand ?? (downCommand = new Command(DownExecute)); }
            set { downCommand = value; }
        }

        public Command BackCommand
        {
            get { return backCommand ?? (backCommand = new Command(BackExecute)); }
            set { backCommand = value; }
        }

        public Command LiveCommand
        {
            get { return liveCommand ?? (liveCommand = new Command(LiveExecute)); }
            set { liveCommand = value; }
        }

        public Command ExitCommand
        {
            get { return exitCommand ?? (exitCommand = new Command(ExitExecute)); }
            set { exitCommand = value; }
        }

        public Command RedCommand
        {
            get { return redCommand ?? (redCommand = new Command(RedExecute)); }
            set { redCommand = value; }
        }

        public Command GreenCommand
        {
            get { return greenCommand ?? (greenCommand = new Command(GreenExecute)); }
            set { greenCommand = value; }
        }

        public Command YellowCommand
        {
            get { return yellowCommand ?? (yellowCommand = new Command(YellowExecute)); }
            set { yellowCommand = value; }
        }

        public Command BlueCommand
        {
            get { return blueCommand ?? (blueCommand = new Command(BlueExecute)); }
            set { blueCommand = value; }
        }

        public Command TextCommand
        {
            get { return textCommand ?? (textCommand = new Command(TextExecute)); }
            set { textCommand = value; }
        }

        public Command ToptCommand
        {
            get { return toptCommand ?? (toptCommand = new Command(TOptExecute)); }
            set { toptCommand = value; }
        }

        public Command AppCommand
        {
            get { return appCommand ?? (appCommand = new Command(AppExecute)); }
            set { appCommand = value; }
        }

        public Command StopCommand
        {
            get { return stopCommand ?? (stopCommand = new Command(StopExecute)); }
            set { stopCommand = value; }
        }

        public Command PlayCommand
        {
            get { return playCommand ?? (playCommand = new Command(PlayExecute)); }
            set { playCommand = value; }
        }

        public Command PauseCommand
        {
            get { return pauseCommand ?? (pauseCommand = new Command(PauseExecute)); }
            set { pauseCommand = value; }
        }

        public Command RewCommand
        {
            get { return rewCommand ?? (rewCommand = new Command(RewExecute)); }
            set { rewCommand = value; }
        }

        public Command FfCommand
        {
            get { return ffCommand ?? (ffCommand = new Command(FfExecute)); }
            set { ffCommand = value; }
        }

        public Command RecCommand
        {
            get { return recCommand ?? (recCommand = new Command(RecExecute)); }
            set { recCommand = value; }
        }

        public Command SubCommand
        {
            get { return subCommand ?? (subCommand = new Command(SubExecute)); }
            set { subCommand = value; }
        }

        public Command ADCommand
        {
            get { return aDCommand ?? (aDCommand = new Command(ADExecute)); }
            set { aDCommand = value; }
        }

        public Command TVradCommand
        {
            get { return tVradCommand ?? (tVradCommand = new Command(TVRadExecute)); }
            set { tVradCommand = value; }
        }
    }
}
