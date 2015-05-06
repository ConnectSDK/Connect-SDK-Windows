using System.Net.Http;
using ConnectSdk.Windows.Fakes;

namespace ConnectSdk.Windows.Wrappers
{
    public class HttpClientWrapper
    {
        private readonly HttpClient client;

        public HttpClientWrapper()
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