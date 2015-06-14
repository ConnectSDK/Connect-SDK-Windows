using System;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using ConnectSdk.Windows.Service;
using ConnectSdk.Windows.Service.Capability;
using ConnectSdk.Windows.Service.Capability.Listeners;

namespace ConnectSdk.Demo.Demo
{
    public partial class Model
    {
        public void SetControlsMedia()
        {
            if (selectedDevice != null)
            {
                if (selectedDevice.HasCapability(VolumeControl.VolumeSet))
                {
                    CanChangeVolume = true;
                }
                if (selectedDevice.HasCapability(VolumeControl.VolumeGet))
                {
                    volumeControl.GetVolume(volumeListener);
                }
                if (selectedDevice.HasCapability(VolumeControl.VolumeSubscribe))
                {
                    volumeControl.SubscribeVolume(volumeListener);
                }
            }


            if (!isPlaying || !isPlayingImage)
                DisableMedia();
            if (isPlaying) EnableMedia();

            playStateListener = new ResponseListener
                (
                loadEventArg =>
                {
                    var v = loadEventArg as LoadEventArgs;
                    if (v != null && v.Load.GetPayload() != null)
                    {
                        var ps = v.Load.GetPayload() is PlayStateStatus ? (PlayStateStatus)v.Load.GetPayload() : PlayStateStatus.Unknown;
                        switch (ps)
                        {
                            case PlayStateStatus.Playing:
                                StartUpdating();

                                if (mediaControl != null && selectedDevice.HasCapability(MediaControl.Duration))
                                {
                                    mediaControl.GetDuration(durationListener);
                                }
                                break;
                            case PlayStateStatus.Finished:
                                App.MainDispatcher.RunAsync(CoreDispatcherPriority.High, () =>
                                {
                                    Duration = "--:--:--";
                                    Position = "--:--:--";
                                });

                                break;
                                //mSeekBar.setProgress(0);

                            default:
                                StopUpdating();
                                break;
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
                    if (v != null && v.Load.GetPayload() != null)
                    {
                        var d = v.Load.GetPayload() is double ? (double)v.Load.GetPayload() : 0;
                        var t = TimeSpan.FromMilliseconds(d);
                        App.MainDispatcher.RunAsync(CoreDispatcherPriority.High, () =>
                        {
                            Duration = string.Format("{0}:{1}:{2}", t.Minutes.ToString("D2"), t.Seconds.ToString("D2"), t.Milliseconds.ToString("D3"));
                            HasPositionInfo = true;
                            TotalDuration = (long)d;
                        });
                    }
                },
                serviceCommandError =>
                {

                }
                );
            
            positionListener = new ResponseListener
                (
                loadEventArg =>
                {
                    var v = loadEventArg as LoadEventArgs;
                    if (v != null && v.Load.GetPayload() != null)
                    {
                        var d = v.Load.GetPayload() is double ? (double)v.Load.GetPayload() : 0;
                        var t = TimeSpan.FromMilliseconds(d);
                        App.MainDispatcher.RunAsync(CoreDispatcherPriority.High, () =>
                        {
                            Position = string.Format("{0}:{1}:{2}", t.Minutes.ToString("D2"), t.Seconds.ToString("D2"), t.Milliseconds.ToString("D3"));
                            CurrentPosition = (long) d;
                        });
                    }
                },
                serviceCommandError =>
                {

                }
                );

            volumeListener = new ResponseListener
                (
                loadEventArg =>
                {
                    var v = loadEventArg as LoadEventArgs;
                    if (v != null && v.Load.GetPayload() != null)
                    {
                        var d = v.Load.GetPayload() is float ? (float)v.Load.GetPayload() : 0;
                        App.MainDispatcher.RunAsync(CoreDispatcherPriority.High, () =>
                        {
                            if (d <= 1)
                                Volume = d*100;
                            else
                                Volume = d;
                        });
                    }
                },
                serviceCommandError =>
                {

                }
                );
        }

        private void StartUpdating()
        {
            App.MainDispatcher.RunAsync(CoreDispatcherPriority.High, () =>
            {
                dispatcherTimer = new DispatcherTimer();
                dispatcherTimer.Tick += delegate
                {
                    if (dispatcherTimer == null) return;
                    if (mediaControl != null && selectedDevice != null &&
                        selectedDevice.HasCapability(MediaControl.Position))
                        mediaControl.GetPosition(positionListener);

                    //if (volumeControl != null && selectedDevice != null &&
                    //    selectedDevice.HasCapability(VolumeControl.VolumeGet))
                    //    volumeControl.GetVolume(volumeListener);

                    if (mediaControl != null && selectedDevice != null
                        && selectedDevice.HasCapability(MediaControl.Duration)
                        && selectedDevice.HasCapability(MediaControl.PlayStateSubscribe)
                        && totalTimeDuration <= 0)
                    {
                        mediaControl.GetDuration(durationListener);
                    }
                };
                dispatcherTimer.Interval = new TimeSpan(0, 0, 1);
                dispatcherTimer.Start();
            });
        }

        private void StopUpdating()
        {
            App.MainDispatcher.RunAsync(CoreDispatcherPriority.High, () =>
            {
                if (dispatcherTimer != null)
                {
                    dispatcherTimer.Stop();
                    dispatcherTimer = null;
                }
            });
        }

        public void DisableMedia()
        {
            if (closeCommand != null)
                closeCommand.Enabled = false;
            HasPositionInfo = false;
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
                previousCommand.Enabled = false;
                nextCommand.Enabled = false;
                closeCommand.Enabled = false;

                Duration = "--:--:--";
                Position = "--:--:--";

                HasPositionInfo = false;

                totalTimeDuration = -1;
            }
            HasPositionInfo = false;
        }

        private void EnableMedia()
        {
            playCommand.Enabled = selectedDevice.HasCapability(MediaControl.Play);
            pauseCommand.Enabled = selectedDevice.HasCapability(MediaControl.Pause);
            stopCommand.Enabled = selectedDevice.HasCapability(MediaControl.Stop);
            rewindCommand.Enabled = selectedDevice.HasCapability(MediaControl.Rewind);
            fastForwardCommand.Enabled = selectedDevice.HasCapability(MediaControl.FastForward);
            previousCommand.Enabled = selectedDevice.HasCapability(MediaControl.Previous);
            nextCommand.Enabled = selectedDevice.HasCapability(MediaControl.Next);
            closeCommand.Enabled = selectedDevice.HasCapability(MediaPlayer.Close);

            if (selectedDevice.HasCapability(MediaControl.PlayStateSubscribe) && !isPlaying)
            {
                mediaControl.SubscribePlayState(playStateListener);
            }
            else
            {
                if (mediaControl != null)
                {
                    mediaControl.GetDuration(durationListener);
                }
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
                    if (v != null)
                    {
                        var mlo = v.Load.GetPayload() as MediaLaunchObject;
                        if (mlo != null)
                        {
                            launchSession = mlo.LaunchSession;
                            mediaControl = mlo.MediaControl;
                            playListControl = mlo.PlaylistControl;
                            StopUpdating();
                            closeCommand.Enabled = true;
                            isPlayingImage = true;
                        }
                    }
                },
                serviceCommandError =>
                {
                    var msg =
                        new MessageDialog(
                            "Something went wrong; The application could not be started. Press 'Close' to continue");
                    msg.ShowAsync();
                    if (launchSession != null)
                    {
                        if (launchSession.Service is WebOstvService)
                            (launchSession.Service as WebOstvService).CloseApp(launchSession, null);
                        else
                            launchSession.Close(null);
                    }
                    launchSession = null;
                    StopUpdating();
                    DisableMedia();
                    isPlaying = isPlayingImage = false;
                }
                );

            mediaPlayer.DisplayImage(imagePath, mimeType, title, description, icon, listener);
        }

