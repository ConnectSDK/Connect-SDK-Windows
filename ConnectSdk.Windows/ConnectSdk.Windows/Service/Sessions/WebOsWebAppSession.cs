#region Copyright Notice
/*
 * ConnectSdk.Windows
 * WebOsWebAppSession.cs
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
using System.Collections.Concurrent;
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
        private const String NamespaceKey = "connectsdk.";
        private readonly ConcurrentDictionary<String, ServiceCommand> mActiveCommands;
        private readonly WebOsWebAppSession.WebOstvServiceSocketClientListener mSocketListener;

        public UrlServiceSubscription AppToAppSubscription;
        private bool connected;
        public ResponseListener MConnectionListener;
        public WebOstvServiceSocketClient Socket;
        private String mFullAppId;
        private IServiceSubscription mMessageSubscription;
        private IServiceSubscription mPlayStateSubscription;
        protected new WebOstvService Service;
        private int uid;

        public bool Connected
        {
            get
            {
                return connected;
            }
            set
            {
                connected = value;
            }
        }

        public WebOsWebAppSession(LaunchSession launchSession, DeviceService service) :
            base(launchSession, service)
        {
            uid = 0;
            mActiveCommands = new ConcurrentDictionary<String, ServiceCommand>();
            Connected = false;

            Service = (WebOstvService) service;
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

        //public void SetConnected(bool connected)
        //{
        //    Connected = connected;
        //}

        public void HandleMediaEvent(JsonObject payload)
        {
            var type = payload.GetNamedString("type");
            if (type.Length == 0)
                return;

            if (!type.Equals("playState")) return;
            if (mPlayStateSubscription == null)
                return;

            var playStateString = payload.GetNamedString(type);
            if (playStateString.Length == 0)
                return;

            var playState = ParsePlayState(playStateString);

            foreach (var listener in mPlayStateSubscription.GetListeners())
            {
                Util.PostSuccess(listener, playState);
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
                    foreach (var pair in Service.AppToAppIdMappings)
                    {
                        var mappedAppId = pair.Value;

                        if (!mappedAppId.Equals(LaunchSession.AppId)) continue;

                        mFullAppId = mappedAppId;
                        break;
                    }
                }
            }

            return mFullAppId ?? LaunchSession.AppId;
        }

        public void SetFullAppId(String fullAppId)
        {
            mFullAppId = fullAppId;
        }

        public void HandleMediaCommandResponse(JsonObject payload)
        {
            var requestId = payload.GetNamedString("requestId");
            if (requestId.Length == 0)
                return;

            var command = mActiveCommands[requestId];

            if (command == null)
                return;

            var mError = payload.GetNamedString("error","");

            if (mError.Length != 0)
            {
                Util.PostError(command.ResponseListenerValue, new ServiceCommandError(0, mError));
            }
            else
            {
                Util.PostSuccess(command.ResponseListenerValue, payload);
            }
            ServiceCommand cmd;
            mActiveCommands.TryRemove(requestId, out cmd);
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
            return playStateString.Equals("finished") ? PlayStateStatus.Finished : PlayStateStatus.Unknown;
        }

        public new void Connect(ResponseListener connectionListener)
        {
            //// reuse the socket from the service if present. Fixes bug for displayImage
            //if (Service.Socket != null && Socket == null)
            //{
            //    Socket = Service.Socket;

            //}
            Connect(false, connectionListener);
        }

        public new void Join(ResponseListener connectionListener)
        {
            Connect(true, connectionListener);
        }

        private void Connect(bool joinOnly, ResponseListener connectionListener)
        {
            if (Socket != null && Socket.State == State.Connecting)
            {
                if (connectionListener != null)
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


            MConnectionListener = new ResponseListener
            (
                loadEventArg =>
                {
                    var finalConnectionListener = new ResponseListener
                    (
                        loadEventArg2 =>
                        {
                            Connected = true;

                            if (connectionListener != null)
                                connectionListener.OnSuccess(loadEventArg2);
                        },
                        serviceCommandError =>
                        {
                            DisconnectFromWebApp();

                            if (connectionListener != null)
                                connectionListener.OnError(serviceCommandError);
                        }
                    );

                    Service.ConnectToWebApp(this, joinOnly, finalConnectionListener);
                },
                serviceCommandError =>
                {
                    if (Socket != null)
                        DisconnectFromWebApp();

                    if (connectionListener != null)
                    {
                        if (serviceCommandError == null)
                            serviceCommandError = new ServiceCommandError(0, "Unknown error connecting to web app");

                        connectionListener.OnError(serviceCommandError);
                    }
                }
            );


            if (Socket != null)
            {
                if (Socket.IsConnected())
                    MConnectionListener.OnSuccess(null);
                else
                    Socket.Connect();
            }
            else
            {
                //var uri = WebOstvServiceSocketClient.GetUri(Service);
                //Socket = new WebOstvServiceSocketClient(Service, uri);
                //Socket.Listener = mSocketListener;
                //Socket.Connect();

                var uri = WebOstvServiceSocketClient.GetUri(Service);
                if (WebOstvServiceSocketClient.SocketCache.ContainsKey(uri.ToString()))
                {
                    Socket = WebOstvServiceSocketClient.SocketCache[uri.ToString()];
                    if (mSocketListener != null)
                    {
                        Socket.Listener = mSocketListener;
                    }
                    MConnectionListener.OnSuccess(null);
                }
                else
                {
                    Socket = new WebOstvServiceSocketClient(Service, uri)
                    {
                        Listener = mSocketListener
                    };
                    Socket.Connect();
                    WebOstvServiceSocketClient.SocketCache.Add(uri.ToString(), Socket);
                }
            }
        }

        public new void DisconnectFromWebApp()
        {
            Connected = false;
            MConnectionListener = null;

            if (AppToAppSubscription != null)
            {
                AppToAppSubscription.RemoveListeners();
                AppToAppSubscription = null;
            }

            if (Socket == null) return;

            Socket.Listener = null;
            Socket.Disconnect();
            Connected = false;
            Socket = null;
        }

        public new void SendMessage(String message, ResponseListener listener)
        {
            if (string.IsNullOrEmpty(message))
            {
                if (listener != null)
                    listener.OnError(new ServiceCommandError(0, null));

                return;
            }

            SendP2PMessage(message, listener);
        }

        public new void SendMessage(JsonObject message, ResponseListener listener)
        {
            if (message == null || message.Count == 0)
            {
                Util.PostError(listener, new ServiceCommandError(0, null));

                return;
            }

            SendP2PMessage(message, listener);
        }

        private void SendP2PMessage(Object message, ResponseListener listener)
        {
            var payload = new JsonObject();

            try
            {
                payload.Add("type", JsonValue.CreateStringValue("p2p"));
                payload.Add("to", JsonValue.CreateStringValue(GetFullAppId()));
                if (message is JsonObject)
                    //todo: check if this is the fix
                    payload.Add("payload", (message as JsonObject));
                else
                    payload.Add("payload", JsonValue.CreateStringValue(message.ToString()));
            }
            catch (Exception ex)
            {
                throw ex;
            }

            if (IsConnected())
            {
                Socket.SendMessage(payload, null);

                if (listener != null)
                    listener.OnSuccess(null);
            }
            else
            {
                var connectListener = new ResponseListener
                (
                loadEventArg => SendP2PMessage(message, listener),
                serviceCommandError =>
                {
                    if (listener != null)
                        listener.OnError(serviceCommandError);
                }
                );

                Connect(connectListener);
            }
        }

        public new void Close(ResponseListener listener)
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

            Service.GetWebAppLauncher().CloseWebApp(LaunchSession, listener);
        }

        public new void Seek(long position, ResponseListener listener)
        {
            if (position < 0)
            {
                if (listener != null)
                    listener.OnError(new ServiceCommandError(0, null));

                return;
            }

            var requestIdNumber = GetNextId();
            var requestId = String.Format("req{0}", requestIdNumber);

            JsonObject message = null;
            try
            {
                message = new JsonObject {{"contentType", JsonValue.CreateStringValue(NamespaceKey + "mediaCommand")}};
                var mediaCommandObject = new JsonObject
                {
                    {"type", JsonValue.CreateStringValue("seek")},
                    {"position", JsonValue.CreateNumberValue((int)(position/1000))},
                    {"requestId", JsonValue.CreateStringValue(requestId)}
                };
                message.Add("mediaCommand", mediaCommandObject);
            }
            catch (Exception)
            {
                if (listener != null)
                    listener.OnError(new ServiceCommandError(0, null));
            }

            var command = new ServiceCommand(null, null, null, listener);

            mActiveCommands.TryAdd(requestId, command);

            SendMessage(message, listener);
        }

        public new void GetPosition(ResponseListener listener)
        {
            var requestIdNumber = GetNextId();
            var requestId = String.Format("req{0}", requestIdNumber);

            JsonObject message = null;
            try
            {
                message = new JsonObject {{"contentType", JsonValue.CreateStringValue(NamespaceKey + "mediaCommand")}};
                var mediaCommandObject = new JsonObject
                {
                    {"type", JsonValue.CreateStringValue("getPosition")},
                    {"requestId", JsonValue.CreateStringValue(requestId)}
                };
                message.Add("mediaCommand", mediaCommandObject);
            }
            catch (Exception)
            {
                if (listener != null)
                    listener.OnError(new ServiceCommandError(0, null));
            }


            var commandResponseListener = new ResponseListener
            (
                loadEventArg =>
                {
                    try
                    {
                        var position = ((JsonObject)loadEventArg).GetNamedNumber("position");

                        if (listener != null)
                            listener.OnSuccess(position * 1000);
                    }
                    catch (Exception)
                    {
                        if (listener != null) listener.OnError(new ServiceCommandError(0, null));
                    }
                },
                serviceCommandError =>
                {
                    if (listener != null)
                        listener.OnError(serviceCommandError);
                }
            );

            var command = new ServiceCommand(null, null, null, commandResponseListener);
            mActiveCommands.TryAdd(requestId, command);


            var messageResponseListener = new ResponseListener
            (
                loadEventArg =>
                {

                },
                serviceCommandError =>
                {
                    if (listener != null)
                        listener.OnError(serviceCommandError);
                }
            );

            SendMessage(message, messageResponseListener);
        }

        public new void GetDuration(ResponseListener listener)
        {
            var requestIdNumber = GetNextId();
            var requestId = String.Format("req{0}", requestIdNumber);

            JsonObject message = null;
            try
            {
                message = new JsonObject {{"contentType", JsonValue.CreateStringValue(NamespaceKey + "mediaCommand")}};
                var mediaCommandObject = new JsonObject
                {
                    {"type", JsonValue.CreateStringValue("getDuration")},
                    {"requestId", JsonValue.CreateStringValue(requestId)}
                };
                message.Add("mediaCommand", mediaCommandObject);
            }
            catch (Exception)
            {
                if (listener != null)
                    listener.OnError(new ServiceCommandError(0, null));
            }


            var commandResponseListener = new ResponseListener
            (
                loadEventArg =>
                {
                    try
                    {
                        var position = ((JsonObject)loadEventArg).GetNamedNumber("duration");

                        if (listener != null)
                            listener.OnSuccess(position * 1000);
                    }
                    catch (Exception)
                    {
                        if (listener != null) listener.OnError(new ServiceCommandError(0, null));
                    }
                },
                serviceCommandError =>
                {
                    if (listener != null)
                        listener.OnError(serviceCommandError);
                }
            );



            var command = new ServiceCommand(null, null, null, commandResponseListener);

            mActiveCommands.TryAdd(requestId, command);


            var responseListener = new ResponseListener
            (
                loadEventArg =>
                {

                },
                serviceCommandError =>
                {
                    if (listener != null)
                        listener.OnError(serviceCommandError);
                }
            );

            SendMessage(message, responseListener);
        }

        public new void GetPlayState(ResponseListener listener)
        {
            var requestIdNumber = GetNextId();
            var requestId = String.Format("req{0}", requestIdNumber);


            JsonObject message = null;
            try
            {
                message = new JsonObject {{"contentType", JsonValue.CreateStringValue(NamespaceKey + "mediaCommand")}};
                var mediaCommandObject = new JsonObject
                {
                    {"type", JsonValue.CreateStringValue("getPlayState")},
                    {"requestId", JsonValue.CreateStringValue(requestId)}
                };
                message.Add("mediaCommand", mediaCommandObject);
            }
            catch (Exception)
            {
                if (listener != null)
                    listener.OnError(new ServiceCommandError(0, null));
            }

            var commandResponseListener = new ResponseListener
            (
                loadEventArg =>
                {
                    try
                    {
                        var position = ((JsonObject)loadEventArg).GetNamedNumber("duration");

                        if (listener != null)
                            listener.OnSuccess(position * 1000);
                    }
                    catch (Exception)
                    {
                        if (listener != null) listener.OnError(new ServiceCommandError(0, null));
                    }
                },
                serviceCommandError =>
                {
                    if (listener != null)
                        listener.OnError(serviceCommandError);
                }
            );

            var command = new ServiceCommand(null, null, null, commandResponseListener);

            mActiveCommands.TryAdd(requestId, command);

            var responseListener = new ResponseListener
            (
                loadEventArg =>
                {

                },
                serviceCommandError =>
                {
                    if (listener != null)
                        listener.OnError(serviceCommandError);
                }
            );

            SendMessage(message, responseListener);
        }

        public new IServiceSubscription SubscribePlayState(ResponseListener listener)
        {
            if (mPlayStateSubscription == null)
                mPlayStateSubscription = new UrlServiceSubscription(null, null, null, null);

            if (!Connected)
            {

                var connectResponseListener = new ResponseListener
                (
                loadEventArg => { },
                serviceCommandError => Util.PostError(listener, serviceCommandError)
                );

                Connect(connectResponseListener);
            }

            if (!mPlayStateSubscription.GetListeners().Contains(listener))
                mPlayStateSubscription.AddListener(listener);

            return mPlayStateSubscription;
        }


        #region Media Control
        public new IMediaControl GetMediaControl()
        {
            return this;
        }

        public new CapabilityPriorityLevel GetMediaControlCapabilityLevel()
        {
            return CapabilityPriorityLevel.High;
        } 
        #endregion

        /****************
         * Media Player *
         ****************/

        public new IMediaPlayer GetMediaPlayer()
        {
            return this;
        }

        public new CapabilityPriorityLevel GetMediaPlayerCapabilityLevel()
        {
            return CapabilityPriorityLevel.High;
        }

        public new void DisplayImage(String url, String mimeType, String title, String description, String iconSrc,
            ResponseListener listener)
        {
            var requestIdNumber = GetNextId();
            var requestId = String.Format("req{0}", requestIdNumber);

            JsonObject message = null;
            try
            {
                message = new JsonObject {{"contentType", JsonValue.CreateStringValue(NamespaceKey + "mediaCommand")}};
                var mediaCommandObject = new JsonObject
                {
                    {"mimeType", JsonValue.CreateStringValue(mimeType)},
                    {"requestId", JsonValue.CreateStringValue(requestId)},
                    {"type", JsonValue.CreateStringValue("displayImage")},
                    {"mediaURL", JsonValue.CreateStringValue(url)},
                    {"iconURL", JsonValue.CreateStringValue(iconSrc)},
                    {"title", JsonValue.CreateStringValue(title)},
                    {"description", JsonValue.CreateStringValue(description)},
                };
                message.Add("mediaCommand", mediaCommandObject);
            }
            catch (Exception)
            {
                if (listener != null)
                    listener.OnError(new ServiceCommandError(0, null));
            }

            var responseListener = new ResponseListener
            (
                loadEventArg =>
                {
                    Util.PostSuccess(listener, new MediaLaunchObject(LaunchSession, GetMediaControl()));
                },
                serviceCommandError =>
                {
                    Util.PostError(listener, serviceCommandError);
                });

            var command = new ServiceCommand(Socket, null, null, responseListener);

            mActiveCommands.TryAdd(requestId, command);

            var messageResponseListener = new ResponseListener
            (
                loadEventArg =>
                {
                    
                },
                serviceCommandError =>
                {
                    Util.PostError(listener, serviceCommandError);
                });

            SendP2PMessage(message, messageResponseListener);
        }

        public new void PlayMedia(String url, String mimeType, String title, String description, String iconSrc,
            bool shouldLoop, ResponseListener listener)
        {
            var requestIdNumber = GetNextId();
            var requestId = String.Format("req{0}", requestIdNumber);

            JsonObject message = null;
            try
            {
                message = new JsonObject {{"contentType", JsonValue.CreateStringValue(NamespaceKey + "mediaCommand")}};
                var mediaCommandObject = new JsonObject
                {
                    {"mimeType", JsonValue.CreateStringValue(mimeType)},
                    {"requestId", JsonValue.CreateStringValue(requestId)},
                    {"iconURL", JsonValue.CreateStringValue(iconSrc)},
                    {"title", JsonValue.CreateStringValue(title)},
                    {"type", JsonValue.CreateStringValue("playMedia")},
                    {"description", JsonValue.CreateStringValue(description)},
                    {"mediaURL", JsonValue.CreateStringValue(url)},
                    {"shouldLoop", JsonValue.CreateBooleanValue(shouldLoop)},
                };

                message.Add("mediaCommand", mediaCommandObject);
            }
            catch (Exception)
            {
                if (listener != null)
                    listener.OnError(new ServiceCommandError(0, null));
            }

            var responseListener = new ResponseListener
            (
                loadEventArg => Util.PostSuccess(listener, new MediaLaunchObject(LaunchSession, GetMediaControl())),
                serviceCommandError => Util.PostError(listener, serviceCommandError)
            );

            var command = new ServiceCommand(Socket, null, null, responseListener);

            mActiveCommands.TryAdd(requestId, command);

            var messageResponseListener = new ResponseListener
            (
                loadEventArg => { },
                serviceCommandError => Util.PostError(listener, serviceCommandError)
            );

            SendP2PMessage(message, messageResponseListener);
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

            public void OnBeforeRegister(PairingType pairingType)
            {
            }

            public void OnRegistrationFailed(ServiceCommandError error)
            {
            }

            public bool OnReceiveMessage(JsonObject payload)
            {
                try
                {

                var type = payload.GetNamedString("type");

                if (!"p2p".Equals(type)) return true;

                string fromAppId = payload.GetNamedString("from");

                if (!fromAppId.Equals(webOsWebAppSession.GetFullAppId()))
                    return false;

                Object message = payload.GetNamedObject("payload");

                if (message != null)
                {
                    var messageJson = (JsonObject) message;

                    var contentType = messageJson.GetNamedString("contentType");
                    var contentTypeIndex = contentType.IndexOf("connectsdk.", StringComparison.OrdinalIgnoreCase);

                    if (contentTypeIndex >= 0)
                    {
                        //String payloadKey = contentType.Split("connectsdk.",)[1];

                        var payloadKey = contentType.Split('.')[1];
                        if (string.IsNullOrEmpty(payloadKey))
                            return false;

                        var messagePayload = messageJson.GetNamedObject(payloadKey);

                        if (messagePayload == null)
                            return false;

                        if (payloadKey.Equals("mediaEvent", StringComparison.OrdinalIgnoreCase))
                            webOsWebAppSession.HandleMediaEvent(messagePayload);
                        else if (payloadKey.Equals("mediaCommandResponse", StringComparison.OrdinalIgnoreCase))
                            webOsWebAppSession.HandleMediaCommandResponse(messagePayload);
                    }
                    else
                    {
                        webOsWebAppSession.HandleMessage(messageJson);
                    }
                }
                }
                catch (Exception)
                {

                    throw;
                }

                return false;
            }
        }
    }
}