using System;
using System.Net.Http;
using Windows.Data.Json;
using ConnectSdk.Windows.Service.Capability.Listeners;

namespace ConnectSdk.Windows.Service.Command
{
    public class ServiceCommand<T>
    {
        public string TypeReq = "request";
        public string TypeSub = "subscribe";
        public string TypeGet = "GET";
        public string TypePost = "POST";
        public string TypeDel = "DELETE";

        public IServiceCommandProcessor<T> Service { get; set; }

        public string HttpMethod { get; set; }  // WebOSTV: {request, subscribe}, NetcastTV: {GET, POST}

        public object Payload { get; set; }

        public string Target { get; set; }

        public int RequestId { get; set; }

        readonly ResponseListener<T> responseListener;

        public ServiceCommand()
        {
        }

        public ServiceCommand(IServiceCommandProcessor<T> service, string targetUrl, object payload, ResponseListener<T> listener)
        {
            Service = service;
            Target = targetUrl;
            Payload = payload;
            responseListener = listener;
            HttpMethod = TypePost;
        }

        public ServiceCommand(IServiceCommandProcessor<T> service, string uri, JsonObject payload, ResponseListener<T> listener)
        {
            Service = service;
            Target = uri;
            Payload = payload;
            RequestId = -1;
            HttpMethod = "request";
            responseListener = listener;
        }

        public void Send()
        {
            //todo: check this
            Service.SendCommand(this);
        }

        public HttpRequestMessage GetRequest()
        {
            if (Target == null)
            {
                throw new Exception("ServiceCommand has no target url");
            }

            if (HttpMethod.Equals(TypeGet))
            {
                return new HttpRequestMessage(System.Net.Http.HttpMethod.Get, Target);
            }
            if (HttpMethod.Equals(TypePost))
            {
                return new HttpRequestMessage(System.Net.Http.HttpMethod.Post, Target);
            }
            return HttpMethod.Equals(TypeDel) ? new HttpRequestMessage(System.Net.Http.HttpMethod.Delete, Target) : null;
        }

        public ResponseListener<T> ResponseListenerValue
        {
            get { return responseListener; }
        }
    }
}
