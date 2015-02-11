using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238
using ConnectSdk.Demo.Demo;
using ConnectSdk.Windows.Device;
using ConnectSdk.Windows.Discovery;
using ConnectSdk.Windows.Service;
using ConnectSdk.Windows.Service.Config;

namespace ConnectSdk.Demo
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private Model model;
        public MainPage()
        {
            this.InitializeComponent();
            Search();
            this.DataContext = model;
        }

        private void Search()
        {
            var a = new Task(() =>
            {
                var listener = new DiscoveryManagerListener();

                DiscoveryManager.Init();
                DiscoveryManager discoveryManager = DiscoveryManager.GetInstance();
                discoveryManager.AddListener(listener);
                discoveryManager.SetPairingLevel(DiscoveryManager.PairingLevel.ON);
                discoveryManager.Start();
            });

            a.Start();
        }

        private void TvListBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var tvdef = (ConnectableDevice)TvListBox.SelectedItem;

            var netCastService = (NetcastTvService)tvdef.GetServiceByName(NetcastTvService.Id);
            if (netCastService != null)
            {
                if (!(netCastService.ServiceConfig is NetcastTvServiceConfig))
                {
                    netCastService.ServiceConfig = new NetcastTvServiceConfig(netCastService.ServiceConfig.ServiceUuid);
                }
                var netCastServiceConfig = (NetcastTvServiceConfig)netCastService.ServiceConfig;


                    netCastService.ShowPairingKeyOnTv();
            }
        }
    }
}
