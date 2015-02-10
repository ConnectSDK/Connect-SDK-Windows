using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Windows.Data.Json;
using Windows.Foundation;
using ConnectSdk.Windows.Core;
using ConnectSdk.Windows.Device.Netcast;
using ConnectSdk.Windows.Discovery;
using ConnectSdk.Windows.Etc.Helper;
using ConnectSdk.Windows.Service.Capability;
using ConnectSdk.Windows.Service.Capability.Listeners;
using ConnectSdk.Windows.Service.Command;
using ConnectSdk.Windows.Service.Config;
using ConnectSdk.Windows.Service.Sessions;

namespace ConnectSdk.Windows.Service
{
    public class NetcastTvService : DeviceService, ILauncher, IMediaControl, IMediaPlayer, ITvControl, IVolumeControl,
        IExternalInputControl, IMouseControl, ITextInputControl, IPowerControl, IKeyControl
    {

        public static string Id = "Netcast TV";

        public static string UdapPathPairing = "/udap/api/pairing";
        public static string UdapPathData = "/udap/api/data";
        public static string UdapPathCommand = "/udap/api/command";
        public static string UdapPathEvent = "/udap/api/event";

        public static string UdapPathApptoappData = "/udap/api/apptoapp/data/";
        public static string UdapPathApptoappCommand = "/udap/api/apptoapp/command/";
        public static string RoapPathAppStore = "/roap/api/command/";

        public static string UdapApiPairing = "pairing";
        public static string UdapApiCommand = "command";
        public static string UdapApiEvent = "event";

        public static string TargetChannelList = "channel_list";
        public static string TargetCurrentChannel = "cur_channel";
        public static string TargetVolumeInfo = "volume_info";
        public static string TargetApplistGet = "applist_get";
        public static string TargetAppnumGet = "appnum_get";
        public static string Target_3DMode = "3DMode";
        public static string TargetIs_3D = "is_3D";

        public enum State
        {
            NONE,
            INITIAL,
            CONNECTING,
            PAIRING,
            PAIRED,
            DISCONNECTING
        };

        private readonly HttpClient httpClient;
        //private NetcastHttpServer httpServer;

        private readonly DlnaService dlnaService;

        private LaunchSession inputPickerSession;

        private readonly List<AppInfo> applications;
        private readonly List<IServiceSubscription> subscriptions;
        private StringBuilder keyboardstring;

        private State state = State.INITIAL;

        private Point mMouseDistance;
        private bool mMouseIsMoving;

        public State ServiceState
        {
            get { return state; }
            set { state = value; }
        }

        private string GetUdapRequestUrl(string path, string target = null, string type = null, string index = null, string number = null)
        {
            // Type Values
            // 1: List of all apps
            // 2: List of apps in the Premium category
            // 3: List of apps in the My Apps category

            var sb = new StringBuilder();
            sb.Append("http://");
            sb.Append(ServiceDescription.IpAddress);
            sb.Append(":");
            sb.Append(ServiceDescription.Port);
            sb.Append(path);

            if (target == null) return sb.ToString();

            sb.Append("?target=");
            sb.Append(target);

            if (type != null)
            {
                sb.Append("&type=");
                sb.Append(type);
            }

            if (index != null)
            {
                sb.Append("&index=");
                sb.Append(index);
            }

            if (number == null) return sb.ToString();

            sb.Append("&number=");
            sb.Append(number);

            return sb.ToString();
        }

        private string GetUdapMessageBody(string api, Dictionary<string, string> ps)
        {
            var sb = new StringBuilder();
            sb.Append("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
            sb.Append("<envelope>");
            sb.Append("<api type=\"" + api + "\">");

            foreach (var entry in ps)
            {
                var key = entry.Key;
                var value = entry.Value;

                sb.Append(CreateNode(key, value));
            }

            sb.Append("</api>");
            sb.Append("</envelope>");

            return sb.ToString();
        }

        private static string CreateNode(string tag, string value)
        {
            var sb = new StringBuilder();

            sb.Append("<" + tag + ">");
            sb.Append(value);
            sb.Append("</" + tag + ">");

            return sb.ToString();
        }

        public string DecToHex(string dec)
        {
            if (!string.IsNullOrEmpty(dec))
                return DecToHex(long.Parse(dec));
            return null;
        }

        public string DecToHex(long dec)
        {
            return dec.ToString("X");
        }

        public NetcastTvService(ServiceDescription serviceDescription, ServiceConfig serviceConfig) :
            base(serviceDescription, serviceConfig)
        {
            dlnaService = new DlnaService(serviceDescription, serviceConfig);

            if (serviceDescription != null && serviceDescription.Port != 8080)
                serviceDescription.Port = 8080;

            applications = new List<AppInfo>();
            subscriptions = new List<IServiceSubscription>();

            keyboardstring = new StringBuilder();

            httpClient = new HttpClient();
            inputPickerSession = null;

            var mTextChangedListener = new ResponseListener();

            mTextChangedListener.Success += (sender, o) =>
            {
                keyboardstring = new StringBuilder((string)o);
            };

            mTextChangedListener.Error += (sender, o) =>
            {
            };
        }

        public new static JsonObject DiscoveryParameters()
        {
            var ps = new JsonObject();

            try
            {
                ps.Add("serviceId", JsonValue.CreateStringValue(Id));
                ps.Add("filter", JsonValue.CreateStringValue("udap:rootservice"));
            }
            catch (Exception e)
            {

            }

            return ps;
        }


        public override void SetServiceDescription(ServiceDescription serviceDescriptionParam)
        {
            ServiceDescription = serviceDescriptionParam;
            if (dlnaService != null)
                dlnaService.SetServiceDescription(serviceDescriptionParam);
            serviceDescriptionParam.Port = 8080;
        }


        public override void Connect()
        {
            if (ServiceState != State.INITIAL)
            {
                return; // don't try to connect again while connected
            }

            if (!(ServiceConfig is NetcastTvServiceConfig))
            {
                ServiceConfig = new NetcastTvServiceConfig(ServiceConfig.ServiceUuid);
            }

            if (DiscoveryManager.GetInstance().GetPairingLevel() != DiscoveryManager.PairingLevel.ON) return;

            if (!string.IsNullOrEmpty(((NetcastTvServiceConfig)ServiceConfig).PairingKey))
            {
                SendPairingKey(((NetcastTvServiceConfig)ServiceConfig).PairingKey);
            }
            else
            {
                ShowPairingKeyOnTv();
            }
        }

        public override void Disconnect()
        {
            EndPairing(null);

            connected = false;

            if (mServiceReachability != null)
                mServiceReachability.Stop();

            if (Listener != null)
                Listener.OnDisconnect(this, null);

            ServiceState = State.INITIAL;
        }

        public override bool IsConnectable()
        {
            return true;
        }

        public override bool IsConnected()
        {
            return connected;
        }

        private void ConnectSuccess()
        {
            //  TODO:  Fix this for Netcast.  Right now it is using the InetAddress reachable function.  Need to use an HTTP Method.
            //		mServiceReachability = DeviceServiceReachability.getReachability(serviceDescription.getIpAddress(), this);
            //		mServiceReachability.start();

            connected = true;

            // Pairing was successful, so report connected and ready
            ReportConnected(true);
        }

        public override void OnLoseReachability(DeviceServiceReachability reachability)
        {
            if (connected)
            {
                Disconnect();
            }
            else
            {
                if (mServiceReachability != null)
                    mServiceReachability.Stop();
            }
        }

        public void HostByeBye()
        {
            Disconnect();
        }

        //============= Auth ==============================
        public void ShowPairingKeyOnTv()
        {
            ServiceState = State.CONNECTING;

            var responseListener = new ResponseListener();
            responseListener.Success += (sender, args) =>
            {
                if (Listener != null)
                    Listener.OnPairingRequired(this, PairingType.PIN_CODE, null);
            };
            responseListener.Error += (sender, error) =>
            {
                ServiceState = State.INITIAL;

                if (Listener != null)
                    Listener.OnConnectionFailure(this, new Exception(error.ToString()));
            };

            var requestUrl = GetUdapRequestUrl(UdapPathPairing);

            var ps = new Dictionary<string, string> { { "name", "showKey" } };

            string httpMessage = GetUdapMessageBody(UdapApiPairing, ps);

            var command = new ServiceCommand(this, requestUrl,
                httpMessage, responseListener);
            command.Send();
        }

        // TODO add this when user cancel pairing
        public void RemovePairingKeyOnTv()
        {
            var responseListener = new ResponseListener();
            responseListener.Success += (sender, args) => { };
            responseListener.Error += (sender, args) => { };

            var requestUrl = GetUdapRequestUrl(UdapPathPairing);

            var ps = new Dictionary<string, string> { { "name", "CancelAuthKeyReq" } };

            string httpMessage = GetUdapMessageBody(UdapApiPairing, ps);

            var command = new ServiceCommand(this, requestUrl, httpMessage, responseListener);
            command.Send();
        }

        public override void SendPairingKey(string pairingKey)
        {
            ServiceState = State.PAIRING;

            if (!(ServiceConfig is NetcastTvServiceConfig))
            {
                ServiceConfig = new NetcastTvServiceConfig(ServiceConfig.ServiceUuid);
            }

            var responseListener = new ResponseListener();

            responseListener.Success += (sender, args) =>
            {
                ServiceState = State.PAIRED;
                ((NetcastTvServiceConfig)ServiceConfig).PairingKey = pairingKey;
                ConnectSuccess();
            };
            responseListener.Error += (sender, args) =>
            {
                ServiceState = State.INITIAL;

                if (Listener != null)
                    Listener.OnConnectionFailure(this, new Exception(args.ToString()));
            };

            var requestUrl = GetUdapRequestUrl(UdapPathPairing);

            var ps = new Dictionary<string, string>
            {
                {"name", "hello"},
                {"value", pairingKey},
                {"port", ServiceDescription.Port.ToString()}
            };

            var httpMessage = GetUdapMessageBody(UdapApiPairing, ps);

            var command = new ServiceCommand(this, requestUrl,
                httpMessage, responseListener);
            command.Send();
        }

        private void EndPairing(ResponseListener listener)
        {
            var requestUrl = GetUdapRequestUrl(UdapPathPairing);

            var ps = new Dictionary<string, string>
            {
                {"name", "byebye"},
                {"port", ServiceDescription.Port.ToString()}
            };

            var httpMessage = GetUdapMessageBody(UdapApiPairing, ps);

            var command = new ServiceCommand(this, requestUrl,
                httpMessage, listener);
            command.Send();
        }


        /// <summary>
        /// Launcher
        /// </summary>
        /// <returns></returns>
        public ILauncher GetLauncher()
        {
            return this;
        }


        public CapabilityPriorityLevel GetLauncherCapabilityLevel()
        {
            return CapabilityPriorityLevel.HIGH;
        }

        public void GetApplication(string appName, ResponseListener listener)
        {
            var responseListener = new ResponseListener();
            responseListener.Success += (sender, args) =>
            {
                //string strObj = ((string) args);

                //AppInfo appId = new AppInfo(decToHex(strObj));

                //if (appId != null)
                //{
                //    Util.PostSuccess(listener, appId);
                //}

            };
            responseListener.Error += (sender, args) =>
            {
                if (listener != null)
                    Util.PostError(listener, args);
            };



            var uri = UdapPathApptoappData + appName;
            var requestUrl = GetUdapRequestUrl(uri);

            var command = new ServiceCommand(this, requestUrl, null, responseListener) { HttpMethod = ServiceCommand.TypeGet };
            command.Send();
        }

        public void LaunchApp(string appId, ResponseListener listener)
        {
            var appInfoListener = new ResponseListener();

            appInfoListener.Success += (sender, args) => LaunchAppWithInfo((AppInfo)args, listener);
            appInfoListener.Error += (sender, args) => Util.PostError(listener, args);

            GetAppInfoForId(appId, appInfoListener);
        }

        private void GetAppInfoForId(string appId, ResponseListener listener)
        {
            var appListListener = new ResponseListener();
            appListListener.Success += (sender, args) =>
            {
                var appInfos = args as List<AppInfo>;
                if (appInfos != null)
                    foreach (var info in appInfos)
                    {
                        if (info.Name.Equals(appId))
                        {
                            Util.PostSuccess(listener, info);
                        }
                        return;
                    }

                Util.PostError(listener, new ServiceCommandError(0, "Unable to find the App with id", null));
            };

            appListListener.Error += (sender, args) =>
            {
                Util.PostError(listener, args);
            };

            GetAppList(appListListener);
        }

        private void LaunchApplication(string appName, string auid, string contentId, ResponseListener listener)
        {
            var jsonObj = new JsonObject();

            try
            {
                jsonObj.Add("id", JsonValue.CreateStringValue(auid));
                jsonObj.Add("title", JsonValue.CreateStringValue(appName));
            }
            catch (Exception e)
            {
                throw e;
            }

            var responseListener = new ResponseListener();

            responseListener.Success += (sender, args) =>
            {
                var launchSession = LaunchSession.LaunchSessionForAppId(auid);
                launchSession.AppName = appName;
                launchSession.Service = this;
                launchSession.SessionType = LaunchSessionType.App;

                Util.PostSuccess(listener, launchSession);
            };

            responseListener.Error += (sender, args) =>
            {
                Util.PostError(listener, (ServiceCommandError)args);
            };


            var requestUrl = GetUdapRequestUrl(UdapPathApptoappCommand);

            var ps = new Dictionary<string, string> { { "name", "AppExecute" }, { "auid", auid } };
            if (appName != null)
            {
                ps.Add("appname", appName);
            }
            if (contentId != null)
            {
                ps.Add("contentid", contentId);
            }

            var httpMessage = GetUdapMessageBody(UdapApiCommand, ps);

            var request = new ServiceCommand(this, requestUrl,
                httpMessage, responseListener);
            request.Send();
        }


        public void LaunchAppWithInfo(AppInfo appInfo, ResponseListener listener)
        {
            LaunchAppWithInfo(appInfo, null, listener);
        }


        public void LaunchAppWithInfo(AppInfo appInfo, Object ps, ResponseListener listener)
        {
            var appName = HttpMessage.Encode(appInfo.Name);
            var appId = appInfo.Id;
            string contentId = null;
            JsonObject mps = null;
            var o = ps as JsonObject;
            if (o != null)
                mps = o;

            if (mps != null)
            {
                try
                {
                    contentId = mps.GetNamedString("contentId");
                }
                catch (Exception e)
                {
                    throw e;
                }
            }

            LaunchApplication(appName, appId, contentId, listener);
        }


        public void LaunchBrowser(string url, ResponseListener listener)
        {
            const string appName = "Internet";

            var appInfoListener = new ResponseListener();
            appInfoListener.Success += (sender, o) =>
            {
                string contentId = null;
                var ai = o as AppInfo;
                if (ai != null) LaunchApplication(appName, ai.Id, null, listener);
            };

            appInfoListener.Error += (sender, o) =>
            {
                Util.PostError(listener, o);
            };
            GetApplication(appName, appInfoListener);
        }


        public void LaunchYouTube(string contentId, ResponseListener listener)
        {
            //    string appName = "YouTube";

            //    getApplication(appName, new ResponseListener() {


            //        public void onSuccess(AppInfo appInfo) {
            //            launchApplication(appName, appInfo.getId(), contentId, listener);
            //        }


            //        public void onError(ServiceCommandError error) {
            //            Util.PostError(listener, error);
            //        }
            //    });
        }


        public void LaunchHulu(string contentId, ResponseListener listener)
        {
            //    string appName = "Hulu";

            //    getApplication(appName, new ResponseListener() {


            //        public void onSuccess(AppInfo appInfo) {
            //            launchApplication(appName, appInfo.getId(), contentId, listener);
            //        }


            //        public void onError(ServiceCommandError error) {
            //            Util.PostError(listener, error);
            //        }
            //    });		
        }


        public void LaunchNetflix(string contentId, ResponseListener listener)
        {
            //    string appName = "Netflix";

            //    getApplication(appName, new ResponseListener() {


            //        public void onSuccess( AppInfo appInfo) {
            //            JsonObject jsonObj = new JsonObject();

            //            try {
            //                jsonObj.put("id", appInfo.getId());
            //                jsonObj.put("name", appName);
            //            } catch (Exception e) {
            //                throw e;
            //            }

            //            ResponseListener responseListener = new ResponseListener() {


            //                public void onSuccess(Object response) {
            //                    LaunchSession launchSession = LaunchSession.launchSessionForAppId(appInfo.getId());
            //                    launchSession.setAppName(appName);
            //                    launchSession.setService(NetcastTVService.this);
            //                    launchSession.setSessionType(LaunchSessionType.App);

            //                    Util.PostSuccess(listener, launchSession);
            //                }


            //                public void onError(ServiceCommandError error) {
            //                    if ( listener != null ) 
            //                        Util.PostError(listener, error);
            //                }
            //            };

            //            string requestURL = getUDAPRequestURL(UDAP_PATH_APPTOAPP_COMMAND);

            //            Dictionary<string,string> ps = new Dictionary<string,string>();
            //            ps.Add("name", "SearchCMDPlaySDPContent");
            //            ps.Add("content_type", "1");
            //            ps.Add("conts_exec_type", "20");
            //            ps.Add("conts_plex_type_flag", "N");
            //            ps.Add("conts_search_id", "2023237");
            //            ps.Add("conts_age", "18");
            //            ps.Add("exec_id", "netflix");
            //            ps.Add("item_id", "-Q m=http%3A%2F%2Fapi.netflix.com%2Fcatalog%2Ftitles%2Fmovies%2F" + contentId + "&amp;source_type=4&amp;trackId=6054700&amp;trackUrl=https%3A%2F%2Fapi.netflix.com%2FAPI_APP_ID_6261%3F%23Search%3F");
            //            ps.Add("app_type", "");

            //            string httpMessage = getUDAPMessageBody(UDAP_API_COMMAND, ps);

            //            ServiceCommand request = new ServiceCommand(NetcastTVService.this, requestURL, httpMessage, responseListener);
            //            request.send();
            //        }


            //        public void onError(ServiceCommandError error) {
            //            if ( listener != null ) 
            //                Util.PostError(listener, error);
            //        }
            //    });		
        }


        public void LaunchAppStore(string appId, ResponseListener listener)
        {
            //    string targetPath = getUDAPRequestURL(ROAP_PATH_APP_STORE);

            //    Map<string, string> ps = new HashMap<string, string>();
            //    ps.Add("name", "SearchCMDPlaySDPContent");
            //    ps.Add("content_type", "4");
            //    ps.Add("conts_exec_type", "");
            //    ps.Add("conts_plex_type_flag", "");
            //    ps.Add("conts_search_id", "");
            //    ps.Add("conts_age", "12");
            //    ps.Add("exec_id", "");
            //    ps.Add("item_id", HttpMessage.encode(appId));
            //    ps.Add("app_type", "S");

            //    string httpMessage = getUDAPMessageBody(UDAP_API_COMMAND, ps);

            //    ResponseListener responseListener = new ResponseListener() {


            //        public void onSuccess(Object response) {
            //            LaunchSession launchSession = LaunchSession.launchSessionForAppId(appId);
            //            launchSession.setAppName("LG Smart World"); // TODO: this will not work in Korea, use Korean name instead
            //            launchSession.setService(NetcastTVService.this);
            //            launchSession.setSessionType(LaunchSessionType.App);

            //            Util.PostSuccess(listener, launchSession);
            //        }


            //        public void onError(ServiceCommandError error) {
            //            Util.PostError(listener, error);
            //        }
            //    };	
            //    ServiceCommand command = new ServiceCommand(this, targetPath, httpMessage, responseListener);
            //    command.send();
        }


        public void CloseApp(LaunchSession launchSession, ResponseListener listener)
        {
            var requestUrl = GetUdapRequestUrl(UdapPathApptoappCommand);

            var ps = new Dictionary<string, string> { { "name", "AppTerminate" }, { "auid", launchSession.AppId } };
            if (launchSession.AppName != null)
                ps.Add("appname", HttpMessage.Encode(launchSession.AppName));

            var httpMessage = GetUdapMessageBody(UdapApiCommand, ps);

            var command = new ServiceCommand(launchSession.Service,
                requestUrl, httpMessage, listener);
            command.Send();
        }

        private void GetTotalNumberOfApplications(int type, ResponseListener listener)
        {
            var responseListener = new ResponseListener();
            responseListener.Success += (sender, o) =>
            {
                var lea = o as LoadEventArgs;
                if (lea == null) return;
                var load = lea.Load.GetPayload() as HttpResponseMessage;

                if (load == null) return;

                var strObj = load.Content.ReadAsStringAsync().Result;

                JsonObject jsonObject;
                JsonObject.TryParse(strObj, out jsonObject);

                var tarray = jsonObject.GetNamedArray("Channel List", new JsonArray());
                var applicationNumber = parseAppNumberXmlToJSON(strObj);

                Util.PostSuccess(listener, applicationNumber);
            };
            responseListener.Error += (sender, o) =>
            {
                Util.PostError(listener, o);
            };

            var requestUrl = GetUdapRequestUrl(UdapPathData, TargetAppnumGet, type.ToString());

            var command = new ServiceCommand(this, requestUrl, null,
                responseListener) { HttpMethod = ServiceCommand.TypeGet };
            command.Send();
        }

        private void GetApplications(int type, int number, ResponseListener listener)
        {
            var responseListener = new ResponseListener();
            responseListener.Success += (sender, o) =>
            {
                var lea = o as LoadEventArgs;
                if (lea == null) return;
                var load = lea.Load.GetPayload() as HttpResponseMessage;

                if (load == null) return;
                var strObj = load.Content.ReadAsStringAsync().Result;

                var appList = new List<AppInfo>();

                var reader = Util.GenerateStreamFromstring(strObj);
                var xmlReader = XmlReader.Create(reader);

                while (xmlReader.Read())
                {
                    if (xmlReader.Name.Equals("data", StringComparison.OrdinalIgnoreCase))
                        appList.Add(new AppInfo(""));
                    if (xmlReader.Name.Equals("auid", StringComparison.OrdinalIgnoreCase))
                        appList[appList.Count - 1].Id = xmlReader.ReadElementContentAsString();
                    if (xmlReader.Name.Equals("name", StringComparison.OrdinalIgnoreCase))
                        appList[appList.Count - 1].Name = xmlReader.ReadElementContentAsString();
                }
                appList = (from a in appList where !string.IsNullOrEmpty(a.Name) select a).ToList();
                if (listener != null)
                {
                    Util.PostSuccess(listener, appList);
                }
            };
            responseListener.Error += (sender, o) =>
            {

            };

            var requestUrl = GetUdapRequestUrl(UdapPathData, TargetApplistGet, type.ToString(), "0",
                number.ToString());

            var command = new ServiceCommand(this, requestUrl, null, responseListener) { HttpMethod = ServiceCommand.TypeGet };
            command.Send();
        }


        public void GetAppList(ResponseListener listener)
        {
            applications.Clear();

            var responseListener = new ResponseListener();
            responseListener.Success += (sender, o) =>
            {
                var responseListener2 = new ResponseListener();
                responseListener2.Success += (sender2, o2) =>
                {
                    var loadEventArgs = o2 as LoadEventArgs;
                    if (loadEventArgs != null)
                        applications.AddRange((List<AppInfo>)(loadEventArgs.Load.GetPayload()));
                    var responseListener3 = new ResponseListener();
                    responseListener3.Success += (sender3, o3) =>
                    {
                        var responseListener4 = new ResponseListener();
                        responseListener4.Success += (sender4, o4) =>
                        {
                            var eventArgs = o4 as LoadEventArgs;
                            if (eventArgs != null)
                            {
                                var apps = (List<AppInfo>)(eventArgs.Load.GetPayload());
                                applications.AddRange(apps);
                            }
                            Util.PostSuccess(listener, applications);
                        };

                        responseListener4.Error += (sender4, o4) =>
                        {
                            Util.PostError(listener, o4);
                        };
                        var args = o3 as LoadEventArgs;
                        if (args != null)
                            GetApplications(3, (int)args.Load.GetPayload(), responseListener4);
                    };

                    responseListener3.Error += (sender3, o3) =>
                    {
                        Util.PostError(listener, o3);
                    };
                    GetTotalNumberOfApplications(3, responseListener3);
                };
                responseListener2.Error += (sender2, o2) =>
                {
                    Util.PostError(listener, o2);
                };
                var loadEventArgs1 = o as LoadEventArgs;
                if (loadEventArgs1 != null)
                    GetApplications(2, (int)loadEventArgs1.Load.GetPayload(), responseListener2);
            };

            responseListener.Error += (sender, o) =>
            {
                Util.PostError(listener, o);
            };
            GetTotalNumberOfApplications(2, responseListener);
        }


        public void GetRunningApp(ResponseListener listener)
        {
            // Do nothing - Not Supported
            Util.PostError(listener, ServiceCommandError.NotSupported());
        }


        public IServiceSubscription SubscribeRunningApp(ResponseListener listener)
        {
            // Do nothing - Not Supported
            Util.PostError(listener, ServiceCommandError.NotSupported());

            return new NotSupportedServiceSubscription();
        }


        public void GetAppState(LaunchSession launchSession, ResponseListener listener)
        {
            var requestUrl = string.Format("{0}{1}", GetUdapRequestUrl(UdapPathApptoappData),
                string.Format("/{0}/status", launchSession.AppId));

            var responseListener = new ResponseListener();
            responseListener.Success += (sender, o) =>
            {
                var response = (string)o;
                AppState appState;
                if (response.Equals("NONE"))
                    appState = new AppState(false, false);
                else if (response.Equals("LOAD"))
                    appState = new AppState(false, true);
                else if (response.Equals("RUN_NF"))
                    appState = new AppState(true, false);
                else if (response.Equals("TERM"))
                    appState = new AppState(false, true);
                else
                    appState = new AppState(false, false);

                Util.PostSuccess(listener, appState);
            };

            responseListener.Error += (sender, o) =>
            {
                Util.PostError(listener, o);
            };

            var command = new ServiceCommand(this, requestUrl, null, responseListener) { HttpMethod = ServiceCommand.TypeGet };
            command.Send();
        }


        public IServiceSubscription SubscribeAppState(LaunchSession launchSession,
            ResponseListener listener)
        {
            Util.PostError(listener, ServiceCommandError.NotSupported());
            return null;
        }


        /******************
        TV CONTROL
        *****************/

        public ITvControl GetTvControl()
        {
            return this;
        }


        public CapabilityPriorityLevel GetTvControlCapabilityLevel()
        {
            return CapabilityPriorityLevel.HIGH;
        }


        public void GetChannelList(ResponseListener listener)
        {
            var requestUrl = GetUdapRequestUrl(UdapPathData, TargetChannelList);

            var responseListener = new ResponseListener();
            responseListener.Success += (sender, o) =>
            {
                var lea = o as LoadEventArgs;
                if (lea == null) return;
                var load = lea.Load.GetPayload() as HttpResponseMessage;

                if (load == null) return;
                var strObj = load.Content.ReadAsStringAsync().Result;

                JsonObject jsonObject;
                JsonObject.TryParse(strObj, out jsonObject);

                var tarray = jsonObject.GetNamedArray("Channel List", new JsonArray());

                //todo: fix this
                //try {
                //    SAXParserFactory saxParserFactory = SAXParserFactory.newInstance();
                //    InputStream stream = new ByteArrayInputStream(strObj.getBytes("UTF-8"));
                //    SAXParser saxParser = saxParserFactory.newSAXParser();

                //    NetcastChannelParser parser = new NetcastChannelParser();
                //    saxParser.parse(stream, parser);

                //    JSONArray channelArray = parser.getJSONChannelArray();
                //    ArrayList<ChannelInfo> channelList = new ArrayList<ChannelInfo>();

                //    for (int i = 0; i < channelArrayLength; i++) {
                //         JsonObject rawData;
                //         try {
                //             rawData = (JsonObject) channelArray.get(i);

                //             ChannelInfo channel = NetcastChannelParser.parseRawChannelData(rawData);
                //             channelList.add(channel);
                //         } catch (Exception e) {
                //             throw e;
                //         }
                //    }

                //    Util.PostSuccess(responseListener, channelList);
                //} catch (Exception e) {
                //    throw e;
            };

            responseListener.Error += (sender, o) =>
            {
                Util.PostError(responseListener, o);
            };

            var command = new ServiceCommand(this, requestUrl, null, responseListener) { HttpMethod = ServiceCommand.TypeGet };
            command.Send();
        }


        public void ChannelUp(ResponseListener listener)
        {
            SendKeyCode((int)VirtualKeycodes.CHANNEL_UP, listener);
        }


        public void ChannelDown(ResponseListener listener)
        {
            SendKeyCode((int)VirtualKeycodes.CHANNEL_DOWN, listener);
        }


        public void SetChannel(ChannelInfo channelInfo, ResponseListener listener)
        {
            var responseListener = new ResponseListener();
            responseListener.Success += (sender, o) =>
            {
                var channelList = o as List<ChannelInfo>;
                var requestUrl = GetUdapRequestUrl(UdapPathCommand);

                var ps = new Dictionary<string, string>();

                if (channelList != null)
                    foreach (ChannelInfo ch in channelList)
                    {
                        var rawData = ch.RawData;

                        try
                        {
                            var major = channelInfo.ChannelNumber.Split('-')[0];
                            var minor = channelInfo.ChannelNumber.Split('-')[1];

                            var majorNumber = ch.MajorNumber;
                            var minorNumber = ch.MinorNumber;

                            var sourceIndex = rawData.GetNamedString("sourceIndex");
                            var physicalNum = (int)rawData.GetNamedNumber("physicalNumber");

                            if (major != majorNumber.ToString() || minor != minorNumber.ToString()) continue;
                            ps.Add("name", "HandleChannelChange");
                            ps.Add("major", major);
                            ps.Add("minor", minor);
                            ps.Add("sourceIndex", sourceIndex);
                            ps.Add("physicalNum", physicalNum.ToString());

                            break;
                        }
                        catch (Exception e)
                        {
                            throw e;
                        }
                    }

                var httpMessage = GetUdapMessageBody(UdapApiCommand, ps);

                var request = new ServiceCommand(this, requestUrl, httpMessage, listener);
                request.Send();
            };

            responseListener.Error += (sender, o) =>
            {
                Util.PostError(listener, o);
            };


            GetChannelList(responseListener);
        }


        public void GetCurrentChannel(ResponseListener listener)
        {
            var requestUrl = GetUdapRequestUrl(UdapPathData, TargetCurrentChannel);
            var responseListener = new ResponseListener();
            responseListener.Success += (sender, o) =>
            {
                //TODO: fix this
                var strObj = (string)o;

                try
                {
                    //SAXParserFactory saxParserFactory = SAXParserFactory.newInstance();
                    //InputStream stream = new ByteArrayInputStream(strObj.getBytes("UTF-8"));
                    //SAXParser saxParser = saxParserFactory.newSAXParser();

                    //NetcastChannelParser parser = new NetcastChannelParser();
                    //saxParser.parse(stream, parser);

                    //JSONArray channelArray = parser.getJSONChannelArray();

                    //if ( channelArrayLength > 0 ) {
                    //    JsonObject rawData = (JsonObject) channelArray.get(0);

                    //    ChannelInfo channel = NetcastChannelParser.parseRawChannelData(rawData);

                    //    Util.PostSuccess(listener, channel);
                    //}
                }
                catch (Exception e)
                {
                    throw e;
                }
            };

            responseListener.Error += (sender, o) =>
            {
                Util.PostError(listener, o);
            };

            var request = new ServiceCommand(this, requestUrl, null, responseListener);
            request.Send();
        }


        public IServiceSubscription SubscribeCurrentChannel(ResponseListener listener)
        {
            GetCurrentChannel(listener); // This is for the initial Current TV Channel Info.

            var request = new UrlServiceSubscription(this, "ChannelChanged", null, null);
            request.HttpMethod = ServiceCommand.TypeGet;
            request.AddListener(listener);
            AddSubscription(request);

            return request;
        }


        public void GetProgramInfo(ResponseListener listener)
        {
            // Do nothing - Not Supported
            Util.PostError(listener, ServiceCommandError.NotSupported());
        }


        public IServiceSubscription SubscribeProgramInfo(ResponseListener listener)
        {
            // Do nothing - Not Supported
            Util.PostError(listener, ServiceCommandError.NotSupported());

            return null;
        }


        public void GetProgramList(ResponseListener listener)
        {
            // Do nothing - Not Supported
            Util.PostError(listener, ServiceCommandError.NotSupported());
        }


        public IServiceSubscription SubscribeProgramList(ResponseListener listener)
        {
            // Do nothing - Not Supported
            Util.PostError(listener, ServiceCommandError.NotSupported());

            return null;
        }


        public void Set3DEnabled(bool enabled, ResponseListener listener)
        {
            var responseListener = new ResponseListener();
            responseListener.Success += (sender, o) =>
            {
                if (enabled != (bool)o)
                {
                    SendKeyCode((int)VirtualKeycodes.VIDEO_3D, listener);
                }
            };

            responseListener.Error += (sender, o) =>
            {
                Util.PostError(listener, o);
            };

            Get3DEnabled(responseListener);
        }


        public void Get3DEnabled(ResponseListener listener)
        {
            var responseListener = new ResponseListener();
            responseListener.Success += (sender, o) =>
            {
                var strObj = (string)o;
                var upperStr = strObj.ToUpper();

                Util.PostSuccess(listener, upperStr.Contains("TRUE"));
            };

            responseListener.Error += (sender, o) =>
            {
                Util.PostError(listener, o);
            };

            var requestUrl = GetUdapRequestUrl(UdapPathData, TargetIs_3D);

            var request = new ServiceCommand(this, requestUrl, null, responseListener)
            {
                HttpMethod = ServiceCommand.TypeGet
            };
            request.Send();
        }


        public IServiceSubscription Subscribe3DEnabled(ResponseListener listener)
        {
            Get3DEnabled(listener);

            var request = new UrlServiceSubscription(this, Target_3DMode, null, null) { HttpMethod = ServiceCommand.TypeGet };
            request.AddListener(listener);

            AddSubscription(request);

            return request;
        }

        /**************
        VOLUME
        **************/
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
            SendKeyCode((int)VirtualKeycodes.VOLUME_UP, listener);
        }

        public void VolumeDown(ResponseListener listener)
        {
            SendKeyCode((int)VirtualKeycodes.VOLUME_DOWN, listener);
        }

        public void SetVolume(float volume, ResponseListener listener)
        {
            // Do nothing - not supported
            Util.PostError(listener, ServiceCommandError.NotSupported());
        }

        public void GetVolume(ResponseListener listener)
        {
            var responseListener = new ResponseListener();
            responseListener.Success += (sender, o) =>
            {
                Util.PostSuccess(listener, ((VolumeStatus)o).Volume);
            };

            responseListener.Error += (sender, o) =>
            {
                Util.PostError(listener, o);
            };
            GetVolumeStatus(responseListener);
        }

        public void SetMute(bool isMute, ResponseListener listener)
        {
            var responseListener = new ResponseListener();
            responseListener.Success += (sender, o) =>
            {
                //if (isMute != ((VolumeStatus) o).isMute)
                {
                    SendKeyCode((int)VirtualKeycodes.MUTE, listener);
                }
            };

            responseListener.Error += (sender, o) =>
            {
                Util.PostError(listener, o);
            };
            GetVolumeStatus(responseListener);
        }


        public void GetMute(ResponseListener listener)
        {
            var responseListener = new ResponseListener();
            responseListener.Success += (sender, o) =>
            {
                Util.PostSuccess(listener, ((VolumeStatus)o).IsMute);
            };

            responseListener.Error += (sender, o) =>
            {
                Util.PostError(listener, o);
            };
            GetVolumeStatus(responseListener);
        }

        public IServiceSubscription SubscribeVolume(ResponseListener listener)
        {
            // Do nothing - not supported
            Util.PostError(listener, ServiceCommandError.NotSupported());

            return null;
        }

        public IServiceSubscription SubscribeMute(ResponseListener listener)
        {
            // Do nothing - not supported
            Util.PostError(listener, ServiceCommandError.NotSupported());

            return null;
        }

        private void GetVolumeStatus(ResponseListener listener)
        {
            var responseListener = new ResponseListener();
            responseListener.Success += (sender, o) =>
            {
                var strObj =
                    (new StreamReader(
                        (((HttpResponseMessage)((LoadEventArgs)o).Load.GetPayload()).Content.ReadAsStreamAsync()
                            .Result)))
                        .ReadToEnd();

                var reader = Util.GenerateStreamFromstring(strObj);
                var xmlReader = XmlReader.Create(reader);
                var isMute = false;
                var volume = 0;
                try
                {
                    while (xmlReader.Read())
                    {
                        if (xmlReader.Name == "mute")
                            isMute = bool.Parse(xmlReader.ReadElementContentAsString());
                        if (xmlReader.Name == "level")
                            volume = int.Parse(xmlReader.ReadElementContentAsString());
                    }

                    Util.PostSuccess(listener, new VolumeStatus(isMute, volume));
                }
                catch (Exception e)
                {
                    throw e;
                }

            };

            responseListener.Error += (sender, o) =>
            {
                Util.PostError(listener, o);
            };

            var requestUrl = GetUdapRequestUrl(UdapPathData, TargetVolumeInfo);

            var request = new ServiceCommand(this, requestUrl, null,
                responseListener) { HttpMethod = ServiceCommand.TypeGet };
            request.Send();
        }

        /**************
        EXTERNAL INPUT
        **************/

        public IExternalInputControl GetExternalInput()
        {
            return this;
        }

        public CapabilityPriorityLevel GetExternalInputControlPriorityLevel()
        {
            return CapabilityPriorityLevel.HIGH;
        }

        public void LaunchInputPicker(ResponseListener listener)
        {
            const string appName = "Input List";
            var encodedStr = HttpMessage.Encode(appName);

            var responseListener = new ResponseListener();
            responseListener.Success += (sender, o) =>
            {
                var responseLaunchListener = new ResponseListener();
                responseListener.Success += (sender2, o2) =>
                {
                    if (inputPickerSession == null)
                    {
                        inputPickerSession = (LaunchSession)o2;
                    }

                    Util.PostSuccess(listener, (LaunchSession)o2);
                };

                responseListener.Error += (sender2, o2) =>
                {
                    Util.PostError(listener, o2);
                };
                LaunchApplication(appName, ((AppInfo)o).Id, null, responseLaunchListener);
            };

            responseListener.Error += (sender, o) =>
            {
                Util.PostError(listener, o);
            };

            GetApplication(encodedStr, responseListener);
        }

        public void CloseInputPicker(LaunchSession launchSession, ResponseListener listener)
        {
            if (inputPickerSession != null)
            {
                inputPickerSession.Close(listener);
            }
        }


        //public void GetExternalInputList(ExternalInputListListener listener)
        //{
        //    // Do nothing - not Supported
        //    Util.PostError(listener, ServiceCommandError.NotSupported());
        //}


        public void SetExternalInput(ExternalInputInfo input, ResponseListener listener)
        {
            // Do nothing - not Supported
            Util.PostError(listener, ServiceCommandError.NotSupported());
        }

        /******************
        MEDIA PLAYER
        *****************/
        public IMediaPlayer GetMediaPlayer()
        {
            return this;
        }

        public CapabilityPriorityLevel GetMediaPlayerCapabilityLevel()
        {
            return CapabilityPriorityLevel.HIGH;
        }

        public void DisplayImage(string url, string mimeType, string title, string description, string iconSrc,
            ResponseListener listener)
        {
            if (dlnaService != null)
            {
                dlnaService.DisplayImage(url, mimeType, title, description, iconSrc, listener);
            }
        }

        public void PlayMedia(string url, string mimeType, string title, string description, string iconSrc,
            bool shouldLoop, ResponseListener listener)
        {
            if (dlnaService != null)
            {
                dlnaService.PlayMedia(url, mimeType, title, description, iconSrc, shouldLoop, listener);
            }
        }

        public void CloseMedia(LaunchSession launchSession, ResponseListener listener)
        {
            if (dlnaService == null)
            {
                Util.PostError(listener, new ServiceCommandError(0, "Service is not connected", null));
                return;
            }

            dlnaService.CloseMedia(launchSession, listener);
        }

        /******************
        MEDIA CONTROL
        *****************/

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
            SendKeyCode((int)VirtualKeycodes.PLAY, listener);
        }

