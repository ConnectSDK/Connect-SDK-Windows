using System.Collections.Generic;
using Windows.UI.Core;
using ConnectSdk.Windows.Core;
using ConnectSdk.Windows.Service.Capability;
using ConnectSdk.Windows.Service.Capability.Listeners;

namespace ConnectSdk.Demo.Demo
{
    public partial class Model
    {

        private void SetControlKeys()
        {
            if (selectedDevice != null)
            {
                KeyCommand.Enabled = selectedDevice.HasCapability(KeyControl.KeyCode);
                ChannelCommand.Enabled = selectedDevice.HasCapability(TvControl.ChannelUp);
                PowerCommand.Enabled = selectedDevice.HasCapability(PowerControl.Off);

                if (selectedDevice.HasCapability(TvControl.ChannelList))
                {
                    var listener = new ResponseListener
                        (
                        loadEventArg =>
                        {
                            var channels = LoadEventArgs.GetValue<List<ChannelInfo>>(loadEventArg);
                            App.MainDispatcher.RunAsync(CoreDispatcherPriority.High, () =>
                            {
                                Channels = channels;
                            });
                        },
                        serviceCommandError =>
                        {

                        }
                        );
                    tvControl.GetChannelList(listener);
                }

                if (selectedDevice.HasCapability(TvControl.ChannelSubscribe))
                {
                    var listener = new ResponseListener
                        (
                        loadEventArg =>
                        {
                            var channel = LoadEventArgs.GetValue<ChannelInfo>(loadEventArg);
                            App.MainDispatcher.RunAsync(CoreDispatcherPriority.High, () =>
                            {
                                SelectedChannel = channel;
                            });
                        },
                        serviceCommandError =>
                        {

                        }
                        );
                    tvControl.SubscribeCurrentChannel(listener);
                }
            }


        }

        private void KeyCommandExecute(object obj)
        {
            var key = obj as string;
            switch (key)
            {
                case "0": keyControl.SendKeyCode(KeyCode.NUM_0, null); break;
                case "1": keyControl.SendKeyCode(KeyCode.NUM_1, null); break;
                case "2": keyControl.SendKeyCode(KeyCode.NUM_2, null); break;
                case "3": keyControl.SendKeyCode(KeyCode.NUM_3, null); break;
                case "4": keyControl.SendKeyCode(KeyCode.NUM_4, null); break;
                case "5": keyControl.SendKeyCode(KeyCode.NUM_5, null); break;
                case "6": keyControl.SendKeyCode(KeyCode.NUM_6, null); break;
                case "7": keyControl.SendKeyCode(KeyCode.NUM_7, null); break;
                case "8": keyControl.SendKeyCode(KeyCode.NUM_8, null); break;
                case "9": keyControl.SendKeyCode(KeyCode.NUM_9, null); break;
                case "-": keyControl.SendKeyCode(KeyCode.DASH, null); break;
                case "Enter": keyControl.SendKeyCode(KeyCode.ENTER, null); break;
            }
        }

        private void ChannelCommandExecute(object obj)
        {
            var key = obj as string;
            if (key == "ChUp") 
                tvControl.ChannelUp(null);
            else tvControl.ChannelDown(null);
        }

        private void PowerCommandExecute(object obj)
        {
            powerControl.PowerOff(null);
        }

        private void ThreeDCommandExecute(object obj)
        {
            tvControl.Set3DEnabled(true, null);
        }

        private void ChangeChannel(ChannelInfo channelInfo)
        {
            tvControl.SetChannel(channelInfo, null);
        }
    }
}

