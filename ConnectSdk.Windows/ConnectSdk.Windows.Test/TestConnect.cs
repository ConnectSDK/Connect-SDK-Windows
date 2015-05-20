using System;
using System.Threading.Tasks;
using ConnectSdk.Windows.Device;
using ConnectSdk.Windows.Discovery;
using ConnectSdk.Windows.Fakes;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;

namespace ConnectSdk.Windows.Test
{
    [TestClass]
    public class TestConnect
    {
        private static Model model;

        [ClassInitialize]
        public static void Initialize(TestContext context)
        {
            MessageFakeFactory.Start();
            model = new Model();
        }

        [TestMethod]
        public async Task DiscoverTest()
        {
            await Discover();
        }


        [TestMethod]
        public async Task PairTest()
        {
            await Discover();
        }

        private static async Task Discover()
        {
            var listener = new DiscoveryManagerListener(model);

            listener.Paired += (sender, o) =>
            {
                model.SelectedDevice = o as ConnectableDevice;
            };
            DiscoveryManager.Init();
            var discoveryManager = DiscoveryManager.GetInstance();
            discoveryManager.AddListener(listener);
            discoveryManager.PairingLevel = DiscoveryManager.PairingLevelEnum.On;
            discoveryManager.Start();

            await AssertAsync();
        }

        public static async Task AssertAsync()
        {
            // wait to ensure that all calls went through
            await Task.Delay(500);
            //we expect 2 services: webos and dlna. There is also a Netcast but WebOs is also implicitly netcase so it is not added to the list
            Assert.AreEqual(2, model.DiscoverredDeviceServices.Count);
        }
    }
}
