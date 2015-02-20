using System;
using System.Collections.Generic;
using Windows.Data.Json;
using ConnectSdk.Windows.Core;
using ConnectSdk.Windows.Service.Capability;
using ConnectSdk.Windows.Service.Capability.Listeners;
using ConnectSdk.Windows.Service.Command;
using ConnectSdk.Windows.Service.WebOs;

namespace ConnectSdk.Windows.Service.Sessions
{
    public class WebOsWebAppSession : WebAppSession
    {
        private const String namespaceKey = "connectsdk.";
        private readonly Dictionary<String, ServiceCommand> mActiveCommands;
        private readonly WebOstvServiceSocketClientListener mSocketListener;

        private int uid;
        public UrlServiceSubscription AppToAppSubscription;
        public bool Connected;
        public ResponseListener MConnectionListener;
        private String mFullAppId;
        private IServiceSubscription mMessageSubscription;
        private IServiceSubscription mPlayStateSubscription;
        protected WebOstvService service;
        public WebOstvServiceSocketClient Socket;

        public WebOsWebAppSession(LaunchSession launchSession, DeviceService service) :
            base(launchSession, service)
        {
            uid = 0;
            mActiveCommands = new Dictionary<String, ServiceCommand>();
            Connected = false;

            this.service = (WebOstvService)service;
            mSocketListener = new WebOstvServiceSocketClientListener(this);
        }

        private int GetNextId()
        {
            return ++uid;
        }

        public bool IsConnected()
        {
            return Connected;
        }

        public void SetConnected(bool connected)
        {
            Connected = connected;
        }

        public void HandleMediaEvent(JsonObject payload)
        {
            String type = "";

            type = payload.GetNamedString("type");
            if (type.Length == 0)
                return;

            if (type.Equals("playState"))
            {
                if (mPlayStateSubscription == null)
                    return;

                String playStateString = payload.GetNamedString(type);
                if (playStateString.Length == 0)
                    return;

                PlayStateStatus playState = ParsePlayState(playStateString);

                foreach (ResponseListener listener in mPlayStateSubscription.GetListeners())
                {
                    Util.PostSuccess(listener, playState);
                }
            }
        }

        public String GetFullAppId()
        {
            if (mFullAppId == null)
            {
                if (LaunchSession.SessionType != LaunchSessionType.WebApp)
                    mFullAppId = LaunchSession.AppId;
                else
                {
                    foreach (var mapPair in service.AppToAppIdMappings)
                    {
                    }
                    foreach (var pair in service.AppToAppIdMappings)
                    {
                        String mappedFullAppId = pair.Key;
                        String mappedAppId = pair.Value;

                        if (mappedAppId.Equals(LaunchSession.AppId))
                        {
                            mFullAppId = mappedAppId;
                            break;
                        }
                    }
                }
            }

            if (mFullAppId == null)
                return LaunchSession.AppId;
            return mFullAppId;
        }

        public void SetFullAppId(String fullAppId)
        {
            mFullAppId = fullAppId;
        }


        public void HandleMediaCommandResponse(JsonObject payload)
        {
            String requetID = payload.GetNamedString("requestId");
            if (requetID.Length == 0)
                return;

            ServiceCommand command = mActiveCommands[requetID];

            if (command == null)
                return;

            String mError = payload.GetNamedString("error");

            if (mError.Length != 0)
            {
                Util.PostError(command.ResponseListenerValue, new ServiceCommandError(0, mError));
            }
            else
            {
                Util.PostSuccess(command.ResponseListenerValue, payload);
            }

            mActiveCommands.Remove(requetID);
        }

        public void HandleMessage(Object message)
        {
            if (WebAppSessionListener != null)
                WebAppSessionListener.OnReceiveMessage(this, message);
        }

        public PlayStateStatus ParsePlayState(String playStateString)
        {
            if (playStateString.Equals("playing"))
                return PlayStateStatus.Playing;
            if (playStateString.Equals("paused"))
                return PlayStateStatus.Paused;
            if (playStateString.Equals("idle"))
                return PlayStateStatus.Idle;
            if (playStateString.Equals("buffering"))
                return PlayStateStatus.Buffering;
            if (playStateString.Equals("finished"))
                return PlayStateStatus.Finished;

            return PlayStateStatus.Unknown;
        }

        public void Connect(ResponseListener connectionListener)
        {
            connect(false, connectionListener);
        }

        public void Join(ResponseListener connectionListener)
        {
            connect(true, connectionListener);
        }

