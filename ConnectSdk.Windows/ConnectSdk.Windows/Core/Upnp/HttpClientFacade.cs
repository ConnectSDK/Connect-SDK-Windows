using System.Net.Http;
using ConnectSdk.Windows.Fakes;

namespace ConnectSdk.Windows.Core.Upnp
{
    public class HttpClientFacade
    {
        private readonly HttpClient client;

        public HttpClientFacade()
        {
            client = new HttpClient();
        }

        public HttpResponseMessage GetAsync(string url)
        {
            if (MessageFakeFactory.Instance != null)
            {
                return MessageFakeFactory.Instance.GetResponseMessage(url).Result;
            }
            return client.GetAsync(url).Result;
        }
    }
}