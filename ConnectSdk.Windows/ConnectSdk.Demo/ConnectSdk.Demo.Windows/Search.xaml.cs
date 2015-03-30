using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238
using Windows.UI.Xaml.Navigation;
using ConnectSdk.Demo.Demo;
using ConnectSdk.Windows.Device;
using ConnectSdk.Windows.Discovery;
using ConnectSdk.Windows.Service;
using ConnectSdk.Windows.Service.Config;
using UpdateControls.XAML;

namespace ConnectSdk.Demo
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Search : Page
    {
        private readonly Model model; 

        public Search()
        {
            InitializeComponent();
            model = App.ApplicationModel;
            DataContext = ForView.Wrap(model);
            //SearchTvs();

        }

        private void SearchTvs()
        {
            var a = new Task(() =>
            {
                var listener = new DiscoveryManagerListener();

                DiscoveryManager.Init();
                var discoveryManager = DiscoveryManager.GetInstance();
                discoveryManager.AddListener(listener);
                discoveryManager.PairingLevel = DiscoveryManager.PairingLevelEnum.On;
                discoveryManager.Start();
            });

            a.Start();
        }

        private void TvListBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var tvdef = ForView.Unwrap<ConnectableDevice>(e.AddedItems[0]);

            App.ApplicationModel.SelectedDevice = tvdef;

            var netCastService = (NetcastTvService)tvdef.GetServiceByName(NetcastTvService.Id);
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
            var tvdef = model.SelectedDevice;
            var netCastService = (NetcastTvService)tvdef.GetServiceByName(NetcastTvService.Id);
            if (netCastService != null)
            {
                var netCastServiceConfig = (NetcastTvServiceConfig) netCastService.ServiceConfig;

                netCastServiceConfig.PairingKey = PairingKeyTextBox.Text;

                netCastService.ServiceConnectionState = NetcastTvService.ConnectionState.Initial;

                tvdef.Connect();
                if (tvdef.IsConnected())
                {
                    netCastService.RemovePairingKeyOnTv();
                    tvdef.OnConnectionSuccess(netCastService);
                    model.SelectedDevice = tvdef;
                    Frame.Navigate(typeof (Main));
                }
            }
            else
            {
                var webOstvService = (WebOstvService)tvdef.GetServiceByName(WebOstvService.Id);
                if (webOstvService != null)
                {
                    if (!(webOstvService.ServiceConfig is WebOstvServiceConfig))
                    {
                        webOstvService.ServiceConfig = new WebOstvServiceConfig(webOstvService.ServiceConfig.ServiceUuid);
                    }
                    var webOsServiceConfig = (WebOstvServiceConfig)webOstvService.ServiceConfig;
                    tvdef.Connect();
                    tvdef.OnConnectionSuccess(webOstvService);
                    model.SelectedDevice = tvdef;
                    Frame.Navigate(typeof(Main));
                }
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            model.SelectedDevice = null;
            model.DiscoverredTvList.Clear();

            SearchTvs();
        }

    }
}
