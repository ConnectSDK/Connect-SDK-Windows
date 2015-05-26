using System;
using Windows.UI.Popups;
using ConnectSdk.Windows.Device;
using ConnectSdk.Windows.Device.Netcast;
using ConnectSdk.Windows.Service;
using ConnectSdk.Windows.Service.Capability;
using ConnectSdk.Windows.Service.Capability.Listeners;
using ConnectSdk.Windows.Service.Command;
using ConnectSdk.Windows.Service.NetCast;
using ConnectSdk.Windows.Service.Sessions;

namespace ConnectSdk.Demo.Demo
{
    public partial class Model
    {
        private ConnectableDevice mTv;
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
        private WebOsWebAppSession launchSession;

        private void SetControls()
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
                webAppLauncher = null;
            }
            else
            {
                launcher = selectedDevice.GetControl<ILauncher>();
                mediaPlayer = selectedDevice.GetControl<IMediaPlayer>();
                mediaControl = selectedDevice.GetControl<IMediaControl>();
                tvControl = selectedDevice.GetControl<ITvControl>();
                volumeControl = selectedDevice.GetControl<IVolumeControl>();
                toastControl = selectedDevice.GetControl<IToastControl>();
                textInputControl = selectedDevice.GetControl<ITextInputControl>();
                mouseControl = selectedDevice.GetControl<IMouseControl>();
                externalInputControl = selectedDevice.GetControl<IExternalInputControl>();
                powerControl = selectedDevice.GetControl<IPowerControl>();
                keyControl = selectedDevice.GetControl<IKeyControl>();
                webAppLauncher = selectedDevice.GetControl<IWebAppLauncher>();
            }
        }

        private void ShowFotoCommandExecute(object obj)
        {
            const string imagePath = "http://ec2-54-201-108-205.us-west-2.compute.amazonaws.com/samples/media/photo.jpg";
            const string mimeType = "image/jpeg";
            const string title = "Sintel Character Design";
            const string description = "Blender Open Movie Project";
            const string icon = "http://ec2-54-201-108-205.us-west-2.compute.amazonaws.com/samples/media/photoIcon.jpg";

            var listener = new ResponseListener
                (
                    loadEventArg =>
                    {
                        var v = loadEventArg as LoadEventArgs;
                    },
                    serviceCommandError =>
                    {
                        var msg =
                            new MessageDialog(
                                "Something went wrong; The application could not be started. Press 'Close' to continue");
                        msg.ShowAsync();
                    }
                    );

            mediaPlayer.DisplayImage(imagePath, mimeType, title, description, icon, listener);
        }

        private void PlayMediaCommandxecute(object obj)
        {
            const string videoPath = "http://connectsdk.com/files/8913/9657/0225/test_video.mp4";
            const string mimeType = "video/mp4";
            const string title = "Sintel Trailer";
            const string description = "Blender Open Movie Project";
            const string icon = "http://www.connectsdk.com/files/7313/9657/0225/test_video_icon.jpg";

            var listener = new ResponseListener
                (
                    loadEventArg =>
                    {
                        var v = loadEventArg as LoadEventArgs;
                    },
                    serviceCommandError =>
                    {
                        var msg =
                            new MessageDialog(
                                "Something went wrong; The application could not be started. Press 'Close' to continue");
                        msg.ShowAsync();
                    }
                    );

            mediaPlayer.PlayMedia(videoPath, mimeType, title, description, icon, false, listener);
        }

        private void PlayAudioCommandxecute(object obj)
        {
            const string mediaUrl = "http://ec2-54-201-108-205.us-west-2.compute.amazonaws.com/samples/media/audio.mp3";
            const string mimeType = "audio/mp3";
            const string title = "The Song that Doesn't End";
            const string description = "Lamb Chop's Play Along";
            const string icon = "http://ec2-54-201-108-205.us-west-2.compute.amazonaws.com/samples/media/audioIcon.jpg";

            var listener = new ResponseListener
                (
                    loadEventArg =>
                    {
                        var v = loadEventArg as LoadEventArgs;
                    },
                    serviceCommandError =>
                    {
                        var msg =
                            new MessageDialog(
                                "Something went wrong; The application could not be started. Press 'Close' to continue");
                        msg.ShowAsync();
                    }
                    );

            mediaPlayer.PlayMedia(mediaUrl, mimeType, title, description, icon, false, listener);
        }


        private void LaunchMediaPlayerCommandExecute(object obj)
        {
            var webostvService = (WebOstvService)selectedDevice.GetServiceByName(WebOstvService.Id);
            const string webappname = "MediaPlayer";
            var listener = new ResponseListener
                (
                loadEventArg =>
                {
                    var v = loadEventArg as LoadEventArgs;
                    if (v != null)
                    {
                        launchSession = v.Load.GetPayload() as WebOsWebAppSession;
                        if (launchSession != null) launchSession.Connect(null);
                    }
                },
                serviceCommandError =>
                {
                    var msg =
                        new MessageDialog(
                            "Something went wrong; The application could not be started. Press 'Close' to continue");
                    msg.ShowAsync();
                }
                );

            webostvService.LaunchWebApp(webappname, listener);

        }

        private void PlayCommandExecute(object obj)
        {
            if (mediaControl != null)
                mediaControl.Play(null);
        }

        private void PauseCommandExecute(object obj)
        {
            if (mediaControl != null)
                mediaControl.Pause(null);
        }

        private void StopCommandExecute(object obj)
        {
            if (mediaControl != null)
                mediaControl.Stop(null);
        }

        private void RewindCommandExecute(object obj)
        {
            if (mediaControl != null)
                mediaControl.Rewind(null);
        }

        private void FastForwardCommandExecute(object obj)
        {
            if (mediaControl != null)
                mediaControl.FastForward(null);
        }

        private void CloseCommandExecute(object obj)
        {
            if (mediaControl != null)
                mediaControl.Stop(null);
        }
    }
}
