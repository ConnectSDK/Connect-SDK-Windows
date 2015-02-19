using System;
using System.Collections.Generic;
using Windows.Data.Json;
using Windows.Foundation;
using ConnectSdk.Windows.Core;
using ConnectSdk.Windows.Device;
using ConnectSdk.Windows.Device.Netcast;
using ConnectSdk.Windows.Discovery;
using ConnectSdk.Windows.Service.Capability;
using ConnectSdk.Windows.Service.Capability.Listeners;
using ConnectSdk.Windows.Service.Command;
using ConnectSdk.Windows.Service.Config;
using ConnectSdk.Windows.Service.Sessions;
using ConnectSdk.Windows.Service.WebOs;

namespace ConnectSdk.Windows.Service
{
    public class WebOstvService : DeviceService, ILauncher, IMediaControl, IMediaPlayer, IVolumeControl, ITvControl,
        IToastControl, IExternalInputControl, IMouseControl, ITextInputControl, IPowerControl, IKeyControl,
        IWebAppLauncher
    {
        public static String Id = "webOS TV";


        public static String[] KWebOstvServiceOpenPermissions =
        {
            "LAUNCH",
            "LAUNCH_WEBAPP",
            "APP_TO_APP",
            "CONTROL_AUDIO",
            "CONTROL_INPUT_MEDIA_PLAYBACK"
        };

        public static String[] KWebOstvServiceProtectedPermissions =
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

        public static String[] KWebOstvServicePersonalActivityPermissions =
        {
            "CONTROL_INPUT_TEXT",
            "CONTROL_MOUSE_AND_KEYBOARD",
            "READ_CURRENT_CHANNEL",
            "READ_RUNNING_APPS"
        };


        //private static String FOREGROUND_APP = "ssap://com.webos.applicationManager/getForegroundAppInfo";
        //private static String APP_STATUS = "ssap://com.webos.service.appstatus/getAppStatus";
        //private static String APP_STATE = "ssap://system.launcher/getAppState";
        //private static String VOLUME = "ssap://audio/getVolume";
        //private static String MUTE = "ssap://audio/getMute";
        //private static String VOLUME_STATUS = "ssap://audio/getStatus";
        //private static String CHANNEL_LIST = "ssap://tv/getChannelList";
        //private static String CHANNEL = "ssap://tv/getCurrentChannel";
        //private static String PROGRAM = "ssap://tv/getChannelProgramInfo";

        //private static String CLOSE_APP_URI = "ssap://system.launcher/close";
        //private static String CLOSE_MEDIA_URI = "ssap://media.viewer/close";
        //private static String CLOSE_WEBAPP_URI = "ssap://webapp/closeWebApp";

        private readonly Dictionary<String, String> mAppToAppIdMappings;
        private bool isMouseConnected;
        private WebOsTvKeyboardInput keyboardInput;
        private Dictionary<String, WebOsWebAppSession> mWebAppSessions;
        private WebOstvMouseSocketConnection mouseSocket;

        private bool mute;

        private List<String> permissions;
        private WebOstvServiceSocketClient socket;

        public WebOstvService(ServiceDescription serviceDescription, ServiceConfig serviceConfig) :
            base(serviceDescription, serviceConfig)
        {
            SetServiceDescription(serviceDescription);

            mAppToAppIdMappings = new Dictionary<String, String>();
            mWebAppSessions = new Dictionary<String, WebOsWebAppSession>();
        }

        public IExternalInputControl GetExternalInput()
        {
            return this;
        }

        public CapabilityPriorityLevel GetExternalInputControlPriorityLevel()
        {
            return CapabilityPriorityLevel.HIGH;
        }

        public void LaunchInputPicker(ResponseListener pListener)
        {
            var appInfo = new AppInfo("com.webos.app.inputpicker") {Name = "InputPicker"};
            LaunchAppWithInfo(appInfo, null, pListener);
        }

        public void CloseInputPicker(LaunchSession launchSession, ResponseListener pListener)
        {
            CloseApp(launchSession, pListener);
        }

        public void SetExternalInput(ExternalInputInfo externalInputInfo, ResponseListener pListener)
        {
            //String uri = "ssap://tv/switchInput";

            //JsonObject payload = new JsonObject();

            //try
            //{
            //    if (externalInputInfo != null && externalInputInfo.getId() != null)
            //    {
            //        payload.put("inputId", externalInputInfo.getId());
            //    }
            //    else
            //    {
            //        Log.w("Connect SDK", "ExternalInputInfo has no id");
            //    }
            //}
            //catch (Exception e)
            //{
            //    throw e;
            //}

            //ServiceCommand request = new ServiceCommand(this, uri,
            //    payload, true, listener);
            //request.Send();
        }

        public void SendKeyCode(int keyCode, ResponseListener pListener)
        {
            switch (keyCode)
            {
                case (int) VirtualKeycodes.KEY_UP:
                    Up(null);
                    break;
                case (int) VirtualKeycodes.KEY_DOWN:
                    Down(null);
                    break;
                case (int) VirtualKeycodes.KEY_LEFT:
                    Left(null);
                    break;
                case (int) VirtualKeycodes.KEY_RIGHT:
                    Right(null);
                    break;
                case (int) VirtualKeycodes.CHANNEL_UP:
                    ChannelUp();
                    break;
                case (int) VirtualKeycodes.CHANNEL_DOWN:
                    ChannelDown();
                    break;
                case (int) VirtualKeycodes.OK:
                    Ok(null);
                    break;

                case (int) VirtualKeycodes.PLAY:
                    Play(null);
                    break;
                case (int) VirtualKeycodes.PAUSE:
                    Pause(null);
                    break;
                case (int) VirtualKeycodes.STOP:
                    stop(null);
                    break;
                case (int) VirtualKeycodes.REWIND:
                    Rewind(null);
                    break;
                case (int) VirtualKeycodes.FAST_FORWARD:
                    fastForward(null);
                    break;

                case (int) VirtualKeycodes.POWER:
                    PowerOff(null);
                    break;

                case (int) VirtualKeycodes.EXTERNAL_INPUT:
                    LaunchInputPicker(null);
                    break;

                case (int) VirtualKeycodes.MUTE:
                    SetMute(!mute, null);
                    mute = !mute;
                    break;

                default:
                    Util.PostError(pListener, ServiceCommandError.NotSupported());
                    break;
            }
        }

        public IKeyControl GetKeyControl()
        {
            return this;
        }

        public CapabilityPriorityLevel GetKeyControlCapabilityLevel()
        {
            return CapabilityPriorityLevel.HIGH;
        }

        public void Up(ResponseListener pListener)
        {
            SendSpecialKey("UP");
        }

        public void Down(ResponseListener pListener)
        {
            SendSpecialKey("DOWN");
        }

        public void Left(ResponseListener pListener)
        {
            SendSpecialKey("LEFT");
        }

        public void Right(ResponseListener pListener)
        {
            SendSpecialKey("RIGHT");
        }

        public void Ok(ResponseListener pListener)
        {
            if (mouseSocket != null)
            {
                mouseSocket.Click();
            }
            else
            {
                var responseListener = new ResponseListener();
                responseListener.Success += (sender, o) =>
                {
                    try
                    {
                        var jsonObj = (JsonObject) o;
                        var socketPath = jsonObj.GetNamedString("socketPath");
                        mouseSocket = new WebOstvMouseSocketConnection(socketPath);

                        mouseSocket.Click();
                    }
                    catch
                        (Exception
                            e)
                    {
                        throw e;
                    }
                };
                ConnectMouse(responseListener);
            }
        }

        public void Back(ResponseListener pListener)
        {
            SendSpecialKey("BACK");
        }

        public void Home(ResponseListener pListener)
        {
            SendSpecialKey("HOME");
        }


        public ILauncher GetLauncher()
        {
            return this;
        }


        public CapabilityPriorityLevel GetLauncherCapabilityLevel()
        {
            return CapabilityPriorityLevel.HIGH;
        }


        public void LaunchApp(String appId, ResponseListener pListener)
        {
            var appInfo = new AppInfo(appId);
            LaunchAppWithInfo(appInfo, pListener);
        }

        public void LaunchAppWithInfo(AppInfo appInfo, ResponseListener pListener)
        {
            LaunchAppWithInfo(appInfo, null, pListener);
        }

        public void LaunchAppWithInfo(AppInfo appInfo, Object ps, ResponseListener pListener)
        {
            String uri = "ssap://system.launcher/launch";
            var payload = new JsonObject();

            String appId = appInfo.Id;

            String contentId = null;

            if (ps != null)
            {
                try
                {
                    contentId = ((JsonObject) ps).GetNamedString("contentId");
                }
                catch (Exception e)
                {
                    throw e;
                }
            }

            try
            {
                payload.Add("id", JsonValue.CreateStringValue(appId));

                if (contentId != null)
                    payload.Add("contentId", JsonValue.CreateStringValue(contentId));

                if (ps != null)
                    payload.Add("ps", JsonValue.CreateStringValue(ps as string));
            }
            catch (Exception e)
            {
                throw e;
            }

            var responseListener = new ResponseListener();
            responseListener.Success += (sender, o) =>
            {
                var obj = (JsonObject) o;
                var launchSession = new LaunchSession
                {
                    Service = this,
                    AppId = appId,
                    SessionId = obj.GetNamedString("sessionId"),
                    SessionType = LaunchSessionType.App
                };

                Util.PostSuccess(pListener, launchSession);
            };
            responseListener.Error += (sender, error) => Util.PostError(pListener, error);

            var request = new ServiceCommand(this, uri, payload, responseListener);
            request.Send();
        }

        public void LaunchBrowser(String url, ResponseListener pListener)
        {
            const string uri = "ssap://system.launcher/open";
            var payload = new JsonObject();

            var responseListener = new ResponseListener();
            responseListener.Success += (sender, o) =>
            {
                var obj = (JsonObject) o;
                var launchSession = new LaunchSession();

                launchSession.Service = this;
                launchSession.AppId = obj.GetNamedString("id"); // note that response uses id to mean appId
                launchSession.SessionId = obj.GetNamedString("sessionId");
                launchSession.SessionType = LaunchSessionType.App;
                launchSession.RawData = obj;

                Util.PostSuccess(pListener, launchSession);
            };
            responseListener.Error += (sender, o) => Util.PostError(pListener, o);
            try
            {
                payload.Add("target", JsonValue.CreateStringValue(url));
            }
            catch (Exception e)
            {
                throw e;
            }

            var request = new ServiceCommand(this, uri, payload, responseListener);
            request.Send();
        }

        public void LaunchYouTube(String contentId, ResponseListener pListener)
        {
            var ps = new JsonObject();

            try
            {
                ps.Add("contentId", JsonValue.CreateStringValue(contentId));
            }
            catch (Exception e)
            {
                throw e;
            }

            var appInfo = new AppInfo("youtube.leanback.v4");
            LaunchAppWithInfo(appInfo, ps, pListener);
        }

        public void LaunchHulu(String contentId, ResponseListener listener)
        {
            var ps = new JsonObject();

            try
            {
                ps.Add("contentId", JsonValue.CreateStringValue(contentId));
            }
            catch (Exception e)
            {
                throw e;
            }

            var appInfo = new AppInfo("hulu") {Name = "Hulu"};
            LaunchAppWithInfo(appInfo, ps, listener);
        }

        public void LaunchNetflix(String contentId, ResponseListener listener)
        {
            var ps = new JsonObject();
            String netflixContentId = "m=http%3A%2F%2Fapi.netflix.com%2Fcatalog%2Ftitles%2Fmovies%2F" + contentId +
                                      "&source_type=4";
            try
            {
                ps.Add("contentId", JsonValue.CreateStringValue(netflixContentId));
            }
            catch (Exception e)
            {
                throw e;
            }

            var appInfo = new AppInfo("netflix");
            appInfo.Name = "Netflix";
            LaunchAppWithInfo(appInfo, ps, listener);
        }

        public void LaunchAppStore(String appId, ResponseListener listener)
        {
            var appInfo = new AppInfo("com.webos.app.discovery");
            appInfo.Name = "LG Store";
            var ps = new JsonObject();

            if (!string.IsNullOrEmpty(appId))
            {
                String query = string.Format("category/GAME_APPS/{0}", appId);
                try
                {
                    ps.Add("query", JsonValue.CreateStringValue(query));
                }
                catch (Exception e)
                {
                    throw e;
                }
            }

            LaunchAppWithInfo(appInfo, ps, listener);
        }

        public void CloseApp(LaunchSession launchSession, ResponseListener listener)
        {
            const string uri = "ssap://system.launcher/close";
            string appId = launchSession.AppId;
            string sessionId = launchSession.SessionId;

            var payload = new JsonObject();

            try
            {
                payload.Add("id", JsonValue.CreateStringValue(appId));
                payload.Add("sessionId", JsonValue.CreateStringValue(sessionId));
            }
            catch (Exception e)
            {
                throw e;
            }

            var request = new ServiceCommand(launchSession.Service, uri, payload, listener);
            request.Send();
        }

        public void GetAppList(ResponseListener listener)
        {
            const string uri = "ssap://com.webos.applicationManager/listApps";
            var responseListener = new ResponseListener();

            listener.Success += (sender, o) =>
            {
                try
                {
                    var jsonObj = (JsonObject) o;

                    JsonArray apps = jsonObj.GetNamedArray("apps");
                    var appList = new List<AppInfo>();

                    for (uint i = 0; i < apps.Count; i++)
                    {
                        JsonObject appObj = apps.GetObjectAt(i);

                        var appInfo = new AppInfo(appObj.GetNamedString("id"))
                        {
                            Name = appObj.GetNamedString("title"),
                            RawData = appObj
                        };
                        appList.Add(appInfo);
                    }
                    Util.PostSuccess(listener, appList);
                }
                catch (Exception e)
                {
                    throw e;
                }
            };
            listener.Error += (sender, error) => Util.PostError(listener, error);

            var request = new ServiceCommand(this, uri, null, responseListener);
            request.Send();
        }

        public void GetRunningApp(ResponseListener listener)
        {
            GetRunningApp(false, listener);
        }

        public IServiceSubscription SubscribeRunningApp(ResponseListener listener)
        {
            return (UrlServiceSubscription) GetRunningApp(true, listener);
        }


        public void GetAppState(LaunchSession launchSession, ResponseListener listener)
        {
            GetAppState(false, launchSession, listener);
        }

        public IServiceSubscription SubscribeAppState(LaunchSession launchSession,
            ResponseListener listener)
        {
            return (UrlServiceSubscription) GetAppState(true, launchSession, listener);
        }

        public IMediaControl GetMediaControl()
        {
            return this;
        }

        public CapabilityPriorityLevel GetMediaControlCapabilityLevel()
        {
            return CapabilityPriorityLevel.NORMAL;
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
            throw new NotImplementedException();
        }

        public void Rewind(ResponseListener listener)
        {
            const string uri = "ssap://media.controls/rewind";
            var request = new ServiceCommand(this, uri, null, listener);

            request.Send();
        }

        public void FastForward(ResponseListener listener)
        {
            throw new NotImplementedException();
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
            throw new NotImplementedException();
        }

        public void Previous(ResponseListener listener)
        {
            throw new NotImplementedException();
        }

        public IMediaPlayer GetMediaPlayer()
        {
            return this;
        }

        public CapabilityPriorityLevel GetMediaPlayerCapabilityLevel()
        {
            return CapabilityPriorityLevel.HIGH;
        }

        public void DisplayImage(string url, String mimeType, String title, String description, String iconSrc,
            ResponseListener listener)
        {
            //    if ("4.0.0".equalsIgnoreCase(this.serviceDescription.getVersion()))
            //    {
            //        DeviceService dlnaService = this.getDLNAService();

            //        if (dlnaService != null)
            //        {
            //            MediaPlayer mediaPlayer = dlnaService.getAPI(MediaPlayer.class)
            //            ;

            //            if (mediaPlayer != null)
            //            {
            //                mediaPlayer.displayImage(url, mimeType, title, description, iconSrc, listener);
            //                return;
            //            }
            //        }

            //        JsonObject ps = null;

            //        try
            //        {
            //            ps = new JsonObject()
            //            {
            //                {
            //                    put("target", url);
            //put("title",
            //                    title == null ? NULL : title);
            //put("description",
            //                    description == null ? NULL : description);
            //put("mimeType",
            //                    mimeType == null ? NULL : mimeType);
            //put("iconSrc",
            //                    iconSrc == null ? NULL : iconSrc);
            //                }
            //            };
            //        }
            //        catch (Exception ex)
            //        {
            //            ex.printStackTrace();
            //            Util.postError(listener, new ServiceCommandError(-1, ex.getLocalizedMessage(), ex));
            //        }

            //        if (ps != null)
            //            this.displayMedia(ps, listener);
            //    }
            //    else
            //    {
            //        final
            //        String webAppId = "MediaPlayer";

            //        final
            //        WebAppSession.LaunchListener webAppLaunchListener = new WebAppSession.LaunchListener()
            //        {

            //            @Override
            //        public void onError(ServiceCommandError error) {
            //listener.onError(error);
            //        }

            //        @Override
            //        public
            //        void onSuccess 
            //        (WebAppSession
            //        webAppSession)
            //        {
            //            webAppSession.displayImage(url, mimeType, title, description, iconSrc, listener);
            //        }
            //    }
            //        ;

            //        this.getWebAppLauncher().launchWebApp(webAppId, webAppLaunchListener);
            //    }
        }

        public void PlayMedia(String uri, string mimeType, string title, string description, string iconSrv,
            bool shouldLoop, ResponseListener listener)
        {
            //    if ("4.0.0".equalsIgnoreCase(this.serviceDescription.getVersion()))
            //    {
            //        DeviceService dlnaService = this.getDLNAService();

            //        if (dlnaService != null)
            //        {
            //            MediaPlayer mediaPlayer = dlnaService.getAPI(MediaPlayer.class)
            //            ;

            //            if (mediaPlayer != null)
            //            {
            //                mediaPlayer.playMedia(url, mimeType, title, description, iconSrc, shouldLoop, listener);
            //                return;
            //            }
            //        }

            //        JsonObject ps = null;

            //        try
            //        {
            //            ps = new JsonObject()
            //            {
            //                {
            //                    put("target", url);
            //put("title",
            //                    title == null ? NULL : title);
            //put("description",
            //                    description == null ? NULL : description);
            //put("mimeType",
            //                    mimeType == null ? NULL : mimeType);
            //put("iconSrc",
            //                    iconSrc == null ? NULL : iconSrc);
            //put("loop",
            //                    shouldLoop);
            //                }
            //            };
            //        }
            //        catch (Exception ex)
            //        {
            //            ex.printStackTrace();
            //            Util.postError(listener, new ServiceCommandError(-1, ex.getLocalizedMessage(), ex));
            //        }

            //        if (ps != null)
            //            this.displayMedia(ps, listener);
            //    }
            //    else
            //    {
            //        final
            //        String webAppId = "MediaPlayer";

            //        final
            //        WebAppSession.LaunchListener webAppLaunchListener = new WebAppSession.LaunchListener()
            //        {

            //            @Override
            //        public void onError(ServiceCommandError error) {
            //listener.onError(error);
            //        }

            //        @Override
            //        public
            //        void onSuccess 
            //        (WebAppSession
            //        webAppSession)
            //        {
            //            webAppSession.playMedia(url, mimeType, title, description, iconSrc, shouldLoop, listener);
            //        }
            //    }
            //        ;

            //        this.getWebAppLauncher().launchWebApp(webAppId, webAppLaunchListener);
            //    }
        }

        public void CloseMedia(LaunchSession launchSession, ResponseListener listener)
        {
            //JsonObject payload = new JsonObject();

            //try
            //{
            //    if (launchSession.getAppId() != null && launchSession.getAppId().length() > 0)
            //        payload.put("id", launchSession.getAppId());

            //    if (launchSession.getSessionId() != null && launchSession.getSessionId().length() > 0)
            //        payload.put("sessionId", launchSession.getSessionId());
            //}
            //catch (Exception e)
            //{
            //    throw e;
            //}

            //ServiceCommand request =
            //    new ServiceCommand(launchSession.getService(), CLOSE_MEDIA_URI, payload, true,
            //        listener);
            //request.Send();
        }

        public IMouseControl GetMouseControl()
        {
            return this;
        }

        public CapabilityPriorityLevel GetMouseControlCapabilityLevel()
        {
            return CapabilityPriorityLevel.HIGH;
        }

        public void ConnectMouse()
        {
            if (mouseSocket != null)
                return;

            var listener = new ResponseListener();
            listener.Success += (sender, o) =>
            {
                try
                {
                    JsonObject jsonObj = null;
                    var args = o as LoadEventArgs;
                    if (args != null)
                    {
                        jsonObj = args.Load.GetPayload() as JsonObject;
                    }
                    else jsonObj = (JsonObject) o;
                    if (jsonObj != null)
                    {
                        string socketPath = jsonObj.GetNamedString("socketPath");

                        mouseSocket = new WebOstvMouseSocketConnection(socketPath);
                    }
                    mouseSocket.Connect();
                    isMouseConnected = true;
                }
                catch (Exception e)
                {
                    throw e;
                }
            };

            ConnectMouse(listener);
        }


        public void DisconnectMouse()
        {
            if (mouseSocket == null)
                return;

            mouseSocket = null;
            isMouseConnected = false;
        }

        public void Click()
        {
            if (mouseSocket != null)
                mouseSocket.Click();
            else
            {
                ConnectMouse();
            }
        }

        public void Move(double dx, double dy)
        {
            if (mouseSocket != null)
                mouseSocket.Move(dx, dy);
            //else
            //Log.w("Connect SDK", "Mouse Socket is not ready yet");
        }

        public void Move(Point diff)
        {
            Move(diff.X, diff.Y);
        }


        public void Scroll(double dx, double dy)
        {
            if (mouseSocket != null)
                mouseSocket.Scroll(dx, dy);
            //else
            //    Log.w("Connect SDK", "Mouse Socket is not ready yet");
        }

        public void Scroll(Point diff)
        {
            Scroll(diff.X, diff.Y);
        }

        public IPowerControl GetPowerControl()
        {
            return this;
        }

        public CapabilityPriorityLevel GetPowerControlCapabilityLevel()
        {
            return CapabilityPriorityLevel.HIGH;
        }

        public void PowerOff(ResponseListener listener)
        {
            const string uri = "ssap://system/turnOff";
            var request = new ServiceCommand(this, uri, null, null);

            request.Send();
        }

        public void PowerOn(ResponseListener listener)
        {
            if (listener != null)
                listener.OnError(ServiceCommandError.NotSupported());
        }

        public ITextInputControl GetTextInputControl()
        {
            return this;
        }

        public CapabilityPriorityLevel GetTextInputControlCapabilityLevel()
        {
            return CapabilityPriorityLevel.HIGH;
        }

        public IServiceSubscription SubscribeTextInputStatus(ResponseListener listener)
        {
            keyboardInput = new WebOsTvKeyboardInput(this);
            return keyboardInput.connect(listener);
        }

        public void SendText(String input)
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


        /******************
    TOAST CONTROL
    *****************/

        public IToastControl GetToastControl()
        {
            return this;
        }

        public CapabilityPriorityLevel GetToastControlCapabilityLevel()
        {
            return CapabilityPriorityLevel.HIGH;
        }

        public void ShowToast(String message, ResponseListener listener)
        {
            ShowToast(message, null, null, listener);
        }

        public void ShowToast(String message, String iconData, String iconExtension, ResponseListener listener)
        {
            //JsonObject payload = new JsonObject();

            //try
            //{
            //    payload.put("message", message);

            //    if (iconData != null)
            //    {
            //        payload.put("iconData", iconData);
            //        payload.put("iconExtension", iconExtension);
            //    }
            //}
            //catch (Exception e)
            //{
            //    throw e;
            //}

            //sendToast(payload, listener);
        }

        public void ShowClickableToastForApp(String message, AppInfo appInfo, JsonObject ps,
            ResponseListener listener)
        {
            ShowClickableToastForApp(message, appInfo, ps, null, null, listener);
        }

        public void ShowClickableToastForApp(String message, AppInfo appInfo, JsonObject ps, String iconData,
            String iconExtension, ResponseListener listener)
        {
            //JsonObject payload = new JsonObject();

            //try
            //{
            //    payload.put("message", message);

            //    if (iconData != null)
            //    {
            //        payload.put("iconData", iconData);
            //        payload.put("iconExtension", iconExtension);
            //    }

            //    if (appInfo != null)
            //    {
            //        JsonObject onClick = new JsonObject();
            //        onClick.put("appId", appInfo.getId());
            //        if (ps != null)
            //        {
            //            onClick.put("ps", ps);
            //        }
            //        payload.put("onClick", onClick);
            //    }
            //}
            //catch (Exception e)
            //{
            //    throw e;
            //}

            //sendToast(payload, listener);
        }


        public void ShowClickableToastForUrl(String message, String url, ResponseListener listener)
        {
            ShowClickableToastForUrl(message, url, null, null, listener);
        }

        public void ShowClickableToastForUrl(String message, String url, String iconData, String iconExtension,
            ResponseListener listener)
        {
            //JsonObject payload = new JsonObject();

            //try
            //{
            //    payload.put("message", message);

            //    if (iconData != null)
            //    {
            //        payload.put("iconData", iconData);
            //        payload.put("iconExtension", iconExtension);
            //    }

            //    if (url != null)
            //    {
            //        JsonObject onClick = new JsonObject();
            //        onClick.put("target", url);
            //        payload.put("onClick", onClick);
            //    }
            //}
            //catch (Exception e)
            //{
            //    throw e;
            //}

            //sendToast(payload, listener);
        }

        public ITvControl GetTvControl()
        {
            return this;
        }

        public CapabilityPriorityLevel GetTvControlCapabilityLevel()
        {
            return CapabilityPriorityLevel.HIGH;
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
                payload.Add("channelNumber", JsonValue.CreateStringValue(channelInfo.ChannelNumber));
            }
            catch (Exception e)
            {
                throw e;
            }

            var request = new ServiceCommand(this, uri, payload, listener);
            request.Send();
        }