        public void Pause(ResponseListener listener)
        {
            SendKeyCode((int)VirtualKeycodes.PAUSE, listener);
        }

        public void Stop(ResponseListener listener)
        {
            SendKeyCode((int)VirtualKeycodes.STOP, listener);
        }

        public void Rewind(ResponseListener listener)
        {
            SendKeyCode((int)VirtualKeycodes.REWIND, listener);
        }

        public void FastForward(ResponseListener listener)
        {
            SendKeyCode((int)VirtualKeycodes.FAST_FORWARD, listener);
        }

        public void Seek(long position, ResponseListener listener)
        {
            if (dlnaService != null)
            {
                dlnaService.Seek(position, listener);
            }
        }

        public void GetDuration(ResponseListener listener)
        {
            if (dlnaService != null)
            {
                dlnaService.GetDuration(listener);
            }
        }

        public void GetPosition(ResponseListener listener)
        {
            if (dlnaService != null)
            {
                dlnaService.GetPosition(listener);
            }
        }

        /**************
        MOUSE CONTROL
        **************/

        public IMouseControl GetMouseControl()
        {
            return this;
        }

        public CapabilityPriorityLevel GetMouseControlCapabilityLevel()
        {
            return CapabilityPriorityLevel.HIGH;
        }

        private void SetMouseCursorVisible(bool visible, ResponseListener listener)
        {
            var requestUrl = GetUdapRequestUrl(UdapPathEvent);

            var ps = new Dictionary<string, string>
            {
                {"name", "CursorVisible"},
                {"value", visible ? "true" : "false"},
                {"mode", "auto"}
            };

            var httpMessage = GetUdapMessageBody(UdapApiEvent, ps);

            var request = new ServiceCommand(this, requestUrl, httpMessage, listener);
            request.Send();
        }