        private void connect(bool joinOnly, ResponseListener connectionListener)
        {
            if (Socket != null && Socket.State == State.Connecting)
            {
                if (connectionListener != null) ;
                connectionListener.OnError(new ServiceCommandError(0,
                    "You have a connection request pending,  please wait until it has finished"));

                return;
            }

            if (IsConnected())
            {
                if (connectionListener != null)
                    connectionListener.OnSuccess(null);

                return;
            }

            MConnectionListener = new ResponseListener();
            MConnectionListener.Success += (sender, o) =>
            {
                var finalConnectionListener = new ResponseListener();
                finalConnectionListener.Success += (sender1, o1) =>
                {
                    Connected = true;

                    if (connectionListener != null)
                        connectionListener.OnSuccess(o1);
                };
                finalConnectionListener.Error += (sender1, error1) =>
                {
                    DisconnectFromWebApp();

                    if (MConnectionListener != null)
                        MConnectionListener.OnError(error1);
                };
                service.ConnectToWebApp(this, joinOnly, finalConnectionListener);
            };
            MConnectionListener.Error += (sender, error) =>
            {
                if (Socket != null)
                    DisconnectFromWebApp();

                if (connectionListener != null)
                {
                    if (error == null)
                        error = new ServiceCommandError(0, "Unknown error connecting to web app");

                    connectionListener.OnError(error);
                }
            };


            if (Socket != null)
            {
                if (Socket.IsConnected())
                    MConnectionListener.OnSuccess(null);
                else
                    Socket.Connect();
            }
            else
            {
                Socket = new WebOstvServiceSocketClient(service, WebOstvServiceSocketClient.GetUri(service));
                Socket.Listener = mSocketListener;
                Socket.Connect();
            }
        }

        public void DisconnectFromWebApp()
        {
            Connected = false;
            MConnectionListener = null;

            if (AppToAppSubscription != null)
            {
                AppToAppSubscription.RemoveListeners();
                AppToAppSubscription = null;
            }

            if (Socket != null)
            {
                Socket.Listener = null;
                Socket.Disconnect();
                Socket = null;
            }
        }

        public void sendMessage(String message, ResponseListener listener)
        {
            if (string.IsNullOrEmpty(message))
            {
                if (listener != null)
                    listener.OnError(new ServiceCommandError(0, null));

                return;
            }

            sendP2PMessage(message, listener);
        }

        public void sendMessage(JsonObject message, ResponseListener listener)
        {
            if (message == null || message.Count == 0)
            {
                Util.PostError(listener, new ServiceCommandError(0, null));

                return;
            }

            sendP2PMessage(message, listener);
        }

        private void sendP2PMessage(Object message, ResponseListener listener)
        {
            var _payload = new JsonObject();

            try
            {
                _payload.Add("type", JsonValue.CreateStringValue("p2p"));
                _payload.Add("to", JsonValue.CreateStringValue(GetFullAppId()));
                _payload.Add("payload", JsonValue.CreateStringValue(message.ToString()));
            }
            catch (Exception ex)
            {
                throw ex;
            }

            JsonObject payload = _payload;

            if (IsConnected())
            {
                Socket.SendMessage(payload, null);

                if (listener != null)
                    listener.OnSuccess(null);
            }
            else
            {
                var connectListener = new ResponseListener();
                connectListener.Error += (sender, error) =>
                {
                    if (listener != null)
                        listener.OnError(error);
                };
                connectListener.Success += (sender, o) => { sendP2PMessage(message, listener); };

                Connect(connectListener);
            }
        }

        public void close(ResponseListener listener)
        {
            mActiveCommands.Clear();

            if (mPlayStateSubscription != null)
            {
                mPlayStateSubscription.Unsubscribe();
                mPlayStateSubscription = null;
            }

            if (mMessageSubscription != null)
            {
                mMessageSubscription.Unsubscribe();
                mMessageSubscription = null;
            }

            service.GetWebAppLauncher().CloseWebApp(LaunchSession, listener);
        }

        public void Seek(long position, ResponseListener listener)
        {
            if (position < 0)
            {
                if (listener != null)
                    listener.OnError(new ServiceCommandError(0, null));

                return;
            }

            int requestIdNumber = GetNextId();
            String requestId = String.Format("req{0}", requestIdNumber);

            JsonObject message = null;
            try
            {
                message = new JsonObject();
                message.Add("contentType", JsonValue.CreateStringValue(namespaceKey + "mediaCommand"));
                var mediaCommandObject = new JsonObject();
                mediaCommandObject.Add("type", JsonValue.CreateStringValue("seek"));
                mediaCommandObject.Add("position", JsonValue.CreateNumberValue(position / 1000));
                mediaCommandObject.Add("requestId", JsonValue.CreateStringValue(requestId));
                message.Add("mediaCommand", mediaCommandObject);
            }
            catch (Exception e)
            {
                if (listener != null)
                    listener.OnError(new ServiceCommandError(0, null));
            }

            var command = new ServiceCommand(null, null, null, listener);

            mActiveCommands.Add(requestId, command);

            sendMessage(message, listener);
        }

