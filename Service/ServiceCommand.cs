using System;
using System.Net.Http;
using Windows.Data.Json;
using ConnectSdk.Windows.Service.Capability.Listeners;
using ConnectSdk.Windows.Service.Command;

namespace ConnectSdk.Windows.Service
{
    public class ServiceCommand
    {
        public static string TypeReq = "request";
        public static string TypeSub = "subscribe";
        public static string TypeGet = "GET";
        public static string TypePost = "POST";
        public static string TypeDel = "DELETE";

        readonly ResponseListener responseListener;

        public ServiceCommand()
        {
        }

        public ServiceCommand(IServiceCommandProcessor service, string targetUrl, Object payload, ResponseListener listener)
        {
            Service = service;
            Target = targetUrl;
            Payload = payload;
            responseListener = listener;
            HttpMethod = TypePost;
        }

        public ServiceCommand(IServiceCommandProcessor service, string uri, IJsonValue payload, ResponseListener listener)
        {
            Service = service;
            Target = uri;
            Payload = payload;
            RequestId = -1;
            HttpMethod = "request";
            responseListener = listener;
        }

        public IServiceCommandProcessor Service { get; set; }

        public string HttpMethod { get; set; }

        public object Payload { get; set; }

        public string Target { get; set; }

        public int RequestId { get; set; }

        public void Send()
        {
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

        public ResponseListener ResponseListenerValue
        {
            get { return responseListener; }
        }
    }
}