        public void ConnectMouse()
        {
            var responseListener = new ResponseListener();
            responseListener.Success += (sender, o) =>
            {
                mMouseDistance = new Point(0, 0);
                mMouseIsMoving = false;
                isMouseConnected = true;
            };

            responseListener.Error += (sender, o) =>
            {
                isMouseConnected = false;
            };

            SetMouseCursorVisible(true, responseListener);
        }

        public void DisconnectMouse()
        {
            SetMouseCursorVisible(false, null);
            isMouseConnected = false;
        }

        private bool isMouseConnected;
        public bool MouseConnected()
        {
            return isMouseConnected;
        }

        public void Click()
        {
            var responseListener = new ResponseListener();

            var requestUrl = GetUdapRequestUrl(UdapPathCommand);

            var ps = new Dictionary<string, string> { { "name", "HandleTouchClick" } };

            var httpMessage = GetUdapMessageBody(UdapApiCommand, ps);

            var request = new ServiceCommand(this, requestUrl, httpMessage, responseListener);
            request.Send();
        }

        public void Move(double dx, double dy)
        {
            mMouseDistance.X += dx;
            mMouseDistance.Y += dy;

            if (mMouseIsMoving) return;
            mMouseIsMoving = true;
            MoveMouse();
        }

        private void MoveMouse()
        {
            var requestUrl = GetUdapRequestUrl(UdapPathCommand);

            var x = (int)mMouseDistance.X;
            var y = (int)mMouseDistance.Y;

            var ps = new Dictionary<string, string>
            {
                {"name", "HandleTouchMove"},
                {"x", x.ToString()},
                {"y", y.ToString()}
            };

            mMouseDistance.X = mMouseDistance.Y = 0;

            var mouseService = this;

            var responseListener = new ResponseListener();
            responseListener.Success += (sender, o) =>
            {
                if (mMouseDistance.X > 0 || mMouseDistance.Y > 0)
                    mouseService.MoveMouse();
                else
                    mMouseIsMoving = false;
            };

            responseListener.Error += (sender, o) =>
            {
                mMouseIsMoving = false;
            };

            var httpMessage = GetUdapMessageBody(UdapApiCommand, ps);

            var request = new ServiceCommand(this, requestUrl, httpMessage, responseListener);
            request.Send();
        }