        public void getPosition(ResponseListener listener)
        {
            int requestIdNumber = GetNextId();
            String requestId = String.Format("req{0}", requestIdNumber);

            JsonObject message = null;
            try
            {
                message = new JsonObject();
                message.Add("contentType", JsonValue.CreateStringValue(namespaceKey + "mediaCommand"));
                var mediaCommandObject = new JsonObject();
                mediaCommandObject.Add("type", JsonValue.CreateStringValue("getPosition"));
                mediaCommandObject.Add("requestId", JsonValue.CreateStringValue(requestId));
                message.Add("mediaCommand", mediaCommandObject);
            }
            catch (Exception e)
            {
                if (listener != null)
                    listener.OnError(new ServiceCommandError(0, null));
            }

            var commandResponseListener = new ResponseListener();
            commandResponseListener.Success += (sender, o) =>
            {
                try
                {
                    double position = ((JsonObject)o).GetNamedNumber("position");

                    if (listener != null)
                        listener.OnSuccess(position * 1000);
                }
                catch (Exception e)
                {
                    commandResponseListener.OnError(new ServiceCommandError(0, null));
                }
            };
            commandResponseListener.Error += (sender, error) =>
            {
                if (listener != null)
                    listener.OnError(error);
            };


            var command = new ServiceCommand(null, null, null, commandResponseListener);


            mActiveCommands.Add(requestId, command);
            var messageResponseListener = new ResponseListener();
            messageResponseListener.Error += (sender, error) =>
            {
                if (listener != null)
                    listener.OnError(error);
            };
            messageResponseListener.Success += (sender, o) => { };
            sendMessage(message, messageResponseListener);
        }

        public void getDuration(ResponseListener listener)
        {
            int requestIdNumber = GetNextId();
            String requestId = String.Format("req%d", requestIdNumber);


            JsonObject message = null;
            try
            {
                message = new JsonObject();
                message.Add("contentType", JsonValue.CreateStringValue(namespaceKey + "mediaCommand"));
                var mediaCommandObject = new JsonObject();
                mediaCommandObject.Add("type", JsonValue.CreateStringValue("getDuration"));
                mediaCommandObject.Add("requestId", JsonValue.CreateStringValue(requestId));
                message.Add("mediaCommand", mediaCommandObject);
            }
            catch (Exception e)
            {
                if (listener != null)
                    listener.OnError(new ServiceCommandError(0, null));
            }

            var commandResponseListener = new ResponseListener();
            commandResponseListener.Success += (sender, o) =>
            {
                try
                {
                    double position = ((JsonObject)o).GetNamedNumber("duration");

                    if (listener != null)
                        listener.OnSuccess(position * 1000);
                }
                catch (Exception e)
                {
                    commandResponseListener.OnError(new ServiceCommandError(0, null));
                }
            };
            commandResponseListener.Error += (sender, error) =>
            {
                if (listener != null)
                    listener.OnError(error);
            };


            var command = new ServiceCommand(null, null, null, commandResponseListener);

            mActiveCommands.Add(requestId, command);
            var messageResponseListener = new ResponseListener();
            messageResponseListener.Error += (sender, error) =>
            {
                if (listener != null)
                    listener.OnError(error);
            };
            messageResponseListener.Success += (sender, o) => { };
            sendMessage(message, messageResponseListener);
        }

        public void getPlayState(ResponseListener listener)
        {
            int requestIdNumber = GetNextId();
            String requestId = String.Format("req%d", requestIdNumber);


            JsonObject message = null;
            try
            {
                message = new JsonObject();
                message.Add("contentType", JsonValue.CreateStringValue(namespaceKey + "mediaCommand"));
                var mediaCommandObject = new JsonObject();
                mediaCommandObject.Add("type", JsonValue.CreateStringValue("getPlayState"));
                mediaCommandObject.Add("requestId", JsonValue.CreateStringValue(requestId));
                message.Add("mediaCommand", mediaCommandObject);
            }
            catch (Exception e)
            {
                if (listener != null)
                    listener.OnError(new ServiceCommandError(0, null));
            }

            var commandResponseListener = new ResponseListener();
            commandResponseListener.Success += (sender, o) =>
            {
                try
                {
                    double position = ((JsonObject)o).GetNamedNumber("duration");

                    if (listener != null)
                        listener.OnSuccess(position * 1000);
                }
                catch (Exception e)
                {
                    commandResponseListener.OnError(new ServiceCommandError(0, null));
                }
            };
            commandResponseListener.Error += (sender, error) =>
            {
                if (listener != null)
                    listener.OnError(error);
            };


            var command = new ServiceCommand(null, null, null, commandResponseListener);

            mActiveCommands.Add(requestId, command);
            var messageResponseListener = new ResponseListener();
            messageResponseListener.Error += (sender, error) =>
            {
                if (listener != null)
                    listener.OnError(error);
            };
            messageResponseListener.Success += (sender, o) => { };
            sendMessage(message, messageResponseListener);
        }

