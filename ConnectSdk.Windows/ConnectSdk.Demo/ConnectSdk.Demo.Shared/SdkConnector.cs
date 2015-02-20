using System;
using ConnectSdk.Windows.Device;
using ConnectSdk.Windows.Device.Netcast;
using ConnectSdk.Windows.Service.Capability;

namespace ConnectSdk.Demo.Demo
{
    public class SdkConnector
    {
        private readonly ConnectableDevice device;

        public SdkConnector()
        {
        }

        public SdkConnector(ConnectableDevice device)
        {
            this.device = device;
        }

        private IVolumeControl volumeControlDevice;
        private IKeyControl keyControlDevice;
        private IExternalInputControl externalInputControlDevice;
        private IMediaControl mediaControlDevice;
        private IPowerControl powerControlDevice;
        private ITvControl tvControlDevice;
        private IMouseControl mouseControlDevice;
        private ITextInputControl textControlDevice;

        public void MakeCommand(string key)
        {
            VirtualKeycodes code;

            if (volumeControlDevice == null)
                volumeControlDevice = device.GetControl<IVolumeControl>();
            if (keyControlDevice == null)
                keyControlDevice = device.GetControl<IKeyControl>();
            if (externalInputControlDevice == null)
                externalInputControlDevice = device.GetControl<IExternalInputControl>();
            if (mediaControlDevice == null)
                mediaControlDevice = device.GetControl<IMediaControl>();
            if (powerControlDevice == null)
                powerControlDevice = device.GetControl<IPowerControl>();
            if (tvControlDevice == null)
                tvControlDevice = device.GetControl<ITvControl>();

            if (Enum.TryParse(key, out code))
            {
                switch (code)
                {
                    case VirtualKeycodes.VolumeUp:
                        if (volumeControlDevice != null) volumeControlDevice.VolumeUp(null); break;
                    case VirtualKeycodes.VolumeDown:
                        if (volumeControlDevice != null) volumeControlDevice.VolumeDown(null); break;
                    case VirtualKeycodes.Mute:
                        if (volumeControlDevice != null) volumeControlDevice.SetMute(true, null); break;

                    case VirtualKeycodes.KeyUp:
                        if (keyControlDevice != null) keyControlDevice.Up(null); break;
                    case VirtualKeycodes.KeyDown:
                        if (keyControlDevice != null) keyControlDevice.Down(null); break;
                    case VirtualKeycodes.KeyLeft:
                        if (keyControlDevice != null) keyControlDevice.Left(null); break;
                    case VirtualKeycodes.KeyRight:
                        if (keyControlDevice != null) keyControlDevice.Right(null); break;
                    case VirtualKeycodes.Ok:
                        if (keyControlDevice != null) keyControlDevice.Ok(null); break;
                    case VirtualKeycodes.Back:
                        if (keyControlDevice != null) keyControlDevice.Back(null); break;
                    case VirtualKeycodes.Home:
                        if (keyControlDevice != null) keyControlDevice.Home(null); break;

                    //case VirtualKeycodes.EXTERNAL_INPUT:
                    //    if (externalInputControlDevice != null) externalInputControlDevice.launchInputPicker(null); break;

                    case VirtualKeycodes.Play:
                        if (mediaControlDevice != null) mediaControlDevice.Play(null); break;
                    case VirtualKeycodes.Pause:
                        if (mediaControlDevice != null) mediaControlDevice.Pause(null); break;
                    case VirtualKeycodes.Stop:
                        if (mediaControlDevice != null) mediaControlDevice.Stop(null); break;
                    case VirtualKeycodes.Rewind:
                        if (mediaControlDevice != null) mediaControlDevice.Rewind(null); break;
                    case VirtualKeycodes.FastForward:
                        if (mediaControlDevice != null) mediaControlDevice.FastForward(null); break;

                    case VirtualKeycodes.Power:
                        if (powerControlDevice != null) powerControlDevice.PowerOff(null); break;

                    case VirtualKeycodes.ChannelUp:
                        if (tvControlDevice != null) tvControlDevice.ChannelUp(null); break;
                    case VirtualKeycodes.ChannelDown:
                        if (tvControlDevice != null) tvControlDevice.ChannelDown(null); break;

                    default:
                        if (keyControlDevice != null)
                            keyControlDevice.SendKeyCode((int)code, null);
                        break;
                }
            }


        }

        public void EnableMouse(string url, bool state)
        {
        }



        public void GetChannelList(string url)
        {

        }

        public void Move(double x, double y)
        {
            if (mouseControlDevice == null)
                mouseControlDevice = device.GetControl<IMouseControl>();
            //if (!mouseControlDevice.)
                mouseControlDevice.ConnectMouse();
            mouseControlDevice.Move(x, y);
        }

        public void Tap()
        {
            if (mouseControlDevice == null)
                mouseControlDevice = device.GetControl<IMouseControl>();
            //if (!mouseControlDevice.MouseConnected())
                mouseControlDevice.ConnectMouse();
            mouseControlDevice.Click();
        }

        public void Scroll(ScrollDirection direction)
        {
            if (mouseControlDevice == null)
                mouseControlDevice = device.GetControl<IMouseControl>();
            //if (!mouseControlDevice.MouseConnected())
                mouseControlDevice.ConnectMouse();
            mouseControlDevice.Scroll(0, (int)direction);
        }


        public void SendText(string text)
        {
            if (textControlDevice == null)
                textControlDevice = device.GetControl<ITextInputControl>();
            textControlDevice.SendText(text);
        }
    }
}
