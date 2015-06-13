using ConnectSdk.Windows.Service.Capability;

namespace ConnectSdk.Demo.Demo
{
    public partial class Model
    {
        private Command showFotoCommand;
        private Command playMediaCommand;
        private Command playAudioCommand;

        private Command playCommand;
        private Command pauseCommand;
        private Command stopCommand;
        private Command rewindCommand;
        private Command fastForwardCommand;
        private Command closeCommand;


        private Command playListCommand;
        private Command previousCommand;
        private Command nextCommand;
        private Command jumpCommand;


        private Command launchWebAppCommand;
        private Command joinWebAppCommand;
        private Command sendMessageCommand;
        private Command sendJsonCommand;
        private Command leaveWebAppCommand;
        private Command closeWebAppCommand;
        private Command pinWebAppCommand;
        private Command unPinWebAppCommand;

        private Command upCommand;
        private Command leftCommand;
        private Command clickCommand;
        private Command rightCommand;
        private Command backCommand;
        private Command downCommand;
        private Command homeCommand;
        private Command manipulationDeltaCommand;
        private Command googleCommand;
        private Command myDialAppCommand;
        private Command showToastCommand;
        private Command netflixCommand;
        private Command appStoreCommand;
        private Command youTubeCommand;
        private Command startAppCommand;
        private Command keyCommand;
        private Command channelCommand;
        private Command powerCommand;
        private Command threeDCommand;
        private Command playInputCommand;
        private Command pauseInputCommand;
        private Command stopInputCommand;
        private Command rewindInputCommand;
        private Command fastForwardInputCommand;
        private Command inputCommand;
        private Command volumeUpCommand;
        private Command volumeDownCommand;


        public Command ShowFotoCommand
        {
            get { return showFotoCommand ?? (showFotoCommand = new Command(ShowFotoCommandExecute) { Enabled = selectedDevice.HasCapability(MediaPlayer.DisplayImage)}); }
            set { showFotoCommand = value; }
        }

        public Command PlayMediaCommand
        {
            get { return playMediaCommand ?? (playMediaCommand = new Command(PlayMediaCommandxecute) { Enabled = selectedDevice.HasCapability(MediaPlayer.PlayVideo) }); }
            set { playMediaCommand = value; }
        }

        public Command PlayAudioCommand
        {
            get { return playAudioCommand ?? (playAudioCommand = new Command(PlayAudioCommandxecute) { Enabled = selectedDevice.HasCapability(MediaPlayer.PlayAudio) }); }
            set { playAudioCommand = value; }
        }



        public Command PlayCommand
        {
            get { return playCommand ?? (playCommand = new Command(PlayCommandExecute) { Enabled = false }); }
            set { playCommand = value; }
        }

        public Command PauseCommand
        {
            get { return pauseCommand ?? (pauseCommand = new Command(PauseCommandExecute) { Enabled = false }); }
            set { pauseCommand = value; }
        }

        public Command StopCommand
        {
            get { return stopCommand ?? (stopCommand = new Command(StopCommandExecute) { Enabled = false }); }
            set { stopCommand = value; }
        }

        public Command RewindCommand
        {
            get { return rewindCommand ?? (rewindCommand = new Command(RewindCommandExecute) { Enabled = false }); }
            set { rewindCommand = value; }
        }

        public Command FastForwardCommand
        {
            get { return fastForwardCommand ?? (fastForwardCommand = new Command(FastForwardCommandExecute) { Enabled = false }); }
            set { fastForwardCommand = value; }
        }

        public Command CloseCommand
        {
            get { return closeCommand ?? (closeCommand = new Command(CloseCommandExecute) { Enabled = false }); }
            set { closeCommand = value; }
        }

        public Command PlayListCommand
        {
            get { return playListCommand ?? (playListCommand = new Command(PlayListCommandExecute)
            {
                Enabled = selectedDevice.HasCapability(MediaPlayer.PlayPlaylist)
            }); }
            set { playListCommand = value; }
        }

