using System;
using System.Collections.Generic;
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
        IToastControl, IExternalInputControl, IMouseControl, ITextInputControl, IPowerControl, IKeyControl, IWebAppLauncher
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

        private const string VolumeUrl = "ssap://audio/getVolume";
        private const string MuteUrl = "ssap://audio/getMute";

        private PairingType pairingType;
        public Dictionary<string, string> AppToAppIdMappings { get; set; }
        public Dictionary<string, WebOsWebAppSession> WebAppSessions { get; set; }

        private WebOstvServiceSocketClient socket;

        private List<String> permissions;

        public WebOstvService(ServiceDescription serviceDescription, ServiceConfig serviceConfig)
            : base(serviceDescription, serviceConfig)
        {
            pairingType = PairingType.FIRST_SCREEN;

            AppToAppIdMappings = new Dictionary<String, String>();
            WebAppSessions = new Dictionary<String, WebOsWebAppSession>();
        }

        public WebOstvService(ServiceConfig serviceConfig) : base(serviceConfig)
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
                       (((WebOSTVServiceConfig)serviceConfig).getClientKey() != null);
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
            var getVolumeResponseListener = new ResponseListener();
            getVolumeResponseListener.Success += (sender, o) =>
            {
                try
                {
                    var jsonObj = (JsonObject) o;
                    var iVolume = (int) jsonObj.GetNamedNumber("volume");
                    var fVolume = (float) (iVolume/100.0);

                    Util.PostSuccess(listener, fVolume);
                }
                catch (Exception)
                {

                }
            };
            getVolumeResponseListener.Error += (sender, error) => Util.PostError(listener, error);

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
            var getMuteResponseListener = new ResponseListener();
            getMuteResponseListener.Success += (sender, o) =>
            {
                try
                {
                    var jsonObj = (JsonObject) o;
                    var isMute = jsonObj.GetNamedBoolean("mute");
                    Util.PostSuccess(listener, isMute);
                }
                catch (Exception)
                {

                }
            };
            getMuteResponseListener.Error += (sender, error) => Util.PostError(listener, error);

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
            throw new NotImplementedException();
        }

        public CapabilityPriorityLevel GetLauncherCapabilityLevel()
        {
            throw new NotImplementedException();
        }

        public void LaunchAppWithInfo(AppInfo appInfo, ResponseListener listener)
        {
            throw new NotImplementedException();
        }

        public void LaunchAppWithInfo(AppInfo appInfo, object ps, ResponseListener listener)
        {
            throw new NotImplementedException();
        }

        public void LaunchApp(string appId, ResponseListener listener)
        {
            throw new NotImplementedException();
        }

        public void CloseApp(LaunchSession launchSession, ResponseListener listener)
        {
            throw new NotImplementedException();
        }

        public void GetAppList(ResponseListener listener)
        {
            throw new NotImplementedException();
        }

        public void GetRunningApp(ResponseListener listener)
        {
            throw new NotImplementedException();
        }

        public IServiceSubscription SubscribeRunningApp(ResponseListener listener)
        {
            throw new NotImplementedException();
        }

        public void GetAppState(LaunchSession launchSession, ResponseListener listener)
        {
            throw new NotImplementedException();
        }

        public IServiceSubscription SubscribeAppState(LaunchSession launchSession, ResponseListener listener)
        {
            throw new NotImplementedException();
        }

        public void LaunchBrowser(string url, ResponseListener listener)
        {
            throw new NotImplementedException();
        }

        public void LaunchYouTube(string contentId, ResponseListener listener)
        {
            throw new NotImplementedException();
        }

        public void LaunchNetflix(string contentId, ResponseListener listener)
        {
            throw new NotImplementedException();
        }

        public void LaunchHulu(string contentId, ResponseListener listener)
        {
            throw new NotImplementedException();
        }

        public void LaunchAppStore(string appId, ResponseListener listener)
        {
            throw new NotImplementedException();
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
            throw new NotImplementedException();
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
            throw new NotImplementedException();
        }

        public CapabilityPriorityLevel GetPowerControlCapabilityLevel()
        {
            throw new NotImplementedException();
        }

        public void PowerOff(ResponseListener listener)
        {
            throw new NotImplementedException();
        }

        public void PowerOn(ResponseListener listener)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Key Control

        public IKeyControl GetKeyControl()
        {
            throw new NotImplementedException();
        }

        public CapabilityPriorityLevel GetKeyControlCapabilityLevel()
        {
            throw new NotImplementedException();
        }

        public void Up(ResponseListener listener)
        {
            throw new NotImplementedException();
        }

        public void Down(ResponseListener listener)
        {
            throw new NotImplementedException();
        }

        public void Left(ResponseListener listener)
        {
            throw new NotImplementedException();
        }

        public void Right(ResponseListener listener)
        {
            throw new NotImplementedException();
        }

        public void Ok(ResponseListener listener)
        {
            throw new NotImplementedException();
        }

        public void Back(ResponseListener listener)
        {
            throw new NotImplementedException();
        }

        public void Home(ResponseListener listener)
        {
            throw new NotImplementedException();
        }

        public void SendKeyCode(int keyCode, ResponseListener pListener)
        {
            throw new NotImplementedException();
        }

        #endregion    }

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

            var webAppSession = WebAppSessions.ContainsKey(webAppId)? WebAppSessions[webAppId] : null;

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

            var responseListener = new ResponseListener();
            responseListener.Success += (sender, o) =>
            {
                JsonObject obj;
                if (o is LoadEventArgs)
                    obj = (o as LoadEventArgs).Load.GetPayload() as JsonObject;
                else obj = (JsonObject)o;

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
                launchSession.SessionId = obj.GetNamedString("sessionId");
                launchSession.SessionType = LaunchSessionType.WebApp;
                launchSession.RawData = obj;

                Util.PostSuccess(listener, webAppSession);
            };
            responseListener.Error += (sender, error) =>
            {
                Util.PostError(listener, error); 
                
            };


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
                var responseListener = new ResponseListener();

                listener.Success += (sender, o) =>
                {
                    var appInfo = o as AppInfo;
                    if (appInfo.Id.IndexOf(webAppId) != -1)
                    {
                        LaunchSession launchSession = LaunchSession.LaunchSessionForAppId(webAppId);
                        launchSession.SessionType = LaunchSessionType.WebApp;
                        launchSession.Service = this;
                        launchSession.RawData = appInfo.RawData;

                        WebOsWebAppSession webAppSession = WebAppSessionForLaunchSession(launchSession);

                        Util.PostSuccess(listener, webAppSession);
                    }
                    else
                    {
                        LaunchWebApp(webAppId, ps, listener);
                    }
                };
                listener.Error += (sender, error) => { };

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

                    var responseListener = new ResponseListener();

                    listener.Success += (sender, o) =>
                    {
                        webAppSession.DisconnectFromWebApp();

                        if (Listener != null)
                            listener.OnSuccess(o);
                    };
                    listener.Error += (sender, error) =>
                    {
                        webAppSession.DisconnectFromWebApp();

                        if (Listener != null)
                            listener.OnError(error);
                    };


                    webAppSession.SendMessage(closeCommand, new ResponseListener());
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

        public void ConnectToWebApp(WebOsWebAppSession webAppSession, bool joinOnly,
            ResponseListener connectionListener)
        {
            throw new NotImplementedException();
        }

        public void PinWebApp(string webAppId, ResponseListener listener)
        {
            throw new NotImplementedException();
        }

        public void UnPinWebApp(string webAppId, ResponseListener listener)
        {
            throw new NotImplementedException();
        }

        public void IsWebAppPinned(string webAppId, ResponseListener listener)
        {
            throw new NotImplementedException();
        }

        public IServiceSubscription SubscribeIsWebAppPinned(string webAppId, ResponseListener listener)
        {
            throw new NotImplementedException();
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

            var defaultPermissions = new List<String>();
            foreach (String perm in WebOstvServiceOpenPermissionList)
            {
                defaultPermissions.Add(perm);
            }

            if (DiscoveryManager.GetInstance().PairingLevel == DiscoveryManager.PairingLevelEnum.On)
            {
                foreach (String perm in WebOstvServiceProtectedPermissionList)
                {
                    defaultPermissions.Add(perm);
                }

                foreach (String perm in WebOstvServicePersonalActivityPermissionList)
                {
                    defaultPermissions.Add(perm);
                }
            }
            permissions = defaultPermissions;
            return permissions;
        }

        public void SetPermissions(List<String> permissions)
        {
            this.permissions = permissions;

            var config = (WebOSTVServiceConfig)serviceConfig;

            if (config.getClientKey() != null)
            {
                config.setClientKey(null);

                if (IsConnected())
                {
                    //Log.w("Connect SDK", "Permissions changed -- you will need to re-pair to the TV.");
                    Disconnect();
                }
            }
        }
    }
}