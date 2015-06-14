using System.Collections.Generic;
using Windows.Data.Json;
using Windows.UI.Core;
using ConnectSdk.Windows.Core;
using ConnectSdk.Windows.Service.Capability;
using ConnectSdk.Windows.Service.Capability.Listeners;
using ConnectSdk.Windows.Service.Sessions;

namespace ConnectSdk.Demo.Demo
{
    public partial class Model
    {
        private LaunchSession inputPickerSession;

        private void SetControlSystem()
        {
            if (selectedDevice != null)
            {
                KeyCommand.Enabled = selectedDevice.HasCapability(KeyControl.KeyCode);
                InputCommand.Enabled = selectedDevice.HasCapability(ExternalInputControl.PickerLaunch);
                CanMute = selectedDevice.HasCapability(VolumeControl.MuteSet);

                PlayInputCommand.Enabled = selectedDevice.HasCapability(MediaControl.Play);
                PauseInputCommand.Enabled = selectedDevice.HasCapability(MediaControl.Pause);
                StopInputCommand.Enabled = selectedDevice.HasCapability(MediaControl.Stop);
                FastForwardInputCommand.Enabled = selectedDevice.HasCapability(MediaControl.FastForward);
                RewindInputCommand.Enabled = selectedDevice.HasCapability(MediaControl.Rewind);
                VolumeUpCommand.Enabled = selectedDevice.HasCapability(VolumeControl.VolumeUpDown);
                VolumeDownCommand.Enabled = selectedDevice.HasCapability(VolumeControl.VolumeUpDown);

                var muteListener = new ResponseListener
                    (
                    loadEventArg =>
                    {
                        var vm = LoadEventArgs.GetValueBool(loadEventArg);
                        App.MainDispatcher.RunAsync(CoreDispatcherPriority.High, () =>
                        {
                            IsMuted = vm;
                        });
                    },
                    serviceCommandError =>
                    {

                    }
                    );

                var externalInputListener = new ResponseListener
                    (
                    loadEventArg =>
                    {
                        var inputs = LoadEventArgs.GetValue<JsonArray>(loadEventArg);
                        var inputList = new List<ExternalInputInfo>();

                        foreach (var input in inputs)
                        {
                            var ei = ExternalInputInfo.FromJson(input.Stringify());
                            inputList.Add(ei);
                        }

                        App.MainDispatcher.RunAsync(CoreDispatcherPriority.High, () =>
                        {
                            ExternalInputs = inputList;
                        });
                        
                    },
                    serviceCommandError =>
                    {

                    }
                    );

                if (selectedDevice.HasCapability(VolumeControl.MuteSubscribe))
                    volumeControl.SubscribeMute(muteListener);

                if (selectedDevice.HasCapability(ExternalInputControl.List))
                {
                    externalInputControl.GetExternalInputList(externalInputListener);
                }
            }
        }

        private void PlayInputCommandExecute(object obj)
        {
            mediaControl.Play(null);
        }

        private void PauseInputCommandExecute(object obj)
        {
            mediaControl.Pause(null);
        }

        private void StopInputCommandExecute(object obj)
        {
            mediaControl.Stop(null);
        }

        private void RewindInputCommandExecute(object obj)
        {
            mediaControl.Rewind(null);
        }

        private void FastForwardInputCommandExecute(object obj)
        {
            mediaControl.FastForward(null);
        }

        private void InputCommandExecute(object obj)
        {
            var listener = new ResponseListener
                (
                loadEventArg =>
                {
                    var launchPickerSession = LoadEventArgs.GetValue<LaunchSession>(loadEventArg);
                    inputPickerSession = launchPickerSession;
                },
                serviceCommandError =>
                {

                }
                );
            externalInputControl.LaunchInputPicker(listener);
        }

        private void SetMute(bool muted)
        {
            volumeControl.SetMute(muted, null);
        }

        private void VolumeUpCommandExecute(object obj)
        {
            volumeControl.VolumeUp(null);
        }

        private void VolumeDownCommandExecute(object obj)
        {
            volumeControl.VolumeDown(null);
        }

        private void SetExternalInput(ExternalInputInfo externalInputInfo)
        {
            externalInputControl.SetExternalInput(externalInputInfo, null);
        }
    }
}

