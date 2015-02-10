using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Windows.Data.Json;
using MyRemote.ConnectSDK.Service.Capability.Listeners;
using MyRemote.ConnectSDK.Service.Command;


namespace MyRemote.ConnectSDK.Service
{
    public class ServiceCommand
    {
        public static string TYPE_REQ = "request";
        public static string TYPE_SUB = "subscribe";
        public static string TYPE_GET = "GET";
        public static string TYPE_POST = "POST";
        public static string TYPE_DEL = "DELETE";

        private IServiceCommandProcessor service;
        private string httpMethod; // WebOSTV: {request, subscribe}, NetcastTV: {GET, POST}
        private Object payload;
        private string target;
        private int requestId;

        ResponseListener responseListener;

        public ServiceCommand()
        {
        }

        public ServiceCommand(IServiceCommandProcessor service, string targetURL, Object payload, ResponseListener listener)
        {
            this.Service = service;
            this.Target = targetURL;
            this.Payload = payload;
            this.responseListener = listener;
            this.HttpMethod = TYPE_POST;
        }

        public ServiceCommand(IServiceCommandProcessor service, string uri, JsonObject payload, bool isWebOS, ResponseListener listener)
        {
            this.Service = service;
            Target = uri;
            this.Payload = payload;
            RequestId = -1;
            HttpMethod = "request";
            responseListener = listener;
        }

        public IServiceCommandProcessor Service
        {
            get { return service; }
            set { service = value; }
        }

        public string HttpMethod
        {
            get { return httpMethod; }
            set { httpMethod = value; }
        }

        public object Payload
        {
            get { return payload; }
            set { payload = value; }
        }

        public string Target
        {
            get { return target; }
            set { target = value; }
        }

        public int RequestId
        {
            get { return requestId; }
            set { requestId = value; }
        }

        public void send()
        {
            Service.SendCommand(this);
        }



        public HttpRequestMessage getRequest()
        {
            if (Target == null)
            {
                throw new Exception("ServiceCommand has no target url");
            }

            if (this.HttpMethod.Equals(TYPE_GET))
            {
                return new HttpRequestMessage(System.Net.Http.HttpMethod.Get, Target);
            }
            else if (this.HttpMethod.Equals(TYPE_POST))
            {
                return new HttpRequestMessage(System.Net.Http.HttpMethod.Post, Target);
            }
            else if (this.HttpMethod.Equals(TYPE_DEL))
            {
                return new HttpRequestMessage(System.Net.Http.HttpMethod.Delete, Target);
            }
            else
            {
                return null;
            }
        }


        public ResponseListener ResponseListenerValue
        {
            get { return responseListener; }
        }
    }
}
