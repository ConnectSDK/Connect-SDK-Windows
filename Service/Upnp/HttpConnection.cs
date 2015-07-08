#region Copyright Notice
/*
 * ConnectSdk.Windows
 * httpconnection.cs
 * 
 * Copyright (c) 2015, https://github.com/sdaemon
 * Created by Sorin S. Serban on 5-7-2015,
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
using ConnectSdk.Windows.Service.Capability.Listeners;

namespace ConnectSdk.Windows.Service.Upnp
{
    public abstract class HttpConnection
    {
        public enum Method
        {
            // ReSharper disable InconsistentNaming
            GET,
            POST,
            PUT,
            DELETE,
            SUBSCRIBE,
            UNSUBSCRIBE
            // ReSharper restore InconsistentNaming
        }

        public Method MethodType { get; set; }
        public string CallBack { get; set; }
        public string Nt { get; set; }
        public string Timeout { get; set; }
        public string Connection { get; set; }
        public string ContentLength { get; set; }
        public string UserAgent { get; set; }

        public static HttpConnection NewSubscriptionInstance(Uri uri)
        {
            return new CustomConnectionClient(uri);
        }

        public static HttpConnection NewInstace(Uri uri)
        {
            return new HttpUrlConnectionClient(uri, 0);
        }

        public abstract void SetMethod(Method method1);

        public abstract int GetResponseCode();

        public abstract String GetResponseString();

        public abstract void Execute(ResponseListener listener);

        public abstract void SetPayload(String payload);

        public abstract void SetPayload(byte[] payload);

        public abstract void SetHeader(String name, String value);

        public abstract String GetResponseHeader(String name);

    }
}