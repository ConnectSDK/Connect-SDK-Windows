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
using Microsoft.VisualBasic.CompilerServices;

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
        private IPlayListControl playListControl;
        private WebOsWebAppSession launchSession;
        private bool isPlaying;
        private bool isPlayingImage;
        private ResponseListener playStateListener;
        private ResponseListener durationListener;

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

            if (!isPlaying || !isPlayingImage)
                DisableMedia();
            if (isPlaying) EnableMedia();

            playStateListener = new ResponseListener
                (
                loadEventArg =>
                {
                    var v = loadEventArg as LoadEventArgs;
                    if (v.Load.GetPayload() != null)
                    {
                        var ps = v.Load.GetPayload() is PlayStateStatus ? (PlayStateStatus)v.Load.GetPayload() : PlayStateStatus.Unknown;
                        if (ps != null)
                        {
                            switch (ps)
                            {
                                case PlayStateStatus.Playing:
                                    //startUpdating();

                                    if (mediaControl != null && selectedDevice.HasCapability(MediaControl.Duration))
                                    {
                                        mediaControl.GetDuration(durationListener);
                                    }
                                    break;
                                case PlayStateStatus.Finished:
                                //positionTextView.setText("--:--");
                                //durationTextView.setText("--:--");
                                //mSeekBar.setProgress(0);

                                default:
                                    //stopUpdating();
                                    break;
                            }
                        }
                    }
                },
                serviceCommandError =>
                {

                }
                );

            durationListener = new ResponseListener
                (
                loadEventArg =>
                {
                    var v = loadEventArg as LoadEventArgs;
                    if (v.Load.GetPayload() != null)
                    {
                        var d = v.Load.GetPayload() is long ? (long)v.Load.GetPayload() : 0;
                        totalTimeDuration = d;
                        //mSeekBar.setMax(duration.intValue());
                        //durationTextView.setText(formatTime(duration.intValue()));
                    }
                },
                serviceCommandError =>
                {

                }
                );

        }

        public void DisableMedia()
        {
            if (closeCommand != null)
                closeCommand.Enabled = false;
            StopMedia();
        }

        public void StopMedia()
        {
            if (playCommand != null)
            {
                playCommand.Enabled = false;
                pauseCommand.Enabled = false;
                stopCommand.Enabled = false;
                rewindCommand.Enabled = false;
                fastForwardCommand.Enabled = false;
                //previousButton.Enabled = false;
                //nextButton.setEnabled(false);
                //jumpButton.setEnabled(false);

                //mSeekBar.setEnabled(false);
                //mSeekBar.setOnSeekBarChangeListener(null);
                //mSeekBar.setProgress(0);

                //positionTextView.setText("--:--:--");
                //durationTextView.setText("--:--:--");

                totalTimeDuration = -1;
            }
        }

        private void EnableMedia()
        {
            playCommand.Enabled = selectedDevice.HasCapability(MediaControl.Play);
            pauseCommand.Enabled = selectedDevice.HasCapability(MediaControl.Pause);
            stopCommand.Enabled = selectedDevice.HasCapability(MediaControl.Stop);
            rewindCommand.Enabled = selectedDevice.HasCapability(MediaControl.Rewind);
            fastForwardCommand.Enabled = selectedDevice.HasCapability(MediaControl.FastForward);
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
                    isPlayingImage = true;
                },
                serviceCommandError =>
                {
                    var msg =
                        new MessageDialog(
                            "Something went wrong; The application could not be started. Press 'Close' to continue");
                    msg.ShowAsync();
                    DisableMedia();
                    isPlaying = isPlayingImage = false;
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
                    //todo: add implementation here
                    if (v != null)
                    {
                        EnableMedia();

                        if (selectedDevice.HasCapability(MediaControl.PlayStateSubscribe))
                        {
                            mediaControl.SubscribePlayState(playStateListener);
                        }
                        EnableMedia();
                        isPlaying = true;
                    }
                },
                serviceCommandError =>
                {
                    var msg =
                        new MessageDialog(
                            "Something went wrong; The application could not be started. Press 'Close' to continue");
                    msg.ShowAsync();
                    DisableMedia();
                    isPlaying = isPlayingImage = false;
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
                    EnableMedia();
                    isPlaying = true;
                },
                serviceCommandError =>
                {
                    var msg =
                        new MessageDialog(
                            "Something went wrong; The application could not be started. Press 'Close' to continue");
                    msg.ShowAsync();
                    DisableMedia();
                    isPlaying = isPlayingImage = false;
                }
                );

            mediaPlayer.PlayMedia(mediaUrl, mimeType, title, description, icon, false, listener);
        }


        private void LaunchMediaPlayerCommandExecute(object obj)
        {
            var webostvService = (WebOstvService) selectedDevice.GetServiceByName(WebOstvService.Id);
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

        private void PlayListCommandExecute(object obj)
        {
            const string mediaUrl =
                "http://ec2-54-201-108-205.us-west-2.compute.amazonaws.com/samples/media/example-m3u-playlist.m3u";
            const string mimeType = "application/x-mpegurl";
            const string title = "Playlist";
            const string description = "Playlist description";
            const string icon = "http://ec2-54-201-108-205.us-west-2.compute.amazonaws.com/samples/media/audioIcon.jpg";

            var listener = new ResponseListener
                (
                loadEventArg =>
                {
                    var v = loadEventArg as LoadEventArgs;
                    SetEnabledMedia(true);
                },
                serviceCommandError =>
                {
                    var msg =
                        new MessageDialog(
                            "Error playing audio");
                    msg.ShowAsync();
                    SetEnabledMedia(false);
                }
                );

            mediaPlayer.PlayMedia(mediaUrl, mimeType, title, description, icon, false, listener);
        }

        private void PreviousCommandExecute(object obj)
        {
            if (playListControl != null)
                playListControl.Previous(null);
        }

        private void NextCommandExecute(object obj)
        {
            if (playListControl != null)
                playListControl.Next(null);
        }

        public void SetEnabledMedia(bool enabled)
        {
            playCommand.Enabled = selectedDevice.HasCapability(MediaControl.Play) && enabled;
            pauseCommand.Enabled = selectedDevice.HasCapability(MediaControl.Pause) && enabled;
            stopCommand.Enabled = selectedDevice.HasCapability(MediaControl.Stop) && enabled;
            rewindCommand.Enabled = selectedDevice.HasCapability(MediaControl.Rewind) && enabled;
            fastForwardCommand.Enabled = selectedDevice.HasCapability(MediaControl.FastForward) && enabled;
            closeCommand.Enabled = selectedDevice.HasCapability(MediaPlayer.Close) && enabled;
            previousCommand.Enabled = selectedDevice.HasCapability(PlaylistControl.Previous) && enabled;
            nextCommand.Enabled = selectedDevice.HasCapability(PlaylistControl.Next) && enabled;
            jumpCommand.Enabled = selectedDevice.HasCapability(PlaylistControl.JumpToTrack) && enabled;

        }

        private void JumpCommandExecute(object obj)
        {
            if (playListControl != null)
                playListControl.JumpToTrack((long)obj, null);
        }
    }
}