        private void PlayMediaCommandxecute(object obj)
        {
            const string videoPath = "http://ec2-54-201-108-205.us-west-2.compute.amazonaws.com/samples/media/video.mp4";
            const string mimeType = "video/mp4";
            const string title = "Sintel Trailer";
            const string description = "Blender Open Movie Project";
            const string icon = "http://ec2-54-201-108-205.us-west-2.compute.amazonaws.com/samples/media/videoIcon.jpg";

            var listener = new ResponseListener
                (
                loadEventArg =>
                {
                    var v = loadEventArg as LoadEventArgs;
                    if (v != null)
                    {
                        var mlo = v.Load.GetPayload() as MediaLaunchObject;
                        if (mlo != null)
                        {
                            launchSession = mlo.LaunchSession;
                            mediaControl = mlo.MediaControl;
                            playListControl = mlo.PlaylistControl;
                            StopUpdating();
                            EnableMedia();
                            isPlaying = true;
                        }
                    }
                },
                serviceCommandError =>
                {
                    var msg =
                        new MessageDialog(
                            "Something went wrong; The application could not be started. Press 'Close' to continue");
                    msg.ShowAsync();

                    if (launchSession != null)
                        launchSession.Close(null);
                    launchSession = null;
                    StopUpdating();
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
                    if (v != null)
                    {
                        var mlo = v.Load.GetPayload() as MediaLaunchObject;
                        if (mlo != null)
                        {
                            mediaControl = mlo.MediaControl;
                            launchSession = mlo.LaunchSession;
                            playListControl = mlo.PlaylistControl;
                            StopUpdating();
                            EnableMedia();
                            isPlaying = true;
                        }
                    }
                },
                serviceCommandError =>
                {
                    var msg =
                        new MessageDialog(
                            "Something went wrong; The application could not be started. Press 'Close' to continue");
                    msg.ShowAsync();
                    if (launchSession != null)
                        launchSession.Close(null);
                    launchSession = null;
                    StopUpdating();
                    DisableMedia();
                    isPlaying = isPlayingImage = false;
                }
                );

            mediaPlayer.PlayMedia(mediaUrl, mimeType, title, description, icon, false, listener);
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
            if (mediaPlayer != null)
            {
                DisableMedia();
                StopUpdating();

                if (launchSession != null)
                {
                    if (launchSession.Service is WebOstvService)
                        (launchSession.Service as WebOstvService).CloseWebApp(launchSession, null);
                    else
                        launchSession.Close(null);
                }

                //if (launchSession != null)
                //    launchSession.Close(null);
                launchSession = null;


            }
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
                    if (v != null)
                    {
                        var mlo = v.Load.GetPayload() as MediaLaunchObject;
                        if (mlo != null)
                        {
                            launchSession = mlo.LaunchSession;
                            mediaControl = mlo.MediaControl;
                            playListControl = mlo.PlaylistControl;
                            StopUpdating();
                            EnableMedia();
                            isPlaying = true;
                        }
                    }
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

        public void SetVolume(double newValue)
        {
            volumeControl.SetVolume((float)newValue, null);
        }


    }
}
