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
using ConnectSdk.Windows.Service;
using MyRemote.ConnectSDK.Core;
using MyRemote.ConnectSDK.Discovery;
using MyRemote.ConnectSDK.Etc.Helper;
using MyRemote.ConnectSDK.Service;
using MyRemote.ConnectSDK.Service.Capability;
using MyRemote.ConnectSDK.Service.Capability.Listeners;
using MyRemote.ConnectSDK.Service.Command;
using MyRemote.ConnectSDK.Service.Config;
using MyRemote.ConnectSDK.Service.Sessions;

namespace MyRemote.ConnectSDK.Service
{
    public class NetcastTVService : DeviceService, ILauncher, IMediaControl, IMediaPlayer, ITvControl, IVolumeControl,
        IExternalInputControl, IMouseControl, ITextInputControl, IPowerControl, IKeyControl
    {

        public static string ID = "Netcast TV";

        public static string UDAP_PATH_PAIRING = "/udap/api/pairing";
        public static string UDAP_PATH_DATA = "/udap/api/data";
        public static string UDAP_PATH_COMMAND = "/udap/api/command";
        public static string UDAP_PATH_EVENT = "/udap/api/event";

        public static string UDAP_PATH_APPTOAPP_DATA = "/udap/api/apptoapp/data/";
        public static string UDAP_PATH_APPTOAPP_COMMAND = "/udap/api/apptoapp/command/";
        public static string ROAP_PATH_APP_STORE = "/roap/api/command/";

        public static string UDAP_API_PAIRING = "pairing";
        public static string UDAP_API_COMMAND = "command";
        public static string UDAP_API_EVENT = "event";

        public static string TARGET_CHANNEL_LIST = "channel_list";
        public static string TARGET_CURRENT_CHANNEL = "cur_channel";
        public static string TARGET_VOLUME_INFO = "volume_info";
        public static string TARGET_APPLIST_GET = "applist_get";
        public static string TARGET_APPNUM_GET = "appnum_get";
        public static string TARGET_3D_MODE = "3DMode";
        public static string TARGET_IS_3D = "is_3D";

        public enum State
        {
            NONE,
            INITIAL,
            CONNECTING,
            PAIRING,
            PAIRED,
            DISCONNECTING
        };

        private HttpClient httpClient;
        //private NetcastHttpServer httpServer;

        private DlnaService dlnaService;

        private LaunchSession inputPickerSession;

        private List<AppInfo> applications;
        private List<IServiceSubscription> subscriptions;
        private StringBuilder keyboardstring;

        private State state = State.INITIAL;
        //private Context context;

        private Point mMouseDistance;
        private bool mMouseIsMoving;

        private ResponseListener mTextChangedListener;

        public State ServiceState
        {
            get { return state; }
            set { state = value; }
        }

        private string getUDAPRequestURL(string path)
        {
            return getUDAPRequestURL(path, null);
        }

        private string getUDAPRequestURL(string path, string target)
        {
            return getUDAPRequestURL(path, target, null);
        }

        private string getUDAPRequestURL(string path, string target, string type)
        {
            return getUDAPRequestURL(path, target, type, null, null);
        }

        private string getUDAPRequestURL(string path, string target, string type, string index, string number)
        {
            // Type Values
            // 1: List of all apps
            // 2: List of apps in the Premium category
            // 3: List of apps in the My Apps category

            StringBuilder sb = new StringBuilder();
            sb.Append("http://");
            sb.Append(ServiceDescription.IpAddress);
            sb.Append(":");
            sb.Append(ServiceDescription.Port);
            sb.Append(path);

            if (target != null)
            {
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

                if (number != null)
                {
                    sb.Append("&number=");
                    sb.Append(number);
                }
            }

            return sb.ToString();
        }

        private string getUDAPMessageBody(string api, Dictionary<string, string> ps)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
            sb.Append("<envelope>");
            sb.Append("<api type=\"" + api + "\">");

            foreach (var entry in ps)
            {
                string key = entry.Key;
                string value = entry.Value;

                sb.Append(createNode(key, value));
            }

            sb.Append("</api>");
            sb.Append("</envelope>");

            return sb.ToString();
        }

        private string createNode(string tag, string value)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("<" + tag + ">");
            sb.Append(value);
            sb.Append("</" + tag + ">");

