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
            get { return playCommand ?? (playCommand = new Command(PlayCommandExecute) { Enabled = true }); }
            set { playCommand = value; }
        }

        public Command PauseCommand
        {
            get { return pauseCommand ?? (pauseCommand = new Command(PauseCommandExecute) { Enabled = true }); }
            set { pauseCommand = value; }
        }

        public Command StopCommand
        {
            get { return stopCommand ?? (stopCommand = new Command(StopCommandExecute) { Enabled = true }); }
            set { stopCommand = value; }
        }

        public Command RewindCommand
        {
            get { return rewindCommand ?? (rewindCommand = new Command(RewindCommandExecute) { Enabled = true }); }
            set { rewindCommand = value; }
        }

        public Command FastForwardCommand
        {
            get { return fastForwardCommand ?? (fastForwardCommand = new Command(FastForwardCommandExecute) { Enabled = true }); }
            set { fastForwardCommand = value; }
        }

        public Command CloseCommand
        {
            get { return closeCommand ?? (closeCommand = new Command(CloseCommandExecute) { Enabled = true }); }
            set { closeCommand = value; }
        }









        public Command LaunchMediaPlayerCommand
        {
            get { return launchMediaPlayerCommand ?? (launchMediaPlayerCommand = new Command(LaunchMediaPlayerCommandExecute) { Enabled = selectedDevice.HasCapability(Launcher.Application) }); }
            set { launchMediaPlayerCommand = value; }
        }
    }
}