        public void GetCurrentChannel(ResponseListener listener)
        {
            GetCurrentChannel(false, listener);
        }

        public IServiceSubscription SubscribeCurrentChannel(ResponseListener listener)
        {
            return (IServiceSubscription) GetCurrentChannel(true, listener);
        }

        public void GetChannelList(ResponseListener listener)
        {
            GetChannelList(false, listener);
        }

        public void GetProgramInfo(ResponseListener listener)
        {
            // TODO need to parse current program when program id is correct
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

        public IServiceSubscription SubscribeProgramList(ResponseListener listener)
        {
            return (IServiceSubscription) GetProgramList(true, listener);
        }

        public void Set3DEnabled(bool enabled, ResponseListener listener)
        {
            String uri;
            if (enabled)
                uri = "ssap://com.webos.service.tv.display/set3DOn";
            else
                uri = "ssap://com.webos.service.tv.display/set3DOff";

            var request = new ServiceCommand(this, uri, null, listener);

            request.Send();
        }

        public void Get3DEnabled(ResponseListener listener)
        {
            Get3DEnabled(false, listener);
        }

        public IServiceSubscription Subscribe3DEnabled(ResponseListener listener)
        {
            return (IServiceSubscription) Get3DEnabled(true, listener);
        }


        /******************
    VOLUME CONTROL
    *****************/

        public IVolumeControl GetVolumeControl()
        {
            return this;
        }

        public CapabilityPriorityLevel GetVolumeControlCapabilityLevel()
        {
            return CapabilityPriorityLevel.HIGH;
        }

        public void VolumeUp(ResponseListener listener)
        {
            const string uri = "ssap://audio/volumeUp";
            var request = new ServiceCommand(this, uri, null, listener);

            request.Send();
        }

        public void VolumeDown(ResponseListener listener)
        {
            const string uri = "ssap://audio/volumeDown";
            var request = new ServiceCommand(this, uri, null, listener);

            request.Send();
        }

        public void SetVolume(float volume, ResponseListener listener)
        {
            String uri = "ssap://audio/setVolume";
            var payload = new JsonObject();

            try
            {
                payload.Add("volume", JsonValue.CreateNumberValue((volume*100.0f)));
            }
            catch (Exception e)
            {
                throw e;
            }

            var request = new ServiceCommand(this, uri, payload, listener);
            request.Send();
        }

        public void GetVolume(ResponseListener listener)
        {
            GetVolume(false, listener);
        }

        public IServiceSubscription SubscribeVolume(ResponseListener listener)
        {
            return (IServiceSubscription) GetVolume(true, listener);
        }

        public void SetMute(bool isMute, ResponseListener listener)
        {
            const string uri = "ssap://audio/setMute";
            var payload = new JsonObject();

            try
            {
                payload.Add("mute", JsonValue.CreateBooleanValue(isMute));
            }
            catch (Exception e)
            {
                throw e;
            }

            var request = new ServiceCommand(this, uri, payload, listener);
            request.Send();
        }

        public void GetMute(ResponseListener listener)
        {
            getMuteStatus(false, listener);
        }

        public IServiceSubscription SubscribeMute(ResponseListener listener)
        {
            return (IServiceSubscription) getMuteStatus(true, listener);
        }

        public IWebAppLauncher GetWebAppLauncher()
        {
            return this;
        }

        public CapabilityPriorityLevel GetWebAppLauncherCapabilityLevel()
        {
            return CapabilityPriorityLevel.HIGH;
        }

        public void LaunchWebApp(String webAppId, ResponseListener listener)
        {
            LaunchWebApp(webAppId, null, true, listener);
        }

        public void LaunchWebApp(String webAppId, bool relaunchIfRunning, ResponseListener listener)
        {
            LaunchWebApp(webAppId, null, relaunchIfRunning, listener);
        }

        public void LaunchWebApp(String webAppId, JsonObject ps, ResponseListener listener)
        {
            if (string.IsNullOrEmpty(webAppId))
            {
                Util.PostError(listener, new ServiceCommandError(-1, null));

                return;
            }

            WebOsWebAppSession webAppSession1 = mWebAppSessions[webAppId];
            if (webAppSession1 == null) throw new ArgumentNullException("webAppSession1");

            String uri = "ssap://webapp/launchWebApp";
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
                var obj = (JsonObject) o;

                LaunchSession launchSession = null;
                WebOsWebAppSession webAppSession = webAppSession1;

                if (webAppSession != null)
                    launchSession = webAppSession.LaunchSession;
                else
                {
                    launchSession = LaunchSession.LaunchSessionForAppId(webAppId);
                    webAppSession = new WebOsWebAppSession(launchSession, this);
                    mWebAppSessions.Add(webAppId,
                        webAppSession);
                }

                launchSession.Service = this;
                launchSession.SessionId = obj.GetNamedString("sessionId");
                launchSession.SessionType = LaunchSessionType.WebApp;
                launchSession.RawData = obj;

                Util.PostSuccess(listener, webAppSession);
            };
            responseListener.Error += (sender, error) => { Util.PostError(listener, error); };


            var request = new ServiceCommand(this, uri, payload, responseListener);
            request.Send();
        }

