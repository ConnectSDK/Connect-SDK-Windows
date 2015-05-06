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
        public void ConnectToWebOsTv()
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

            // wait for a while 
            Task t = new Task(() =>
            {
                Assert.AreEqual(model.DiscoverredDeviceServices.Count,2);
            });
            t.Start();
        }
    }
}
