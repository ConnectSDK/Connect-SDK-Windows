using System;
using System.Collections.Generic;
using System.Linq;
using Windows.Data.Json;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;
using ConnectSdk.Windows.Core;
using ConnectSdk.Windows.Service.Capability.Listeners;
using ConnectSdk.Windows.Service.Command;
using ConnectSdk.Windows.Service.Config;

namespace ConnectSdk.Windows.Service.WebOs
{
    public class WebOstvServiceSocketClient : IServiceCommandProcessor
    {
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

        public WebOstvServiceSocketClient(WebOstvService service, Uri uri)
        {
            destinationUri = uri;
            this.service = service;
            State = State.Initial;
            CreateSocket();
            State = State.Registered;
            SetDefaultManifest();
        }

        public IWebOstvServiceSocketClientListener Listener { get; set; }

        public State State
        {
            get { return state; }
            set { state = value; }
        }

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
            };
            messageWebSocket.Closed += (sender, args) => OnClose(0, args.ToString());
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
                await messageWebSocket.ConnectAsync(destinationUri);
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
            }
                // ReSharper disable once EmptyGeneralCatchClause
            catch
            {
                //throw e;
            }
        }

        protected void HandleMessage(JsonObject message)
        {
            var shouldProcess = true;

            if (Listener != null)
                shouldProcess = Listener.OnReceiveMessage(message);

            if (!shouldProcess)
                return;

            var type = message.GetNamedString("type");
            Object payload = message.GetNamedObject("payload");

            int id;
            try
            {
                id = (int)message.GetNamedNumber("id");
            }
            catch
            {
                var intstr = message.GetNamedString("id");
                int.TryParse(intstr, out id);
            }
            ServiceCommand request = null;


            try
            {
                request = Requests[id];
            }
                // ReSharper disable once EmptyGeneralCatchClause
            catch
            {
                // since request is assigned to null, don't need to do anything here
            }

            if (type.Length == 0)
                return;

            if ("response".Equals(type))
            {
                if (request != null)
                {
                    if (payload != null)
                    {
                        try
                        {
                            Util.PostSuccess(request.ResponseListenerValue, payload);
                        }
                            // ReSharper disable once EmptyGeneralCatchClause
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
                        Requests.Remove(id);
                    }
                }
            }
            else if ("registered".Equals(type))
            {
                if (!(service.ServiceConfig is WebOSTVServiceConfig))
                {
                    service.ServiceConfig = new WebOSTVServiceConfig(service.ServiceConfig.ServiceUuid);
                }

                if (payload != null)
                {
                    var clientKey = ((JsonObject)payload).GetNamedString("client-key");
                    ((WebOSTVServiceConfig)service.ServiceConfig).setClientKey(clientKey);

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
                try
                {
                    var parts = error.Split(' ');
                    errorCode = int.Parse(parts[0]);
                }
                // ReSharper disable once RedundantCatchClause
                catch
                {
                    throw;
                }

                if (!message.ContainsKey("id")) return;
                if (request == null) return;
                Util.PostError(request.ResponseListenerValue,
                    new ServiceCommandError(errorCode, payload));

                if (!(request is UrlServiceSubscription))
                    Requests.
                        Remove(id);

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
                        ((WebOSTVServiceConfig)service.ServiceConfig).setClientKey(null);
                        String cert = null;
                        // ReSharper disable once ExpressionIsAlwaysNull
                        ((WebOSTVServiceConfig)service.ServiceConfig).setServerCertificate(cert);
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

        private void HelloTv()
        {
            //Context context = DiscoveryManager.getInstance().getContext();
            //PackageManager packageManager = context.getPackageManager();

            //// app Id
            //String packageName = context.getPackageName();

            //// SDK Version
            //String sdkVersion = DiscoveryManager.CONNECT_SDK_VERSION;

            //// Device Model
            //String deviceModel = Build.MODEL;

            //// OS Version
            //String OSVersion = String.valueOf(android.os.Build.VERSION.SDK_INT);

            //// resolution
            //WindowManager wm = (WindowManager) context.getSystemService(Context.WINDOW_SERVICE);
            //Display display = wm.getDefaultDisplay();

            //@SuppressWarnings("deprecation")
            //int width = display.getWidth(); // deprecated, but still needed for supporting API levels 10-12

            //@SuppressWarnings("deprecation")
            //int height = display.getHeight(); // deprecated, but still needed for supporting API levels 10-12

            //String screenResolution = String.format("%dx%d", width, height); 

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

            //JsonObject payload = new JsonObject();
            //try
            //{
            //    payload.put("sdkVersion", sdkVersion);
            //    payload.put("deviceModel", deviceModel);
            //    payload.put("OSVersion", OSVersion);
            //    payload.put("resolution", screenResolution);
            //    payload.put("appId", packageName);
            //    payload.put("appName", applicationName);
            //    payload.put("appRegion", appRegion);
            //}
            //catch (Exception e)
            //{
            //    throw e;
            //}

            //int dataId = this.nextRequestId++;

            //JsonObject sendData = new JsonObject();
            //try
            //{
            //    sendData.put("id", dataId);
            //    sendData.put("type", "hello");
            //    sendData.put("payload", payload);
            //}
            //catch (Exception e)
            //{
            //    throw e;
            //}

            //ServiceCommand<ResponseListener<Object>> request = new ServiceCommand<ResponseListener<Object>>(this, null, sendData, true, null);
            //this.sendCommandImmediately(request);
        }

        protected void SendRegister()
        {
            var requestListener = new ResponseListener();
            requestListener.Success += (sender, o) =>
            {

            };
            requestListener.Error += (sender, o) =>
            {
                if (Listener != null)
                    Listener.OnRegistrationFailed(o);
            };

            int dataId = nextRequestId++;

            var command = new ServiceCommand(this, null, null, requestListener) {RequestId = dataId};

            var headers = new JsonObject();
            var payload = new JsonObject();

            try
            {
                headers.Add("type", JsonValue.CreateStringValue("register"));
                headers.Add("id", JsonValue.CreateNumberValue(dataId));

                if (!(service.ServiceConfig is WebOSTVServiceConfig))
                {
                    service.ServiceConfig = new WebOSTVServiceConfig(service.ServiceConfig.ServiceUuid);
                }

                if (((WebOSTVServiceConfig)service.ServiceConfig).getClientKey() != null)
                {
                    payload.Add("client-key", JsonValue.CreateStringValue(((WebOSTVServiceConfig)service.ServiceConfig).getClientKey()));
                }
                else
                {
                    if (Listener != null)
                        Listener.OnBeforeRegister();
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
                    SendCommandImmediately(command);
                    commandQueue.Dequeue();
                }
            }

            if (Listener != null)
                Listener.OnConnect();
        }

        public void Unsubscribe(IServiceSubscription subscription)
        {
            throw new NotImplementedException();
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
                SendCommandImmediately(command);
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

        protected void SendCommandImmediately(ServiceCommand command)
        {
            var headers = new JsonObject();
            var payload = (JsonObject)command.Payload;
            var payloadType = "";

            if (payload != null)
            {
                try
                {
                    payloadType = payload.GetNamedString("type");
                }
                    // ReSharper disable once EmptyGeneralCatchClause
                catch
                {
                }

                if (payloadType == "p2p")
                {
                    foreach (var key in payload.Select(pair => pair.Key))
                    {
                        try
                        {
                            headers.Add(key, payload.GetNamedObject(key));
                        }
                            // ReSharper disable once EmptyGeneralCatchClause
                        catch
                        {
                            // ignore
                        }
                    }
                    SendMessage(headers, null);
                }
                else SendMessage(headers, payload);
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
                catch
                {
                    // TODO: handle this
                }
                SendMessage(headers, payload);
            }
        }

        public bool IsConnected()
        {
            return messageWebSocket != null;
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

            for (int i = 0; i < Requests.Count; i++)
            {
                var request = Requests[i];

                if (request != null)
                    Util.PostError(request.ResponseListenerValue, new ServiceCommandError(0, "connection lost"));
            }

            Requests.Clear();
        }
    }
}