            return sb.ToString();
        }

        public string decToHex(string dec)
        {
            if (dec != null && dec.Length > 0)
                return decToHex(long.Parse(dec));
            return null;
        }

        public string decToHex(long dec)
        {
            return dec.ToString("X");
        }

        public NetcastTVService(ServiceDescription serviceDescription, ServiceConfig serviceConfig) :
            base(serviceDescription, serviceConfig)
        {
            dlnaService = new DlnaService(serviceDescription, serviceConfig);

            if (serviceDescription.Port != 8080)
                serviceDescription.Port = 8080;

            applications = new List<AppInfo>();
            subscriptions = new List<IServiceSubscription>();

            keyboardstring = new StringBuilder();

            httpClient = new HttpClient();
            //ClientConnectionManager mgr = httpClient.getConnectionManager();
            //Httpps ps = httpClient.getps();
            //httpClient = new DefaultHttpClient(new ThreadSafeClientConnManager(ps, mgr.getSchemeRegistry()), ps);

            //state = State.INITIAL;

            inputPickerSession = null;

            mTextChangedListener = new ResponseListener();

            mTextChangedListener.Success += (sender, o) =>
            {
                keyboardstring = new StringBuilder((string) o);
            };

            mTextChangedListener.Error += (sender, o) =>
            {
            };
        }

        public static JsonObject discoveryParameters()
        {
            JsonObject ps = new JsonObject();

            try
            {
                ps.Add("serviceId", JsonValue.CreateStringValue(ID));
                ps.Add("filter", JsonValue.CreateStringValue("udap:rootservice"));
            }
            catch (Exception e)
            {

            }

            return ps;
        }


        public void setServiceDescription(ServiceDescription serviceDescription)
        {
            ServiceDescription = serviceDescription;
            if (dlnaService != null)
                dlnaService.SetServiceDescription(serviceDescription);
            serviceDescription.Port = 8080;
        }


        public override void connect()
        {
            if (ServiceState != State.INITIAL)
            {
                ////Log.w("Connect SDK", "already connecting; not trying to connect again: " + state);
                return; // don't try to connect again while connected
            }

            if (!(ServiceConfig is NetcastTvServiceConfig))
            {
                ServiceConfig = new NetcastTvServiceConfig(ServiceConfig.ServiceUuid);
            }

            if ( DiscoveryManager.GetInstance().GetPairingLevel() == DiscoveryManager.PairingLevel.ON ) 
            {
                if (((NetcastTvServiceConfig) ServiceConfig).PairingKey != null
                    && ((NetcastTvServiceConfig) ServiceConfig).PairingKey.Length != 0)
                {
                    sendPairingKey(((NetcastTvServiceConfig) ServiceConfig).PairingKey);
                }
                else
                {
                    showPairingKeyOnTV();
                }

                //httpServer = new NetcastHttpServer(this, ServiceDescription.Port, mTextChangedListener);
                //httpServer.setSubscriptions(subscriptions);
                //httpServer.start();
            }
            //else
            //{
            //    hConnectSuccess();
            //}
        }

        public override void disconnect()
        {
            endPairing(null);

            connected = false;

            if (mServiceReachability != null)
                mServiceReachability.Stop();

            if (Listener != null)
                Listener.OnDisconnect(this, null);

            //if (httpServer != null)
            //{
            //    httpServer.stop();
            //    httpServer = null;
            //}

            ServiceState = State.INITIAL;
        }

        public override bool isConnectable()
        {
            return true;
        }

        public override bool isConnected()
        {
            return connected;
        }

        private void hConnectSuccess()
        {
            //  TODO:  Fix this for Netcast.  Right now it is using the InetAddress reachable function.  Need to use an HTTP Method.
//		mServiceReachability = DeviceServiceReachability.getReachability(serviceDescription.getIpAddress(), this);
//		mServiceReachability.start();

            connected = true;

            // Pairing was successful, so report connected and ready
            reportConnected(true);
        }

        public override void OnLoseReachability(DeviceServiceReachability reachability)
        {
            if (connected)
            {
                disconnect();
            }
            else
            {
                if (mServiceReachability != null)
                    mServiceReachability.Stop();
            }
        }

        public void hostByeBye()
        {
            disconnect();
        }

        //============= Auth ==============================
        public void showPairingKeyOnTV()
        {
            ServiceState = State.CONNECTING;

            ResponseListener responseListener = new ResponseListener();
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

            string requestURL = getUDAPRequestURL(UDAP_PATH_PAIRING);

            Dictionary<string, string> ps = new Dictionary<string, string>();
            ps.Add("name", "showKey");

            string httpMessage = getUDAPMessageBody(UDAP_API_PAIRING, ps);

            ServiceCommand command = new ServiceCommand(this, requestURL,
                httpMessage, responseListener);
            command.send();
        }

        // TODO add this when user cancel pairing
        public void removePairingKeyOnTV()
        {
            ResponseListener responseListener = new ResponseListener();
            responseListener.Success += (sender, args) => { };
            responseListener.Error += (sender, args) => { };

            string requestURL = getUDAPRequestURL(UDAP_PATH_PAIRING);

            Dictionary<string, string> ps = new Dictionary<string, string>();
            ps.Add("name", "CancelAuthKeyReq");

            string httpMessage = getUDAPMessageBody(UDAP_API_PAIRING, ps);

            ServiceCommand command = new ServiceCommand(this, requestURL,
                httpMessage, responseListener);
            command.send();
        }

        public override void sendPairingKey(string pairingKey)
        {
            ServiceState = State.PAIRING;

            if (!(ServiceConfig is NetcastTvServiceConfig))
            {
                ServiceConfig = new NetcastTvServiceConfig(ServiceConfig.ServiceUuid);
            }

            ResponseListener responseListener = new ResponseListener();

            responseListener.Success += (sender, args) =>
            {
                ServiceState = State.PAIRED;
                ((NetcastTvServiceConfig) ServiceConfig).PairingKey = pairingKey;
                hConnectSuccess();
            };
            responseListener.Error += (sender, args) =>
            {
                ServiceState = State.INITIAL;

                if (Listener != null)
                    Listener.OnConnectionFailure(this, new Exception(args.ToString()));
            };

            string requestURL = getUDAPRequestURL(UDAP_PATH_PAIRING);

            Dictionary<string, string> ps = new Dictionary<string, string>();
            ps.Add("name", "hello");
            ps.Add("value", pairingKey);
            ps.Add("port", ServiceDescription.Port.ToString());

            string httpMessage = getUDAPMessageBody(UDAP_API_PAIRING, ps);

            ServiceCommand command = new ServiceCommand(this, requestURL,
                httpMessage, responseListener);
            command.send();
        }

        private void endPairing(ResponseListener listener)
        {
            string requestURL = getUDAPRequestURL(UDAP_PATH_PAIRING);

            Dictionary<string, string> ps = new Dictionary<string, string>();
            ps.Add("name", "byebye");
            ps.Add("port", ServiceDescription.Port.ToString());

            string httpMessage = getUDAPMessageBody(UDAP_API_PAIRING, ps);

            ServiceCommand command = new ServiceCommand(this, requestURL,
                httpMessage, listener);
            command.send();
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

        private class NetcastTVLaunchSessionR : LaunchSession
        {
            private string appName;
            private NetcastTVService service;

            private NetcastTVLaunchSessionR(NetcastTVService service, string auid, string appName)
            {
                this.service = service;
                AppId = auid;
            }

            private NetcastTVLaunchSessionR(NetcastTVService service, JsonObject obj)
            {
                this.service = service;
                fromJsonObject(obj);
            }

            public void close(ResponseListener responseListener)
            {
            }

            public JsonObject toJsonObject()
            {
                JsonObject obj = base.ToJsonObject();
                obj.Add("type", JsonValue.CreateStringValue("netcasttv"));
                obj.Add("appName", JsonValue.CreateStringValue(appName));
                return obj;
            }

            public void fromJsonObject(JsonObject obj)
            {
                base.FromJsonObject(obj);
                appName = obj.GetNamedString("appName");
            }
        }

        public void getApplication(string appName, ResponseListener listener)
        {
            ResponseListener responseListener = new ResponseListener();
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



            string uri = UDAP_PATH_APPTOAPP_DATA + appName;
            string requestURL = getUDAPRequestURL(uri);

            ServiceCommand command = new ServiceCommand(this, requestURL, null,
                responseListener);
            command.HttpMethod = ServiceCommand.TYPE_GET;
            command.send();
        }

        public void LaunchApp(string appId, ResponseListener listener)
        {
            ResponseListener appInfoListener = new ResponseListener();
            appInfoListener.Success += (sender, args) =>
            {
                LaunchAppWithInfo((AppInfo) args, listener);
            };
            appInfoListener.Error += (sender, args) =>
            {
                Util.PostError(listener, args);
            };

            getAppInfoForId(appId, appInfoListener);
        }

        private void getAppInfoForId(string appId, ResponseListener listener)
        {
            ResponseListener appListListener = new ResponseListener();
            appListListener.Success += (sender, args) =>
            {
                foreach (AppInfo info in args as List<AppInfo>)
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

        private void launchApplication(string appName, string auid, string contentId, ResponseListener listener)
        {
            JsonObject jsonObj = new JsonObject();

            try
            {
                jsonObj.Add("id", JsonValue.CreateStringValue(auid));
                jsonObj.Add("title", JsonValue.CreateStringValue(appName));
            }
            catch (Exception e)
            {
                throw e;
            }

            ResponseListener responseListener = new ResponseListener();

            responseListener.Success += (sender, args) =>
            {
                LaunchSession launchSession = LaunchSession.LaunchSessionForAppId(auid);
                launchSession.AppName = appName;
                launchSession.Service = this;
                launchSession.SessionType = LaunchSessionType.App;

                Util.PostSuccess(listener, launchSession);
            };

            responseListener.Error += (sender, args) =>
            {
                Util.PostError(listener, (ServiceCommandError) args);
            };


            string requestURL = getUDAPRequestURL(UDAP_PATH_APPTOAPP_COMMAND);

            Dictionary<string, string> ps = new Dictionary<string, string>();
            ps.Add("name", "AppExecute");
            ps.Add("auid", auid);
            if (appName != null)
            {
                ps.Add("appname", appName);
            }
            if (contentId != null)
            {
                ps.Add("contentid", contentId);
            }

            string httpMessage = getUDAPMessageBody(UDAP_API_COMMAND, ps);

            ServiceCommand request = new ServiceCommand(this, requestURL,
                httpMessage, responseListener);
            request.send();
        }


        public void LaunchAppWithInfo(AppInfo appInfo, ResponseListener listener)
        {
            LaunchAppWithInfo(appInfo, null, listener);
        }


        public void LaunchAppWithInfo(AppInfo appInfo, Object ps, ResponseListener listener)
        {
            string appName = HttpMessage.Encode(appInfo.Name);
            string appId = appInfo.Id;
            string contentId = null;
            JsonObject mps = null;
            if (ps is JsonObject)
                mps = (JsonObject) ps;

            if (mps != null)
            {
                try
                {
                    contentId = (string) mps.GetNamedString("contentId");
                }
                catch (Exception e)
                {
                    throw e;
                }
            }

            launchApplication(appName, appId, contentId, listener);
        }


        public void LaunchBrowser(string url, ResponseListener listener)
        {
            //if ( !(url == null || url.Length == 0) ) 
            //Log.w("Connect SDK", "Netcast TV does not support deeplink for Browser");

            string appName = "Internet";

            ResponseListener appInfoListener = new ResponseListener();
            appInfoListener.Success += (sender, o) =>
            {
                string contentId = null;
                AppInfo ai = o as AppInfo;
                launchApplication(appName, ai.Id, contentId, listener);

            };

            appInfoListener.Error += (sender, o) =>
            {
                Util.PostError(listener, o);
            };
            getApplication(appName, appInfoListener);
        }


        public void LaunchYouTube(string contentId,  ResponseListener listener) {
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


        public void LaunchHulu(string contentId,  ResponseListener listener) {
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


        public void LaunchNetflix(string contentId,  ResponseListener listener) {
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


        public void LaunchAppStore(string appId,  ResponseListener listener) {
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
            string requestURL = getUDAPRequestURL(UDAP_PATH_APPTOAPP_COMMAND);

            Dictionary<string, string> ps = new Dictionary<string, string>();
            ps.Add("name", "AppTerminate");
            ps.Add("auid", launchSession.AppId);
            if (launchSession.AppName != null)
                ps.Add("appname", HttpMessage.Encode(launchSession.AppName));

            string httpMessage = getUDAPMessageBody(UDAP_API_COMMAND, ps);

            ServiceCommand command = new ServiceCommand(launchSession.Service,
                requestURL, httpMessage, listener);
            command.send();
        }

        private void getTotalNumberOfApplications(int type, ResponseListener listener)
        {
            ResponseListener responseListener = new ResponseListener();
            responseListener.Success += (sender, o) =>
            {
                var lea = o as LoadEventArgs;
                var load = lea.Load.GetPayload() as HttpResponseMessage;

                string strObj = load.Content.ReadAsStringAsync().Result;

                JsonObject jsonObject;
                JsonObject.TryParse(strObj, out jsonObject);

                var tarray = jsonObject.GetNamedArray("Channel List", new JsonArray());
                int applicationNumber = parseAppNumberXmlToJSON(strObj);

                Util.PostSuccess(listener, applicationNumber);
            };
            responseListener.Error += (sender, o) =>
            {
                Util.PostError(listener, o);
            };

            string requestURL = getUDAPRequestURL(UDAP_PATH_DATA, TARGET_APPNUM_GET, type.ToString());

            ServiceCommand command = new ServiceCommand(this, requestURL, null,
                responseListener);
            command.HttpMethod = ServiceCommand.TYPE_GET;
            command.send();
        }

        private void getApplications(int type, int number, ResponseListener listener)
        {
            ResponseListener responseListener = new ResponseListener();
            responseListener.Success += (sender, o) =>
            {
                var lea = o as LoadEventArgs;
                var load = lea.Load.GetPayload() as HttpResponseMessage;

                string strObj = load.Content.ReadAsStringAsync().Result;

                List<AppInfo> appList = new List<AppInfo>();

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

            string requestURL = getUDAPRequestURL(UDAP_PATH_DATA, TARGET_APPLIST_GET, type.ToString(), "0",
                number.ToString());

            ServiceCommand command = new ServiceCommand(this, requestURL, null,
                responseListener);
            command.HttpMethod = ServiceCommand.TYPE_GET;
            command.send();
        }


        public void GetAppList(ResponseListener listener)
        {
            applications.Clear();

            ResponseListener responseListener = new ResponseListener();
            responseListener.Success += (sender, o) =>
            {
                ResponseListener responseListener2 = new ResponseListener();
                responseListener2.Success += (sender2, o2) =>
                {
                    applications.AddRange((List<AppInfo>) (((o2 as LoadEventArgs).Load as ServiceCommandError).GetPayload()));
                    ResponseListener responseListener3 = new ResponseListener();
                    responseListener3.Success += (sender3, o3) =>
                    {
                        ResponseListener responseListener4 = new ResponseListener();
                        responseListener4.Success += (sender4, o4) =>
                        {
                            List<AppInfo> apps = (List<AppInfo>)(((o4 as LoadEventArgs).Load as ServiceCommandError).GetPayload());
                            applications.AddRange(apps);
                            Util.PostSuccess(listener, applications);
                        };

                        responseListener4.Error += (sender4, o4) =>
                        {
                            Util.PostError(listener, o4);
                        };
                        getApplications(3, (int)((o3 as LoadEventArgs).Load as ServiceCommandError).GetPayload(), responseListener4);
                    };

                    responseListener3.Error += (sender3, o3) =>
                    {
                        Util.PostError(listener, o3);
                    };
                    getTotalNumberOfApplications(3, responseListener3);
                };
                responseListener2.Error += (sender2, o2) =>
                {
                    Util.PostError(listener, o2);
                };
                getApplications(2, (int)((o as LoadEventArgs).Load as ServiceCommandError).GetPayload(), responseListener2);
            };

            responseListener.Error += (sender, o) =>
            {
                Util.PostError(listener, o);
            };
            getTotalNumberOfApplications(2, responseListener);


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
            string requestURL = string.Format("{0}{1}", getUDAPRequestURL(UDAP_PATH_APPTOAPP_DATA),
                string.Format("/{0}/status", launchSession.AppId));

            ResponseListener responseListener = new ResponseListener();
            responseListener.Success += (sender, o) =>
            {
                string response = (string) o;
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

            ServiceCommand command = new ServiceCommand(this, requestURL, null,
                responseListener);
            command.HttpMethod = ServiceCommand.TYPE_GET;
            command.send();
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
            string requestURL = getUDAPRequestURL(UDAP_PATH_DATA, TARGET_CHANNEL_LIST);

            ResponseListener responseListener = new ResponseListener();
            responseListener.Success += (sender, o) =>
            {
                var lea = o as LoadEventArgs;
                var load = lea.Load.GetPayload() as HttpResponseMessage;

                string strObj = load.Content.ReadAsStringAsync().Result;

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

            ServiceCommand request = new ServiceCommand(this, requestURL, null,
                responseListener);
            request.HttpMethod = ServiceCommand.TYPE_GET;
            request.send();
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
            ResponseListener responseListener = new ResponseListener();
            responseListener.Success += (sender, o) =>
            {
                List<ChannelInfo> channelList = o as List<ChannelInfo>;
                string requestURL = getUDAPRequestURL(UDAP_PATH_COMMAND);

                Dictionary<string, string> ps = new Dictionary<string, string>();

                for (int i = 0; i < channelList.Count; i++)
                {
                    ChannelInfo ch = channelList[i];
                    JsonObject rawData = ch.RawData;

                    try
                    {
                        string major = channelInfo.ChannelNumber.Split('-')[0];
                        string minor = channelInfo.ChannelNumber.Split('-')[1];

                        int majorNumber = ch.MajorNumber;
                        int minorNumber = ch.MinorNumber;

                        string sourceIndex = (string) rawData.GetNamedString("sourceIndex");
                        int physicalNum = (int) rawData.GetNamedNumber("physicalNumber");

                        if (major == majorNumber.ToString()
                            && minor == minorNumber.ToString())
                        {
                            ps.Add("name", "HandleChannelChange");
                            ps.Add("major", major);
                            ps.Add("minor", minor);
                            ps.Add("sourceIndex", sourceIndex);
                            ps.Add("physicalNum", physicalNum.ToString());

                            break;
                        }
                    }
                    catch (Exception e)
                    {
                        throw e;
                    }
                }

                string httpMessage = getUDAPMessageBody(UDAP_API_COMMAND, ps);

                ServiceCommand request = new ServiceCommand(this,requestURL,httpMessage,listener);
                request.send();
            };

            responseListener.Error += (sender, o) =>
            {
                Util.PostError(listener, o);
            };


            GetChannelList(responseListener);
        }


        public void GetCurrentChannel(ResponseListener listener)
        {
            string requestURL = getUDAPRequestURL(UDAP_PATH_DATA, TARGET_CURRENT_CHANNEL);
            ResponseListener responseListener = new ResponseListener();
            responseListener.Success += (sender, o) =>
            {
                string strObj = (string) o;

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
            ServiceCommand request = new ServiceCommand(this, requestURL, null,
                responseListener);
            request.send();
        }


        public IServiceSubscription SubscribeCurrentChannel(ResponseListener listener)
        {
            GetCurrentChannel(listener); // This is for the initial Current TV Channel Info.

            UrlServiceSubscription request = new UrlServiceSubscription(this,
                "ChannelChanged", null, null);
            request.HttpMethod = ServiceCommand.TYPE_GET;
            request.AddListener(listener);
            addSubscription(request as IServiceSubscription);

            return (IServiceSubscription)request;
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
            ResponseListener responseListener = new ResponseListener();
            responseListener.Success += (sender, o) =>
            {
                if (enabled != (bool) o)
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
            ResponseListener responseListener = new ResponseListener();
            responseListener.Success += (sender, o) =>
            {
                string strObj = (string) o;
                string upperStr = strObj.ToUpper();

                Util.PostSuccess(listener, upperStr.Contains("TRUE"));
            };

            responseListener.Error += (sender, o) =>
            {
                Util.PostError(listener, o);
            };

            string requestURL = getUDAPRequestURL(UDAP_PATH_DATA, TARGET_IS_3D);

            ServiceCommand request = new ServiceCommand(this, requestURL, null,
                responseListener);
            request.HttpMethod = ServiceCommand.TYPE_GET;
            request.send();
        }


        public IServiceSubscription Subscribe3DEnabled(ResponseListener listener)
        {
            Get3DEnabled(listener);

            UrlServiceSubscription request = new UrlServiceSubscription(this,
                TARGET_3D_MODE, null, null);
            request.HttpMethod = ServiceCommand.TYPE_GET;
            request.AddListener(listener);

            addSubscription(request);

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
            ResponseListener responseListener = new ResponseListener();
            responseListener.Success += (sender, o) =>
            {
                Util.PostSuccess(listener, ((VolumeStatus) o).Volume);
            };

            responseListener.Error += (sender, o) =>
            {
                Util.PostError(listener, o);
            };
            getVolumeStatus(responseListener);
        }


        public void SetMute(bool isMute, ResponseListener listener)
        {
            ResponseListener responseListener = new ResponseListener();
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
            getVolumeStatus(responseListener);
        }


        public void GetMute(ResponseListener listener)
        {
            ResponseListener responseListener = new ResponseListener();
            responseListener.Success += (sender, o) =>
            {
                Util.PostSuccess(listener, ((VolumeStatus) o).IsMute);
            };

            responseListener.Error += (sender, o) =>
            {
                Util.PostError(listener, o);
            };
            getVolumeStatus(responseListener);
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

        private void getVolumeStatus(ResponseListener listener)
        {
            ResponseListener responseListener = new ResponseListener();
            responseListener.Success += (sender, o) =>
            {
                string strObj =
                    (new StreamReader(
                        (((HttpResponseMessage) ((LoadEventArgs) o).Load.GetPayload()).Content.ReadAsStreamAsync()
                            .Result)))
                        .ReadToEnd();

                var reader = Util.GenerateStreamFromstring(strObj);
                var xmlReader = XmlReader.Create(reader);
                var number = "";
                bool isMute = false;
                int volume = 0;
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

            string requestURL = getUDAPRequestURL(UDAP_PATH_DATA, TARGET_VOLUME_INFO);

            ServiceCommand request = new ServiceCommand(this, requestURL, null,
                responseListener);
            request.HttpMethod = ServiceCommand.TYPE_GET;
            request.send();
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
            string appName = "Input List";
            string encodedStr = HttpMessage.Encode(appName);

            ResponseListener responseListener = new ResponseListener();
            responseListener.Success += (sender, o) =>
            {
                ResponseListener responseLaunchListener = new ResponseListener();
                responseListener.Success += (sender2, o2) =>
                {
                    if (inputPickerSession == null)
                    {
                        inputPickerSession = (LaunchSession) o2;
                    }

                    Util.PostSuccess(listener, (LaunchSession) o2);
                };

                responseListener.Error += (sender2, o2) =>
                {
                    Util.PostError(listener, o2);
                };
                launchApplication(appName, ((AppInfo) o).Id, null, responseLaunchListener);
            };

            responseListener.Error += (sender, o) =>
            {
                Util.PostError(listener, o);
            };

            getApplication(encodedStr, responseListener);


        }


        public void CloseInputPicker(LaunchSession launchSession, ResponseListener listener)
        {
            if (inputPickerSession != null)
            {
                inputPickerSession.Close(listener);
            }
        }


        public void GetExternalInputList(ExternalInputListListener listener)
        {
            // Do nothing - not Supported
            Util.PostError(listener, ServiceCommandError.NotSupported());
        }


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
            else
            {
                //System.err.println("DLNA Service is not ready yet");
            }
        }


        public void PlayMedia(string url, string mimeType, string title, string description, string iconSrc,
            bool shouldLoop, ResponseListener listener)
        {
            if (dlnaService != null)
            {
                dlnaService.PlayMedia(url, mimeType, title, description, iconSrc, shouldLoop, listener);
            }
            else
            {
                //System.err.println("DLNA Service is not ready yet");
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
            SendKeyCode((int) VirtualKeycodes.PLAY, listener);
        }


        public void Pause(ResponseListener listener)
        {
            SendKeyCode((int) VirtualKeycodes.PAUSE, listener);
        }


        public void Stop(ResponseListener listener)
        {
            SendKeyCode((int) VirtualKeycodes.STOP, listener);
        }


        public void Rewind(ResponseListener listener)
        {
            SendKeyCode((int) VirtualKeycodes.REWIND, listener);
        }


        public void FastForward(ResponseListener listener)
        {
            SendKeyCode((int) VirtualKeycodes.FAST_FORWARD, listener);
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

        private void setMouseCursorVisible(bool visible, ResponseListener listener)
        {
            string requestURL = getUDAPRequestURL(UDAP_PATH_EVENT);

            Dictionary<string, string> ps = new Dictionary<string, string>();
            ps.Add("name", "CursorVisible");
            ps.Add("value", visible ? "true" : "false");
            ps.Add("mode", "auto");

            string httpMessage = getUDAPMessageBody(UDAP_API_EVENT, ps);

            ServiceCommand request = new ServiceCommand(this, requestURL,
                httpMessage, listener);
            request.send();
        }


        public void ConnectMouse()
        {
            ResponseListener responseListener = new ResponseListener();
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

            setMouseCursorVisible(true, responseListener);
        }


        public void DisconnectMouse()
        {
            setMouseCursorVisible(false, null);
            isMouseConnected = false;
        }

        private bool isMouseConnected;
        public bool MouseConnected()
        {
            return isMouseConnected;
        }


        public void Click()
        {
            ResponseListener responseListener = new ResponseListener();

            string requestURL = getUDAPRequestURL(UDAP_PATH_COMMAND);

            Dictionary<string, string> ps = new Dictionary<string, string>();
            ps.Add("name", "HandleTouchClick");

            string httpMessage = getUDAPMessageBody(UDAP_API_COMMAND, ps);

            ServiceCommand request = new ServiceCommand(this, requestURL,
                httpMessage, responseListener);
            request.send();
        }


        public void Move(double dx, double dy)
        {
            mMouseDistance.X += dx;
            mMouseDistance.Y += dy;

            if (!mMouseIsMoving)
            {
                mMouseIsMoving = true;
                this.moveMouse();
            }
        }

        private void moveMouse()
        {
            string requestURL = getUDAPRequestURL(UDAP_PATH_COMMAND);

            int x = (int) mMouseDistance.X;
            int y = (int) mMouseDistance.Y;

            Dictionary<string, string> ps = new Dictionary<string, string>();
            ps.Add("name", "HandleTouchMove");
            ps.Add("x", x.ToString());
            ps.Add("y", y.ToString());

            mMouseDistance.X = mMouseDistance.Y = 0;

            NetcastTVService mouseService = this;

            ResponseListener responseListener = new ResponseListener();
            responseListener.Success += (sender, o) =>
            {
                if (mMouseDistance.X > 0 || mMouseDistance.Y > 0)
                    mouseService.moveMouse();
                else
                    mMouseIsMoving = false;
            };

            responseListener.Error += (sender, o) =>
            {
                mMouseIsMoving = false;
            };

            string httpMessage = getUDAPMessageBody(UDAP_API_COMMAND, ps);

            ServiceCommand request = new ServiceCommand(this, requestURL,
                httpMessage, responseListener);
            request.send();
        }


        public void Move(Point diff)
        {
            Move(diff.X, diff.Y);
        }


        public void Scroll(double dx, double dy)
        {
            ResponseListener responseListener = new ResponseListener();

            string requestURL = getUDAPRequestURL(UDAP_PATH_COMMAND);

            Dictionary<string, string> ps = new Dictionary<string, string>();
            ps.Add("name", "HandleTouchWheel");
            if (dy > 0)
                ps.Add("value", "up");
            else
                ps.Add("value", "down");

            string httpMessage = getUDAPMessageBody(UDAP_API_COMMAND, ps);

            ServiceCommand request = new ServiceCommand(this, requestURL,
                httpMessage, responseListener);
            request.send();
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

            UrlServiceSubscription request =
                new UrlServiceSubscription(this, "KeyboardVisible", null, null);
            request.AddListener(listener);

            addSubscription(request);

            return request;
        }


        public void SendText(string input)
        {
            //Log.d("Connect SDK", "Add to Queue: " + input);
            keyboardstring.Append(input);
            handleKeyboardInput("Editing", keyboardstring.ToString());
        }


        public void SendEnter()
        {
            ResponseListener responseListener = new ResponseListener();
            handleKeyboardInput("EditEnd", keyboardstring.ToString());
            SendKeyCode((int) VirtualKeycodes.RED, responseListener); // Send RED Key to enter the "ENTER" button
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

            handleKeyboardInput("Editing", keyboardstring.ToString());
        }

        private void handleKeyboardInput(string state, string buffer)
        {
            ResponseListener responseListener = new ResponseListener();

            string requestURL = getUDAPRequestURL(UDAP_PATH_EVENT);

            Dictionary<string, string> ps = new Dictionary<string, string>();
            ps.Add("name", "TextEdited");
            ps.Add("state", state);
            ps.Add("value", buffer);

            string httpMessage = getUDAPMessageBody(UDAP_API_EVENT, ps);

            ServiceCommand request = new ServiceCommand(this, requestURL,
                httpMessage, responseListener);
            request.send();
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
            SendKeyCode((int) VirtualKeycodes.KEY_UP, listener);
        }


        public void Down(ResponseListener listener)
        {
            SendKeyCode((int) VirtualKeycodes.KEY_DOWN, listener);
        }


        public void Left(ResponseListener listener)
        {
            SendKeyCode((int) VirtualKeycodes.KEY_LEFT, listener);
        }


        public void Right(ResponseListener listener)
        {
            SendKeyCode((int) VirtualKeycodes.KEY_RIGHT, listener);
        }


        public void Ok(ResponseListener listener)
        {
            SendKeyCode((int) VirtualKeycodes.OK, listener);
        }


        public void Back(ResponseListener listener)
        {
            SendKeyCode((int) VirtualKeycodes.BACK, listener);
        }


        public void Home(ResponseListener listener)
        {
            SendKeyCode((int) VirtualKeycodes.HOME, listener);
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
            ResponseListener responseListener = new ResponseListener();

            SendKeyCode((int) VirtualKeycodes.POWER, responseListener);
        }


        public void PowerOn(ResponseListener listener)
        {
            if (listener != null)
                listener.OnError(ServiceCommandError.NotSupported());
        }

        private JsonObject parseVolumeXmlToJSON(string data)
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
            return null;
        }

        private int parseAppNumberXmlToJSON(string data)
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

        private JsonArray parseApplicationsXmlToJSON(string data)
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

        public string getHttpMessageForHandleKeyInput(int keycode)
        {
            string strKeycode = keycode.ToString();

            Dictionary<string, string> ps = new Dictionary<string, string>();
            ps.Add("name", "HandleKeyInput");
            ps.Add("value", strKeycode);

            return getUDAPMessageBody(UDAP_API_COMMAND, ps);
        }


        public void SendKeyCode(int keycode, ResponseListener listener)
        {
            ResponseListener responseListener = new ResponseListener();

            responseListener.Success += (sender, o) =>
            {
                string requestURL = getUDAPRequestURL(UDAP_PATH_COMMAND);
                string httpMessage = getHttpMessageForHandleKeyInput(keycode);

                ServiceCommand request = new ServiceCommand(this, requestURL,
                    httpMessage, listener);
                request.send();
            };
            responseListener.Error += (sender, o) =>
            {
                Util.PostError(listener, o);
            };

            {
                setMouseCursorVisible(false, responseListener);
            }
        }

        public override void SendCommand(ServiceCommand mCommand)
        {

            Task t = new Task(() =>
            {
                ServiceCommand command = (ServiceCommand)mCommand;

                Object payload = command.Payload;

                var request = command.getRequest();
                request.Headers.Add(HttpMessage.USER_AGENT, HttpMessage.UDAP_USER_AGENT);


                //request.Headers.Add(HttpMessage.CONTENT_TYPE_HEADER, HttpMessage.CONTENT_TYPE);
                HttpWebResponse response = null;

                if (payload != null && command.HttpMethod.Equals(ServiceCommand.TYPE_POST))
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

        private void addSubscription(IServiceSubscription subscription)
        {
            subscriptions.Add(subscription);

            //if (httpServer != null)
            //    httpServer.setSubscriptions(subscriptions);
        }


        public override void Unsubscribe(UrlServiceSubscription subscription)
        {
            subscriptions.Remove(subscription);

            //if (httpServer != null)
            //    httpServer.setSubscriptions(subscriptions);
        }


        protected void setCapabilities()
        {
            
            if (DiscoveryManager.GetInstance().GetPairingLevel() == DiscoveryManager.PairingLevel.ON) 
            {
                appendCapabilites(TextInputControl.Capabilities.ToList());
                appendCapabilites(MouseControl.Capabilities.ToList());
                appendCapabilites(KeyControl.Capabilities.ToList());
                appendCapabilites(MediaPlayer.Capabilities.ToList());

                appendCapabilites(
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
                appendCapabilites(MediaPlayer.Capabilities.ToList());
                appendCapabilites(new List<string>
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