        public Command PreviousCommand
        {
            get { return previousCommand ?? (previousCommand = new Command(PreviousCommandExecute) { Enabled = false }); }
            set { previousCommand = value; }
        }

        public Command NextCommand
        {
            get { return nextCommand ?? (nextCommand = new Command(NextCommandExecute) { Enabled = false }); }
            set { nextCommand = value; }
        }

        public Command JumpCommand
        {
            get { return jumpCommand ?? (jumpCommand = new Command(JumpCommandExecute) { Enabled = false }); }
            set { jumpCommand = value; }
        }



        public Command LaunchWebAppCommand
        {
            get { return launchWebAppCommand ?? (launchWebAppCommand = new Command(LaunchWebAppCommandExecute) { Enabled = false }); }
            set { launchWebAppCommand = value; }
        }

        public Command JoinWebAppCommand
        {
            get { return joinWebAppCommand ?? (joinWebAppCommand = new Command(JoinWebAppCommandExecute) { Enabled = false }); }
            set { joinWebAppCommand = value; }
        }


        public Command SendMessageCommand
        {
            get { return sendMessageCommand ?? (sendMessageCommand = new Command(SendMessageCommandExecute) { Enabled = false }); }
            set { sendMessageCommand = value; }
        }

        public Command SendJsonCommand
        {
            get { return sendJsonCommand ?? (sendJsonCommand = new Command(SendJsonCommandExecute) { Enabled = false }); }
            set { sendJsonCommand = value; }
        }

        public Command LeaveWebAppCommand
        {
            get { return leaveWebAppCommand ?? (leaveWebAppCommand = new Command(LeaveWebAppCommandExecute) { Enabled = false }); }
            set { leaveWebAppCommand = value; }
        }
        public Command CloseWebAppCommand
        {
            get { return closeWebAppCommand ?? (closeWebAppCommand = new Command(CloseWebAppCommandExecute) { Enabled = false }); }
            set { closeWebAppCommand = value; }
        }
        public Command PinWebAppCommand
        {
            get { return pinWebAppCommand ?? (pinWebAppCommand = new Command(PinWebAppCommandExecute) { Enabled = false }); }
            set { pinWebAppCommand = value; }
        }

        public Command UnPinWebAppCommand
        {
            get { return unPinWebAppCommand ?? (unPinWebAppCommand = new Command(UnPinWebAppCommandExecute) { Enabled = false }); }
            set { unPinWebAppCommand = value; }
        }

        public Command UpCommand
        {
            get { return upCommand ?? (upCommand = new Command(UpCommandExecute) { Enabled = false }); }
            set { upCommand = value; }
        }

        public Command LeftCommand
        {
            get { return leftCommand ?? (leftCommand = new Command(LeftCommandExecute) { Enabled = false }); }
            set { leftCommand = value; }
        }
        public Command ClickCommand
        {
            get { return clickCommand ?? (clickCommand = new Command(ClickCommandExecute) { Enabled = false }); }
            set { clickCommand = value; }
        }
        public Command RightCommand
        {
            get { return rightCommand ?? (rightCommand = new Command(RightCommandExecute) { Enabled = false }); }
            set { rightCommand = value; }
        }
        public Command BackCommand
        {
            get { return backCommand ?? (backCommand = new Command(BackCommandExecute) { Enabled = false }); }
            set { backCommand = value; }
        }
        public Command DownCommand
        {
            get { return downCommand ?? (downCommand = new Command(DownCommandExecute) { Enabled = false }); }
            set { downCommand = value; }
        }
        public Command HomeCommand
        {
            get { return homeCommand ?? (homeCommand = new Command(HomeCommandExecute) { Enabled = false }); }
            set { homeCommand = value; }
        }

        public Command ManipulationDeltaCommand
        {
            get { return manipulationDeltaCommand ?? (manipulationDeltaCommand = new Command(ManipulationDeltaCommandExecute) { Enabled = false }); }
            set { manipulationDeltaCommand = value; }
        }

        public Command ManipulationTappedCommand
        {
            get { return manipulationTappedCommand ?? (manipulationTappedCommand = new Command(ManipulationTappedCommandExecute) { Enabled = false }); }
            set { manipulationTappedCommand = value; }
        }

