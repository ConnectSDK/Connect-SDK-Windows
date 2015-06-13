using System;
using System.Collections.Generic;
using System.Text;
using Windows.UI.Xaml.Input;
using ConnectSdk.Windows.Service.Capability;

namespace ConnectSdk.Demo.Demo
{
    public partial class Model
    {
        public void DisableControlButtons()
        {
            
        }

        private void SetControlControls()
        {
            if (selectedDevice != null)
            {

                if (mouseControl != null)
                    mouseControl.ConnectMouse();

                UpCommand.Enabled = selectedDevice.HasCapability(KeyControl.Up);
                DownCommand.Enabled = selectedDevice.HasCapability(KeyControl.Down);
                LeftCommand.Enabled = selectedDevice.HasCapability(KeyControl.Left);
                RightCommand.Enabled = selectedDevice.HasCapability(KeyControl.Right);
                HomeCommand.Enabled = selectedDevice.HasCapability(KeyControl.Home);
                BackCommand.Enabled = selectedDevice.HasCapability(KeyControl.Back);
                ClickCommand.Enabled = selectedDevice.HasCapability(KeyControl.Ok);

                ManipulationDeltaCommand.Enabled = selectedDevice.HasCapability(MouseControl.Move);
                ManipulationTappedCommand.Enabled = selectedDevice.HasCapability(MouseControl.Click);

                if (selectedDevice.HasCapability(TextInputControl.Subscribe))
                {
                    textInputControl.SubscribeTextInputStatus(null);
                }
            }
        }

        private void UpCommandExecute(object obj)
        {
            keyControl.Up(null);
        }

        private void LeftCommandExecute(object obj)
        {
            keyControl.Left(null);
        }

        private void ClickCommandExecute(object obj)
        {
            keyControl.Ok(null);
        }

        private void RightCommandExecute(object obj)
        {
            keyControl.Right(null);

        }

        private void BackCommandExecute(object obj)
        {
            keyControl.Back(null);

        }

        private void DownCommandExecute(object obj)
        {
            keyControl.Down(null);

        }

        private void HomeCommandExecute(object obj)
        {
            keyControl.Home(null);

        }

        public void Move(double x, double y)
        {
            mouseControl.Move(x, y);
        }

        public void Tap()
        {
            mouseControl.Click();
        }

        public void SendText(string text)
        {
            if (text.Equals("Back"))
                textInputControl.SendDelete();
            else if (text.Equals("Enter")) 
                textInputControl.SendEnter();
            else
                textInputControl.SendText(text);
        }

        private void ManipulationDeltaCommandExecute(object obj)
        {
            var arg = obj as ManipulationDeltaRoutedEventArgs;
            Move(arg.Delta.Translation.X, arg.Delta.Translation.Y);
        }

        private void ManipulationTappedCommandExecute(object obj)
        {
            Tap();
        }
    }
}
