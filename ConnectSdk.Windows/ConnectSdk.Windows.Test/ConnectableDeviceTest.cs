using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Windows.Devices.Enumeration;
using ConnectSdk.Windows.Device;
using ConnectSdk.Windows.Service;
using ConnectSdk.Windows.Service.Capability;
using FakeItEasy;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;

namespace ConnectSdk.Windows.Test
{
    [TestClass]
    public class ConnectableDeviceTest
    {
        private ConnectableDevice device;

        [TestInitialize]
        public void SetUp()
        {
            device = new ConnectableDevice();
        }

        [TestMethod]
        public void TestHasCapabilityWithEmptyServices()
        {
            Assert.IsFalse(device.HasCapability(MediaPlayer.DisplayImage));
        }

        [TestMethod]
        public void TestHasCapabilityWithServices()
        {
         
            //var v = A.Fake<DeviceService>();
            //A.CallTo(() => v.HasCapability(MediaPlayer.DisplayImage)).Returns(true);
            //device.AddService(v);
            //Assert.IsTrue(device.HasCapability(MediaPlayer.DisplayImage));

            //DeviceService service = Mockito.mock(DeviceService.class)
            //;
            //Mockito.when(service.hasCapability(MediaPlayer.Display_Image)).thenReturn(Boolean.TRUE);
            //device.services.put("service", service);
            //Assert.assertTrue(device.hasCapability(MediaPlayer.Display_Image));
        }

        //[TestMethod]
    //public void testHasAnyCapabilities() {
    //    DeviceService service = Mockito.mock(DeviceService.class);
    //    String[] capabilities = {Launcher.Browser, Launcher.YouTube};
    //    Mockito.when(service.hasAnyCapability(capabilities)).thenReturn(Boolean.TRUE);
    //    device.services.put("service", service);
    //    Assert.assertTrue(device.hasAnyCapability(capabilities));
    //}

    }
}