        public Command GoogleCommand
        {
            get { return googleCommand ?? (googleCommand = new Command(GoogleCommandExecute) { Enabled = false }); }
            set { googleCommand = value; }
        }

        public Command MyDialAppCommand
        {
            get { return myDialAppCommand ?? (myDialAppCommand = new Command(MyDialAppCommandExecute) { Enabled = false }); }
            set { myDialAppCommand = value; }
        }

        public Command ShowToastCommand
        {
            get { return showToastCommand ?? (showToastCommand = new Command(ShowToastCommandExecute) { Enabled = false }); }
            set { showToastCommand = value; }
        }

        public Command NetflixCommand
        {
            get { return netflixCommand ?? (netflixCommand = new Command(NetflixCommandExecute) { Enabled = false }); }
            set { netflixCommand = value; }
        }

        public Command AppStoreCommand
        {
            get { return appStoreCommand ?? (appStoreCommand = new Command(AppStoreCommandExecute) { Enabled = false }); }
            set { appStoreCommand = value; }
        }

        public Command YouTubeCommand
        {
            get { return youTubeCommand ?? (youTubeCommand = new Command(YouTubeCommandExecute) { Enabled = false }); }
            set { youTubeCommand = value; }
        }

        public Command StartAppCommand
        {
            get { return startAppCommand ?? (startAppCommand = new Command(StartApp) { Enabled = false }); }
            set { startAppCommand = value; }
        }

        public Command KeyCommand
        {
            get { return keyCommand ?? (keyCommand = new Command(KeyCommandExecute) { Enabled = false }); }
            set { keyCommand = value; }
        }

        public Command ChannelCommand
        {
            get { return channelCommand ?? (channelCommand = new Command(ChannelCommandExecute) { Enabled = false }); }
            set { channelCommand = value; }
        }

        public Command PowerCommand
        {
            get { return powerCommand ?? (powerCommand = new Command(PowerCommandExecute) { Enabled = false }); }
            set { powerCommand = value; }
        }

        public Command ThreeDCommand
        {
            get { return threeDCommand ?? (threeDCommand = new Command(ThreeDCommandExecute) { Enabled = false }); }
            set { threeDCommand = value; }
        }

        public Command PlayInputCommand
        {
            get { return playInputCommand ?? (playInputCommand = new Command(PlayInputCommandExecute) { Enabled = false }); }
            set { playInputCommand = value; }
        }

        public Command PauseInputCommand
        {
            get { return pauseInputCommand ?? (pauseInputCommand = new Command(PauseInputCommandExecute) { Enabled = false }); }
            set { pauseInputCommand = value; }
        }

        public Command StopInputCommand
        {
            get { return stopInputCommand ?? (stopInputCommand = new Command(StopInputCommandExecute) { Enabled = false }); }
            set { stopInputCommand = value; }
        }

        public Command RewindInputCommand
        {
            get { return rewindInputCommand ?? (rewindInputCommand = new Command(RewindInputCommandExecute) { Enabled = false }); }
            set { rewindInputCommand = value; }
        }

        public Command FastForwardInputCommand
        {
            get { return fastForwardInputCommand ?? (fastForwardInputCommand = new Command(FastForwardInputCommandExecute) { Enabled = false }); }
            set { fastForwardInputCommand = value; }
        }

        public Command InputCommand
        {
            get { return inputCommand ?? (inputCommand = new Command(InputCommandExecute) { Enabled = false }); }
            set { inputCommand = value; }
        }

        public Command VolumeUpCommand
        {
            get { return volumeUpCommand ?? (volumeUpCommand = new Command(VolumeUpCommandExecute) { Enabled = false }); }
            set { volumeUpCommand = value; }
        }

        public Command VolumeDownCommand
        {
            get { return volumeDownCommand ?? (volumeDownCommand = new Command(VolumeDownCommandExecute) { Enabled = false }); }
            set { volumeDownCommand = value; }
        }
    }
}