        public void LaunchWebApp(String webAppId, JsonObject ps, bool relaunchIfRunning, ResponseListener listener)
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

        public void CloseWebApp(LaunchSession launchSession, ResponseListener listener)
        {
            if (launchSession == null || launchSession.AppId == null || launchSession.AppId.Length == 0)
            {
                Util.PostError(listener, new ServiceCommandError(0, null));
                return;
            }

            WebOsWebAppSession webAppSession = mWebAppSessions[launchSession.AppId];

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

                {
                    var responseListener = new ResponseListener();

                    listener.Success += (sender, o) =>
                    {
                        webAppSession.DisconnectFromWebApp();

                        listener.OnSuccess(o);
                    };
                    listener.Error += (sender, error) =>
                    {
                        webAppSession.DisconnectFromWebApp();

                        listener.OnError(error);
                    };


                    webAppSession.sendMessage(closeCommand, new ResponseListener());
                }
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

        public void JoinWebApp(LaunchSession webAppLaunchSession, ResponseListener listener)
        {
            WebOsWebAppSession webAppSession = WebAppSessionForLaunchSession(webAppLaunchSession);

            webAppSession.Join(new ResponseListener());
        }

        public void JoinWebApp(String webAppId, ResponseListener listener)
        {
            LaunchSession launchSession = LaunchSession.LaunchSessionForAppId(webAppId);
            launchSession.SessionType = LaunchSessionType.WebApp;
            launchSession.Service = this;

            JoinWebApp(launchSession, listener);
        }

        public void GetExternalInputList(ResponseListener listener)
        {
            //        String uri = "ssap://tv/getExternalInputList";

            //        ResponseListener responseListener = new ResponseListener()
            //        {

            //            @Override
            //        public void onSuccess(Object response) {
            //try {
            //JsonObject jsonObj = (JsonObject)response;
            //JSONArray devices = (JSONArray) jsonObj.get("devices");
            //Util.postSuccess(listener,
            //            externalnputInfoFromJSONArray(devices));
            //        } catch
            //        (Exception
            //        e)
            //        {
            //            throw e;
            //        }
            //    }

            //        @Override
            //        public
            //        void onError 
            //        (ServiceCommandError
            //        error)
            //        {
            //            Util.postError(listener, error);
            //        }
            //    }
            //        ;

            //        ServiceCommand request = new ServiceCommand(this, uri,
            //            null, true, responseListener);
            //        request.Send();
        }

        public void stop(ResponseListener listener)
        {
            const string uri = "ssap://media.controls/stop";
            var request = new ServiceCommand(this, uri, null, listener);

            request.Send();
        }

        public void fastForward(ResponseListener listener)
        {
            const string uri = "ssap://media.controls/fastForward";
            var request = new ServiceCommand(this, uri, null, listener);

            request.Send();
        }

        public bool MouseConnected()
        {
            return isMouseConnected;
        }

        public void SetServiceDescription(ServiceDescription serviceDescription)
        {
            base.ServiceDescription = serviceDescription;
            if (ServiceDescription.Version == null && ServiceDescription.ResponseHeaders != null)
            {
                try
                {
                    String serverInfo = serviceDescription.ResponseHeaders[Core.Upnp.Device.HeaderServer][0];
                    String systemOS = serverInfo.Split(' ')[0];
                    String[] versionComponents = systemOS.Split('/');
                    String systemVersion = versionComponents[versionComponents.Length - 1];

                    ServiceDescription.Version = systemVersion;

                    UpdateCapabilities();
                }
                catch
                {
                }
            }
        }

        private DeviceService GetDlnaService()
        {
            Dictionary<String, ConnectableDevice> allDevices = DiscoveryManager.GetInstance().GetAllDevices();
            ConnectableDevice device = null;
            DeviceService service = null;

            if (allDevices != null && allDevices.Count > 0)
                device = allDevices[ServiceDescription.IpAddress];

            if (device != null)
                service = device.GetServiceByName("DLNA");

            return service;
        }

        public static JsonObject DiscoveryParameters()
        {
            var ps = new JsonObject();
            try
            {
                ps.Add("serviceId", JsonValue.CreateStringValue(Id));
                ps.Add("filter", JsonValue.CreateStringValue("urn:lge-com:service:webos-second-screen:1"));
            }
            catch (Exception e)
            {
                throw e;
            }
            return ps;
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
                //this.socket.setListener(mSocketListener);
            }

            if (!IsConnected())
                socket.Connect();
        }


