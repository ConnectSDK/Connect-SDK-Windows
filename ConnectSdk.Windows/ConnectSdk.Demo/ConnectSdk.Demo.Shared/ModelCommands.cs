﻿using ConnectSdk.Windows.Service.Capability;
using ConnectSdk.Windows.Service.Sessions;

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


        private Command launchMediaPlayerCommand;
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
    }
}
