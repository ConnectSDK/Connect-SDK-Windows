using ConnectSdk.Windows.Service.Capability;
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
            get { return playListCommand ?? (playListCommand = new Command(PlayListCommandExecute) { Enabled = true }); }
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






        public Command LaunchMediaPlayerCommand
        {
            get { return launchMediaPlayerCommand ?? (launchMediaPlayerCommand = new Command(LaunchMediaPlayerCommandExecute) { Enabled = selectedDevice.HasCapability(Launcher.Application) }); }
            set { launchMediaPlayerCommand = value; }
        }
    }
}