        public override void Disconnect()
        {
            //Log.d("Connect SDK", "attempting to disconnect to " + serviceDescription.getIpAddress());

            if (listener != null)
                listener.OnDisconnect(this, null);

            if (socket != null)
            {
                socket.Listener = null;
                socket.Disconnect();
                socket = null;
            }

            if (mAppToAppIdMappings != null)
                mAppToAppIdMappings.Clear();

            if (mWebAppSessions != null)
            {
                foreach (var pair in mWebAppSessions)
                {
                    pair.Value.DisconnectFromWebApp();
                }

                mWebAppSessions.Clear();
            }
        }


        //    ; = new IWebOSTVServiceSocketClientListener() {

        //    public void onRegistrationFailed(ServiceCommandError error) {
        //        disconnect();

        //        Util.runOnUI(new Runnable() {

        //            @Override
        //            public void run() {
        //                if (listener != null)
        //                    listener.onConnectionFailure(WebOSTVService.this, error);
        //            }
        //        });
        //    }

        //    @Override
        //    public bool onReceiveMessage(JsonObject message) { return true; }

        //    @Override
        //    public void onFailWithError(ServiceCommandError error) {
        //        socket.setListener(null);
        //        socket.disconnect();
        //        socket = null;

        //        Util.runOnUI(new Runnable() {

