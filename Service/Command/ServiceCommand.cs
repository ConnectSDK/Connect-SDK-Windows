#region Copyright Notice
/*
 * ConnectSdk.Windows
 * servicecommand.cs
 * 
 * Copyright (c) 2015, https://github.com/sdaemon
 * Created by Sorin S. Serban on 22-4-2015,
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
 #endregion
using System;
using System.Net.Http;
using Windows.Data.Json;
using ConnectSdk.Windows.Service.Capability.Listeners;

namespace ConnectSdk.Windows.Service.Command
{
    public class ServiceCommand
    {
        public static string TypeReq = "request";
        public static string TypeSub = "subscribe";
        public static string TypeGet = "GET";
        public static string TypePost = "POST";
        public static string TypeDel = "DELETE";

        public IServiceCommandProcessor Service { get; set; }

        public string HttpMethod { get; set; }  // WebOSTV: {request, subscribe}, NetcastTV: {GET, POST}

        public object Payload { get; set; }

        public string Target { get; set; }

        public int RequestId { get; set; }

        readonly ResponseListener responseListener;

        public ServiceCommand()
        {
        }

        public ServiceCommand(IServiceCommandProcessor service, string targetUrl, object payload, ResponseListener listener)
        {
            Service = service;
            Target = targetUrl;
            Payload = payload;
            responseListener = listener;
            HttpMethod = TypePost;
        }

        public ServiceCommand(IServiceCommandProcessor service, string uri, JsonObject payload, ResponseListener listener)
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

        public ResponseListener ResponseListenerValue
        {
            get { return responseListener; }
        }
    }
}