        public void Move(Point diff)
        {
            Move(diff.X, diff.Y);
        }

        public void Scroll(double dx, double dy)
        {
            var responseListener = new ResponseListener();

            var requestUrl = GetUdapRequestUrl(UdapPathCommand);

            var ps = new Dictionary<string, string> { { "name", "HandleTouchWheel" }, { "value", dy > 0 ? "up" : "down" } };

            var httpMessage = GetUdapMessageBody(UdapApiCommand, ps);

            var request = new ServiceCommand(this, requestUrl, httpMessage, responseListener);
            request.Send();
        }

        public void Scroll(Point diff)
        {
            Scroll(diff.X, diff.Y);
        }

        /**************
        KEYBOARD CONTROL
        **************/
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
            keyboardstring = new StringBuilder();

            var request = new UrlServiceSubscription(this, "KeyboardVisible", null, null);
            request.AddListener(listener);

            AddSubscription(request);

            return request;
        }

        public void SendText(string input)
        {
            keyboardstring.Append(input);
            HandleKeyboardInput("Editing", keyboardstring.ToString());
        }

        public void SendEnter()
        {
            var responseListener = new ResponseListener();
            HandleKeyboardInput("EditEnd", keyboardstring.ToString());
            SendKeyCode((int)VirtualKeycodes.RED, responseListener); // Send RED Key to enter the "ENTER" button
        }

        public void SendDelete()
        {
            if (keyboardstring.Length > 1)
            {
                keyboardstring.Remove(keyboardstring.Length - 1, 1);
            }
            else
            {
                keyboardstring = new StringBuilder();
            }

            HandleKeyboardInput("Editing", keyboardstring.ToString());
        }

        private void HandleKeyboardInput(string state, string buffer)
        {
            var responseListener = new ResponseListener();

            var requestUrl = GetUdapRequestUrl(UdapPathEvent);

            var ps = new Dictionary<string, string> { { "name", "TextEdited" }, { "state", state }, { "value", buffer } };

            var httpMessage = GetUdapMessageBody(UdapApiEvent, ps);

            var request = new ServiceCommand(this, requestUrl, httpMessage, responseListener);
            request.Send();
        }

        /**************
        KEY CONTROL
        **************/

        public IKeyControl GetKeyControl()
        {
            return this;
        }

        public CapabilityPriorityLevel GetKeyControlCapabilityLevel()
        {
            return CapabilityPriorityLevel.HIGH;
        }

        public void Up(ResponseListener listener)
        {
            SendKeyCode((int)VirtualKeycodes.KEY_UP, listener);
        }

        public void Down(ResponseListener listener)
        {
            SendKeyCode((int)VirtualKeycodes.KEY_DOWN, listener);
        }

        public void Left(ResponseListener listener)
        {
            SendKeyCode((int)VirtualKeycodes.KEY_LEFT, listener);
        }

        public void Right(ResponseListener listener)
        {
            SendKeyCode((int)VirtualKeycodes.KEY_RIGHT, listener);
        }

        public void Ok(ResponseListener listener)
        {
            SendKeyCode((int)VirtualKeycodes.OK, listener);
        }

        public void Back(ResponseListener listener)
        {
            SendKeyCode((int)VirtualKeycodes.BACK, listener);
        }

        public void Home(ResponseListener listener)
        {
            SendKeyCode((int)VirtualKeycodes.HOME, listener);
        }

        /**************
        POWER CONTROL
        **************/
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
            SendKeyCode((int)VirtualKeycodes.POWER, new ResponseListener());
        }

        public void PowerOn(ResponseListener listener)
        {
            if (listener != null)
                listener.OnError(ServiceCommandError.NotSupported());
        }

        private JsonObject ParseVolumeXmlToJson(string data)
        {
            throw new NotImplementedException();
            //SAXParserFactory saxParserFactory = SAXParserFactory.newInstance();
            //try
            //{
            //    InputStream stream = new ByteArrayInputStream(data.getBytes("UTF-8"));

            //    SAXParser saxParser = saxParserFactory.newSAXParser();
            //    NetcastVolumeParser handler = new NetcastVolumeParser();
            //    saxParser.parse(stream, handler);

            //    return handler.getVolumeStatus();
            //}
            //catch (Exception e)
            //{
            //    throw e;
            //}
        }

        private static int parseAppNumberXmlToJSON(string data)
        {
            var reader = Util.GenerateStreamFromstring(data);
            var xmlReader = XmlReader.Create(reader);
            var number = "";
            while (xmlReader.Read())
            {
                if (xmlReader.Name == "number")
                    number = xmlReader.ReadElementContentAsString();
            }

            return string.IsNullOrEmpty(number) ? 0 : int.Parse(number);

            throw new NotImplementedException();
            //SAXParserFactory saxParserFactory = SAXParserFactory.newInstance();
            //try {
            //    InputStream stream = new ByteArrayInputStream(data.getBytes("UTF-8"));

            //    SAXParser saxParser = saxParserFactory.newSAXParser();
            //    NetcastAppNumberParser handler = new NetcastAppNumberParser();
            //    saxParser.parse(stream, handler);

            //    return handler.getApplicationNumber();
            //} catch (ParserConfigurationException e) {
            //    throw e;
            //} catch (SAXException e) {
            //    throw e;
            //} catch (IOException e) {
            //    throw e;
            //}
            return 0;
        }

        private JsonArray ParseApplicationsXmlToJson(string data)
        {
            throw new NotImplementedException();
            //SAXParserFactory saxParserFactory = SAXParserFactory.newInstance();
            //try {
            //    InputStream stream = new ByteArrayInputStream(data.getBytes("UTF-8"));

            //    SAXParser saxParser = saxParserFactory.newSAXParser();
            //    NetcastApplicationsParser handler = new NetcastApplicationsParser();
            //    saxParser.parse(stream, handler);

            //    return handler.getApplications();
            //} catch (ParserConfigurationException e) {
            //    throw e;
            //} catch (SAXException e) {
            //    throw e;
            //} catch (IOException e) {
            //    throw e;
            //}
            return null;
        }

        public string GetHttpMessageForHandleKeyInput(int keycode)
        {
            string strKeycode = keycode.ToString();

            var ps = new Dictionary<string, string> { { "name", "HandleKeyInput" }, { "value", strKeycode } };

            return GetUdapMessageBody(UdapApiCommand, ps);
        }


        public void SendKeyCode(int keycode, ResponseListener listener)
        {
            var responseListener = new ResponseListener();

            responseListener.Success += (sender, o) =>
            {
                var requestUrl = GetUdapRequestUrl(UdapPathCommand);
                var httpMessage = GetHttpMessageForHandleKeyInput(keycode);

                var request = new ServiceCommand(this, requestUrl, httpMessage, listener);
                request.Send();
            };
            responseListener.Error += (sender, o) =>
            {
                Util.PostError(listener, o);
            };
            SetMouseCursorVisible(false, responseListener);
        }

        public override void SendCommand(ServiceCommand mCommand)
        {
            var t = new Task(() =>
            {
                var command = mCommand;

                var payload = command.Payload;

                var request = command.GetRequest();
                request.Headers.Add(HttpMessage.USER_AGENT, HttpMessage.UDAP_USER_AGENT);


                //request.Headers.Add(HttpMessage.CONTENT_TYPE_HEADER, HttpMessage.CONTENT_TYPE);
                HttpWebResponse response = null;

                if (payload != null && command.HttpMethod.Equals(ServiceCommand.TypePost))
                {
                    request.Method = HttpMethod.Post;
                    request.Content =
                        new StreamContent(new MemoryStream(System.Text.Encoding.UTF8.GetBytes(payload.ToString())));
                    request.Content.Headers.ContentType = new MediaTypeHeaderValue("text/xml");
                    request.Content.Headers.ContentType.CharSet = "utf-8";
                }

                try
                {
                    var res = httpClient.SendAsync(request).Result;
                    if (res.IsSuccessStatusCode)
                    {
                        Util.PostSuccess(command.ResponseListenerValue, res);
                    }
                    else
                    {
                        Util.PostError(command.ResponseListenerValue, ServiceCommandError.GetError((int)res.StatusCode));
                    }
                }
                catch (Exception e)
                {
                    throw e;
                }
            });

            t.RunSynchronously();
        }

        private void AddSubscription(IServiceSubscription subscription)
        {
            subscriptions.Add(subscription);
        }

        public override void Unsubscribe(UrlServiceSubscription subscription)
        {
            subscriptions.Remove(subscription);
        }

        protected void setCapabilities()
        {

            if (DiscoveryManager.GetInstance().GetPairingLevel() == DiscoveryManager.PairingLevel.ON)
            {
                AppendCapabilites(TextInputControl.Capabilities.ToList());
                AppendCapabilites(MouseControl.Capabilities.ToList());
                AppendCapabilites(KeyControl.Capabilities.ToList());
                AppendCapabilites(MediaPlayer.Capabilities.ToList());

                AppendCapabilites(
                    new List<string>
                    {
                        PowerControl.Off,

                        MediaControl.Play,
                        MediaControl.Pause,
                        MediaControl.Stop,
                        MediaControl.Rewind,
                        MediaControl.FastForward,
                        MediaControl.Duration,
                        MediaControl.Position,
                        MediaControl.Seek, 
                        //MediaControl.MetaData_Title, 
                        //MediaControl.MetaData_MimeType, 

                        Launcher.Application,
                        Launcher.ApplicationClose,
                        Launcher.ApplicationList,
                        Launcher.Browser,
                        Launcher.Hulu,
                        Launcher.Netflix,
                        Launcher.YouTube,
                        Launcher.AppStore,

                        TvControl.ChannelUp,
                        TvControl.ChannelDown,
                        TvControl.ChannelGet,
                        TvControl.ChannelList,
                        TvControl.ChannelSubscribe,
                        TvControl.Get_3D,
                        TvControl.Set_3D,
                        TvControl.Subscribe_3D,

                        ExternalInputControl.PickerLaunch,
                        ExternalInputControl.PickerClose,

                        VolumeControl.VolumeGet,
                        VolumeControl.VolumeUpDown,
                        VolumeControl.MuteGet,
                        VolumeControl.MuteSet
                    }
                    );
            }
            else
            {
                AppendCapabilites(MediaPlayer.Capabilities.ToList());
                AppendCapabilites(new List<string>
                {
                    MediaControl.Play,
                    MediaControl.Pause,
                    MediaControl.Stop,
                    MediaControl.Rewind,
                    MediaControl.FastForward,

                    Launcher.YouTube
                }
                    );
            }
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
    }
}