        //            @Override
        //            public void run() {
        //                if (listener != null)
        //                    listener.onConnectionFailure(WebOSTVService.this, error);
        //            }
        //        });
        //    }

        //    @Override
        //    public void onConnect() {
        //        reportConnected(true);
        //    }

        //    @Override
        //    public void onCloseWithError(ServiceCommandError error) {
        //        socket.setListener(null);
        //        socket.disconnect();
        //        socket = null;

        //        Util.runOnUI(new Runnable() {

        //            @Override
        //            public void run() {
        //                if (listener != null)
        //                    listener.onDisconnect(WebOSTVService.this, error);
        //            }
        //        });
        //    }

        //    @Override
        //    public void onBeforeRegister() {
        //        if ( DiscoveryManager.getInstance().getPairingLevel() == PairingLevel.ON ) {
        //            Util.runOnUI(new Runnable() {

        //                @Override
        //                public void run() {
        //                    if (listener != null)
        //                        listener.onPairingRequired(WebOSTVService.this, pairingType, null);
        //                }
        //            });
        //        }
        //    }
        //};

        public Dictionary<String, String> getWebAppIdMappings()
        {
            return mAppToAppIdMappings;
        }

        private ServiceCommand GetRunningApp(bool isSubscription, ResponseListener listener)
        {
            ServiceCommand request;

            //        ResponseListener responseListener = new ResponseListener()
            //        {

            //            @Override
            //        public void onSuccess(Object response) {
            //JsonObject jsonObj = (JsonObject)response;
            //AppInfo app = new AppInfo() {{
            //setId(jsonObj.optString("appId"));
            //setName(jsonObj.optString("appName"));
            //setRawData(jsonObj);
            //        }
            //    }
            //        ;

            //        Util.postSuccess(listener, app);
            //    }

            //        @Override
            //        public
            //        void onError 
            //        (ServiceCommandError
            //        error)
            //        {
            //            Util.postError(listener, error);
            //        }
            //    }
            //        ;

            //        if (isSubscription)
            //            request = new URLServiceSubscription<ResponseListener>(this, FOREGROUND_APP, null, true, responseListener);
            //        else
            //            request = new ServiceCommand<ResponseListener>(this, FOREGROUND_APP, null, true, responseListener);

            //        request.Send();

            //        return request;
            return null;
        }

        private ServiceCommand GetAppState(bool subscription, LaunchSession launchSession,
            ResponseListener listener)
        {
            //        ServiceCommand<AppStateListener> request;
            //        JsonObject ps = new JsonObject();

            //        try
            //        {
            //            ps.put("appId", launchSession.getAppId());
            //            ps.put("sessionId", launchSession.getSessionId());
            //        }
            //        catch (Exception e)
            //        {
            //            throw e;
            //        }

            //        ResponseListener responseListener = new ResponseListener()
            //        {

            //            @Override
            //        public void onError(ServiceCommandError error) {
            //Util.postError(listener,
            //            error);
            //        }

            //        @Override
            //        public
            //        void onSuccess 
            //        (Object
            //        object )
            //        {
            //            JsonObject json = (JsonObject) object;
            //            try
            //            {
            //                Util.postSuccess(listener, new AppState(json.getbool("running"), json.getbool("visible")));
            //            }
            //            catch (Exception e)
            //            {
            //                Util.postError(listener, new ServiceCommandError(0, "Malformed JsonObject", null));
            //                throw e;
            //            }
            //        }
            //    }
            //        ;

            //        if (subscription)
            //        {
            //            request = new URLServiceSubscription<Launcher.AppStateListener>(this, APP_STATE, ps, true,
            //                responseListener);
            //        }
            //        else
            //        {
            //            request = new ServiceCommand<Launcher.AppStateListener>(this, APP_STATE, ps, true, responseListener);
            //        }

            //        request.Send();

            //        return request;
            return null;
        }

        private void sendToast(JsonObject payload, ResponseListener listener)
        {
            //if (!payload.has("iconData"))
            //{
            //    Context context = DiscoveryManager.getInstance().getContext();

            //    try
            //    {
            //        Drawable drawable = context.getPackageManager().getApplicationIcon(context.getPackageName());

            //        if (drawable != null)
            //        {
            //            BitmapDrawable bitDw = ((BitmapDrawable) drawable);
            //            Bitmap bitmap = bitDw.getBitmap();

            //            ByteArrayOutputStream stream = new ByteArrayOutputStream();
            //            bitmap.compress(Bitmap.CompressFormat.PNG, 100, stream);

            //            byte[] bitmapByte = stream.toByteArray();
            //            bitmapByte = Base64.encode(bitmapByte, Base64.NO_WRAP);
            //            String bitmapData = new String(bitmapByte);

            //            payload.put("iconData", bitmapData);
            //            payload.put("iconExtension", "png");
            //        }
            //    }
            //    catch (NameNotFoundException e)
            //    {
            //        throw e;
            //    }
            //    catch (Exception e)
            //    {
            //        throw e;
            //    }
            //}

            //String uri = "palm://system.notifications/createToast";
            //ServiceCommand request = new ServiceCommand(this, uri,
            //    payload, true, listener);
            //request.Send();
        }

        public void VolumeUp()
        {
            VolumeUp(null);
        }

        public void VolumeDown()
        {
            VolumeDown(null);
        }

        public void SetVolume(int volume)
        {
            SetVolume(volume, null);
        }