        public IServiceSubscription SubscribePlayState(ResponseListener listener)
        {
            if (mPlayStateSubscription == null)
                mPlayStateSubscription = new UrlServiceSubscription(null, null, null, null);

            if (!Connected)
            {
                var connectResponseListener = new ResponseListener();
                connectResponseListener.Error += (sender, error) => Util.PostError(listener, error);
                connectResponseListener.Success += (sender, o) => { };
                Connect(connectResponseListener);
            }

            if (!mPlayStateSubscription.GetListeners().Contains(listener))
                mPlayStateSubscription.AddListener(listener);

            return mPlayStateSubscription;
        }

        /*****************
	 * Media Control *
	 *****************/

        public IMediaControl GetMediaControl()
        {
            return this;
        }

        public CapabilityPriorityLevel GetMediaControlCapabilityLevel()
        {
            return CapabilityPriorityLevel.HIGH;
        }

        /****************
         * Media Player *
         ****************/

        public IMediaPlayer GetMediaPlayer()
        {
            return this;
        }

        public CapabilityPriorityLevel GetMediaPlayerCapabilityLevel()
        {
            return CapabilityPriorityLevel.HIGH;
        }

        public void DisplayImage(String url, String mimeType, String title, String description, String iconSrc,
            ResponseListener listener)
        {
            int requestIdNumber = GetNextId();
            String requestId = String.Format("req{0}", requestIdNumber);

            JsonObject message = null;
            try
            {
                message = new JsonObject();
                message.Add("contentType", JsonValue.CreateStringValue(namespaceKey + "mediaCommand"));
                var mediaCommandObject = new JsonObject();
                mediaCommandObject.Add("type", JsonValue.CreateStringValue("getPlayState"));
                mediaCommandObject.Add("mediaURL", JsonValue.CreateStringValue(url));
                mediaCommandObject.Add("iconURL", JsonValue.CreateStringValue(iconSrc));
                mediaCommandObject.Add("title", JsonValue.CreateStringValue(title));
                mediaCommandObject.Add("description", JsonValue.CreateStringValue(description));
                mediaCommandObject.Add("mimeType", JsonValue.CreateStringValue(mimeType));
                mediaCommandObject.Add("requestId", JsonValue.CreateStringValue(requestId));
                message.Add("mediaCommand", mediaCommandObject);
            }
            catch (Exception e)
            {
                if (listener != null)
                    listener.OnError(new ServiceCommandError(0, null));
            }
            var responseListener = new ResponseListener();
            responseListener.Error += (sender, error) => { Util.PostError(listener, error); };
            responseListener.Success +=
                (sender, o) => { Util.PostSuccess(listener, new MediaLaunchObject(LaunchSession, GetMediaControl())); };


            var command = new ServiceCommand(Socket, null, null, responseListener);

            mActiveCommands.Add(requestId, command);

            var messageResponseListener = new ResponseListener();
            messageResponseListener.Error += (sender, error) => { Util.PostError(listener, error); };
            messageResponseListener.Success += (sender, o) => { };

            sendP2PMessage(message, messageResponseListener);
        }

