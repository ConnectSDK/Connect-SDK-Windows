#region Copyright Notice
/*
 * ConnectSdk.Windows
 * customconnectionclient.cs
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
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using ConnectSdk.Windows.Service.Capability.Listeners;

namespace ConnectSdk.Windows.Service.Upnp
{
    public class CustomConnectionClient : HttpConnection
    {
        private readonly Uri uri;
        private Method method;
        private int code;
        private string response;
        private readonly Dictionary<String, String> headers = new Dictionary<String, String>();
        private readonly Dictionary<String, String> responseHeaders = new Dictionary<String, String>();
        private string payload;

        public CustomConnectionClient(Uri uri)
        {
            this.uri = uri;
        }

        public override void SetMethod(Method paramMethod)
        {
            method = paramMethod;
        }

        public override int GetResponseCode()
        {
            return code;
        }

        public override string GetResponseString()
        {
            return response;
        }

        private string CreateRequest(int port)
        {
            var sb = new StringBuilder();
            sb.Append(method);
            sb.Append(" ");
            sb.Append(uri.AbsolutePath);
            sb.Append(string.IsNullOrEmpty(uri.Query) ? "" : "?" + uri.Query);
            sb.Append(" HTTP/1.1\r\n");

            sb.Append("Host:");
            sb.Append(uri.Host);
            sb.Append(":");
            sb.Append(port);
            sb.Append("\r\n");

            if (payload != null)
                sb.Append(payload);

            return sb.ToString();
        }

        private void ProcessResponse(WebResponse webResponse, string responseContent)
        {
            foreach (var header in webResponse.Headers)
            {
                responseHeaders.Add(header.ToString(), webResponse.Headers[header.ToString()]);
            }
            var httpWebResponse = webResponse as HttpWebResponse;
            if (httpWebResponse != null) code = (int) (httpWebResponse.StatusCode);
            response = responseContent;
        }

        public override void Execute(ResponseListener listener)
        {
            int port = uri.Port > 0 ? uri.Port : 80;

            var request = WebRequest.Create(uri) as HttpWebRequest;

            if (request != null)
            {
                request.ContentType = "text/plain; charset=utf-8";
                request.Method = MethodType.ToString();
                request.Headers["UserAgent"] = UserAgent;
                request.Headers["CALLBACK"] = CallBack;
                request.Headers["NT"] = Nt;
                request.Headers["TIMEOUT"] = Timeout;
                //request.Headers["CONNECTION"] = this.Connection;

                request.BeginGetRequestStream(result =>
                {
                    var preq = result.AsyncState as HttpWebRequest;
                    if (preq != null)
                    {
                        var postStream = preq.EndGetRequestStream(result);


                        var byteArray = Encoding.UTF8.GetBytes(CreateRequest(port));

                        postStream.Write(byteArray, 0, byteArray.Length);
                        postStream.Flush();
                        postStream.Dispose();

                        preq.BeginGetResponse(finalResult =>
                        {
                            var req = finalResult.AsyncState as HttpWebRequest;
                            if (req != null)
                            {
                                try
                                {
                                    var rsp = req.EndGetResponse(finalResult);
                                    using (var stream = rsp.GetResponseStream())
                                    {
                                        var reader = new StreamReader(stream, Encoding.UTF8);
                                        var responseString = reader.ReadToEnd();
                                        ProcessResponse(rsp, responseString);

                                        if (listener != null)
                                            listener.OnSuccess(code);
                                    }
                                }
                                catch (WebException)
                                {
                                }
                            }
                        }, preq);
                    }
                }, request);
            }
        }

        public override void SetPayload(string paramPayload)
        {
            payload = paramPayload;
        }

        public override void SetPayload(byte[] paramPayload)
        {
            throw new NotImplementedException();
        }

        public override void SetHeader(string name, string value)
        {
            if (!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(value))
                headers.Add(name.Trim(), value.Trim());
        }

        public override string GetResponseHeader(string name)
        {
            return responseHeaders[name];
        }
    }
}