        private ServiceCommand GetVolume(bool isSubscription, ResponseListener listener)
        {
            //        ServiceCommand<VolumeListener> request;

            //        ResponseListener responseListener = new ResponseListener()
            //        {

            //            @Override
            //        public void onSuccess(Object response) {

            //try {
            //JsonObject jsonObj = (JsonObject)response;
            //int iVolume = (Integer) jsonObj.get("volume");
            //float fVolume = (float) (iVolume / 100.0);

            //Util.postSuccess(listener,
            //            fVolume);
            //        } catch
            //        (Exception
            //        e)
            //        {
            //            throw e;
            //        }
            //    }

            //        @Override
            //        public
            //        void onError 
            //        (ServiceCommandError
            //        error)
            //        {
            //            Util.postError(listener, error);
            //        }
            //    }
            //        ;

            //        if (isSubscription)
            //            request = new URLServiceSubscription<VolumeListener>(this, VOLUME, null, true, responseListener);
            //        else
            //            request = new ServiceCommand<VolumeListener>(this, VOLUME, null, true, responseListener);

            //        request.Send();

            //        return request;
            return null;
        }

        private ServiceCommand getMuteStatus(bool isSubscription,
            ResponseListener listener)
        {
            //        ServiceCommand request;

            //        ResponseListener responseListener = new ResponseListener()
            //        {

            //            @Override
            //        public void onSuccess(Object response) {
            //try {
            //JsonObject jsonObj = (JsonObject)response;
            //bool isMute = (bool) jsonObj.get("mute");
            //Util.postSuccess(listener,
            //            isMute);
            //        } catch
            //        (Exception
            //        e)
            //        {
            //            throw e;
            //        }
            //    }

            //        @Override
            //        public
            //        void onError 
            //        (ServiceCommandError
            //        error)
            //        {
            //            Util.postError(listener, error);
            //        }
            //    }
            //        ;

            //        if (isSubscription)
            //            request = new URLServiceSubscription<ResponseListener>(this, MUTE, null, true, responseListener);
            //        else
            //            request = new ServiceCommand(this, MUTE, null, true, responseListener);

            //        request.Send();

            //        return request;
            return null;
        }

        private ServiceCommand getVolumeStatus(bool isSubscription, ResponseListener listener)
        {
            ServiceCommand request;

            //        ResponseListener responseListener = new ResponseListener()
            //        {

            //            @Override
            //        public void onSuccess(Object response) {
            //try {
            //JsonObject jsonObj = (JsonObject) response;
            //bool isMute = (bool) jsonObj.get("mute");
            //int iVolume = jsonObj.getInt("volume");
            //float fVolume = (float) (iVolume / 100.0);

            //Util.postSuccess(listener,
            //            new VolumeStatus(isMute, fVolume));
            //        } catch
            //        (Exception
            //        e)
            //        {
            //            throw e;
            //        }
            //    }

            //        @Override
            //        public
            //        void onError 
            //        (ServiceCommandError
            //        error)
            //        {
            //            Util.postError(listener, error);
            //        }
            //    }
            //        ;

            //        if (isSubscription)
            //            request = new URLServiceSubscription<ResponseListener>(this, VOLUME_STATUS, null, true,
            //                responseListener);
            //        else
            //            request = new ServiceCommand(this, VOLUME_STATUS, null, true, responseListener);

            //        request.Send();

            //        return request;
            return null;
        }

        public void getVolumeStatus(ResponseListener listener)
        {
            getVolumeStatus(false, listener);
        }

        public IServiceSubscription subscribeVolumeStatus(ResponseListener listener)
        {
            return (IServiceSubscription) getVolumeStatus(true, listener);
        }


        /******************
    MEDIA PLAYER
    *****************/

        private void displayMedia(JsonObject ps, ResponseListener listener)
        {
            //        String uri = "ssap://media.viewer/open";

            //        ResponseListener responseListener = new ResponseListener()
            //        {
            //            @Override
            //        public void onSuccess(Object response) {
            //JsonObject obj = (JsonObject) response;

            //LaunchSession launchSession = LaunchSession.launchSessionForAppId(obj.optString("id"));
            //launchSession.setService(WebOSTVService.this);
            //launchSession.setSessionId(obj.optString("sessionId"));
            //launchSession.setSessionType(LaunchSessionType.Media);

            //Util.postSuccess(listener,
            //            new MediaLaunchObject(launchSession, WebOSTVService.this));
            //        }

            //        @Override
            //        public
            //        void onError 
            //        (ServiceCommandError
            //        error)
            //        {
            //            Util.postError(listener, error);
            //        }
            //    }
            //        ;

            //        ServiceCommand request = new ServiceCommand(this, uri,
            //            ps, true, responseListener);
            //        request.Send();
        }

        public void ChannelUp()
        {
            ChannelDown(null);
        }

        public void ChannelDown()
        {
            ChannelDown(null);
        }

        public void setChannelById(String channelId)
        {
            setChannelById(channelId, null);
        }

        public void setChannelById(String channelId, ResponseListener listener)
        {
            String uri = "ssap://tv/openChannel";
            var payload = new JsonObject();

            try
            {
                payload.Add("channelId", JsonValue.CreateStringValue(channelId));
            }
            catch (Exception e)
            {
                throw e;
            }

            var request = new ServiceCommand(this, uri, payload, listener);
            request.Send();
        }

        private ServiceCommand GetCurrentChannel(bool isSubscription,
            ResponseListener listener)
        {
            //        ServiceCommand request;

            //        ResponseListener responseListener = new ResponseListener()
            //        {

            //            @Override
            //        public void onSuccess(Object response) {
            //JsonObject jsonObj = (JsonObject) response;
            //ChannelInfo channel = parseRawChannelData(jsonObj);

            //Util.postSuccess(listener,
            //            channel);
            //        }

            //        @Override
            //        public
            //        void onError 
            //        (ServiceCommandError
            //        error)
            //        {
            //            Util.postError(listener, error);
            //        }
            //    }
            //        ;

            //        if (isSubscription)
            //        {
            //            request = new URLServiceSubscription<ResponseListener>(this, CHANNEL, null, true,
            //                responseListener);
            //        }
            //        else
            //            request = new ServiceCommand(this, CHANNEL, null, true, responseListener);

            //        request.Send();

            //        return request;
            return null;
        }

        private ServiceCommand GetChannelList(bool isSubscription, ResponseListener listener)
        {
            //        ServiceCommand request;

            //        ResponseListener responseListener = new ResponseListener()
            //        {

            //            @Override
            //        public void onSuccess(Object response) {
            //try {
            //JsonObject jsonObj = (JsonObject)response;
            //ArrayList<ChannelInfo> list = new ArrayList<ChannelInfo>();

            //JSONArray array = (JSONArray) jsonObj.get("channelList");
            //for (int i = 0; i < array.length(); i++) {
            //JsonObject object = (JsonObject) array.get(i);

            //ChannelInfo channel = parseRawChannelData(object);
            //list.add(channel);
            //        }

            //        Util.postSuccess(listener, list);
            //    }
            //    catch
            //        (Exception
            //        e)
            //        {
            //            throw e;
            //        }
            //    }

            //        @Override
            //        public
            //        void onError 
            //        (ServiceCommandError
            //        error)
            //        {
            //            Util.postError(listener, error);
            //        }
            //    }
            //        ;

            //        if (isSubscription)
            //            request = new URLServiceSubscription<ResponseListener>(this, CHANNEL_LIST, null, true,
            //                responseListener);
            //        else
            //            request = new ServiceCommand(this, CHANNEL_LIST, null, true, responseListener);

            //        request.Send();

            //        return request;
            return null;
        }


        public IServiceSubscription subscribeChannelList(ResponseListener listener)
        {
            return (IServiceSubscription) GetChannelList(true, listener);
        }

        private ServiceCommand GetProgramList(bool isSubscription, ResponseListener listener)
        {
            //        ServiceCommand request;

            //        ResponseListener responseListener = new ResponseListener()
            //        {

            //            @Override
            //        public void onSuccess(Object response) {
            //try {
            //JsonObject jsonObj = (JsonObject)response;
            //JsonObject jsonChannel = (JsonObject) jsonObj.get("channel");
            //ChannelInfo channel = parseRawChannelData(jsonChannel);
            //JSONArray programList = (JSONArray) jsonObj.get("programList");

            //Util.postSuccess(listener,
            //            new ProgramList(channel, programList));
            //        } catch
            //        (Exception
            //        e)
            //        {
            //            throw e;
            //        }
            //    }

            //        @Override
            //        public
            //        void onError 
            //        (ServiceCommandError
            //        error)
            //        {
            //            Util.postError(listener, error);
            //        }
            //    }
            //        ;

            //        if (isSubscription)
            //            request = new URLServiceSubscription<ResponseListener>(this, PROGRAM, null, true,
            //                responseListener);
            //        else
            //            request = new ServiceCommand(this, PROGRAM, null, true, responseListener);

            //        request.Send();

            //        return request;
            return null;
        }


