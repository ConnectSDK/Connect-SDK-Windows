using Windows.UI.Popups;
using ConnectSdk.Windows.Device.Netcast;
using ConnectSdk.Windows.Service.Capability;
using ConnectSdk.Windows.Service.Capability.Listeners;
using ConnectSdk.Windows.Service.Sessions;

namespace ConnectSdk.Demo.Demo
{
    public partial class Model
    {
        private bool muted;

        private void PowerExecute(object obj)
        {
            selectedDevice.GetControl<IPowerControl>().PowerOff(null);
        }

        private void InputExecute(object obj)
        {
            if (launchSession != null)
            {
                selectedDevice.GetControl<IExternalInputControl>().CloseInputPicker(launchSession, null);
                launchSession = null;
            }
            else
            {
                var responseListener = new ResponseListener
                    (
                    loadEventArg =>
                    {
                        launchSession = ((LoadEventArgs)loadEventArg).Load.GetPayload() as LaunchSession;
                    },
                    serviceCommandError => { }
                    );

                selectedDevice.GetControl<IExternalInputControl>().LaunchInputPicker(responseListener);
            }
        }
        private void QMenuExecute(object obj)
        {
            try
            {
                selectedDevice.GetControl<IKeyControl>().SendKeyCode((int)VirtualKeycodes.QuickMenu, null);
            }
            catch
            {
                var msgd = new MessageDialog("This function is not supported", "Info");
                msgd.ShowAsync();
            }
        }

        private void RatioExecute(object obj)
        {
            try
            {
                selectedDevice.GetControl<IKeyControl>().SendKeyCode((int)VirtualKeycodes.AspectRatio, null);
            }
            catch
            {
                var msgd = new MessageDialog("This function is not supported", "Info");
                msgd.ShowAsync();
            }
        }

        private void HelpExecute(object obj)
        {
            var msgd = new MessageDialog("This function is not supported", "Info");
            msgd.ShowAsync();
        }

        private void OneExecute(object obj)
        {
            selectedDevice.GetControl<ITextInputControl>().SendText("1");
        }

        private void TwoExecute(object obj)
        {
            selectedDevice.GetControl<ITextInputControl>().SendText("2");
        }

        private void ThreeExecute(object obj)
        {
            selectedDevice.GetControl<ITextInputControl>().SendText("3");
        }

        private void FourExecute(object obj)
        {
            selectedDevice.GetControl<ITextInputControl>().SendText("4");
        }

        private void FiveExecute(object obj)
        {
            selectedDevice.GetControl<ITextInputControl>().SendText("5");
        }

        private void SixExecute(object obj)
        {
            selectedDevice.GetControl<ITextInputControl>().SendText("6");
        }

        private void SevenExecute(object obj)
        {
            selectedDevice.GetControl<ITextInputControl>().SendText("7");
        }

        private void EightExecute(object obj)
        {
            selectedDevice.GetControl<ITextInputControl>().SendText("8");
        }

        private void NineExecute(object obj)
        {
            selectedDevice.GetControl<ITextInputControl>().SendText("9");
        }

        private void ZeroExecute(object obj)
        {
            selectedDevice.GetControl<ITextInputControl>().SendText("0");
        }

        private void GuideExecute(object obj)
        {
            var msgd = new MessageDialog("This function is not supported", "Info");
            msgd.ShowAsync();
        }

        private void QViewExecute(object obj)
        {
            var msgd = new MessageDialog("This function is not supported", "Info");
            msgd.ShowAsync();
        }

        private void VolumeUpExecute(object obj)
        {
            selectedDevice.GetControl<IVolumeControl>().VolumeUp(null);
        }

        private void VolumeDownExecute(object obj)
        {
            selectedDevice.GetControl<IVolumeControl>().VolumeDown(null);
        }

        private void ChannelUpExecute(object obj)
        {
            selectedDevice.GetControl<ITvControl>().ChannelUp(null);
        }

        private void ChannelDownExecute(object obj)
        {
            selectedDevice.GetControl<ITvControl>().ChannelDown(null);
        }

        private void FavoritesExecute(object obj)
        {
            var msgd = new MessageDialog("This function is not supported", "Info");
            msgd.ShowAsync();
        }

        private void InfoExecute(object obj)
        {
            var msgd = new MessageDialog("This function is not supported", "Info");
            msgd.ShowAsync();
        }

        private void MuteExecute(object obj)
        {
            muted = !muted;
            selectedDevice.GetControl<IVolumeControl>().SetMute(muted, null);
        }

        private void RecentExecute(object obj)
        {
            var msgd = new MessageDialog("This function is not supported", "Info");
            msgd.ShowAsync();
        }

        private void SmartExecute(object obj)
        {
            var msgd = new MessageDialog("This function is not supported", "Info");
            msgd.ShowAsync();
        }
    }
}
