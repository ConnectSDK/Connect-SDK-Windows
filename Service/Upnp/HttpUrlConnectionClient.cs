#region Copyright Notice
/*
 * ConnectSdk.Windows
 * httpurlconnectionclient.cs
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
using System.IO;
using System.Net;
using System.Text;
using ConnectSdk.Windows.Service.Capability.Listeners;

namespace ConnectSdk.Windows.Service.Upnp
{
    public class HttpUrlConnectionClient : HttpConnection
    {
        private readonly WebRequest request;
        private byte[] payload;
        private String response;
        private readonly int responseCode;

        public HttpUrlConnectionClient(Uri uri, int responseCode)
        {
            this.responseCode = responseCode;
            request = WebRequest.Create(uri);
        }

        public override void SetMethod(Method method)
        {
            request.Method = method.ToString();
        }

        public override int GetResponseCode()
        {
            return responseCode;
        }

        public override string GetResponseString()
        {
            return response;
        }

        public override void Execute(ResponseListener listener)
        {
            //we first obtain an input stream to which to write the body of the HTTP POST
            request.BeginGetRequestStream(result =>
            {
                var preq = result.AsyncState as HttpWebRequest;
                if (preq != null)
                {
                    var postStream = preq.EndGetRequestStream(result);
                    postStream.Write(payload, 0, payload.Length);
                    postStream.Flush();


                    //we can then finalize the request...
                    preq.BeginGetResponse(finalResult =>
                    {
                        var req = finalResult.AsyncState as HttpWebRequest;
                        if (req != null)
                        {
                            try
                            {
                                //we call the success callback as long as we get a response stream
                                var endGetResponse = req.EndGetResponse(finalResult);
                                var streamResponse = endGetResponse.GetResponseStream();

                                /* convert stream to string*/
                                var reader = new StreamReader(streamResponse);
                                response = reader.ReadToEnd();
                                reader.Dispose();
                            }
                            catch (WebException)
                            {
                                //otherwise call the error/failure callback
                            }
                        }
                    }, preq);
                }
            }, request);


        }

        public override void SetPayload(string payloadParam)
        {
            payload = Encoding.UTF8.GetBytes(payloadParam);
        }

        public override void SetPayload(byte[] payloadParam)
        {
            payload = payloadParam;
        }

        public override void SetHeader(string name, string value)
        {
            request.Headers[name] = value;

        }

        public override string GetResponseHeader(string name)
        {
            return request.Headers[name];
        }
    }
}