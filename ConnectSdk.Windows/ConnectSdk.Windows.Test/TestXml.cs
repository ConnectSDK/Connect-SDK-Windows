using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ConnectSdk.Windows.Service;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;

namespace ConnectSdk.Windows.Test
{
    [TestClass]
    public class TestXml
    {

        [TestMethod]
        public void XmlTest()
        {
            string url = "http://ec2-54-201-108-205.us-west-2.compute.amazonaws.com/samples/media/photo.jpg";
            string mimeType = "image/jpeg";
            string title = "Sintel Character Design";
            string description = "Blender Open Movie Project";
            string iconSrc = "http://ec2-54-201-108-205.us-west-2.compute.amazonaws.com/samples/media/photoIcon.jpg";
            string setTransportMethod = "SetAVTransportURI";

            const string instanceId = "0";
            var mediaElements = mimeType.Split('/');
            var mediaType = mediaElements[0];
            var mediaFormat = mediaElements[1];


            mediaFormat = "mp3".Equals(mediaFormat) ? "mpeg" : mediaFormat;
            var mMimeType = String.Format("{0}/{1}", mediaType, mediaFormat);

            var metadata = DlnaService.GetMetadata(url, mMimeType, title, description, iconSrc);

            var setTransportParams = new Dictionary<String, String>
            {
                { "CurrentURI", url }
                ,{ "CurrentURIMetaData", metadata }
            };

            var setTransportPayload = DlnaService.GetMethodBody(DlnaService.AV_TRANSPORT_URN, instanceId, setTransportMethod, setTransportParams);
        }

    }
}