        private ServiceCommand Get3DEnabled(bool isSubscription, ResponseListener listener)
        {
            //        String uri = "ssap://com.webos.service.tv.display/get3DStatus";

            //        ResponseListener responseListener = new ResponseListener()
            //        {

            //            @Override
            //        public void onSuccess(Object response) {
            //JsonObject jsonObj = (JsonObject)response;

            //JsonObject status;
            //try {
            //status = jsonObj.getJsonObject("status3D");
            //bool isEnabled = status.getbool("status");

            //Util.postSuccess(listener,
            //            isEnabled);
            //        } catch
            //        (Exception
            //        e)
            //        {
            //            throw e;
            //        }
            //    }

            //        @Override
            //        public
            //        void onError 
            //        (ServiceCommandError
            //        error)
            //        {
            //            Util.postError(listener, error);
            //        }
            //    }
            //        ;

            //        ServiceCommand<State3DModeListener> request;
            //        if (isSubscription == true)
            //            request = new URLServiceSubscription<State3DModeListener>(this, uri, null, true, responseListener);
            //        else
            //            request = new ServiceCommand<State3DModeListener>(this, uri, null, true, responseListener);

            //        request.Send();

            //        return request;
            return null;
        }

        private void ConnectMouse(ResponseListener listener)
        {
            String uri = "ssap://com.webos.service.networkinput/getPointerInputSocket";

            var request = new ServiceCommand(this, uri, null, listener);
            request.Send();
        }

        private void SendSpecialKey(String key)
        {
            if (mouseSocket != null)
            {
                mouseSocket.Button(key);
            }
            else
            {
                var listener = new ResponseListener();
                listener.Success += (sender, o) =>
                {
                    try
                    {
                        JsonObject jsonObj = null;
                        if (o is LoadEventArgs)
                        {
                            jsonObj = ((LoadEventArgs) o).Load.GetPayload() as JsonObject;
                        }
                        else jsonObj = (JsonObject) o;
                        String socketPath = jsonObj.GetNamedString("socketPath");
                        mouseSocket = new WebOstvMouseSocketConnection(socketPath);

                        mouseSocket.Button(key);
                    }
                    catch
                        (Exception
                            e)
                    {
                        throw e;
                    }
                };
                listener.Error += (sender, o) => { };

                ConnectMouse(listener);
            }
        }

        public void connectToWebApp(WebOsWebAppSession webAppSession, bool joinOnly, ResponseListener connectionListener)
        {
            //{
            //    if (mWebAppSessions == null)
            //        mWebAppSessions = new ConcurrentHashMap<String, WebOsWebAppSession>();

            //    if (mAppToAppIdMappings == null)
            //        mAppToAppIdMappings = new ConcurrentHashMap<String, String>();

            //    if (webAppSession == null || webAppSession.launchSession == null)
            //    {
            //        Util.postError(connectionListener,
            //            new ServiceCommandError(0, "You must provide a valid LaunchSession object", null));

            //        return;
            //    }

            //    String _appId = webAppSession.launchSession.getAppId();
            //    String _idKey = null;

            //    if (webAppSession.launchSession.getSessionType() == LaunchSession.LaunchSessionType.WebApp)
            //        _idKey = "webAppId";
            //    else
            //        _idKey = "appId";

            //    if (_appId == null || _appId.length() == 0)
            //    {
            //        Util.postError(connectionListener,
            //            new ServiceCommandError(-1, "You must provide a valid web app session", null));

            //        return;
            //    }

            //    final
            //    String appId = _appId;
            //    final
            //    String idKey = _idKey;

            //    String uri = "ssap://webapp/connectToApp";
            //    JsonObject payload = new JsonObject();

            //    try
            //    {
            //        payload.put(idKey, appId);
            //    }
            //    catch (Exception e)
            //    {
            //        throw e;
            //    }

            //    ResponseListener responseListener = new ResponseListener()
            //    {

            //        @Override
            //    public void onSuccess(Object response) {
            //JsonObject jsonObj = (JsonObject)response;

            //String state = jsonObj.optString("state");

            //if (!state.equalsIgnoreCase("CONNECTED")) {
            //if (joinOnly && state.equalsIgnoreCase("WAITING_FOR_APP")) {
            //Util.postError(connectionListener,
            //        new ServiceCommandError(0, "Web app is not currently running", null));
            //    }

            //    return;
            //}

            //    String fullAppId = jsonObj.optString("appId");

            //    if (fullAppId != null && fullAppId.length() != 0)
            //    {
            //        if (webAppSession.launchSession.getSessionType() == LaunchSessionType.WebApp)
            //            mAppToAppIdMappings.put(fullAppId, appId);

            //        webAppSession.setFullAppId(fullAppId);
            //    }

            //    if (connectionListener != null)
            //    {
            //        Util.runOnUI(new Runnable()
            //        {

            //            @Override
            //        public void run() {
            //connectionListener.onSuccess(response);
            //        }
            //    }
            //    )
            //        ;
            //    }
            //}

            //    @Override
            //    public
            //    void onError 
            //    (ServiceCommandError
            //    error)
            //    {
            //        webAppSession.disconnectFromWebApp();

            //        bool appChannelDidClose = false;

            //        if (error != null && error.getPayload() != null)
            //            appChannelDidClose = error.getPayload().toString().contains("app channel closed");

            //        if (appChannelDidClose)
            //        {
            //            if (webAppSession != null && webAppSession.getWebAppSessionListener() != null)
            //            {
            //                Util.runOnUI(new Runnable()
            //                {

            //                    @Override
            //                public void run() {
            //webAppSession.getWebAppSessionListener().onWebAppSessionDisconnect(webAppSession);
            //                }
            //            }
            //            )
            //                ;
            //            }
            //        }
            //        else
            //        {
            //            Util.postError(connectionListener, error);
            //        }
            //    }
            //}
            //    ;

            //    webAppSession.appToAppSubscription = new URLServiceSubscription<ResponseListener>(webAppSession.socket,
            //        uri, payload, true, responseListener);
            //    webAppSession.appToAppSubscription.subscribe();
        }

        /* Join a native/installed webOS app */

        public void JoinApp(String appId, ResponseListener listener)
        {
            LaunchSession launchSession = LaunchSession.LaunchSessionForAppId(appId);
            launchSession.SessionType = LaunchSessionType.App;
            launchSession.Service = this;

            JoinWebApp(launchSession, listener);
        }

        /* Connect to a native/installed webOS app */

        public void ConnectToApp(String appId, ResponseListener listener)
        {
            LaunchSession launchSession = LaunchSession.LaunchSessionForAppId(appId);
            launchSession.SessionType = LaunchSessionType.App;
            launchSession.Service = this;

            WebOsWebAppSession webAppSession = WebAppSessionForLaunchSession(launchSession);

            connectToWebApp(webAppSession, false, new ResponseListener());
        }

        private WebOsWebAppSession WebAppSessionForLaunchSession(LaunchSession launchSession)
        {
            if (mWebAppSessions == null)
                mWebAppSessions = new Dictionary<String, WebOsWebAppSession>();

            if (launchSession.Service == null)
                launchSession.Service = this;

            WebOsWebAppSession webAppSession = mWebAppSessions[launchSession.AppId];

            if (webAppSession == null)
            {
                webAppSession = new WebOsWebAppSession(launchSession, this);
                mWebAppSessions.Add(launchSession.AppId, webAppSession);
            }

            return webAppSession;
        }

        private void SendMessage(Object message, LaunchSession launchSession, ResponseListener listener)
        {
            if (launchSession == null || launchSession.AppId == null)
            {
                Util.PostError(listener, new ServiceCommandError(0, null));
                return;
            }

            if (message == null)
            {
                Util.PostError(listener, new ServiceCommandError(0, null));
                return;
            }

            if (socket == null)
            {
                Connect();
            }

            String appId = launchSession.AppId;
            String fullAppId = appId;

            if (launchSession.SessionType == LaunchSessionType.WebApp)
                fullAppId = mAppToAppIdMappings[appId];

            if (fullAppId == null || fullAppId.Length == 0)
            {
                if (listener != null)
                    Util.PostError(listener,
                        new ServiceCommandError(-1, null));

                return;
            }

            var payload = new JsonObject();

            try
            {
                payload.Add("type", JsonValue.CreateStringValue("p2p"));
                payload.Add("to", JsonValue.CreateStringValue(fullAppId));
                payload.Add("payload", JsonValue.CreateStringValue(message as string));

                Object payTest = payload.GetNamedObject("payload");
            }
            catch (Exception e)
            {
                throw e;
            }

            var request = new ServiceCommand(this, null, payload, listener);
            SendCommand(request);
        }

