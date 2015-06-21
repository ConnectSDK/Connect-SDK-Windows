using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkID=390556
using ConnectSdk.Demo.Common;
using ConnectSdk.Demo.Demo;
using ConnectSdk.Windows.Device;
using ConnectSdk.Windows.Discovery;
using ConnectSdk.Windows.Service;
using ConnectSdk.Windows.Service.Config;
using UpdateControls.Fields;
using UpdateControls.XAML;

namespace ConnectSdk.Demo
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Search : Page
    {
        private readonly Model model;
        private WebOstvService webOstvService;
        private DiscoveryManagerListener listener;

        public Search()
        {
            this.InitializeComponent();
            model = App.ApplicationModel;
            DataContext = ForView.Wrap(model);
        }

        private void SearchTvs()
        {
            listener = new DiscoveryManagerListener();

            listener.Paired += (sender, o) =>
            {
                model.Connected = true;
                model.SelectedDevice = new Independent<ConnectableDevice>(o as ConnectableDevice);
                Dispatcher.RunAsync(CoreDispatcherPriority.High, () => Frame.Navigate(typeof(Sampler)));
            };
            DiscoveryManager.Init();
            var discoveryManager = DiscoveryManager.GetInstance();
            discoveryManager.AddListener(listener);
            discoveryManager.PairingLevel = DiscoveryManager.PairingLevelEnum.On;
            discoveryManager.Start();
        }

        private void TvListBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count == 0) return;

            var device = ForView.Unwrap<ConnectableDevice>(e.AddedItems[0]);
            if (device == null) return;

            App.ApplicationModel.SelectedDevice = device;

            var netCastService = (NetcastTvService)device.GetServiceByName(NetcastTvService.Id);
            if (netCastService != null)
            {
                if (!(netCastService.ServiceConfig is NetcastTvServiceConfig))
                {
                    netCastService.ServiceConfig = new NetcastTvServiceConfig(netCastService.ServiceConfig.ServiceUuid);
                }
                netCastService.ShowPairingKeyOnTv();
            }
        }

        private void PairOkButton_OnClick(object sender, RoutedEventArgs e)
        {
            var senderButton = sender as Button;
            if (senderButton == null) return;

            var devicet = (ConnectableDevice)(senderButton.DataContext);
            model.SelectedDevice = devicet;

            if (devicet == null) return;

            var device = model.SelectedDevice;
            var netCastService = (NetcastTvService)device.GetServiceByName(NetcastTvService.Id);
            if (netCastService != null)
            {
                var netCastServiceConfig = (NetcastTvServiceConfig)netCastService.ServiceConfig;
                var txtBox = Util.FindChild<TextBox>(senderButton.Parent, "PairingKeyTextBox");

                netCastServiceConfig.PairingKey = txtBox.Text;

                netCastService.ServiceConnectionState = NetcastTvService.ConnectionState.Initial;

                device.Connect();
                if (device.IsConnected())
                {
                    netCastService.RemovePairingKeyOnTv();
                    device.OnConnectionSuccess(netCastService);
                    model.SelectedDevice = device;
                    Frame.Navigate(typeof(Sampler));
                }
            }
            else
            {
                webOstvService = (WebOstvService)device.GetServiceByName(WebOstvService.Id);
                if (webOstvService != null)
                {
                    if (!(webOstvService.ServiceConfig is WebOsTvServiceConfig))
                    {
                        webOstvService.ServiceConfig = new WebOsTvServiceConfig(webOstvService.ServiceConfig.ServiceUuid);
                    }
                    device.AddListener(listener);
                    device.Connect();
                    model.SelectedDevice.OnConnectionSuccess(webOstvService);

                }
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            model.SelectedDevice = null;
            model.DiscoverredDevices.Clear();

            SearchTvs();
        }

        private void RefreshButton_OnClick(object sender, RoutedEventArgs e)
        {
            SearchTvs();
        }

        private void ShowKeyButton_OnClick(object sender, RoutedEventArgs e)
        {
            var senderButton = sender as Button;
            var device = (ConnectableDevice)senderButton.DataContext;
            var netCastService = (NetcastTvService)device.GetServiceByName(NetcastTvService.Id);
            netCastService.ShowPairingKeyOnTv();
        }
    }
}
