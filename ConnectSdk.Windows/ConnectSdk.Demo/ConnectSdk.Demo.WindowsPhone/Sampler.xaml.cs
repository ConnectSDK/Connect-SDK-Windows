// The Basic Page item template is documented at http://go.microsoft.com/fwlink/?LinkID=390556
using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;
using ConnectSdk.Demo.Demo;
using ConnectSdk.Windows.Device;
using ConnectSdk.Windows.Service.WebOs;
using UpdateControls.Collections;

namespace ConnectSdk.Demo
{
    /// <summary>
    ///     An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Sampler : Page
    {
        private readonly Model model;
        private DispatcherTimer dispatcherTimer;

        public Sampler()
        {
            InitializeComponent();

            model = App.ApplicationModel;
            InitializeComponent();
            DataContext = model;
            model.DeviceDisconnected += model_DeviceDisconnected;
            model.SetControls();

            dispatcherTimer = new DispatcherTimer();
            dispatcherTimer.Tick += delegate
            {
                if (dispatcherTimer == null) return;

                model.GetVolume();
            };
            dispatcherTimer.Interval = new TimeSpan(0, 0, 1);
            dispatcherTimer.Start();
        }

        private void model_DeviceDisconnected(object sender, EventArgs e)
        {
            if (dispatcherTimer != null)
                dispatcherTimer.Stop();
            dispatcherTimer = null;
            model.SelectedDevice = null;
            model.DiscoverredDevices.Clear();
            model.DiscoverredDevices = new IndependentList<ConnectableDevice>();
            foreach (var webOstvServiceSocketClient in WebOstvServiceSocketClient.SocketCache)
            {
                webOstvServiceSocketClient.Value.Disconnect();
            }
            WebOstvServiceSocketClient.SocketCache.Clear();
            Frame.Navigate(typeof (Search));
        }


        private void VolumeRangeBase_OnValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            if (e.NewValue != model.Volume)
            {
                model.SetVolume(e.NewValue/100);
            }
        }

        private void InputTextBox_OnKeyDown(object sender, KeyRoutedEventArgs e)
        {
            model.SendText(e.Key.ToString());
        }

        private void DiconnectButton_OnClick(object sender, RoutedEventArgs e)
        {
            model.SelectedDevice.Disconnect();
        }
    }
}