        public void SendMessage(String message, LaunchSession launchSession, ResponseListener listener)
        {
            if (!string.IsNullOrEmpty(message))
            {
                SendMessage((Object) message, launchSession, listener);
            }
            else
            {
                Util.PostError(listener, new ServiceCommandError(0, null));
            }
        }

        public void SendMessage(JsonObject message, LaunchSession launchSession, ResponseListener listener)
        {
            if (message != null && message.Keys.Count > 0)
                SendMessage((Object) message, launchSession, listener);
            else
                Util.PostError(listener, new ServiceCommandError(0, null));
        }


        /**************
    SYSTEM CONTROL
    **************/

        public void GetServiceInfo(ResponseListener listener)
        {
            const string uri = "ssap://api/getServiceList";

            var responseListener = new ResponseListener();

            responseListener.Success += (sender, o) =>
            {
                try
                {
                    var jsonObj = (JsonObject) o;
                    JsonArray services = jsonObj.GetNamedArray("services");
                    Util.PostSuccess(listener, services);
                }
                catch (Exception
                    e)
                {
                    throw e;
                }
            };
            responseListener.Error += (sender, error) => { Util.PostError(listener, error); };
            var request = new ServiceCommand(this, uri, null, responseListener);


            request.Send();
        }

        public void GetSystemInfo(ResponseListener listener)
        {
            const string uri = "ssap://system/getSystemInfo";

            var responseListener = new ResponseListener();

            responseListener.Success += (sender, o) =>
            {
                try
                {
                    var jsonObj = (JsonObject) o;
                    JsonObject features = jsonObj.GetNamedObject("features");
                    Util.PostSuccess(listener,
                        features);
                }
                catch (Exception e)
                {
                    throw e;
                }
            };
            responseListener.Error += (sender, error) => Util.PostError(listener, error);

            var request = new ServiceCommand(this, uri, null, responseListener);
            request.Send();
        }

        public void SecureAccessTest(SecureAccessTestListener listener)
        {
            const string uri = "ssap://com.webos.service.secondscreen.gateway/test/secure";

            var responseListener = new ResponseListener();

            responseListener.Success += (sender, o) =>
            {
                try
                {
                    var jsonObj = (JsonObject) o;
                    bool isSecure = jsonObj.GetNamedBoolean("returnValue");
                    Util.PostSuccess(listener,
                        isSecure);
                }
                catch (Exception e)
                {
                    throw e;
                }
            };
            responseListener.Error += (sender, error) => Util.PostError(listener, error);

            var request = new ServiceCommand(this, uri, null, responseListener);
            request.Send();
        }

        public void GetAcrAuthToken(AcrAuthTokenListener listener)
        {
            const string uri = "ssap://tv/getACRAuthToken";

            var responseListener = new ResponseListener();

            responseListener.Success += (sender, o) =>
            {
                try
                {
                    var jsonObj = (JsonObject) o;
                    string authToken = jsonObj.GetNamedString("token");
                    Util.PostSuccess(listener, authToken);
                }
                catch (Exception e)
                {
                    throw e;
                }
            };
            responseListener.Error += (sender, error) => Util.PostError(listener, error);

            var request = new ServiceCommand(this, uri, null, responseListener);
            request.Send();
        }

        public void GetLaunchPoints(LaunchPointsListener listener)
        {
            //        String uri = "ssap://com.webos.applicationManager/listLaunchPoints";

            //        ResponseListener responseListener = new ResponseListener()
            //        {

            //            @Override
            //        public void onSuccess(Object response) {

            //try {
            //JsonObject jsonObj = (JsonObject) response;
            //JSONArray launchPoints = (JSONArray) jsonObj.get("launchPoints");
            //Util.postSuccess(listener,
            //            launchPoints);
            //        } catch
            //        (Exception
            //        e)
            //        {
            //            throw e;
            //        }
            //    }

            //        @Override
            //        public
            //        void onError 
            //        (ServiceCommandError
            //        error)
            //        {
            //            Util.postError(listener, error);
            //        }
            //    }
            //        ;

            //        ServiceCommand request = new ServiceCommand(this, uri,
            //            null, true, responseListener);
            //        request.Send();
        }

        public override void SendCommand(ServiceCommand command)
        {
            if (socket != null)
                socket.SendCommand(command);
        }

        public override void Unsubscribe(UrlServiceSubscription subscription)
        {
            if (socket != null)
                socket.Unsubscribe(subscription);
        }

        protected void UpdateCapabilities()
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
                foreach (String capability in VolumeControl.Capabilities)
                {
                    capabilities.Add(capability);
                }
                foreach (String capability in MediaPlayer.Capabilities)
                {
                    capabilities.Add(capability);
                }

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
                if (ServiceDescription.Version.Contains("4.0.0") ||
                    ServiceDescription.Version.Contains("4.0.1"))
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
                    foreach (String capability in WebAppLauncher.Capabilities)
                    {
                        capabilities.Add(capability);
                    }
                    foreach (String capability in MediaControl.Capabilities)
                    {
                        capabilities.Add(capability);
                    }
                }
            }

            SetCapabilities(capabilities);
        }

        public List<String> GetPermissions()
        {
            if (permissions != null)
                return permissions;

            var defaultPermissions = new List<String>();
            foreach (String perm in KWebOstvServiceOpenPermissions)
            {
                defaultPermissions.Add(perm);
            }

            if (DiscoveryManager.GetInstance().PairingLevel == DiscoveryManager.PairingLevelEnum.On)
            {
                foreach (String perm in KWebOstvServiceProtectedPermissions)
                {
                    defaultPermissions.Add(perm);
                }

                foreach (String perm in KWebOstvServicePersonalActivityPermissions)
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

            var config = (WebOSTVServiceConfig) serviceConfig;

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

        private ChannelInfo ParseRawChannelData(JsonObject channelRawData)
        {
            String channelName = null;
            String channelId = null;
            String channelNumber = null;
            int minorNumber;
            int majorNumber;

            var channelInfo = new ChannelInfo();
            channelInfo.RawData = channelRawData;

            try
            {
                if (channelRawData.ContainsKey("channelName"))
                    channelName = channelRawData.GetNamedString("channelName");

                if (channelRawData.ContainsKey("channelId"))
                    channelId = channelRawData.GetNamedString("channelId");

                channelNumber = channelRawData.GetNamedString("channelNumber");

                if (channelRawData.ContainsKey("majorNumber"))
                    majorNumber = (int) channelRawData.GetNamedNumber("majorNumber");
                else
                    majorNumber = ParseMajorNumber(channelNumber);

                if (channelRawData.ContainsKey("minorNumber"))
                    minorNumber = (int) channelRawData.GetNamedNumber("minorNumber");
                else
                    minorNumber = ParseMinorNumber(channelNumber);

                channelInfo.ChannelName = channelName;
                channelInfo.ChannelId = channelId;
                channelInfo.ChannelNumber = channelNumber;
                channelInfo.MajorNumber = majorNumber;
                channelInfo.MinorNumber = minorNumber;
            }
            catch (Exception e)
            {
                throw e;
            }

            return channelInfo;
        }

        private static int ParseMinorNumber(String channelNumber)
        {
            if (channelNumber != null)
            {
                String[] tokens = channelNumber.Split('-');
                return int.Parse(tokens[tokens.Length - 1]);
            }
            return 0;
        }

        private static int ParseMajorNumber(String channelNumber)
        {
            if (channelNumber != null)
            {
                String[] tokens = channelNumber.Split('-');
                return int.Parse(tokens[0]);
            }
            return 0;
        }

        private List<ExternalInputInfo> ExternalnputInfoFromJsonArray(JsonArray inputList)
        {
            var externalInputInfoList = new List<ExternalInputInfo>();

            for (int i = 0; i < inputList.Count; i++)
            {
                try
                {
                    var input = (JsonObject) inputList[i];

                    String id = input.GetNamedString("id");
                    String name = input.GetNamedString("label");
                    bool connected = input.GetNamedBoolean("connected");
                    String iconURL = input.GetNamedString("icon");

                    var inputInfo = new ExternalInputInfo();
                    inputInfo.RawData = input;
                    inputInfo.Id = id;
                    inputInfo.Name = name;
                    inputInfo.Connected = connected;
                    inputInfo.IconUrl = iconURL;

                    externalInputInfoList.Add(inputInfo);
                }
                catch (Exception e)
                {
                    throw e;
                }
            }

            return externalInputInfoList;
        }

        public bool IsConnectable()
        {
            return true;
        }

        public override void SendPairingKey(String pairingKey)
        {
        }

        public class AcrAuthTokenListener : ResponseListener
        {
        }

        public class LaunchPointsListener : ResponseListener
        {
        }

        public class SecureAccessTestListener : ResponseListener
        {
        }
    }
}