#region Copyright Notice
/*
 * ConnectSdk.Windows
 * WebOstvServiceSocketClient.cs
 * 
 * Copyright (c) 2015 LG Electronics.
 * Created by Sorin S. Serban on 16-4-2015,
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
using System.Diagnostics;
using System.Linq;
using System.Text;
using Windows.ApplicationModel;
using Windows.Data.Json;
using Windows.Networking.Sockets;
using Windows.Security.ExchangeActiveSyncProvisioning;
using Windows.Storage.Streams;
using ConnectSdk.Windows.Core;
using ConnectSdk.Windows.Discovery;
using ConnectSdk.Windows.Etc.Helper;
using ConnectSdk.Windows.Service.Capability.Listeners;
using ConnectSdk.Windows.Service.Command;
using ConnectSdk.Windows.Service.Config;
using UnicodeEncoding = Windows.Storage.Streams.UnicodeEncoding;

namespace ConnectSdk.Windows.Service.WebOs
{
    public class WebOstvServiceSocketClient : IServiceCommandProcessor
    {
        private readonly StringBuilder log = new StringBuilder();
        readonly WebOstvService service;

        private static MessageWebSocket messageWebSocket;
        int nextRequestId = 1;
        private State state = State.Initial;
        JsonObject manifest;
        private DataWriter dr;

        //static int PORT = 3001;
        private const int Port = 3000;

        // Queue of commands that should be sent once register is complete
        readonly Queue<ServiceCommand> commandQueue = new Queue<ServiceCommand>();

        public Dictionary<int, ServiceCommand> Requests = new Dictionary<int, ServiceCommand>();

        private readonly Uri destinationUri;

        public static Dictionary<string, WebOstvServiceSocketClient> SocketCache = new Dictionary<string, WebOstvServiceSocketClient>();

        public WebOstvServiceSocketClient(WebOstvService service, Uri uri)
        {

            destinationUri = uri;
            this.service = service;
            State = State.Initial;
            CreateSocket();
            SetDefaultManifest();
        }

        

        public IWebOstvServiceSocketClientListener Listener { get; set; }

        public State State
        {
            get { return state; }
            set
            {
                state = value;
            }
        }

        private bool connected;

        private void CreateSocket()
        {
            messageWebSocket = new MessageWebSocket();
            messageWebSocket.Control.MessageType = SocketMessageType.Utf8;
            
            messageWebSocket.MessageReceived += (sender, args) =>
            {
                string read;

                using (var reader = args.GetDataReader())
                {
                    reader.UnicodeEncoding = UnicodeEncoding.Utf8;
                    read = reader.ReadString(reader.UnconsumedBufferLength);
                }
                OnMessage(read);
                if (!connected)
                {
                    HandleConnected();
                }
            };
            messageWebSocket.Closed += (sender, args) =>
            {
                OnClose(0, args.ToString());
                HandleConnectionLost(false, null);
                messageWebSocket = new MessageWebSocket();
            };
        }

        public static Uri GetUri(WebOstvService service)
        {
            var uriString = "ws://" + service.ServiceDescription.IpAddress + ":" + Port;
            var uri = new Uri(uriString);
            return uri;
        }

        public async void Connect()
        {
            try
            {
                state = State.Connecting;
                if (messageWebSocket.Information.LocalAddress == null)
                {
                    await messageWebSocket.ConnectAsync(destinationUri);
                }
                SendRegister();
            }
            catch (Exception ex)
            {
                // ReSharper disable once UnusedVariable
                var status = WebSocketError.GetStatus(ex.GetBaseException().HResult);
                throw;
            }
        }

        public void Disconnect()
        {
            DisconnectWithError(null);
        }

        public void DisconnectWithError(ServiceCommandError error)
        {
            State = State.Initial;
            
            if (Listener != null)
                Listener.OnCloseWithError(error);
        }

        private void SetDefaultManifest()
        {
            manifest = new JsonObject();
            var permissions = service.GetPermissions();

            try
            {
                manifest.Add("manifestVersion", JsonValue.CreateNumberValue(1));
                manifest.Add("permissions", ConvertStringListToJsonArray(permissions));
            }
            // ReSharper disable once EmptyGeneralCatchClause
            catch
            {

            }
        }

        public JsonArray ConvertStringListToJsonArray(IEnumerable<string> list)
        {
            var jsonArray = new JsonArray();

            foreach (var str in list)
            {
                jsonArray.Add(JsonValue.CreateStringValue(str));
            }

            return jsonArray;
        }

        public void OnMessage(String data)
        {
            Logger.Current.AddMessage("webOS Socket [IN] : " + data);
            HandleMessage(data);
        }

        public void OnClose(int code, String reason)
        {
            HandleConnectionLost(true, null);
        }


        protected void HandleConnected()
        {
            HelloTv();
        }

        protected void HandleConnectError(Exception ex)
        {
            if (Listener != null)
                Listener.OnFailWithError(new ServiceCommandError(0, "connection error"));
        }

        protected void HandleMessage(String data)
        {
            try
            {
                var obj = JsonObject.Parse(data);
                HandleMessage(obj);
                if (!connected) connected = true;
            }
            // ReSharper disable once EmptyGeneralCatchClause
            catch
            {
                //throw e;
            }
        }

        protected void HandleMessage(JsonObject message)
        {
            try
            {

            var shouldProcess = true;

            if (Listener != null)
                shouldProcess = Listener.OnReceiveMessage(message);

            if (!shouldProcess)
                return;

            var type = message.GetNamedString("type");
            Object payload = 1;
            try
            {
                payload = message.GetNamedObject("payload");
            }
            // ReSharper disable once EmptyGeneralCatchClause
            catch
            {
                // we will fail when the type is error because payload is not retrievable
            }
            ServiceCommand request = null;
            int id = 0;
            if (message.ContainsKey("id"))
            {
                if (message.ContainsKey("id"))
                {
                    if (message.GetNamedValue("id").ValueType != JsonValueType.String)
                    {
                        id = (int)message.GetNamedNumber("id");
                    }
                    else
                    {
                        var intstr = message.GetNamedString("id");
                        int.TryParse(intstr, out id);
                    }
                }

                try
                {
                    if (Requests.ContainsKey(id))
                        request = Requests[id];
                }
                // ReSharper disable once EmptyGeneralCatchClause
                catch
                {
                    // since request is assigned to null, don't need to do anything here
                }
            }

            if (type.Length == 0)
                return;

            if ("response".Equals(type))
            {
                if (request != null)
                {
                    Logger.Current.AddMessage("Found requests. Need to handle response.");
                    if (payload != null)
                    {
                        try
                        {
                            Util.PostSuccess(request.ResponseListenerValue, payload);
                        }
                        catch
                        {
                        }

                    }
                    else
                    {
                        try
                        {
                            Util.PostError(request.ResponseListenerValue,
                                new ServiceCommandError(-1, "JSON parse error"));
                        }
                        // ReSharper disable once EmptyGeneralCatchClause
                        catch
                        {

                        }
                    }

                    if (!(request is UrlServiceSubscription))
                    {
                        if (!message.ContainsKey("pairingType"))
                        {
                            Requests.Remove(id);
                        }
                    }
                }
            }
            else if ("registered".Equals(type))
            {
                if (!(service.ServiceConfig is WebOsTvServiceConfig))
                {
                    service.ServiceConfig = new WebOsTvServiceConfig(service.ServiceConfig.ServiceUuid);
                }

                if (payload != null)
                {
                    var clientKey = ((JsonObject)payload).GetNamedString("client-key");
                    ((WebOsTvServiceConfig)service.ServiceConfig).ClientKey = clientKey;

                    HandleRegistered();

                    if (Requests.ContainsKey(id))
                        Requests.Remove(id);
                }
            }
            else if ("error".Equals(type))
            {
                var error = message.GetNamedString("error");
                if (error.Length == 0)
                    return;

                int errorCode;
                string errorDesc;
                try
                {
                    var parts = error.Split(' ');
                    errorCode = int.Parse(parts[0]);
                    errorDesc = error.Replace(parts[0], "");
                }
                // ReSharper disable once RedundantCatchClause
                catch
                {
                    throw;
                }
                if (payload != null)
                    Logger.Current.AddMessage("Error payload: " + payload);

                if (!message.ContainsKey("id")) return;
                Logger.Current.AddMessage("Error desc: " + errorDesc);

                if (request == null) return;
                Util.PostError(request.ResponseListenerValue,
                    new ServiceCommandError(errorCode, payload));

                if (errorCode != 409)
                    if (!(request is UrlServiceSubscription))
                        Requests.Remove(id);

                if (errorCode == 403)
                {
                    // 403 User Denied Access 
                    Disconnect();
                }
            }
            else if ("hello".Equals(type))
            {
                var jsonObj = (JsonObject)payload;

                if (service.ServiceConfig.ServiceUuid != null)
                {
                    if (!service.ServiceConfig.ServiceUuid.Equals(jsonObj.GetNamedString("deviceUUID")))
                    {
                        ((WebOsTvServiceConfig)service.ServiceConfig).ClientKey = null;
                        String cert = null;
                        // ReSharper disable once ExpressionIsAlwaysNull
                        ((WebOsTvServiceConfig)service.ServiceConfig).SetServerCertificate(cert);
                        service.ServiceConfig.ServiceUuid = null;
                        service.ServiceDescription.IpAddress = null;
                        service.ServiceDescription.Uuid = null;

                        Disconnect();
                    }
                }
                else
                {
                    String uuid = jsonObj.GetNamedString("deviceUUID");
                    service.ServiceConfig.ServiceUuid = uuid;
                    service.ServiceDescription.Uuid = uuid;
                }

                State = State.Registering;
                SendRegister();
            }
            }
            catch (Exception)
            {

                throw;
            }

        }

        private void HelloTv()
        {
            //Context context = DiscoveryManager.getInstance().getContext();
            //PackageManager packageManager = context.getPackageManager();

            // app Id

            var packageName = Package.Current.Id.Name;

            //// SDK Version
            String sdkVersion = DiscoveryManager.ConnectSdkVersion;

            var deviceInfo = new EasClientDeviceInformation();

            //// Device Model
            var deviceModel = deviceInfo.FriendlyName;



            //// OS Version
            var osVersion = deviceInfo.OperatingSystem;



            //// resolution
            //WindowManager wm = (WindowManager) context.getSystemService(Context.WINDOW_SERVICE);
            //Display display = wm.getDefaultDisplay();

            //@SuppressWarnings("deprecation")
            //int width = display.getWidth(); // deprecated, but still needed for supporting API levels 10-12

            //@SuppressWarnings("deprecation")
            //int height = display.getHeight(); // deprecated, but still needed for supporting API levels 10-12

            //var screenResolution = String.Format("%{0}x%{1}", 1680, 1050); 

            //// app Name
            //ApplicationInfo applicationInfo;
            //try {
            //    applicationInfo = packageManager.getApplicationInfo(context.getPackageName(), 0);
            //} catch (final NameNotFoundException e) {
            //    applicationInfo = null;
            //}
            //String applicationName = (String) (applicationInfo != null ? packageManager.getApplicationLabel(applicationInfo) : "(unknown)");

            //// app Region
            //Locale current = context.getResources().getConfiguration().locale;
            //String appRegion = current.getDisplayCountry();

            var payload = new JsonObject();
            try
            {
                payload.SetNamedValue("sdkVersion", JsonValue.CreateStringValue(sdkVersion));
                payload.SetNamedValue("deviceModel", JsonValue.CreateStringValue(deviceModel));
                payload.SetNamedValue("OSVersion", JsonValue.CreateStringValue(osVersion));
                //payload.SetNamedValue("resolution", JsonValue.CreateStringValue(resolution));
                //payload.SetNamedValue("appId", JsonValue.CreateStringValue(appId));
                payload.SetNamedValue("appName", JsonValue.CreateStringValue(packageName));
                //payload.SetNamedValue("appRegion", JsonValue.CreateStringValue(appRegion));
            }
            catch (Exception e)
            {
                throw e;
            }

            var dataId = nextRequestId++;

            var sendData = new JsonObject();
            try
            {
                sendData.SetNamedValue("id", JsonValue.CreateNumberValue(dataId));
                sendData.SetNamedValue("type", JsonValue.CreateStringValue("hello"));
                sendData.SetNamedValue("payload", payload);
            }
            catch (Exception e)
            {
                throw e;
            }

            var request = new ServiceCommand(this, null, sendData, null);
            SendCommandImmediately(request);
        }

        protected void SendRegister()
        {
            var requestListener = new ResponseListener
            (
                loadEventArg =>
                {

                },
                serviceCommandError =>
                {
                    if (Listener != null)
                        Listener.OnRegistrationFailed(serviceCommandError);
                }
            );

            var dataId = nextRequestId++;

            var command = new ServiceCommand(this, null, null, requestListener) { RequestId = dataId };

            var headers = new JsonObject();
            var payload = new JsonObject();

            try
            {
                headers.Add("type", JsonValue.CreateStringValue("register"));
                headers.Add("id", JsonValue.CreateNumberValue(dataId));

                if (!(service.ServiceConfig is WebOsTvServiceConfig))
                {
                    service.ServiceConfig = new WebOsTvServiceConfig(service.ServiceConfig.ServiceUuid);
                }

                if (((WebOsTvServiceConfig)service.ServiceConfig).ClientKey != null)
                {
                    payload.Add("client-key", JsonValue.CreateStringValue(((WebOsTvServiceConfig)service.ServiceConfig).ClientKey));
                }
                else
                {
                    if (Listener != null)
                        Listener.OnBeforeRegister(PairingType.NONE);
                }

                if (manifest != null)
                {
                    payload.Add("manifest", manifest);
                }
            }
            catch (Exception e)
            {
                // ReSharper disable once PossibleIntendedRethrow
                throw e;
            }

            Requests.Add(dataId, command);

            SendMessage(headers, payload);
        }

        protected void HandleRegistered()
        {
            State = State.Registered;

            if (commandQueue.Count > 0)
            {
                var tempHashSet = new List<ServiceCommand>(commandQueue);
                foreach (var command in tempHashSet)
                {
                    Logger.Current.AddMessage("Executing queued command for: " + command.Target);
                    SendCommandImmediately(command);
                    commandQueue.Dequeue();
                }
            }

            if (Listener != null)
                Listener.OnConnect();
        }



        public void SendCommand(ServiceCommand command)
        {
            int requestId;
            if (command.RequestId == -1)
            {
                requestId = nextRequestId++;
                command.RequestId = requestId;
            }
            else
            {
                requestId = command.RequestId;
            }

            Requests.Add(requestId, command);
            try
            {
                if (state == State.Registered)
                {
                    this.SendCommandImmediately(command);
                }
                else if (state == State.Connecting || state == State.Disconnecting)
                {
                    
                    commandQueue.Enqueue(command);
                }
                else
                {
                    commandQueue.Enqueue(command);
                    Connect();
                }
            }
            // ReSharper disable once EmptyGeneralCatchClause
            catch
            {

            }
        }
        
        public void Unsubscribe(UrlServiceSubscription subscription)
        {
            var requestId = subscription.RequestId;

            if (Requests[requestId] == null) return;
            var headers = new JsonObject();

            try
            {
                headers.Add("type", JsonValue.CreateStringValue("unsubscribe"));
                headers.Add("id", JsonValue.CreateStringValue(requestId.ToString()));
            }
            // ReSharper disable once RedundantCatchClause
            catch
            {
                throw;
            }

            SendMessage(headers, null);
            Requests.Remove(requestId);
        }

        public void Unsubscribe(IServiceSubscription subscription) { }

        protected void SendCommandImmediately(ServiceCommand command)
        {
            var headers = new JsonObject();
            var payload = (JsonObject)command.Payload;
            var payloadType = "";

            try
            {
                if (payload != null && payload.ContainsKey("type"))
                    payloadType = payload.GetNamedString("type");
            }
            // ReSharper disable once EmptyGeneralCatchClause
            catch (Exception)
            {
                // ignore
            }

            if (payloadType == "p2p")
            {
                foreach (var key in payload.Select(pair => pair.Key))
                {
                    try
                    {
                        if (payload != null) headers.Add(key, payload.GetNamedObject(key));
                    }
                    // ReSharper disable once EmptyGeneralCatchClause
                    catch
                    {
                        // ignore
                    }
                }
                SendMessage(headers, null);
            }
            else if (payloadType == "hello")
            {
                if (payload != null)
                {
                    var message = payload.Stringify();
                    try
                    {
                        messageWebSocket.Control.MessageType = SocketMessageType.Utf8;
                        messageWebSocket.OutputStream.FlushAsync().GetResults();
                        if (dr == null)
                            dr = new DataWriter(messageWebSocket.OutputStream);
                        dr.WriteString(message);
                        dr.StoreAsync();
                        Debug.WriteLine("{0} : {1} : {2}", DateTime.Now, "sent", message);
                    }
                    // ReSharper disable once EmptyGeneralCatchClause
                    catch
                    {

                    }
                }
            }
            else
            {
                try
                {
                    headers.Add("type", JsonValue.CreateStringValue(command.HttpMethod));
                    headers.Add("id", JsonValue.CreateStringValue(command.RequestId.ToString()));
                    headers.Add("uri", JsonValue.CreateStringValue(command.Target));

                }
                // ReSharper disable once EmptyGeneralCatchClause
                catch (Exception)
                {
                    // TODO: handle this
                }


                SendMessage(headers, payload);
            }

        }

        public bool IsConnected()
        {
            return messageWebSocket != null && state != State.Initial;
        }

        public void SendMessage(JsonObject packet, JsonObject payload)
        {
            try
            {
                if (payload != null)
                {
                    packet.Add("payload", payload);
                }
            }
            // ReSharper disable once EmptyGeneralCatchClause
            catch
            {

            }

            if (IsConnected())
            {
                var message = packet.Stringify();
                Logger.Current.AddMessage("webOS Socket [OUT]: " + message);
                try
                {
                    messageWebSocket.Control.MessageType = SocketMessageType.Utf8;
                    messageWebSocket.OutputStream.FlushAsync().GetResults();
                    if (dr == null)
                        dr = new DataWriter(messageWebSocket.OutputStream);
                    dr.WriteString(message);
                    dr.StoreAsync();
                }
                // ReSharper disable once EmptyGeneralCatchClause
                catch
                {
                    HandleConnectionLost(false, null);
                }
            }
            else
            {
                HandleConnectionLost(false, null);
            }
        }


        private void HandleConnectionLost(bool cleanDisconnect, Exception ex)
        {
            ServiceCommandError error = null;

            if (ex != null || !cleanDisconnect)
                error = new ServiceCommandError(0, ex);

            if (Listener != null)
                Listener.OnCloseWithError(error);

            foreach (var serviceCommand in Requests)
            {
                var request = serviceCommand.Value;
                if (request != null)
                    Util.PostError(request.ResponseListenerValue, new ServiceCommandError(0, "connection lost"));

            }

            Requests.Clear();
        }
    }
}