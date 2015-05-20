#region Copyright Notice
/*
 * ConnectSdk.Windows
 * WebOsTvService.cs
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
using System.Linq;
using Windows.Data.Json;
using Windows.Foundation;
using ConnectSdk.Windows.Core;
using ConnectSdk.Windows.Device;
using ConnectSdk.Windows.Discovery;
using ConnectSdk.Windows.Etc.Helper;
using ConnectSdk.Windows.Service.Capability;
using ConnectSdk.Windows.Service.Capability.Listeners;
using ConnectSdk.Windows.Service.Command;
using ConnectSdk.Windows.Service.Config;
using ConnectSdk.Windows.Service.Sessions;
using ConnectSdk.Windows.Service.WebOs;

namespace ConnectSdk.Windows.Service
{
    public class WebOstvService : DeviceService, IVolumeControl, ILauncher, IMediaControl, IMediaPlayer, ITvControl,
        IToastControl, IExternalInputControl, IMouseControl, ITextInputControl, IPowerControl, IKeyControl,
        IWebAppLauncher
    {
        public static String[] WebOstvServiceOpenPermissionList =
        {
            "LAUNCH",
            "LAUNCH_WEBAPP",
            "APP_TO_APP",
            "CONTROL_AUDIO",
            "CONTROL_INPUT_MEDIA_PLAYBACK"
        };

        public static String[] WebOstvServiceProtectedPermissionList =
        {
            "CONTROL_POWER",
            "READ_INSTALLED_APPS",
            "CONTROL_DISPLAY",
            "CONTROL_INPUT_JOYSTICK",
            "CONTROL_INPUT_MEDIA_RECORDING",
            "CONTROL_INPUT_TV",
            "READ_INPUT_DEVICE_LIST",
            "READ_NETWORK_STATE",
            "READ_TV_CHANNEL_LIST",
            "WRITE_NOTIFICATION_TOAST"
        };

        public static String[] WebOstvServicePersonalActivityPermissionList =
        {
            "CONTROL_INPUT_TEXT",
            "CONTROL_MOUSE_AND_KEYBOARD",
            "READ_CURRENT_CHANNEL",
            "READ_RUNNING_APPS"
        };


        public static String Id = "webOS TV";

        private const string ForegroundApp = "ssap://com.webos.applicationManager/getForegroundAppInfo";
        private const string AppStatus = "ssap://com.webos.service.appstatus/getAppStatus";
        private const string VolumeUrl = "ssap://audio/getVolume";
        private const string MuteUrl = "ssap://audio/getMute";
        private const string CloseMediaUri = "ssap://media.viewer/close";
        private const string Channel = "ssap://tv/getCurrentChannel";
        private const string ChannelList = "ssap://tv/getChannelList";
        private const string Program = "ssap://tv/getChannelProgramInfo";

        public Dictionary<string, string> AppToAppIdMappings { get; set; }
        public Dictionary<string, WebOsWebAppSession> WebAppSessions { get; set; }

        public WebOstvServiceSocketClient Socket
        {
            get { return socket; }
            set { socket = value; }
        }

        private WebOstvServiceSocketClient socket;
        private WebOstvMouseSocketConnection mouseSocket;

        private List<String> permissions;
        private WebOsTvKeyboardInput keyboardInput;

        public WebOstvService(ServiceDescription serviceDescription, ServiceConfig serviceConfig)
            : base(serviceDescription, serviceConfig)
        {
            AppToAppIdMappings = new Dictionary<String, String>();
            WebAppSessions = new Dictionary<String, WebOsWebAppSession>();
        }

        public WebOstvService(ServiceConfig serviceConfig)
            : base(serviceConfig)
        {
        }

        public override void SetServiceDescription(ServiceDescription serviceDescription)
        {
            base.SetServiceDescription(serviceDescription);

            if (ServiceDescription.Version != null || ServiceDescription.ResponseHeaders == null) return;

            var serverInfo = serviceDescription.ResponseHeaders["Server"][0];
            var systemOs = serverInfo.Split(' ')[0];
            var versionComponents = systemOs.Split('/');
            var systemVersion = versionComponents[versionComponents.Length - 1];

            ServiceDescription.Version = systemVersion;

            UpdateCapabilities();
        }

        public new static DiscoveryFilter DiscoveryFilter()
        {
            return new DiscoveryFilter(Id, "urn:lge-com:service:webos-second-screen:1");
        }

        public override void SendCommand(ServiceCommand command)
        {
            if (Socket != null)
                Socket.SendCommand(command);
        }

        public override bool IsConnected()
        {
            if (DiscoveryManager.GetInstance().PairingLevel == DiscoveryManager.PairingLevelEnum.On)
            {
                return Socket != null && Socket.IsConnected() &&
                       (((WebOsTvServiceConfig)serviceConfig).ClientKey != null);
            }
            return Socket != null && Socket.IsConnected();
        }

        public override void Connect()
        {
            if (Socket == null)
            {
                Socket = new WebOstvServiceSocketClient(this, WebOstvServiceSocketClient.GetUri(this))
                {
                    Listener = new WebOstvServiceSocketClientListener(this, Listener)
                };
            }

            if (!IsConnected())
                Socket.Connect();
        }

        public override void Disconnect()
        {
            Logger.Current.AddMessage("attempting to disconnect from " + ServiceDescription.IpAddress);
            //Log.d("Connect SDK", "attempting to disconnect to " + serviceDescription.getIpAddress());

            if (Listener != null)
                Listener.OnDisconnect(this, null);

            if (Socket != null)
            {
                Socket.Listener = null;
                Socket.Disconnect();
                Socket = null;
            }

            if (AppToAppIdMappings != null)
                AppToAppIdMappings.Clear();

            if (WebAppSessions != null)
            {
                foreach (var pair in WebAppSessions)
                {
                    pair.Value.DisconnectFromWebApp();
                }

                WebAppSessions.Clear();
            }
        }



        #region Volume Control

        public IVolumeControl GetVolumeControl()
        {
            return this;
        }

        public CapabilityPriorityLevel GetVolumeControlCapabilityLevel()
        {
            return CapabilityPriorityLevel.High;
        }

        public void VolumeUp()
        {
            VolumeUp(null);
        }

        public void VolumeUp(ResponseListener listener)
        {
            const string uri = "ssap://audio/volumeUp";
            var request = new ServiceCommand(this, uri, null, listener);

            request.Send();
        }

        public void VolumeDown()
        {
            VolumeDown(null);
        }

        public void VolumeDown(ResponseListener listener)
        {
            const string uri = "ssap://audio/volumeDown";
            var request = new ServiceCommand(this, uri, null, listener);

            request.Send();
        }

        public void SetVolume(float volume)
        {
            SetVolume(volume, null);
        }

        public void SetVolume(float volume, ResponseListener listener)
        {
            const string uri = "ssap://audio/setVolume";
            var payload = new JsonObject();
            var intVolume = (int)Math.Round(volume * 100.0f);

            try
            {
                payload.Add("volume", JsonValue.CreateNumberValue(intVolume));
            }
            // ReSharper disable once EmptyGeneralCatchClause
            catch (Exception)
            {
                //e.printStackTrace();
            }

            var request = new ServiceCommand(this, uri, payload, listener);
            request.Send();
        }

        public void GetVolume(ResponseListener listener)
        {
            GetVolume(false, listener);
        }


        private ServiceCommand GetVolume(bool isSubscription, ResponseListener listener)
        {
            var getVolumeResponseListener = new ResponseListener
                (
                loadEventArg =>
                {
                    try
                    {
                        var jsonObj = (JsonObject)loadEventArg;
                        var iVolume = (int)jsonObj.GetNamedNumber("volume");
                        var fVolume = (float)(iVolume / 100.0);

                        Util.PostSuccess(listener, fVolume);
                    }
                    // ReSharper disable once EmptyGeneralCatchClause
                    catch (Exception)
                    {

                    }
                },
                serviceCommandError => Util.PostError(listener, serviceCommandError));

            var request = isSubscription
                ? new UrlServiceSubscription(this, VolumeUrl, null, true, getVolumeResponseListener)
                : new ServiceCommand(this, VolumeUrl, null, getVolumeResponseListener);

            request.Send();
            return request;
        }

        public void SetMute(bool isMute, ResponseListener listener)
        {
            const string uri = "ssap://audio/setMute";
            var payload = new JsonObject();
            try
            {
                payload.Add("mute", JsonValue.CreateBooleanValue(isMute));
            }
            // ReSharper disable once EmptyGeneralCatchClause
            catch (Exception)
            {
            }

            var request = new ServiceCommand(this, uri, payload, listener);
            request.Send();
        }

        public void GetMute(ResponseListener listener)
        {
            GetMuteStatus(false, listener);
        }

        private ServiceCommand GetMuteStatus(bool isSubscription, ResponseListener listener)
        {

            var getMuteResponseListener = new ResponseListener
                (
                loadEventArg =>
                {
                    try
                    {
                        var jsonObj = (JsonObject)loadEventArg;
                        var isMute = jsonObj.GetNamedBoolean("mute");
                        Util.PostSuccess(listener, isMute);
                    }
                    // ReSharper disable once EmptyGeneralCatchClause
                    catch (Exception)
                    {

                    }
                },
                serviceCommandError => Util.PostError(listener, serviceCommandError)
                );


            var request = isSubscription
                ? new UrlServiceSubscription(this, MuteUrl, null, true, getMuteResponseListener)
                : new ServiceCommand(this, MuteUrl, null, getMuteResponseListener);

            request.Send();
            return request;
        }

        public IServiceSubscription SubscribeVolume(ResponseListener listener)
        {
            return (IServiceSubscription)GetVolume(true, listener);
        }

        public IServiceSubscription SubscribeMute(ResponseListener listener)
        {
            return (IServiceSubscription) GetMuteStatus(true, listener);
        }

        #endregion

        #region Launcher

        public ILauncher GetLauncher()
        {
            return this;
        }

        public CapabilityPriorityLevel GetLauncherCapabilityLevel()
        {
            return CapabilityPriorityLevel.High;
        }

        public void LaunchAppWithInfo(AppInfo appInfo, ResponseListener listener)
        {
            LaunchAppWithInfo(appInfo, null, listener);
        }

        public void LaunchAppWithInfo(AppInfo appInfo, object ps, ResponseListener listener)
        {
            const string uri = "ssap://system.launcher/launch";
            var payload = new JsonObject();

            var appId = appInfo.Id;
            string contentId = null;

            if (ps != null)
            {
                try
                {
                    contentId = ((JsonObject)ps).GetNamedString("contentId");
                }
                // ReSharper disable once EmptyGeneralCatchClause
                catch
                {

                }
            }

            try
            {
                payload.Add("id", JsonValue.CreateStringValue(appId));

                if (contentId != null)
                    payload.Add("contentId", JsonValue.CreateStringValue(contentId));

                if (ps != null)
                    payload.Add("params", (JsonObject)ps);
            }
            // ReSharper disable once EmptyGeneralCatchClause
            catch
            {

            }

            var responseListener = new ResponseListener
                (
                loadEventArg =>
                {
                    var obj = (JsonObject)(((LoadEventArgs)loadEventArg).Load.GetPayload());
                    var launchSession = new LaunchSession
                    {
                        Service = this,
                        AppId = appId,
                        SessionId = obj.GetNamedString("sessionId"),
                        SessionType = LaunchSessionType.App
                    };

                    Util.PostSuccess(listener, launchSession);
                },
                serviceCommandError => Util.PostError(listener, serviceCommandError)
                );
            var request = new ServiceCommand(this, uri, payload, responseListener);
            request.Send();
        }

        public void LaunchApp(string appId, ResponseListener listener)
        {
            var appInfo = new AppInfo(appId) { Id = appId };

            LaunchAppWithInfo(appInfo, listener);
        }

        public void CloseApp(LaunchSession launchSession, ResponseListener listener)
        {
            const string uri = "ssap://system.launcher/close";
            var appId = launchSession.AppId;
            var sessionId = launchSession.SessionId;
            var payload = new JsonObject();

            try
            {
                payload.Add("id", JsonValue.CreateStringValue(appId));
                payload.Add("sessionId", JsonValue.CreateStringValue(sessionId));
            }
            catch
            {
            }

            var request = new ServiceCommand(this, uri, payload, listener);
            request.Send();
        }

        public void GetAppList(ResponseListener listener)
        {
            const string uri = "ssap://com.webos.applicationManager/listApps";

            var responseListener = new ResponseListener
                (
                loadEventArg =>
                {
                    try
                    {
                        var jsonObj = (JsonObject)(((LoadEventArgs)loadEventArg).Load.GetPayload());

                        var apps = jsonObj.GetNamedArray("apps");
                        var appList = (from t in apps
                            select t.GetObject()
                            into appObj
                            where appObj != null
                            select new AppInfo(appObj.GetNamedString("id"))
                            {
                                Name = appObj.GetNamedString("title"), Url = appObj.GetNamedString("icon"), RawData = appObj
                            }).ToList();

                        Util.PostSuccess(listener, appList);
                    }
                    catch
                    {
                    }
                },
                serviceCommandError => Util.PostError(listener, serviceCommandError)
                );
            var request = new ServiceCommand(this, uri, null, responseListener);
            request.Send();
        }

        public void GetRunningApp(ResponseListener listener)
        {
            GetRunningApp(false, listener);
        }

        public ServiceCommand GetRunningApp(bool isSubscription, ResponseListener listener)
        {
            var responseListener = new ResponseListener
                (
                loadEventArg =>
                {
                    var jsonObj = (JsonObject)(((LoadEventArgs)loadEventArg).Load.GetPayload());
                    var appInfo = new AppInfo(jsonObj.GetNamedString("appId"))
                    {
                        Name = jsonObj.GetNamedString("title", ""),
                        RawData = jsonObj
                    };
                    Util.PostSuccess(listener, appInfo);
                },
                serviceCommandError => Util.PostError(listener, serviceCommandError)
                );

            var request = isSubscription 
                ? new UrlServiceSubscription(this, ForegroundApp, null, true, responseListener) 
                : new ServiceCommand(this, ForegroundApp, null, responseListener);

            request.Send();

            return request;
        }

        public IServiceSubscription SubscribeRunningApp(ResponseListener listener)
        {
            return (UrlServiceSubscription)GetRunningApp(true, listener);
        }

        public void GetAppState(LaunchSession launchSession, ResponseListener listener)
        {
            GetAppState(false, launchSession, listener);
        }

        public ServiceCommand GetAppState(bool subscription, LaunchSession launchSession, ResponseListener listener)
        {
            var payload = new JsonObject();

            try
            {
                payload.Add("id", JsonValue.CreateStringValue(launchSession.AppId));
                payload.Add("sessionId", JsonValue.CreateStringValue(launchSession.SessionId));
            }
            catch
            {
            }

            var responseListener = new ResponseListener
            (
                loadEventArg =>
                {
                    var jsonObj = (JsonObject)(((LoadEventArgs)loadEventArg).Load.GetPayload());

                    try
                    {
                        Util.PostSuccess(listener,
                            new AppState(jsonObj.GetNamedBoolean("running"), jsonObj.GetNamedBoolean("visible")));
                    }
                    catch
                    {
                        Util.PostError(listener, new ServiceCommandError(0, "Malformed JSONObject"));
                    }
                },
                serviceCommandError => Util.PostError(listener, serviceCommandError)
            );

            var request = subscription 
                ? new UrlServiceSubscription(this, AppStatus, payload, true, responseListener) 
                : new ServiceCommand(this, AppStatus, payload, responseListener);

            request.Send();

            return request;
        }

        public IServiceSubscription SubscribeAppState(LaunchSession launchSession, ResponseListener listener)
        {
            return (UrlServiceSubscription)GetAppState(true, launchSession, listener);
        }

        public void LaunchBrowser(string url, ResponseListener listener)
        {
            const string uri = "ssap://system.launcher/launch";
            var payload = new JsonObject();

            var responseListener = new ResponseListener
                (
                loadEventArg =>
                {
                    var obj = (JsonObject)loadEventArg;
                    var launchSession = new LaunchSession
                    {
                        Service = this,
                        AppId = obj.GetNamedString("id"),
                        SessionId = obj.GetNamedString("sessionId"),
                        SessionType = LaunchSessionType.App,
                        RawData = obj
                    };

                    Util.PostSuccess(listener, launchSession);
                },
                serviceCommandError => Util.PostError(listener, serviceCommandError)
                );

            try
            {
                payload.Add("target", JsonValue.CreateStringValue(url));
            }
            // ReSharper disable once EmptyGeneralCatchClause
            catch
            {
            }

            var request = new ServiceCommand(this, uri, payload, responseListener);
            request.Send();
        }

        public void LaunchYouTube(string contentId, ResponseListener listener)
        {
            LaunchYouTube(contentId, (float)0.0, listener);
        }

        public void LaunchYouTube(string contentId, float startTime, ResponseListener listener)
        {
            var ps = new JsonObject();
            if (!string.IsNullOrEmpty(contentId))
            {
                if (startTime < 0.0)
                {
                    Util.PostError(listener, new ServiceCommandError(0, "Start time may not be negative"));
                    return;
                }

                try
                {
                    ps.Add("contentId",
                        JsonValue.CreateStringValue(String.Format("{0}&pairingCode={1}&t={2}", contentId,
                            Guid.NewGuid(), startTime)));
                }
                catch
                {
                }
            }

            var appInfo = new AppInfo("youtube.leanback.v4")
            {
                Name = "YouTube"
            };

            LaunchAppWithInfo(appInfo, ps, listener);
        }

        public void LaunchNetflix(string contentId, ResponseListener listener)
        {
            var ps = new JsonObject();
            var netflixContentId = "m=http%3A%2F%2Fapi.netflix.com%2Fcatalog%2Ftitles%2Fmovies%2F" + contentId + "&source_type=4";
            if (!string.IsNullOrEmpty(contentId))
            {
                try
                {
                    ps.Add("contentId", JsonValue.CreateStringValue(netflixContentId));
                }
                catch
                {
                }
            }

            var appInfo = new AppInfo("netflix")
            {
                Name = "Netflix"
            };

            LaunchAppWithInfo(appInfo, ps, listener);
        }

        public void LaunchHulu(string contentId, ResponseListener listener)
        {
            var ps = new JsonObject();
            if (!string.IsNullOrEmpty(contentId))
            {
                try
                {
                    ps.Add("contentId", JsonValue.CreateStringValue(contentId));
                }
                catch
                {
                }
            }

            var appInfo = new AppInfo("hulu")
            {
                Name = "Hulu"
            };

            LaunchAppWithInfo(appInfo, ps, listener);
        }

        public void LaunchAppStore(string appId, ResponseListener listener)
        {
            var appInfo = new AppInfo("com.webos.app.discovery") { Name = "LG Store" };

            var ps = new JsonObject();

            if (!string.IsNullOrEmpty(appId))
            {
                var query = string.Format("category/GAME_APPS/{0}", appId);
                try
                {
                    ps.Add("query", JsonValue.CreateStringValue(query));
                }
                catch
                {

                }
            }

            LaunchAppWithInfo(appInfo, ps, listener);
        }

        #endregion

        #region Media Control

        public IMediaControl GetMediaControl()
        {
            return this;
        }

        public CapabilityPriorityLevel GetMediaControlCapabilityLevel()
        {
            return CapabilityPriorityLevel.High;
        }

        public void Play(ResponseListener listener)
        {
            const string uri = "ssap://media.controls/play";
            var request = new ServiceCommand(this, uri, null, listener);

            request.Send();
        }

        public void Pause(ResponseListener listener)
        {
            const string uri = "ssap://media.controls/pause";
            var request = new ServiceCommand(this, uri, null, listener);

            request.Send();
        }

        public void Stop(ResponseListener listener)
        {
            const string uri = "ssap://media.controls/stop";
            var request = new ServiceCommand(this, uri, null, listener);

            request.Send();
        }

        public void Rewind(ResponseListener listener)
        {
            const string uri = "ssap://media.controls/rewind";
            var request = new ServiceCommand(this, uri, null, listener);

            request.Send();
        }

        public void FastForward(ResponseListener listener)
        {
            const string uri = "ssap://media.controls/fastForward";
            var request = new ServiceCommand(this, uri, null, listener);

            request.Send();
        }

        public void Seek(long position, ResponseListener listener)
        {
            Util.PostError(listener, ServiceCommandError.NotSupported());
        }

        public void GetDuration(ResponseListener listener)
        {
            Util.PostError(listener, ServiceCommandError.NotSupported());
        }

        public void GetPosition(ResponseListener listener)
        {
            Util.PostError(listener, ServiceCommandError.NotSupported());
        }

        public void GetPlayState(ResponseListener listener)
        {
            Util.PostError(listener, ServiceCommandError.NotSupported());
        }

        public IServiceSubscription SubscribePlayState(ResponseListener listener)
        {
            Util.PostError(listener, ServiceCommandError.NotSupported());
            return null;
        }

        public void Next(ResponseListener listener)
        {
            Util.PostError(listener, ServiceCommandError.NotSupported());
        }

        public void Previous(ResponseListener listener)
        {
            Util.PostError(listener, ServiceCommandError.NotSupported());
        }

        #endregion

        #region Media Player

        private DeviceService GetDlnaService()
        {
            ConcurrentDictionary<string, ConnectableDevice> allDevices = DiscoveryManager.GetInstance().GetAllDevices();
            ConnectableDevice device = null;
            DeviceService service = null;

            if (allDevices != null && allDevices.Count > 0)
                device = allDevices[ServiceDescription.IpAddress];

            if (device != null)
                service = device.GetServiceByName("DLNA");

            return service;
        }


        public IMediaPlayer GetMediaPlayer()
        {
            return null;
        }

        public CapabilityPriorityLevel GetMediaPlayerCapabilityLevel()
        {
            return CapabilityPriorityLevel.High;
        }

        public void DisplayImage(string url, string mimeType, string title, string description, string iconSrc,
            ResponseListener listener)
        {
            if (ServiceDescription.Version != null && ServiceDescription.Version.Equals("4.0.0"))
            {
                var dlnaService = GetDlnaService();

                if (dlnaService != null)
                {
                    var mediaPlayer = dlnaService.GetApi<IMediaPlayer>();

                    if (mediaPlayer != null)
                    {
                        mediaPlayer.DisplayImage(url, mimeType, title, description, iconSrc, listener);
                        return;
                    }
                }

                JsonObject ps = null;
                try
                {

                    ps = new JsonObject
                    {
                        {"target", JsonValue.CreateStringValue(url)},
                        {"title", JsonValue.CreateStringValue(title ?? string.Empty)},
                        {"description", JsonValue.CreateStringValue(description ?? string.Empty)},
                        {"mimeType", JsonValue.CreateStringValue(mimeType ?? string.Empty)},
                        {"iconSrc", JsonValue.CreateStringValue(iconSrc ?? string.Empty)},
                    };
                }
                catch (Exception ex)
                {
                    Util.PostError(listener, new ServiceCommandError(-1, ex.Message));
                }

                if (ps != null)
                    DisplayMedia(ps, listener);
            }
            else
            {
                const string webAppId = "MediaPlayer";
                var webAppLaunchListener = new ResponseListener
                    (
                        loadEventArg =>
                        {
                            var loadEventArgs = loadEventArg as LoadEventArgs;
                            if (loadEventArgs != null)
                            {
                                var webAppSession = (loadEventArgs.Load.GetPayload()) as WebOsWebAppSession;
                                if (webAppSession != null)
                                    webAppSession.DisplayImage(url, mimeType, title, description, iconSrc, listener);
                            }
                        },
                        serviceCommandError => listener.OnError(serviceCommandError)
                    );

                var webappResponseListener = new ResponseListener
                    (
                        loadEventArg =>
                        {
                            var loadEventArgs = loadEventArg as LoadEventArgs;
                            if (loadEventArgs != null)
                            {
                                var webAppSession = (loadEventArgs.Load.GetPayload()) as WebOsWebAppSession;
                                if (webAppSession != null)
                                    webAppSession.DisplayImage(url, mimeType, title, description, iconSrc, listener);
                            }
                        },
                        serviceCommandError => GetWebAppLauncher().LaunchWebApp(webAppId, webAppLaunchListener));

                GetWebAppLauncher().JoinWebApp(webAppId, webappResponseListener);
            }
        }

        private void DisplayMedia(JsonObject ps, ResponseListener listener)
        {
            const string uri = "ssap://media.viewer/open";

            var responseListener = new ResponseListener
                (
                loadEventArg =>
                {
                    var obj = (JsonObject)(((LoadEventArgs)loadEventArg).Load.GetPayload());

                    LaunchSession launchSession = LaunchSession.LaunchSessionForAppId(obj.GetNamedString("id"));
                    launchSession.Service = this;
                    launchSession.SessionId = obj.GetNamedString("sessionId");
                    launchSession.SessionType = LaunchSessionType.Media;

                    Util.PostSuccess(listener, new MediaLaunchObject(launchSession, this));
                },
                serviceCommandError => Util.PostError(listener, serviceCommandError)
            );

            var request = new ServiceCommand(this, uri, ps, responseListener);
            request.Send();
        }

        public void PlayMedia(string url, string mimeType, string title, string description, string iconSrc,
            bool shouldLoop, ResponseListener listener)
        {
            if (ServiceDescription.Version != null && ServiceDescription.Version.Equals("4.0.0"))
            {
                var dlnaService = GetDlnaService();

                if (dlnaService != null)
                {
                    var mediaPlayer = dlnaService.GetApi<IMediaPlayer>();

                    if (mediaPlayer != null)
                    {
                        mediaPlayer.PlayMedia(url, mimeType, title, description, iconSrc, shouldLoop, listener);
                        return;
                    }
                }

                JsonObject ps = null;
                try
                {
                    ps = new JsonObject
                    {
                        {"target", JsonValue.CreateStringValue(url)},
                        {"title", JsonValue.CreateStringValue(title ?? string.Empty)},
                        {"description", JsonValue.CreateStringValue(description ?? string.Empty)},
                        {"mimeType", JsonValue.CreateStringValue(mimeType ?? string.Empty)},
                        {"iconSrc", JsonValue.CreateStringValue(iconSrc ?? string.Empty)},
                        {"loop", JsonValue.CreateBooleanValue(shouldLoop)}
                    };
                }
                catch (Exception ex)
                {
                    Util.PostError(listener, new ServiceCommandError(-1, ex.Message));
                }

                if (ps != null)
                    DisplayMedia(ps, listener);
            }
            else
            {
                const string webAppId = "MediaPlayer";
                var webAppLaunchListener = new ResponseListener
                    (
                        loadEventArg =>
                        {
                            var loadEventArgs = loadEventArg as LoadEventArgs;
                            if (loadEventArgs != null)
                            {
                                var webAppSession = (loadEventArgs.Load.GetPayload()) as WebOsWebAppSession;
                                if (webAppSession != null)
                                    webAppSession.PlayMedia(url, mimeType, title, description, iconSrc, shouldLoop, listener);
                            }
                        },
                        // ReSharper disable once ConvertClosureToMethodGroup
                        serviceCommandError =>
                        {
                            listener.OnError(serviceCommandError);
                        }
                    );

                var webappResponseListener = new ResponseListener
                    (
                        loadEventArg =>
                        {
                            var loadEventArgs = loadEventArg as LoadEventArgs;
                            if (loadEventArgs != null)
                            {
                                var webAppSession = (loadEventArgs.Load.GetPayload()) as WebOsWebAppSession;
                                if (webAppSession != null)
                                    webAppSession.PlayMedia(url, mimeType, title, description, iconSrc, shouldLoop, listener);
                            }
                        },
                        serviceCommandError => GetWebAppLauncher().LaunchWebApp(webAppId, webAppLaunchListener)
                    );

                GetWebAppLauncher().JoinWebApp(webAppId, webappResponseListener);
            }
        }

        public void CloseMedia(LaunchSession launchSession, ResponseListener listener)
        {
            var payload = new JsonObject();

            try
            {
                if (!string.IsNullOrEmpty(launchSession.AppId))
                    payload.Add("id", JsonValue.CreateStringValue(launchSession.AppId));

                if (!string.IsNullOrEmpty(launchSession.SessionId))
                    payload.Add("sessionId", JsonValue.CreateStringValue(launchSession.SessionId));
            }
            catch
            {

            }

            var request = new ServiceCommand(launchSession.Service, CloseMediaUri, payload, listener);
            request.Send();
        }

        #endregion

        #region TV Control

        public ITvControl GetTvControl()
        {
            return this;
        }

        public CapabilityPriorityLevel GetTvControlCapabilityLevel()
        {
            return CapabilityPriorityLevel.High;
        }

        public void ChannelUp(ResponseListener listener)
        {
            const string uri = "ssap://tv/channelUp";
            var request = new ServiceCommand(this, uri, null, listener);
            request.Send();
        }

        public void ChannelDown(ResponseListener listener)
        {
            const string uri = "ssap://tv/channelDown";
            var request = new ServiceCommand(this, uri, null, listener);
            request.Send();
        }

        public void SetChannel(ChannelInfo channelInfo, ResponseListener listener)
        {
            const string uri = "ssap://tv/openChannel";
            var payload = new JsonObject();

            try
            {
                payload.Add("channelNumber", JsonValue.CreateStringValue(channelInfo.Number));
            }
            catch (Exception)
            {

            }

            var request = new ServiceCommand(this, uri, payload, listener);
            request.Send();
        }

        public void GetCurrentChannel(ResponseListener listener)
        {
            GetCurrentChannel(false, listener);
        }

        public ServiceCommand GetCurrentChannel(bool isSubscription, ResponseListener listener)
        {
            var responseListener = new ResponseListener
            (
                loadEventArg =>
                {
                    var jsonObj = (JsonObject)(((LoadEventArgs)loadEventArg).Load.GetPayload());
                    var channel = ParseRawChannelData(jsonObj);
                    Util.PostSuccess(listener, channel);
                },
                serviceCommandError => Util.PostError(listener, serviceCommandError)
            );

            var request = isSubscription 
                ? new UrlServiceSubscription(this, Channel, null, responseListener) 
                : new ServiceCommand(this, Channel, null, responseListener);
            request.Send();

            return request;
        }

        private ChannelInfo ParseRawChannelData(JsonObject channelRawData)
        {
            string channelName = null;
            string channelId = null;

            var channelInfo = new ChannelInfo();
            channelInfo.RawData = channelRawData;

            try
            {
                if (!channelRawData.ContainsKey("channelName"))
                    channelName = channelRawData.GetNamedString("channelName","");

                if (!channelRawData.ContainsKey("channelId"))
                    channelId = channelRawData.GetNamedString("channelId", "");

                string channelNumber = channelRawData.GetNamedString("channelNumber", "");

                int majorNumber;
                if (!channelRawData.ContainsKey("majorNumber"))
                    majorNumber = (int)channelRawData.GetNamedNumber("majorNumber");
                else
                    majorNumber = ParseMajorNumber(channelNumber);

                int minorNumber;
                if (!channelRawData.ContainsKey("minorNumber"))
                    minorNumber = (int)channelRawData.GetNamedNumber("minorNumber");
                else
                    minorNumber = ParseMinorNumber(channelNumber);

                channelInfo.Name = channelName;
                channelInfo.Id = channelId;
                channelInfo.Number = channelNumber;
                channelInfo.MajorNumber = majorNumber;
                channelInfo.MinorNumber = minorNumber;

            }
            catch (Exception e)
            {
                throw e;
            }

            return channelInfo;
        }

        private int ParseMinorNumber(string channelNumber)
        {
            if (channelNumber != null)
            {
                var tokens = channelNumber.Split('-');
                return int.Parse(tokens[tokens.Length - 1]);
            }
            return 0;
        }

        private int ParseMajorNumber(string channelNumber)
        {
            if (channelNumber != null)
            {
                var tokens = channelNumber.Split('-');
                return int.Parse(tokens[0]);
            }
            return 0;
        }

        public IServiceSubscription SubscribeCurrentChannel(ResponseListener listener)
        {
            return (IServiceSubscription)GetCurrentChannel(true, listener);
        }

        public void GetChannelList(ResponseListener listener)
        {
            GetChannelList(false, listener);
        }

        private void GetChannelList(bool isSubscription, ResponseListener listener)
        {
            var responseListener = new ResponseListener
            (
                loadEventArg =>
                {
                    var jsonObj = (JsonObject)(((LoadEventArgs)loadEventArg).Load.GetPayload());

                    var channels = jsonObj.GetNamedArray("channelList");
                    var channelList = new List<ChannelInfo>();

                    for (var i = 0; i < channels.Count; i++)
                    {
                        var chObj = channels[i].GetObject();
                        if (chObj == null) continue;
                        var channelInfo = ParseRawChannelData(chObj);
                        channelList.Add(channelInfo);
                    }
                    Util.PostSuccess(listener, channelList);
                },
                serviceCommandError => Util.PostError(listener, serviceCommandError)
            );

            var request = isSubscription 
                ? new UrlServiceSubscription(this, ChannelList, null, responseListener) 
                : new ServiceCommand(this, ChannelList, null, responseListener);
            request.Send();

            //return request;

        }

        public void GetProgramInfo(ResponseListener listener)
        {
            Util.PostError(listener, ServiceCommandError.NotSupported());
        }

        public IServiceSubscription SubscribeProgramInfo(ResponseListener listener)
        {
            Util.PostError(listener, ServiceCommandError.NotSupported());

            return new NotSupportedServiceSubscription();
        }

        public void GetProgramList(ResponseListener listener)
        {
            GetProgramList(false, listener);
        }

        private ServiceCommand GetProgramList(bool isSubscription, ResponseListener listener)
        {
            var responseListener = new ResponseListener
                (
                loadEventArg =>
                {
                    var jsonObj = (JsonObject) (((LoadEventArgs) loadEventArg).Load.GetPayload());
                    var jsonChannel = jsonObj.GetNamedObject("channel");
                    var channelInfo = ParseRawChannelData(jsonObj);
                    var programList = jsonObj.GetNamedArray("programList");
                    Util.PostSuccess(listener, new ProgramList(channelInfo, programList));
                },
                serviceCommandError => Util.PostError(listener, serviceCommandError)
                );


            var request = isSubscription 
                ? new UrlServiceSubscription(this, Program, null, true, responseListener) 
                : new ServiceCommand(this, Program, null, responseListener);

            request.Send();

            return request;
        }

        public IServiceSubscription SubscribeProgramList(ResponseListener listener)
        {
            return (IServiceSubscription)GetProgramList(true, listener);
        }

        public void Get3DEnabled(ResponseListener listener)
        {
            Get3DEnabled(false, listener);
        }

        private ServiceCommand Get3DEnabled(bool isSubscription, ResponseListener listener)
        {
            const string uri = "ssap://com.webos.service.tv.display/get3DStatus";
            var responseListener = new ResponseListener
            (
                loadEventArg =>
                {
                    var jsonObj = (JsonObject)(((LoadEventArgs)loadEventArg).Load.GetPayload());
                    var statusobj = jsonObj.GetNamedObject("status3D");
                    var status = statusobj.GetNamedBoolean("status", false);

                    Util.PostSuccess(listener, status);
                },
                serviceCommandError => Util.PostError(listener, serviceCommandError)
            );


            var request = isSubscription
                ? new UrlServiceSubscription(this, uri, null, true, responseListener)
                : new ServiceCommand(this, uri, null, responseListener);

            request.Send();

            return request;
        }

        public void Set3DEnabled(bool enabled, ResponseListener listener)
        {
            var uri = enabled ? "ssap://com.webos.service.tv.display/set3DOn" : "ssap://com.webos.service.tv.display/set3DOff";

            var request = new ServiceCommand(this, uri, null, listener);

            request.Send();
        }

        public IServiceSubscription Subscribe3DEnabled(ResponseListener listener)
        {
            return (IServiceSubscription)Get3DEnabled(true, listener);
        }

        #endregion

        #region Toast Control

        public IToastControl GetToastControl()
        {
            return this;
        }

        public CapabilityPriorityLevel GetToastControlCapabilityLevel()
        {
            return CapabilityPriorityLevel.High;
        }

        public void ShowToast(string message, ResponseListener listener)
        {
            ShowToast(message, null, null, listener);
        }

        public void ShowToast(string message, string iconData, string iconExtension, ResponseListener listener)
        {
            var payload = new JsonObject();
            try
            {
                payload.Add("message", JsonValue.CreateStringValue(message));

                if (iconData != null)
                {
                    payload.Add("iconData", JsonValue.CreateStringValue(iconData));
                    payload.Add("iconExtension", JsonValue.CreateStringValue(iconExtension));
                }
            }
            catch 
            {
            }

            SendToast(payload, listener);
        }

        public void ShowClickableToastForApp(string message, AppInfo appInfo, JsonObject ps, ResponseListener listener)
        {
            ShowClickableToastForApp(message, appInfo, ps, null, null, listener);
        }

        public void ShowClickableToastForApp(string message, AppInfo appInfo, JsonObject ps, string iconData, string iconExtension, ResponseListener listener)
        {
            var payload = new JsonObject();

            try
            {
                payload.Add("message", JsonValue.CreateStringValue(message));

                if (iconData != null)
                {
                    payload.Add("iconData", JsonValue.CreateStringValue(iconData));
                    payload.Add("iconExtension", JsonValue.CreateStringValue(iconExtension));
                }

                if (appInfo != null)
                {
                    var onClick = new JsonObject {{"appId", JsonValue.CreateStringValue(appInfo.Id)}};
                    if (ps != null)
                    {
                        onClick.Add("params", ps);
                    }
                    payload.Add("onClick", onClick);
                }
            }
            catch
            {

            }

            SendToast(payload, listener);
        }

        public void ShowClickableToastForUrl(string message, string url, ResponseListener listener)
        {
            ShowClickableToastForUrl(message, url, null, null, listener); 
        }

        public void ShowClickableToastForUrl(string message, string url, string iconData, string iconExtension,
            ResponseListener listener)
        {
            var payload = new JsonObject();

            try
            {
                payload.Add("message", JsonValue.CreateStringValue(message));

                if (iconData != null)
                {
                    payload.Add("iconData", JsonValue.CreateStringValue(iconData));
                    payload.Add("iconExtension", JsonValue.CreateStringValue(iconExtension));
                }

                if (url != null)
                {
                    var onClick = new JsonObject {{"target", JsonValue.CreateStringValue(url)}};
                    payload.Add("onClick", onClick);
                }
            }
            catch
            {

            }

            SendToast(payload, listener);
        }


        private void SendToast(JsonObject payload, ResponseListener listener)
        {
            if (!payload.ContainsKey("iconData"))
            {
                //todo: find a way to get the icon and add it to the request
                //Context context = DiscoveryManager.getInstance().getContext();

                //try
                //{
                //    Drawable drawable = context.getPackageManager().getApplicationIcon(context.getPackageName());

                //    if (drawable != null)
                //    {
                //        BitmapDrawable bitDw = ((BitmapDrawable)drawable);
                //        Bitmap bitmap = bitDw.getBitmap();

                //        ByteArrayOutputStream stream = new ByteArrayOutputStream();
                //        bitmap.compress(Bitmap.CompressFormat.PNG, 100, stream);

                //        byte[] bitmapByte = stream.toByteArray();
                //        bitmapByte = Base64.encode(bitmapByte, Base64.NO_WRAP);
                //        String bitmapData = new String(bitmapByte);

                //        payload.put("iconData", bitmapData);
                //        payload.put("iconExtension", "png");
                //    }
                //}
                //catch (NameNotFoundException e)
                //{
                //    e.printStackTrace();
                //}
                //catch (JSONException e)
                //{
                //    e.printStackTrace();
                //}
            }

            const string uri = "palm://system.notifications/createToast";
            var request = new ServiceCommand(this, uri, payload, listener);
            request.Send();
        }

        #endregion

        #region External Input Control

        public IExternalInputControl GetExternalInput()
        {
            return this;
        }

        public CapabilityPriorityLevel GetExternalInputControlPriorityLevel()
        {
            return CapabilityPriorityLevel.High;
        }

        public void LaunchInputPicker(ResponseListener listener)
        {
            var appInfo = new AppInfo("com.webos.app.inputpicker")
            {
                Name = "InputPicker"
            };
            LaunchAppWithInfo(appInfo, listener);
        }

        public void CloseInputPicker(LaunchSession launchSession, ResponseListener listener)
        {
            CloseApp(launchSession, listener);
        }

        public void SetExternalInput(ExternalInputInfo input, ResponseListener listener)
        {
            const string uri = "ssap://tv/switchInput";

            var payload = new JsonObject();

            try
            {
                if (input != null && input.Id != null)
                {
                    payload.Add("inputId", JsonValue.CreateStringValue(input.Id));
                }
                else
                {
                    Logger.Current.AddMessage("ExternalInputInfo has no id");
                }
            }
            catch (Exception)
            {
                
            }

            var request = new ServiceCommand(this, uri, payload, listener);
            request.Send();
        }

        #endregion

        #region Mouse Control

        public IMouseControl GetMouseControl()
        {
            return this;
        }

        public CapabilityPriorityLevel GetMouseControlCapabilityLevel()
        {
            return CapabilityPriorityLevel.High;
        }

        public void ConnectMouse()
        {

            var listener = new ResponseListener
                (
                loadEventArg =>
                {
                    JsonObject obj;
                    if (loadEventArg is LoadEventArgs)
                        obj = (loadEventArg as LoadEventArgs).Load.GetPayload() as JsonObject;
                    else obj = (JsonObject)loadEventArg;

                    if (obj != null)
                    {
                        var socketPath = obj.GetNamedString("socketPath");
                        mouseSocket = new WebOstvMouseSocketConnection(socketPath);
                    }
                },
                serviceCommandError =>
                {

                }
                );
            ConnectMouse(listener);
        }

        private void ConnectMouse(ResponseListener listener)
        {
            const string uri = "ssap://com.webos.service.networkinput/getPointerInputSocket";

            var request = new ServiceCommand(this, uri, null, listener);
            request.Send();
        }

        public void DisconnectMouse()
        {
            mouseSocket.Disconnect();
            mouseSocket = null;
        }

        public void Click()
        {
            if (mouseSocket != null)
            {
                mouseSocket.Click();
            }
            else ConnectMouse();
        }

        public void Move(double dx, double dy)
        {
            if (mouseSocket != null)
            {
                mouseSocket.Move(dx, dy);
            }
            else
            {

                var responseListener = new ResponseListener
                (
                    loadEventArg =>
                    {
                        JsonObject obj;
                        if (loadEventArg is LoadEventArgs)
                            obj = (loadEventArg as LoadEventArgs).Load.GetPayload() as JsonObject;
                        else obj = (JsonObject)loadEventArg;

                        if (obj != null)
                        {
                            var socketPath = obj.GetNamedString("socketPath");
                            mouseSocket = new WebOstvMouseSocketConnection(socketPath);
                        }

                        mouseSocket.Move(dx, dy);
                    },
                    serviceCommandError =>
                    {

                    }
                );

                ConnectMouse(responseListener);
            }
        }

        public void Move(Point distance)
        {
            Move(distance.X, distance.Y);
        }

        public void Scroll(double dx, double dy)
        {
            if (mouseSocket != null)
            {
                mouseSocket.Scroll(dx, dy);
            }
            else
            {
                Logger.Current.AddMessage("Mouse socket is not ready yet");
            }
        }

        public void Scroll(Point distance)
        {
            if (mouseSocket != null)
            {
                Scroll(distance.X, distance.Y);
            }
            else
            {
                Logger.Current.AddMessage("Mouse socket is not ready yet");
            }
        }

        #endregion

        #region Text Input Control

        public ITextInputControl GetTextInputControl()
        {
            return this;
        }

        public CapabilityPriorityLevel GetTextInputControlCapabilityLevel()
        {
            return CapabilityPriorityLevel.High;
        }

        public IServiceSubscription SubscribeTextInputStatus(ResponseListener listener)
        {
            keyboardInput = new WebOsTvKeyboardInput(this, true);
            return keyboardInput.Connect(listener);
        }

        public void SendText(string input)
        {
            if (keyboardInput != null)
            {
                keyboardInput.AddToQueue(input);
            }
        }

        public void SendEnter()
        {
            if (keyboardInput != null)
            {
                keyboardInput.SendEnter();
            }
        }

        public void SendDelete()
        {
            if (keyboardInput != null)
            {
                keyboardInput.SendDel();
            }
        }

        #endregion

        #region Power Control

        public IPowerControl GetPowerControl()
        {
            return this;
        }

        public CapabilityPriorityLevel GetPowerControlCapabilityLevel()
        {
            return CapabilityPriorityLevel.High;
        }

        public void PowerOff(ResponseListener listener)
        {

            var responseListener = new ResponseListener
            (
                loadEventArg =>
                {

                },
                serviceCommandError =>
                {

                }
            );

            const string uri = "ssap://system/turnOff";
            var request = new ServiceCommand(this, uri, null, responseListener);

            request.Send();
        }

        public void PowerOn(ResponseListener listener)
        {
            Util.PostError(listener, ServiceCommandError.NotSupported());
        }

        #endregion

        #region Key Control

        public IKeyControl GetKeyControl()
        {
            return this;
        }

        public CapabilityPriorityLevel GetKeyControlCapabilityLevel()
        {
            return CapabilityPriorityLevel.High;
        }

        private void SendSpecialKey(String key, ResponseListener listener)
        {
            if (mouseSocket != null)
            {
                mouseSocket.Button(key);
            }
            else
            {

                var responseListener = new ResponseListener
                (
                    loadEventArg =>
                    {
                        JsonObject obj;
                        if (loadEventArg is LoadEventArgs)
                            obj = (loadEventArg as LoadEventArgs).Load.GetPayload() as JsonObject;
                        else obj = (JsonObject)loadEventArg;

                        if (obj != null)
                        {
                            var socketPath = obj.GetNamedString("socketPath");
                            mouseSocket = new WebOstvMouseSocketConnection(socketPath);
                        }

                        mouseSocket.Button(key);
                        Util.PostSuccess(listener, null);
                    },
                    serviceCommandError =>
                    {
                        Util.PostError(listener, null);
                    }
                );

                ConnectMouse(responseListener);
            }
        }

        public void Up(ResponseListener listener)
        {
            SendSpecialKey("UP", listener);
        }

        public void Down(ResponseListener listener)
        {
            SendSpecialKey("DOWN", listener);
        }

        public void Left(ResponseListener listener)
        {
            SendSpecialKey("LEFT", listener);
        }

        public void Right(ResponseListener listener)
        {
            SendSpecialKey("RIGHT", listener);
        }

        public void Ok(ResponseListener listener)
        {
            if (mouseSocket != null)
            {
                mouseSocket.Click();
            }
            else
            {

                var responseListener = new ResponseListener
                (
                    loadEventArg =>
                    {
                        JsonObject obj;
                        if (loadEventArg is LoadEventArgs)
                            obj = (loadEventArg as LoadEventArgs).Load.GetPayload() as JsonObject;
                        else obj = (JsonObject)loadEventArg;

                        if (obj != null)
                        {
                            var socketPath = obj.GetNamedString("socketPath");
                            mouseSocket = new WebOstvMouseSocketConnection(socketPath);
                        }

                        mouseSocket.Click();
                    },
                    serviceCommandError =>
                    {

                    }
                );

                ConnectMouse(responseListener);
            }
        }

        public void Back(ResponseListener listener)
        {
            SendSpecialKey("BACK", listener);
        }

        public void Home(ResponseListener listener)
        {
            SendSpecialKey("HOME", listener);
        }

        public void SendKeyCode(KeyCode keyCode, ResponseListener pListener)
        {
            switch (keyCode)
            {
                case KeyCode.NUM_0:
                case KeyCode.NUM_1:
                case KeyCode.NUM_2:
                case KeyCode.NUM_3:
                case KeyCode.NUM_4:
                case KeyCode.NUM_5:
                case KeyCode.NUM_6:
                case KeyCode.NUM_7:
                case KeyCode.NUM_8:
                case KeyCode.NUM_9:
                    SendSpecialKey(keyCode.ToString(), pListener);
                    break;
                case KeyCode.DASH:
                    SendSpecialKey("DASH", pListener);
                    break;
                case KeyCode.ENTER:
                    SendSpecialKey("ENTER", pListener); break;
                default:
                    Util.PostError(pListener, new ServiceCommandError(0, "The keycode is not available"));
                    break;
            }
        }

        #endregion

        #region Web App Launcher

        public IWebAppLauncher GetWebAppLauncher()
        {
            return this;
        }

        public CapabilityPriorityLevel GetWebAppLauncherCapabilityLevel()
        {
            return CapabilityPriorityLevel.High;
        }

        public void LaunchWebApp(string webAppId, ResponseListener listener)
        {
            LaunchWebApp(webAppId, null, true, listener);
        }

        public void LaunchWebApp(string webAppId, bool relaunchIfRunning, ResponseListener listener)
        {
            LaunchWebApp(webAppId, null, relaunchIfRunning, listener);
        }

        public void LaunchWebApp(string webAppId, JsonObject ps, ResponseListener listener)
        {
            if (string.IsNullOrEmpty(webAppId))
            {
                Util.PostError(listener, new ServiceCommandError(-1, null));

                return;
            }

            var webAppSession = WebAppSessions.ContainsKey(webAppId) ? WebAppSessions[webAppId] : null;

            const string uri = "ssap://webapp/launchWebApp";
            var payload = new JsonObject();

            try
            {
                payload.Add("webAppId", JsonValue.CreateStringValue(webAppId));

                if (ps != null)
                    payload.Add("urlps", ps);
            }
            catch (Exception e)
            {
                throw e;
            }


            var responseListener = new ResponseListener
            (
                loadEventArg =>
                {
                    JsonObject obj;
                    if (loadEventArg is LoadEventArgs)
                        obj = (loadEventArg as LoadEventArgs).Load.GetPayload() as JsonObject;
                    else obj = (JsonObject)loadEventArg;

                    LaunchSession launchSession;

                    if (webAppSession != null)
                        launchSession = webAppSession.LaunchSession;
                    else
                    {
                        launchSession = LaunchSession.LaunchSessionForAppId(webAppId);
                        webAppSession = new WebOsWebAppSession(launchSession, this);
                        WebAppSessions.Add(webAppId,
                            webAppSession);
                    }

                    launchSession.Service = this;
                    if (obj != null)
                    {
                        launchSession.SessionId = obj.GetNamedString("sessionId");
                        launchSession.SessionType = LaunchSessionType.WebApp;
                        launchSession.RawData = obj;
                    }

                    Util.PostSuccess(listener, webAppSession);

                },
                serviceCommandError => Util.PostError(listener, serviceCommandError)
            );

            var request = new ServiceCommand(this, uri, payload, responseListener);
            request.Send();
        }

        public void LaunchWebApp(string webAppId, JsonObject ps, bool relaunchIfRunning, ResponseListener listener)
        {
            if (webAppId == null)
            {
                Util.PostError(listener, new ServiceCommandError(0, null));
                return;
            }

            if (relaunchIfRunning)
            {
                LaunchWebApp(webAppId, ps, listener);
            }
            else
            {

                var responseListener = new ResponseListener
                (
                    loadEventArg =>
                    {
                        var appInfo = loadEventArg as AppInfo;
                        if (appInfo != null && appInfo.Id.IndexOf(webAppId, StringComparison.Ordinal) != -1)
                        {
                            LaunchSession launchSession = LaunchSession.LaunchSessionForAppId(webAppId);
                            launchSession.SessionType = LaunchSessionType.WebApp;
                            launchSession.Service = this;
                            launchSession.RawData = appInfo.RawData;

                            var webAppSession = WebAppSessionForLaunchSession(launchSession);

                            Util.PostSuccess(listener, webAppSession);
                        }
                        else
                        {
                            LaunchWebApp(webAppId, ps, listener);
                        }
                    },
                    serviceCommandError =>
                    {

                    }
                );

                GetLauncher().GetRunningApp(responseListener);
            }
        }

        public void JoinWebApp(LaunchSession webAppLaunchSession, ResponseListener listener)
        {
            var webAppSession = WebAppSessionForLaunchSession(webAppLaunchSession);

            var responseListener = new ResponseListener
            (
                loadEventArg => Util.PostSuccess(listener, webAppSession),
                serviceCommandError => Util.PostError(listener, serviceCommandError)
            );
            webAppSession.Join(responseListener);
        }

        public void JoinWebApp(string webAppId, ResponseListener listener)
        {
            var launchSession = LaunchSession.LaunchSessionForAppId(webAppId);
            launchSession.SessionType = LaunchSessionType.WebApp;
            launchSession.Service = this;

            JoinWebApp(launchSession, listener);
        }

        public void CloseWebApp(LaunchSession launchSession, ResponseListener listener)
        {
            if (launchSession == null || launchSession.AppId == null || launchSession.AppId.Length == 0)
            {
                Util.PostError(listener, new ServiceCommandError(0, null));
                return;
            }

            var webAppSession = WebAppSessions[launchSession.AppId];

            if (webAppSession != null && webAppSession.IsConnected())
            {
                var serviceCommand = new JsonObject();
                var closeCommand = new JsonObject();

                try
                {
                    serviceCommand.Add("type", JsonValue.CreateStringValue("close"));

                    closeCommand.Add("contentType", JsonValue.CreateStringValue("connectsdk.serviceCommand"));
                    closeCommand.Add("serviceCommand", serviceCommand);
                }
                catch (Exception ex)
                {
                    throw ex;
                }


                var responseListener = new ResponseListener
                (
                    loadEventArg =>
                    {
                        webAppSession.DisconnectFromWebApp();

                        if (Listener != null)
                            listener.OnSuccess(loadEventArg);
                    },
                    serviceCommandError =>
                    {
                        webAppSession.DisconnectFromWebApp();

                        if (Listener != null)
                            listener.OnError(serviceCommandError);
                    }
                );

                webAppSession.SendMessage(closeCommand, responseListener);
            }
            else
            {
                if (webAppSession != null)
                    webAppSession.DisconnectFromWebApp();

                const string uri = "ssap://webapp/closeWebApp";
                var payload = new JsonObject();

                try
                {
                    if (launchSession.AppId != null)
                        payload.Add("webAppId", JsonValue.CreateStringValue(launchSession.AppId));
                    if (launchSession.SessionId != null)
                        payload.Add("sessionId", JsonValue.CreateStringValue(launchSession.SessionId));
                }
                catch (Exception e)
                {
                    throw e;
                }

                var request = new ServiceCommand(this, uri, payload, listener);
                request.Send();
            }
        }

        public void ConnectToWebApp(WebOsWebAppSession webAppSession, bool joinOnly, ResponseListener connectionListener)
        {
            if (WebAppSessions == null)
                WebAppSessions = new Dictionary<string, WebOsWebAppSession>();

            if (AppToAppIdMappings == null)
                AppToAppIdMappings = new Dictionary<string, string>();

            if (webAppSession == null || webAppSession.LaunchSession == null)
            {
                Util.PostError(connectionListener, new ServiceCommandError(0, "You must provide a valid LaunchSession object"));

                return;
            }

            var tappId = webAppSession.LaunchSession.AppId;

            var tidKey = webAppSession.LaunchSession.SessionType == LaunchSessionType.WebApp ? "webAppId" : "appId";

            if (string.IsNullOrEmpty(tappId))
            {
                Util.PostError(connectionListener, new ServiceCommandError(-1, "You must provide a valid web app session"));

                return;
            }

            var appId = tappId;
            var idKey = tidKey;

            const string uri = "ssap://webapp/connectToApp";
            var payload = new JsonObject();

            try
            {
                payload.Add(idKey, JsonValue.CreateStringValue(appId));

            }
            // ReSharper disable once EmptyGeneralCatchClause
            catch (Exception)
            {

            }


            var responseListener = new ResponseListener
            (
                loadEventArg =>
                {
                    var loadEventArgs = loadEventArg as LoadEventArgs;
                    if (loadEventArgs != null)
                    {
                        var jsonObj = (JsonObject)(loadEventArgs.Load.GetPayload());
                        var state = jsonObj.GetNamedString("state");
                        if (!state.Equals("Connected", StringComparison.OrdinalIgnoreCase))
                        {
                            if (joinOnly && state.Equals("WAITING_FOR_APP", StringComparison.OrdinalIgnoreCase))
                            {
                                Util.PostError(connectionListener,
                                    new ServiceCommandError(0, "Web app is not currently running"));
                            }
                            return;
                        }
                        var fullAppId = jsonObj.GetNamedString("appId");
                        if (!string.IsNullOrEmpty(fullAppId))
                        {
                            if (webAppSession.LaunchSession.SessionType == LaunchSessionType.WebApp)
                                AppToAppIdMappings.Add(fullAppId, appId);
                            webAppSession.SetFullAppId(fullAppId);
                        }
                    }
                    if (connectionListener != null)
                        connectionListener.OnSuccess(loadEventArg);
                },
                serviceCommandError =>
                {
                    webAppSession.DisconnectFromWebApp();
                    var appChannelDidClose = false;
                    if (serviceCommandError != null && serviceCommandError.GetPayload() != null)
                        appChannelDidClose = serviceCommandError.GetPayload().ToString().Contains("app channel closed");

                    if (appChannelDidClose)
                    {
                        if (webAppSession.WebAppSessionListener != null)
                        {
                            webAppSession.WebAppSessionListener.OnWebAppSessionDisconnect(webAppSession);
                        }
                    }
                    else
                    {
                        Util.PostError(connectionListener, serviceCommandError);

                    }
                }
            );

            webAppSession.AppToAppSubscription = new UrlServiceSubscription(this, uri, payload, true, responseListener);
            webAppSession.AppToAppSubscription.Subscribe();
        }

        private void NotifyPairingRequired()
        {
            if (Listener != null)
            {
                Listener.OnPairingRequired(this, PairingType.FIRST_SCREEN, null);
            }
        }

        public void PinWebApp(string webAppId, ResponseListener listener)
        {
            if (string.IsNullOrEmpty(webAppId))
            {
                if (listener != null)
                {
                    listener.OnError(new ServiceCommandError(-1, "You must provide a valid web app id"));
                }
                return;
            }

            const string uri = "ssap://webapp/pinWebApp";
            var payload = new JsonObject();

            try
            {
                payload.Add("webAppId", JsonValue.CreateStringValue(webAppId));
            }
            // ReSharper disable once EmptyGeneralCatchClause
            catch (Exception)
            {

            }


            var responseListener = new ResponseListener
            (
                loadEventArg =>
                {
                    var loadEventArgs = loadEventArg as LoadEventArgs;
                    if (loadEventArgs == null) return;
                    var jsonObj = (JsonObject)(loadEventArgs.Load.GetPayload());
                    if (jsonObj.ContainsKey("pairingType"))
                    {
                        NotifyPairingRequired();
                    }
                    else
                    {
                        listener.OnSuccess(loadEventArg);
                    }
                },
                serviceCommandError => Util.PostError(listener, serviceCommandError)
            );

            ServiceCommand request = new UrlServiceSubscription(this, uri, payload, true, responseListener);
            request.Send();
        }

        public void UnPinWebApp(string webAppId, ResponseListener listener)
        {
            if (string.IsNullOrEmpty(webAppId))
            {
                if (listener != null)
                {
                    listener.OnError(new ServiceCommandError(-1, "You must provide a valid web app id"));
                }
                return;
            }

            const string uri = "ssap://webapp/removePinnedWebApp";
            var payload = new JsonObject();

            try
            {
                payload.Add("webAppId", JsonValue.CreateStringValue(webAppId));
            }
            // ReSharper disable once EmptyGeneralCatchClause
            catch (Exception)
            {

            }


            var responseListener = new ResponseListener
            (
                loadEventArg =>
                {
                    var loadEventArgs = loadEventArg as LoadEventArgs;
                    if (loadEventArgs == null) return;
                    var jsonObj = (JsonObject)(loadEventArgs.Load.GetPayload());
                    if (jsonObj.ContainsKey("pairingType"))
                    {
                        NotifyPairingRequired();
                    }
                    else
                    {
                        listener.OnSuccess(loadEventArg);
                    }
                },
                serviceCommandError => Util.PostError(listener, serviceCommandError)
            );

            ServiceCommand request = new UrlServiceSubscription(this, uri, payload, true, responseListener);
            request.Send();
        }

        public ServiceCommand IsWebAppPinned(bool isSubscription, string webAppId, ResponseListener listener)
        {
            if (string.IsNullOrEmpty(webAppId))
            {
                if (listener != null)
                {
                    listener.OnError(new ServiceCommandError(-1, "You must provide a valid web app id"));
                }
                return null;
            }

            const string uri = "ssap://webapp/isWebAppPinned";
            var payload = new JsonObject();

            try
            {
                payload.Add("webAppId", JsonValue.CreateStringValue(webAppId));
            }
            // ReSharper disable once EmptyGeneralCatchClause
            catch (Exception)
            {

            }

            var responseListener = new ResponseListener
            (
                loadEventArg =>
                {
                    var loadEventArgs = loadEventArg as LoadEventArgs;
                    if (loadEventArgs == null) return;
                    var jsonObj = (JsonObject)(loadEventArgs.Load.GetPayload());

                    var status = jsonObj.GetNamedBoolean("pinned");

                    if (listener != null)
                        listener.OnSuccess(status);
                },
                serviceCommandError => Util.PostError(listener, serviceCommandError)
            );

            var request = isSubscription ? new UrlServiceSubscription(this, uri, payload, true, responseListener) : new ServiceCommand(this, uri, payload, responseListener);

            request.Send();
            return request;
        }

        public void IsWebAppPinned(String webAppId, ResponseListener listener)
        {
            IsWebAppPinned(false, webAppId, listener);
        }

        public IServiceSubscription SubscribeIsWebAppPinned(string webAppId, ResponseListener listener)
        {
            return (UrlServiceSubscription)IsWebAppPinned(true, webAppId, listener);
        }

        private WebOsWebAppSession WebAppSessionForLaunchSession(LaunchSession launchSession)
        {
            if (WebAppSessions == null)
                WebAppSessions = new Dictionary<String, WebOsWebAppSession>();

            if (launchSession.Service == null)
                launchSession.Service = this;

            WebOsWebAppSession webAppSession = WebAppSessions.ContainsKey(launchSession.AppId) ? WebAppSessions[launchSession.AppId] : null;

            if (webAppSession == null)
            {
                webAppSession = new WebOsWebAppSession(launchSession, this);
                WebAppSessions.Add(launchSession.AppId, webAppSession);
            }

            return webAppSession;
        }
        #endregion

        public List<String> GetPermissions()
        {
            if (permissions != null)
                return permissions;

            var defaultPermissions = WebOstvServiceOpenPermissionList.ToList();

            if (DiscoveryManager.GetInstance().PairingLevel == DiscoveryManager.PairingLevelEnum.On)
            {
                defaultPermissions.AddRange(WebOstvServiceProtectedPermissionList);

                defaultPermissions.AddRange(WebOstvServicePersonalActivityPermissionList);
            }
            permissions = defaultPermissions;
            return permissions;
        }

        public void SetPermissions(List<String> ppermissions)
        {
            permissions = ppermissions;

            var config = (WebOsTvServiceConfig)serviceConfig;

            if (config.ClientKey == null) return;
            config.ClientKey = null;

            if (IsConnected())
            {
                Logger.Current.AddMessage("Permissions changed -- you will need to re-pair to the TV.");
                Disconnect();
            }
        }

        public void GetServiceInfo(ResponseListener listener)
        {
            const string uri = "ssap://api/getServiceList";

            var responseListener = new ResponseListener
                (
                loadEventArg =>
                {
                    var loadEventArgs = loadEventArg as LoadEventArgs;
                    if (loadEventArgs == null) return;
                    var jsonObj = (JsonObject) (loadEventArgs.Load.GetPayload());
                    if (jsonObj.ContainsKey("services"))
                    {
                        listener.OnSuccess(new ServiceCommandError(0, jsonObj.GetNamedArray("services")));
                    }
                },
                serviceCommandError => Util.PostError(listener, serviceCommandError)
                );

            var request = new ServiceCommand(this, uri, null, responseListener);
            request.Send();


        }

        public void GetSystemInfo(ResponseListener listener)
        {
            const string uri = "ssap://api/getServiceList";

            var responseListener = new ResponseListener
                (
                loadEventArg =>
                {
                    var loadEventArgs = loadEventArg as LoadEventArgs;
                    if (loadEventArgs == null) return;
                    var jsonObj = (JsonObject) (loadEventArgs.Load.GetPayload());
                    if (jsonObj.ContainsKey("features"))
                    {
                        listener.OnSuccess(new ServiceCommandError(0, jsonObj.GetNamedArray("features")));
                    }
                },
                serviceCommandError => Util.PostError(listener, serviceCommandError)
                );

            var request = new ServiceCommand(this, uri, null, responseListener);
            request.Send();

        }

        protected override void UpdateCapabilities()
        {
            var capabilities = new List<String>();

            if (DiscoveryManager.GetInstance().PairingLevel == DiscoveryManager.PairingLevelEnum.On)
            {
                capabilities.AddRange(TextInputControl.Capabilities);
                capabilities.AddRange(MouseControl.Capabilities);
                capabilities.AddRange(KeyControl.Capabilities);
                capabilities.AddRange(MediaPlayer.Capabilities);
                capabilities.AddRange(Launcher.Capabilities);
                capabilities.AddRange(TvControl.Capabilities);
                capabilities.AddRange(ExternalInputControl.Capabilities);
                capabilities.AddRange(VolumeControl.Capabilities);
                capabilities.AddRange(ToastControl.Capabilities);
                capabilities.Add(PowerControl.Off);

            }
            else
            {
                capabilities.AddRange(VolumeControl.Capabilities);
                capabilities.AddRange(MediaPlayer.Capabilities);

                capabilities.Add(Launcher.Application);
                capabilities.Add(Launcher.ApplicationParams);
                capabilities.Add(Launcher.ApplicationClose);
                capabilities.Add(Launcher.Browser);
                capabilities.Add(Launcher.BrowserParams);
                capabilities.Add(Launcher.Hulu);
                capabilities.Add(Launcher.Netflix);
                capabilities.Add(Launcher.NetflixParams);
                capabilities.Add(Launcher.YouTube);
                capabilities.Add(Launcher.YouTubeParams);
                capabilities.Add(Launcher.AppStore);
                capabilities.Add(Launcher.AppStoreParams);
                capabilities.Add(Launcher.AppState);
                capabilities.Add(Launcher.AppStateSubscribe);
            }

            if (ServiceDescription != null && ServiceDescription.Version != null)
            {
                if (ServiceDescription.Version.Contains("4.0.0") || ServiceDescription.Version.Contains("4.0.1"))
                {
                    capabilities.Add(WebAppLauncher.Launch);
                    capabilities.Add(WebAppLauncher.LaunchParams);

                    capabilities.Add(MediaControl.Play);
                    capabilities.Add(MediaControl.Pause);
                    capabilities.Add(MediaControl.Stop);
                    capabilities.Add(MediaControl.Seek);
                    capabilities.Add(MediaControl.Position);
                    capabilities.Add(MediaControl.Duration);
                    capabilities.Add(MediaControl.PlayState);

                    capabilities.Add(WebAppLauncher.Close);
                }
                else
                {
                    capabilities.AddRange(WebAppLauncher.Capabilities);
                    capabilities.AddRange(MediaControl.Capabilities.Where(s => !s.Equals(MediaControl.Previous, StringComparison.OrdinalIgnoreCase) && !s.Equals(MediaControl.Next, StringComparison.OrdinalIgnoreCase)));
                }
            }

            SetCapabilities(capabilities);
        }
    }
}