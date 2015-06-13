using Windows.UI.Xaml;
using ConnectSdk.Windows.Service.Capability;
using ConnectSdk.Windows.Service.Capability.Listeners;
using ConnectSdk.Windows.Service.Sessions;

namespace ConnectSdk.Demo.Demo
{
    public partial class Model
    {
        private ILauncher launcher;
        private IMediaPlayer mediaPlayer;
        private IMediaControl mediaControl;
        private ITvControl tvControl;
        private IVolumeControl volumeControl;
        private IToastControl toastControl;
        private IMouseControl mouseControl;
        private ITextInputControl textInputControl;
        private IPowerControl powerControl;
        private IExternalInputControl externalInputControl;
        private IKeyControl keyControl;
        private IWebAppLauncher webAppLauncher;
        private IPlayListControl playListControl;
        private LaunchSession launchSession;
        private bool isPlaying;
        private bool isPlayingImage;
        private ResponseListener playStateListener;
        private ResponseListener durationListener;
        private ResponseListener positionListener;
        private DispatcherTimer dispatcherTimer;
        private ResponseListener volumeListener;
        private long totalTimeDuration;
        private Command manipulationTappedCommand;

        public void SetControls()
        {
            if (selectedDevice == null)
            {
                launcher = null;
                mediaPlayer = null;
                mediaControl = null;
                tvControl = null;
                volumeControl = null;
                toastControl = null;
                textInputControl = null;
                mouseControl = null;
                externalInputControl = null;
                powerControl = null;
                keyControl = null;
                playListControl = null;
                webAppLauncher = null;
            }
            else
            {
                launcher = selectedDevice.GetCapability<ILauncher>();
                mediaPlayer = selectedDevice.GetCapability<IMediaPlayer>();
                mediaControl = selectedDevice.GetCapability<IMediaControl>();
                tvControl = selectedDevice.GetCapability<ITvControl>();
                volumeControl = selectedDevice.GetCapability<IVolumeControl>();
                toastControl = selectedDevice.GetCapability<IToastControl>();
                textInputControl = selectedDevice.GetCapability<ITextInputControl>();
                mouseControl = selectedDevice.GetCapability<IMouseControl>();
                externalInputControl = selectedDevice.GetCapability<IExternalInputControl>();
                powerControl = selectedDevice.GetCapability<IPowerControl>();
                keyControl = selectedDevice.GetCapability<IKeyControl>();
                playListControl = selectedDevice.GetCapability<IPlayListControl>();
                webAppLauncher = selectedDevice.GetCapability<IWebAppLauncher>();
            }

            SetControlsMedia();
            SetWebAppControls();
            SetControlControls();
            SetControlApps();
            SetControlKeys();
            SetControlSystem();
        }

    }
}