        public void playMedia(String url, String mimeType, String title, String description, String iconSrc,
            bool shouldLoop, ResponseListener listener)
        {
            int requestIdNumber = GetNextId();
            String requestId = String.Format("req{0}", requestIdNumber);

            JsonObject message = null;
            try
            {
                message = new JsonObject();
                message.Add("contentType", JsonValue.CreateStringValue(namespaceKey + "mediaCommand"));
                var mediaCommandObject = new JsonObject();
                mediaCommandObject.Add("type", JsonValue.CreateStringValue("playMedia"));
                mediaCommandObject.Add("mediaURL", JsonValue.CreateStringValue(url));
                mediaCommandObject.Add("iconURL", JsonValue.CreateStringValue(iconSrc));
                mediaCommandObject.Add("title", JsonValue.CreateStringValue(title));
                mediaCommandObject.Add("description", JsonValue.CreateStringValue(description));
                mediaCommandObject.Add("mimeType", JsonValue.CreateStringValue(mimeType));
                mediaCommandObject.Add("shouldLoop", JsonValue.CreateBooleanValue(shouldLoop));
                mediaCommandObject.Add("requestId", JsonValue.CreateStringValue(requestId));

                message.Add("mediaCommand", mediaCommandObject);
            }
            catch (Exception e)
            {
                if (listener != null)
                    listener.OnError(new ServiceCommandError(0, null));
            }
            var responseListener = new ResponseListener();
            responseListener.Error += (sender, error) => { Util.PostError(listener, error); };
            responseListener.Success +=
                (sender, o) => Util.PostSuccess(listener, new MediaLaunchObject(LaunchSession, GetMediaControl()));


            var command = new ServiceCommand(Socket, null, null, responseListener);

            mActiveCommands.Add(requestId, command);

            var messageResponseListener = new ResponseListener();
            messageResponseListener.Error += (sender, error) => { Util.PostError(listener, error); };
            messageResponseListener.Success += (sender, o) => { };

            sendP2PMessage(message, messageResponseListener);
        }

        public class WebOstvServiceSocketClientListener : IWebOstvServiceSocketClientListener
        {
            private readonly WebOsWebAppSession webOsWebAppSession;

            public WebOstvServiceSocketClientListener(WebOsWebAppSession webOsWebAppSession)
            {
                this.webOsWebAppSession = webOsWebAppSession;
            }

            public void OnConnect()
            {
                if (webOsWebAppSession.MConnectionListener != null)
                    webOsWebAppSession.MConnectionListener.OnSuccess(null);

                webOsWebAppSession.MConnectionListener = null;
            }

            public void OnCloseWithError(ServiceCommandError error)
            {
                webOsWebAppSession.Connected = false;
                webOsWebAppSession.AppToAppSubscription = null;

                if (webOsWebAppSession.MConnectionListener != null)
                {
                    if (error != null)
                        webOsWebAppSession.MConnectionListener.OnError(error);
                    else
                    {
                        if (webOsWebAppSession.WebAppSessionListener != null)
                            webOsWebAppSession.WebAppSessionListener.OnWebAppSessionDisconnect(webOsWebAppSession);
                    }
                }

                webOsWebAppSession.MConnectionListener = null;
            }

            public void OnFailWithError(ServiceCommandError error)
            {
                webOsWebAppSession.Connected = false;
                webOsWebAppSession.AppToAppSubscription = null;

                if (webOsWebAppSession.MConnectionListener != null)
                {
                    if (error == null)
                        error = new ServiceCommandError(0, null);

                    webOsWebAppSession.MConnectionListener.OnError(error);
                }

                webOsWebAppSession.MConnectionListener = null;
            }

            public void OnBeforeRegister()
            {
            }

            public void OnRegistrationFailed(ServiceCommandError error)
            {
            }

            public bool OnReceiveMessage(JsonObject payload)
            {
                String type = payload.GetNamedString("type");

                if ("p2p".Equals(type))
                {
                    String fromAppId = null;

                    fromAppId = payload.GetNamedString("from");

                    if (!fromAppId.Equals(webOsWebAppSession.GetFullAppId()))
                        return false;

                    Object message = payload.GetNamedObject("payload");

                    if (message is JsonObject)
                    {
                        var messageJSON = (JsonObject)message;

                        String contentType = messageJSON.GetNamedString("contentType");
                        int contentTypeIndex = contentType.IndexOf("connectsdk.", StringComparison.OrdinalIgnoreCase);

                        if (contentType != null && contentTypeIndex >= 0)
                        {
                            //String payloadKey = contentType.Split("connectsdk.",)[1];

                            String payloadKey = contentType.Split('.')[3];
                            if (payloadKey == null || payloadKey.Length == 0)
                                return false;

                            JsonObject messagePayload = messageJSON.GetNamedObject(payloadKey);

                            if (messagePayload == null)
                                return false;

                            if (payloadKey.Equals("mediaEvent", StringComparison.OrdinalIgnoreCase))
                                webOsWebAppSession.HandleMediaEvent(messagePayload);
                            else if (payloadKey.Equals("mediaCommandResponse", StringComparison.OrdinalIgnoreCase))
                                webOsWebAppSession.HandleMediaCommandResponse(messagePayload);
                        }
                        else
                        {
                            webOsWebAppSession.HandleMessage(messageJSON);
                        }
                    }
                    else if (message is String)
                    {
                        webOsWebAppSession.HandleMessage(message);
                    }

                    return false;
                }

                return true;
            }
        }
    }
}
