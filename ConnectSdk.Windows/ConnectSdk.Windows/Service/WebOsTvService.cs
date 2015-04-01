using System;
using System.Collections.Generic;
using System.Linq;
using Windows.Data.Json;
using Windows.Foundation;
using ConnectSdk.Windows.Core;
using ConnectSdk.Windows.Discovery;
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


        //static String FOREGROUND_APP = "ssap://com.webos.applicationManager/getForegroundAppInfo";
        //static String APP_STATUS = "ssap://com.webos.service.appstatus/getAppStatus";
        //static String APP_STATE = "ssap://system.launcher/getAppState";
        //static String VOLUME = "ssap://audio/getVolume";
        //static String MUTE = "ssap://audio/getMute";
        //static String VOLUME_STATUS = "ssap://audio/getStatus";
        //static String CHANNEL_LIST = "ssap://tv/getChannelList";
        //static String CHANNEL = "ssap://tv/getCurrentChannel";
        //static String PROGRAM = "ssap://tv/getChannelProgramInfo";

        public Dictionary<string, string> AppToAppIdMappings { get; set; }
        public Dictionary<string, WebOsWebAppSession> WebAppSessions { get; set; }

        private WebOstvServiceSocketClient socket;
        private WebOstvMouseSocketConnection mouseSocket;

        private List<String> permissions;

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

        public void SetServiceDescription(ServiceDescription serviceDescription)
        {
            ServiceDescription = serviceDescription;

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
            if (socket != null)
                socket.SendCommand(command);
        }

        public override bool IsConnected()
        {
            if (DiscoveryManager.GetInstance().PairingLevel == DiscoveryManager.PairingLevelEnum.On)
            {
                return socket != null && socket.IsConnected() &&
                       (((WebOSTVServiceConfig) serviceConfig).getClientKey() != null);
            }
            return socket != null && socket.IsConnected();
        }

        public override void Connect()
        {
            if (socket == null)
            {
                socket = new WebOstvServiceSocketClient(this, WebOstvServiceSocketClient.GetUri(this));
            }

            if (!IsConnected())
                socket.Connect();
        }

        public override void Disconnect()
        {
            //Log.d("Connect SDK", "attempting to disconnect to " + serviceDescription.getIpAddress());

            if (Listener != null)
                Listener.OnDisconnect(this, null);

            if (socket != null)
            {
                socket.Listener = null;
                socket.Disconnect();
                socket = null;
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
            var intVolume = (int) Math.Round(volume*100.0f);

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


        public void GetVolume(bool isSubscription, ResponseListener listener)
        {
            var getVolumeResponseListener = new ResponseListener
                (
                loadEventArg =>
                {
                    try
                    {
                        var jsonObj = (JsonObject) loadEventArg;
                        var iVolume = (int) jsonObj.GetNamedNumber("volume");
                        var fVolume = (float) (iVolume/100.0);

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
                //e.printStackTrace();
            }

            var request = new ServiceCommand(this, uri, payload, listener);
            request.Send();
        }

        public void GetMute(ResponseListener listener)
        {
            GetMuteStatus(false, listener);
        }

        private void GetMuteStatus(bool isSubscription, ResponseListener listener)
        {

            var getMuteResponseListener = new ResponseListener
                (
                loadEventArg =>
                {
                    try
                    {
                        var jsonObj = (JsonObject) loadEventArg;
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
        }

        public IServiceSubscription SubscribeVolume(ResponseListener listener)
        {
            throw new NotImplementedException();
            //return (ServiceSubscription<VolumeListener>)getVolume(true, listener);
        }

        public IServiceSubscription SubscribeMute(ResponseListener listener)
        {
            throw new NotImplementedException();
            //return (ServiceSubscription<MuteListener>) getMuteStatus(true, listener);
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
                    contentId = ((JsonObject) ps).GetNamedString("contentId");
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
                    payload.Add("params", (JsonObject) ps);
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
            var appInfo = new AppInfo(appId) {Id = appId};

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
                        var appList = new List<AppInfo>();

                        for (var i = 0; i < apps.Count; i++)
                        {
                            var appObj = apps[i].GetObject();

                            if (appObj == null) continue;
                            var appInfo = new AppInfo(appObj.GetNamedString("id"))
                            {

                                Name = appObj.GetNamedString("title"),
                                Url = appObj.GetNamedString("icon"),
                                RawData = appObj
                            };

                            appList.Add(appInfo);
                        }
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
            ServiceCommand request;

            var responseListener = new ResponseListener
                (
                loadEventArg =>
                {
                    var jsonObj = (JsonObject)(((LoadEventArgs)loadEventArg).Load.GetPayload());
                    var appInfo = new AppInfo(jsonObj.GetNamedString("appId"))
                    {
                        Name = jsonObj.GetNamedString("title",""),
                        RawData = jsonObj
                    };
                    Util.PostSuccess(listener, appInfo);
                },
                serviceCommandError => Util.PostError(listener, serviceCommandError)
                );

            if (isSubscription)
                request = new UrlServiceSubscription(this, ForegroundApp, null, true, responseListener);
            else
                request = new ServiceCommand(this, ForegroundApp, null, responseListener);

            request.Send();

            return request;
        }

        public IServiceSubscription SubscribeRunningApp(ResponseListener listener)
        {
            return (UrlServiceSubscription) GetRunningApp(true, listener);
        }

        public void GetAppState(LaunchSession launchSession, ResponseListener listener)
        {
            GetAppState(false, launchSession, listener);
        }

        public ServiceCommand GetAppState(bool subscription, LaunchSession launchSession, ResponseListener listener)
        {
            ServiceCommand request;

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

            if (subscription)
                request = new UrlServiceSubscription(this, AppStatus, payload, true, responseListener);
            else
                request = new ServiceCommand(this, AppStatus, payload, responseListener);

            request.Send();

            return request;
        }

        public IServiceSubscription SubscribeAppState(LaunchSession launchSession, ResponseListener listener)
        {
            return (UrlServiceSubscription) GetAppState(true, launchSession, listener);
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
            throw new NotImplementedException();
        }

        public CapabilityPriorityLevel GetMediaControlCapabilityLevel()
        {
            throw new NotImplementedException();
        }

        public void Play(ResponseListener listener)
        {
            throw new NotImplementedException();
        }

        public void Pause(ResponseListener listener)
        {
            throw new NotImplementedException();
        }

        public void Stop(ResponseListener listener)
        {
            throw new NotImplementedException();
        }

        public void Rewind(ResponseListener listener)
        {
            throw new NotImplementedException();
        }

        public void FastForward(ResponseListener listener)
        {
            throw new NotImplementedException();
        }

        public void Seek(long position, ResponseListener listener)
        {
            throw new NotImplementedException();
        }

        public void GetDuration(ResponseListener listener)
        {
            throw new NotImplementedException();
        }

        public void GetPosition(ResponseListener listener)
        {
            throw new NotImplementedException();
        }

        public void GetPlayState(ResponseListener listener)
        {
            throw new NotImplementedException();
        }

        public IServiceSubscription SubscribePlayState(ResponseListener listener)
        {
            throw new NotImplementedException();
        }

        public void Next(ResponseListener listener)
        {
            throw new NotImplementedException();
        }

        public void Previous(ResponseListener listener)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Media Player

        public IMediaPlayer GetMediaPlayer()
        {
            throw new NotImplementedException();
        }

        public CapabilityPriorityLevel GetMediaPlayerCapabilityLevel()
        {
            throw new NotImplementedException();
        }

        public void DisplayImage(string url, string mimeType, string title, string description, string iconSrc,
            ResponseListener listener)
        {
            throw new NotImplementedException();
        }

        public void PlayMedia(string url, string mimeType, string title, string description, string iconSrc,
            bool shouldLoop,
            ResponseListener listener)
        {
            throw new NotImplementedException();
        }

        public void CloseMedia(LaunchSession launchSession, ResponseListener listener)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region TV Control

        public ITvControl GetTvControl()
        {
            throw new NotImplementedException();
        }

        public CapabilityPriorityLevel GetTvControlCapabilityLevel()
        {
            throw new NotImplementedException();
        }

        public void ChannelUp(ResponseListener listener)
        {
            throw new NotImplementedException();
        }

        public void ChannelDown(ResponseListener listener)
        {
            throw new NotImplementedException();
        }

        public void SetChannel(ChannelInfo channelNumber, ResponseListener listener)
        {
            throw new NotImplementedException();
        }

        public void GetCurrentChannel(ResponseListener listener)
        {
            throw new NotImplementedException();
        }

        public IServiceSubscription SubscribeCurrentChannel(ResponseListener listener)
        {
            throw new NotImplementedException();
        }

        public void GetChannelList(ResponseListener listener)
        {
            throw new NotImplementedException();
        }

        public void GetProgramInfo(ResponseListener listener)
        {
            throw new NotImplementedException();
        }

        public IServiceSubscription SubscribeProgramInfo(ResponseListener listener)
        {
            throw new NotImplementedException();
        }

        public void GetProgramList(ResponseListener listener)
        {
            throw new NotImplementedException();
        }

        public IServiceSubscription SubscribeProgramList(ResponseListener listener)
        {
            throw new NotImplementedException();
        }

        public void Get3DEnabled(ResponseListener listener)
        {
            throw new NotImplementedException();
        }

        public void Set3DEnabled(bool enabled, ResponseListener listener)
        {
            throw new NotImplementedException();
        }

        public IServiceSubscription Subscribe3DEnabled(ResponseListener listener)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Toast Control

        public IToastControl GetToastControl()
        {
            throw new NotImplementedException();
        }

        public CapabilityPriorityLevel GetToastControlCapabilityLevel()
        {
            throw new NotImplementedException();
        }

        public void ShowToast(string message, ResponseListener listener)
        {
            throw new NotImplementedException();
        }

        public void ShowToast(string message, string iconData, string iconExtension, ResponseListener listener)
        {
            throw new NotImplementedException();
        }

        public void ShowClickableToastForApp(string message, AppInfo appInfo, JsonObject ps, ResponseListener listener)
        {
            throw new NotImplementedException();
        }

        public void ShowClickableToastForApp(string message, AppInfo appInfo, JsonObject ps, string iconData,
            string iconExtension,
            ResponseListener listener)
        {
            throw new NotImplementedException();
        }

        public void ShowClickableToastForUrl(string message, string url, ResponseListener listener)
        {
            throw new NotImplementedException();
        }

        public void ShowClickableToastForUrl(string message, string url, string iconData, string iconExtension,
            ResponseListener listener)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region External Input Control

        public IExternalInputControl GetExternalInput()
        {
            throw new NotImplementedException();
        }

        public CapabilityPriorityLevel GetExternalInputControlPriorityLevel()
        {
            throw new NotImplementedException();
        }

        public void LaunchInputPicker(ResponseListener pListener)
        {
            throw new NotImplementedException();
        }

        public void CloseInputPicker(LaunchSession launchSessionm, ResponseListener pListener)
        {
            throw new NotImplementedException();
        }

        public void SetExternalInput(ExternalInputInfo input, ResponseListener pListener)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Mouse Control

        public IMouseControl GetMouseControl()
        {
            throw new NotImplementedException();
        }

        public CapabilityPriorityLevel GetMouseControlCapabilityLevel()
        {
            throw new NotImplementedException();
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
            throw new NotImplementedException();
        }

        public void Click()
        {
            throw new NotImplementedException();
        }

        public void Move(double dx, double dy)
        {
            throw new NotImplementedException();
        }

        public void Move(Point distance)
        {
            throw new NotImplementedException();
        }

        public void Scroll(double dx, double dy)
        {
            throw new NotImplementedException();
        }

        public void Scroll(Point distance)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Text Input Control

        public ITextInputControl GetTextInputControl()
        {
            throw new NotImplementedException();
        }

        public CapabilityPriorityLevel GetTextInputControlCapabilityLevel()
        {
            throw new NotImplementedException();
        }

        public IServiceSubscription SubscribeTextInputStatus(ResponseListener listener)
        {
            throw new NotImplementedException();
        }

        public void SendText(string input)
        {
            throw new NotImplementedException();
        }

        public void SendEnter()
        {
            throw new NotImplementedException();
        }

        public void SendDelete()
        {
            throw new NotImplementedException();
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

        private void SendSpecialKey(String key)
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
                    },
                    serviceCommandError =>
                    {

                    }
                );

                ConnectMouse(responseListener);
            }
        }

        public void Up(ResponseListener listener)
        {
            SendSpecialKey("UP");
        }

        public void Down(ResponseListener listener)
        {
            SendSpecialKey("DOWN");
        }

        public void Left(ResponseListener listener)
        {
            SendSpecialKey("LEFT");
        }

        public void Right(ResponseListener listener)
        {
            SendSpecialKey("RIGHT");
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
            SendSpecialKey("BACK");
        }

        public void Home(ResponseListener listener)
        {
            SendSpecialKey("HOME");
        }

        public void SendKeyCode(int keyCode, ResponseListener pListener)
        {
            Util.PostError(pListener, ServiceCommandError.NotSupported());
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

            webAppSession.Join(new ResponseListener());
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

            WebOsWebAppSession webAppSession = WebAppSessions[launchSession.AppId];

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

            var config = (WebOSTVServiceConfig)serviceConfig;

            if (config.getClientKey() == null) return;
            config.setClientKey(null);

            if (IsConnected())
            {
                //Log.w("Connect SDK", "Permissions changed -- you will need to re-pair to the TV.");
                Disconnect();
            }
        }
    }
}