using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
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

        public enum ConnectionState
        {
            None,
            Initial,
            Connecting,
            Pairing,
            Paired,
            Disconnecting
        };

        private readonly HttpClient httpClient;
        private readonly DlnaService dlnaService;
        private LaunchSession inputPickerSession;
        private readonly List<AppInfo> applications;
        private readonly List<IServiceSubscription> subscriptions;
        private StringBuilder keyboardstring;
        private ConnectionState connectionState = ConnectionState.Initial;
        private Point mMouseDistance;
        private bool mMouseIsMoving;

        public ConnectionState ServiceConnectionState
        {
            get { return connectionState; }
            set { connectionState = value; }
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

        private static string GetUdapMessageBody(string api, Dictionary<string, string> ps)
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
        }

        public static JsonObject DiscoveryParameters()
        {
            var ps = new JsonObject
            {
                {"serviceId", JsonValue.CreateStringValue(Id)},
                {"filter", JsonValue.CreateStringValue("udap:rootservice")}
            };

            return ps;
        }

        public new static DiscoveryFilter DiscoveryFilter()
        {
            return new DiscoveryFilter(Id, "urn:schemas-upnp-org:device:MediaRenderer:1");
        }

        public override void Connect()
        {
            if (ServiceConnectionState != ConnectionState.Initial)
            {
                return; // don't try to connect again while connected
            }

            if (!(ServiceConfig is NetcastTvServiceConfig))
            {
                ServiceConfig = new NetcastTvServiceConfig(ServiceConfig.ServiceUuid);
            }

            if (DiscoveryManager.GetInstance().PairingLevel != DiscoveryManager.PairingLevelEnum.On) return;

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

            ServiceConnectionState = ConnectionState.Initial;
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

        public void ShowPairingKeyOnTv()
        {
            ServiceConnectionState = ConnectionState.Connecting;

            var responseListener = new ResponseListener
            (
                loadEventArg =>
                {
                    if (Listener != null)
                        Listener.OnPairingRequired(this, PairingType.PIN_CODE, null);
                },
                serviceCommandError =>
                {
                    ServiceConnectionState = ConnectionState.Initial;

                    if (Listener != null)
                        Listener.OnConnectionFailure(this, new Exception(serviceCommandError.ToString()));
                }
            );

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
            var responseListener = new ResponseListener
            (
                loadEventArg => { },
                serviceCommandError => { }
            );

            var requestUrl = GetUdapRequestUrl(UdapPathPairing);

            var ps = new Dictionary<string, string> { { "name", "CancelAuthKeyReq" } };

            var httpMessage = GetUdapMessageBody(UdapApiPairing, ps);

            var command = new ServiceCommand(this, requestUrl, httpMessage, responseListener);
            command.Send();
        }

        public override void SendPairingKey(string pairingKey)
        {
            ServiceConnectionState = ConnectionState.Pairing;

            if (!(ServiceConfig is NetcastTvServiceConfig))
            {
                ServiceConfig = new NetcastTvServiceConfig(ServiceConfig.ServiceUuid);
            }


            var responseListener = new ResponseListener
            (
                loadEventArg =>
                {
                    ServiceConnectionState = ConnectionState.Paired;
                    ((NetcastTvServiceConfig)ServiceConfig).PairingKey = pairingKey;
                    ConnectSuccess();
                },
                serviceCommandError =>
                {
                    ServiceConnectionState = ConnectionState.Initial;

                    if (Listener != null)
                        Listener.OnConnectionFailure(this, new Exception(serviceCommandError.ToString()));
                }
            );

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
            return CapabilityPriorityLevel.High;
        }

        public void GetApplication(string appName, ResponseListener listener)
        {

            var responseListener = new ResponseListener
            (
                loadEventArg =>
                {
                    var strObj = ((string)loadEventArg);

                    var appId = new AppInfo(Util.DecToHex(strObj));

                    if (!string.IsNullOrEmpty(strObj))
                    {
                        Util.PostSuccess(listener, appId);
                    }
                },
                serviceCommandError =>
                {
                    if (listener != null)
                        Util.PostError(listener, serviceCommandError);
                }
            );

            var uri = UdapPathApptoappData + appName;
            var requestUrl = GetUdapRequestUrl(uri);

            var command = new ServiceCommand(this, requestUrl, null, responseListener) { HttpMethod = ServiceCommand.TypeGet };
            command.Send();
        }

        public void LaunchApp(string appId, ResponseListener listener)
        {

            var responseListener = new ResponseListener
            (
                loadEventArg => LaunchAppWithInfo((AppInfo)loadEventArg, listener),
                serviceCommandError => Util.PostError(listener, serviceCommandError)
            );

            GetAppInfoForId(appId, responseListener);
        }

        private void GetAppInfoForId(string appId, ResponseListener listener)
        {

            var responseListener = new ResponseListener
            (
                loadEventArg =>
                {
                    var appInfos = loadEventArg as List<AppInfo>;
                    if (appInfos != null)
                        foreach (var info in appInfos)
                        {
                            if (info.Name.Equals(appId))
                            {
                                Util.PostSuccess(listener, info);
                            }
                            return;
                        }

                    Util.PostError(listener, new ServiceCommandError(0, null));
                },
                serviceCommandError => Util.PostError(listener, serviceCommandError)
            );

            GetAppList(responseListener);
        }

        private void LaunchApplication(string appName, string auid, string contentId, ResponseListener listener)
        {

            var responseListener = new ResponseListener
            (
                loadEventArg =>
                {
                    var launchSession = LaunchSession.LaunchSessionForAppId(auid);
                    launchSession.AppName = appName;
                    launchSession.Service = this;
                    launchSession.SessionType = LaunchSessionType.App;

                    Util.PostSuccess(listener, launchSession);
                },
                serviceCommandError => Util.PostError(listener, serviceCommandError)
            );

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
                contentId = mps.GetNamedString("contentId");
            }

            LaunchApplication(appName, appId, contentId, listener);
        }

        private void LaunchNamedApplication(string appName, ResponseListener listener)
        {

            var responseListener = new ResponseListener
            (
                loadEventArg =>
                {
                    var ai = loadEventArg as AppInfo;
                    if (ai != null) LaunchApplication(appName, ai.Id, null, listener);
                },
                serviceCommandError => Util.PostError(listener, serviceCommandError)
            );

            GetApplication(appName, responseListener);
        }

        public void LaunchBrowser(string url, ResponseListener listener)
        {
            const string appName = "Internet";
            LaunchNamedApplication(appName, listener);
        }

        public void LaunchYouTube(string contentId, ResponseListener listener)
        {
            const string appName = "YouTube";
            LaunchNamedApplication(appName, listener);
        }


        public void LaunchHulu(string contentId, ResponseListener listener)
        {
            const string appName = "Hulu";
            LaunchNamedApplication(appName, listener);
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

            var responseListener = new ResponseListener
            (
                loadEventArg =>
                {
                    var lea = loadEventArg as LoadEventArgs;
                    if (lea == null) return;
                    var load = lea.Load.GetPayload() as HttpResponseMessage;

                    if (load == null) return;

                    var strObj = load.Content.ReadAsStringAsync().Result;

                    JsonObject jsonObject;
                    JsonObject.TryParse(strObj, out jsonObject);

                    //var tarray = jsonObject.GetNamedArray("Channel List", new JsonArray());
                    var applicationNumber = ParseAppNumberXmlToJson(strObj);

                    Util.PostSuccess(listener, applicationNumber);
                },
                serviceCommandError => Util.PostError(listener, serviceCommandError)
            );

            var requestUrl = GetUdapRequestUrl(UdapPathData, TargetAppnumGet, type.ToString());

            var command = new ServiceCommand(this, requestUrl, null,
                responseListener) { HttpMethod = ServiceCommand.TypeGet };
            command.Send();
        }

        private void GetApplications(int type, int number, ResponseListener listener)
        {
            var responseListener = new ResponseListener
            (
                loadEventArg =>
                {
                    var lea = loadEventArg as LoadEventArgs;
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
                },
                serviceCommandError =>
                {

                }
            );

            var requestUrl = GetUdapRequestUrl(UdapPathData, TargetApplistGet, type.ToString(), "0",
                number.ToString());

            var command = new ServiceCommand(this, requestUrl, null, responseListener) { HttpMethod = ServiceCommand.TypeGet };
            command.Send();
        }

        public void GetAppList(ResponseListener listener)
        {
            applications.Clear();


            var responseListener = new ResponseListener
            (
                loadEventArg =>
                {

                    var responseListener2 = new ResponseListener
                    (
                        loadEventArg2 =>
                        {
                            var loadEventArgs = loadEventArg2 as LoadEventArgs;
                            if (loadEventArgs != null)
                                applications.AddRange((List<AppInfo>)(loadEventArgs.Load.GetPayload()));


                            var responseListener3 = new ResponseListener
                            (
                                loadEventArg3 =>
                                {

                                    var responseListener4 = new ResponseListener
                                    (
                                        loadEventArg4 =>
                                        {
                                            var eventArgs = loadEventArg4 as LoadEventArgs;
                                            if (eventArgs != null)
                                            {
                                                var apps = (List<AppInfo>)(eventArgs.Load.GetPayload());
                                                applications.AddRange(apps);
                                            }
                                            Util.PostSuccess(listener, applications);
                                        },
                                        serviceCommandError => Util.PostError(listener, serviceCommandError)
                                    );

                                    var args = loadEventArg3 as LoadEventArgs;
                                    if (args != null)
                                        GetApplications(3, (int)args.Load.GetPayload(), responseListener4);
                                },
                                serviceCommandError => Util.PostError(listener, serviceCommandError)
                            );

                            GetTotalNumberOfApplications(3, responseListener3);
                        },
                        serviceCommandError => Util.PostError(listener, serviceCommandError)
                    );

                    var loadEventArgs1 = loadEventArg as LoadEventArgs;
                    if (loadEventArgs1 != null)
                        GetApplications(2, (int)loadEventArgs1.Load.GetPayload(), responseListener2);
                },
                serviceCommandError => Util.PostError(listener, serviceCommandError)
            );

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


            var responseListener = new ResponseListener
            (
                loadEventArg =>
                {
                    var response = (string)loadEventArg;
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

                },
                serviceCommandError => Util.PostError(listener, serviceCommandError)
            );

            var command = new ServiceCommand(this, requestUrl, null, responseListener) { HttpMethod = ServiceCommand.TypeGet };
            command.Send();
        }

        public IServiceSubscription SubscribeAppState(LaunchSession launchSession, ResponseListener listener)
        {
            Util.PostError(listener, ServiceCommandError.NotSupported());
            return null;
        }

        #region TV Control

        public ITvControl GetTvControl()
        {
            return this;
        }

        public CapabilityPriorityLevel GetTvControlCapabilityLevel()
        {
            return CapabilityPriorityLevel.High;
        }

        public void GetChannelList(ResponseListener listener)
        {
            var requestUrl = GetUdapRequestUrl(UdapPathData, TargetChannelList);


            var responseListener = new ResponseListener
            (
                loadEventArg =>
                {
                    var lea = loadEventArg as LoadEventArgs;
                    if (lea == null) return;
                    var load = lea.Load.GetPayload() as HttpResponseMessage;

                    if (load == null) return;
                    var strObj = load.Content.ReadAsStringAsync().Result;

                    var ser = new XmlSerializer(typeof(envelope));
                    var obj = ser.Deserialize(new StringReader(strObj)) as envelope;

                    if (obj != null)
                    {
                        var channels = new List<ChannelInfo>();
                        for (var i = 0; i < obj.dataList.data.Count(); i++)
                        {
                            channels.Add(new ChannelInfo
                            {
                                ChannelId = obj.dataList.data[i].displayMajor.ToString(),
                                ChannelNumber = obj.dataList.data[i].displayMajor.ToString(),
                                ChannelName = obj.dataList.data[i].chname.ToString(),
                                MajorNumber = obj.dataList.data[i].displayMajor,
                                MinorNumber = obj.dataList.data[i].displayMinor,
                                SourceIndex = obj.dataList.data[i].sourceIndex,
                                PhysicalNumber = obj.dataList.data[i].physicalNum

                            });
                        }
                        Util.PostSuccess(listener, channels);
                    }
                    else
                    {
                        Util.PostError(listener, new ServiceCommandError(0, null));
                    }
                },
                serviceCommandError => Util.PostError(listener, serviceCommandError)
            );

            var command = new ServiceCommand(this, requestUrl, null, responseListener) { HttpMethod = ServiceCommand.TypeGet };
            command.Send();
        }

        public void ChannelUp(ResponseListener listener)
        {
            SendKeyCode((int)VirtualKeycodes.ChannelUp, listener);
        }

        public void ChannelDown(ResponseListener listener)
        {
            SendKeyCode((int)VirtualKeycodes.ChannelDown, listener);
        }

        public void SetChannel(ChannelInfo channelInfo, ResponseListener listener)
        {
            var responseListener = new ResponseListener
            (
                loadEventArg =>
                {
                    //((ConnectSdk.Windows.Service.Capability.Listeners.LoadEventArgs)(o)).Load.payload

                    var loa = loadEventArg as LoadEventArgs;
                    if (loa != null)
                    {
                        var channelList = loa.Load.GetPayload() as List<ChannelInfo>;
                        if (channelList != null)
                        {
                            var requestUrl = GetUdapRequestUrl(UdapPathCommand);

                            var ps = new Dictionary<string, string>();

                            foreach (var ch in channelList)
                            {
                                //var rawData = ch.RawData;

                                var major = channelInfo.MajorNumber;
                                var minor = channelInfo.MinorNumber;

                                var majorNumber = ch.MajorNumber;
                                var minorNumber = ch.MinorNumber;


                                var sourceIndex = ch.SourceIndex;
                                var physicalNum = ch.PhysicalNumber;

                                if (major != majorNumber || minor != minorNumber) continue;
                                ps.Add("name", "HandleChannelChange");
                                ps.Add("major", major.ToString());
                                ps.Add("minor", minor.ToString());
                                ps.Add("sourceIndex", sourceIndex.ToString());
                                ps.Add("physicalNum", physicalNum.ToString());

                                break;
                            }
                            var httpMessage = GetUdapMessageBody(UdapApiCommand, ps);

                            var request = new ServiceCommand(this, requestUrl, httpMessage, listener);
                            request.Send();
                        }
                        else
                        {
                            Util.PostError(listener, new ServiceCommandError(500, "Could not retrieve channel list"));
                        }
                    }
                    else
                    {
                        Util.PostError(listener, new ServiceCommandError(500, "Could not retrieve channel list"));
                    }
                },
                serviceCommandError => Util.PostError(listener, serviceCommandError)
            );

            GetChannelList(responseListener);
        }

        public void GetCurrentChannel(ResponseListener listener)
        {
            var requestUrl = GetUdapRequestUrl(UdapPathData, TargetCurrentChannel);


            var responseListener = new ResponseListener
            (
                loadEventArg =>
                {
                    //TODO: fix this
                    //var strObj = (string)o;
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
                },
                serviceCommandError => Util.PostError(listener, serviceCommandError)
            );

            var request = new ServiceCommand(this, requestUrl, null, responseListener);
            request.Send();
        }

        public IServiceSubscription SubscribeCurrentChannel(ResponseListener listener)
        {
            GetCurrentChannel(listener); // This is for the initial Current TV Channel Info.

            var request = new UrlServiceSubscription(this, "ChannelChanged", null, null)
            {
                HttpMethod = ServiceCommand.TypeGet
            };
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

            var responseListener = new ResponseListener
            (
                loadEventArg =>
                {
                    if (enabled != (bool)loadEventArg)
                    {
                        SendKeyCode((int)VirtualKeycodes.Video3D, listener);
                    }
                },
                serviceCommandError => Util.PostError(listener, serviceCommandError)
            );

            Get3DEnabled(responseListener);
        }

        public void Get3DEnabled(ResponseListener listener)
        {

            var responseListener = new ResponseListener
            (
                loadEventArg =>
                {
                    var strObj = (string)loadEventArg;
                    var upperStr = strObj.ToUpper();

                    Util.PostSuccess(listener, upperStr.Contains("TRUE"));
                },
                serviceCommandError => Util.PostError(listener, serviceCommandError)
            );

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

        #endregion

        #region Volume

        public IVolumeControl GetVolumeControl()
        {
            return this;
        }

        public CapabilityPriorityLevel GetVolumeControlCapabilityLevel()
        {
            return CapabilityPriorityLevel.High;
        }

        public void VolumeUp(ResponseListener listener)
        {
            SendKeyCode((int)VirtualKeycodes.VolumeUp, listener);
        }

        public void VolumeDown(ResponseListener listener)
        {
            SendKeyCode((int)VirtualKeycodes.VolumeDown, listener);
        }

        public void SetVolume(float volume, ResponseListener listener)
        {
            // Do nothing - not supported
            Util.PostError(listener, ServiceCommandError.NotSupported());
        }

        public void GetVolume(ResponseListener listener)
        {
            var responseListener = new ResponseListener
            (
                loadEventArg => Util.PostSuccess(listener, ((VolumeStatus)loadEventArg).Volume),
                serviceCommandError => Util.PostError(listener, serviceCommandError)
            );

            GetVolumeStatus(responseListener);
        }

        public void SetMute(bool isMute, ResponseListener listener)
        {
            var responseListener = new ResponseListener
            (
                loadEventArg => SendKeyCode((int)VirtualKeycodes.Mute, listener),
                serviceCommandError => Util.PostError(listener, serviceCommandError)
            );
            GetVolumeStatus(responseListener);
        }

        public void GetMute(ResponseListener listener)
        {
            var responseListener = new ResponseListener
            (
                loadEventArg => Util.PostSuccess(listener, ((VolumeStatus)loadEventArg).IsMute),
                serviceCommandError => Util.PostError(listener, serviceCommandError)
            );
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
            var responseListener = new ResponseListener
            (
                loadEventArg =>
                {
                    var strObj =
                        (new StreamReader(
                            (((HttpResponseMessage)((LoadEventArgs)loadEventArg).Load.GetPayload()).Content.ReadAsStreamAsync()
                                .Result)))
                            .ReadToEnd();

                    var reader = Util.GenerateStreamFromstring(strObj);
                    var xmlReader = XmlReader.Create(reader);
                    var isMute = false;
                    var volume = 0;
                    while (xmlReader.Read())
                    {
                        if (xmlReader.Name == "mute")
                            isMute = bool.Parse(xmlReader.ReadElementContentAsString());
                        if (xmlReader.Name == "level")
                            volume = int.Parse(xmlReader.ReadElementContentAsString());
                    }

                    Util.PostSuccess(listener, new VolumeStatus(isMute, volume));
                },
                serviceCommandError => Util.PostError(listener, serviceCommandError)
            );

            var requestUrl = GetUdapRequestUrl(UdapPathData, TargetVolumeInfo);

            var request = new ServiceCommand(this, requestUrl, null,
                responseListener) { HttpMethod = ServiceCommand.TypeGet };
            request.Send();
        }

        #endregion

        #region External Input

        public IExternalInputControl GetExternalInput()
        {
            return this;
        }

        public CapabilityPriorityLevel GetExternalInputControlPriorityLevel()
        {
            return CapabilityPriorityLevel.High;
        }

        public void LaunchInputPicker(ResponseListener pListener)
        {
            const string appName = "Input List";
            var encodedStr = HttpMessage.Encode(appName);


            var responseListener = new ResponseListener
            (
                loadEventArg =>
                {

                    var responseLaunchListener = new ResponseListener
                    (
                        loadEventArg2 =>
                        {
                            if (inputPickerSession == null)
                            {
                                inputPickerSession = (LaunchSession)loadEventArg2;
                            }

                            Util.PostSuccess(pListener, loadEventArg2);
                        },
                        serviceCommandError => Util.PostError(pListener, serviceCommandError)
                    );
                    LaunchApplication(appName, ((AppInfo)loadEventArg).Id, null, responseLaunchListener);
                },
                serviceCommandError => Util.PostError(pListener, serviceCommandError)
            );

            GetApplication(encodedStr, responseListener);
        }

        public void CloseInputPicker(LaunchSession launchSession, ResponseListener pListener)
        {
            if (inputPickerSession != null)
            {
                inputPickerSession.Close(pListener);
            }
        }

        public void SetExternalInput(ExternalInputInfo input, ResponseListener pListener)
        {
            // Do nothing - not Supported
            Util.PostError(pListener, ServiceCommandError.NotSupported());
        }

        #endregion

        #region Media Player

        public IMediaPlayer GetMediaPlayer()
        {
            return this;
        }

        public CapabilityPriorityLevel GetMediaPlayerCapabilityLevel()
        {
            return CapabilityPriorityLevel.High;
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
                Util.PostError(listener, new ServiceCommandError(0, null));
                return;
            }

            dlnaService.CloseMedia(launchSession, listener);
        }

        #endregion

        #region Media Control

        public IMediaControl GetMediaControl()
        {
            return this;
        }

        public CapabilityPriorityLevel GetMediaControlCapabilityLevel()
        {
            return CapabilityPriorityLevel.Normal;
        }

        public void Play(ResponseListener listener)
        {
            SendKeyCode((int)VirtualKeycodes.Play, listener);
        }

        public void Pause(ResponseListener listener)
        {
            SendKeyCode((int)VirtualKeycodes.Pause, listener);
        }

        public void Stop(ResponseListener listener)
        {
            SendKeyCode((int)VirtualKeycodes.Stop, listener);
        }

        public void Rewind(ResponseListener listener)
        {
            SendKeyCode((int)VirtualKeycodes.Rewind, listener);
        }

        public void FastForward(ResponseListener listener)
        {
            SendKeyCode((int)VirtualKeycodes.FastForward, listener);
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

            var responseListener = new ResponseListener
            (
                loadEventArg =>
                {
                    mMouseDistance = new Point(0, 0);
                    mMouseIsMoving = false;
                    isMouseConnected = true;
                },
                serviceCommandError =>
                {
                    isMouseConnected = false;
                }
            );
          
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
            if (isMouseConnected) ConnectMouse();

            var responseListener = new ResponseListener();

            var requestUrl = GetUdapRequestUrl(UdapPathCommand);

            var ps = new Dictionary<string, string> { { "name", "HandleTouchClick" } };

            var httpMessage = GetUdapMessageBody(UdapApiCommand, ps);

            var request = new ServiceCommand(this, requestUrl, httpMessage, responseListener);
            request.Send();
        }

        public void Move(double dx, double dy)
        {
            if (!isMouseConnected) ConnectMouse();
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


            var responseListener = new ResponseListener
            (
                loadEventArg =>
                {
                    if (mMouseDistance.X > 0 || mMouseDistance.Y > 0)
                        mouseService.MoveMouse();
                    else
                        mMouseIsMoving = false;
                },
                serviceCommandError =>
                {
                    mMouseIsMoving = false;
                }
            );

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
            if (isMouseConnected) ConnectMouse(); 

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
            keyboardstring = new StringBuilder();

            var request = new UrlServiceSubscription(this, "KeyboardVisible", null, null);
            request.AddListener(listener);

            AddSubscription(request);

            return request;
        }

        public void SendText(string input)
        {
            keyboardstring.Clear();
            keyboardstring.Append(input);
            HandleKeyboardInput("Editing", keyboardstring.ToString());
        }

        public void SendEnter()
        {
            var responseListener = new ResponseListener();
            HandleKeyboardInput("EditEnd", keyboardstring.ToString());
            SendKeyCode((int)VirtualKeycodes.Red, responseListener); // Send RED Key to enter the "ENTER" button
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

        public void Up(ResponseListener listener)
        {
            SendKeyCode((int)VirtualKeycodes.KeyUp, listener);
        }

        public void Down(ResponseListener listener)
        {
            SendKeyCode((int)VirtualKeycodes.KeyDown, listener);
        }

        public void Left(ResponseListener listener)
        {
            SendKeyCode((int)VirtualKeycodes.KeyLeft, listener);
        }

        public void Right(ResponseListener listener)
        {
            SendKeyCode((int)VirtualKeycodes.KeyRight, listener);
        }

        public void Ok(ResponseListener listener)
        {
            SendKeyCode((int)VirtualKeycodes.Ok, listener);
        }

        public void Back(ResponseListener listener)
        {
            SendKeyCode((int)VirtualKeycodes.Back, listener);
        }

        public void Home(ResponseListener listener)
        {
            SendKeyCode((int)VirtualKeycodes.Home, listener);
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
            SendKeyCode((int)VirtualKeycodes.Power, new ResponseListener());
        }

        public void PowerOn(ResponseListener listener)
        {
            if (listener != null)
                listener.OnError(ServiceCommandError.NotSupported());
        }

        #endregion

        private static int ParseAppNumberXmlToJson(string data)
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

            //throw new NotImplementedException();
        }

        public string GetHttpMessageForHandleKeyInput(int keycode)
        {
            var strKeycode = keycode.ToString();

            var ps = new Dictionary<string, string> { { "name", "HandleKeyInput" }, { "value", strKeycode } };

            return GetUdapMessageBody(UdapApiCommand, ps);
        }


        public void SendKeyCode(int keycode, ResponseListener pListener)
        {
            var responseListener = new ResponseListener
            (
                loadEventArg =>
                {
                    var requestUrl = GetUdapRequestUrl(UdapPathCommand);
                    var httpMessage = GetHttpMessageForHandleKeyInput(keycode);

                    var request = new ServiceCommand(this, requestUrl, httpMessage, pListener);
                    request.Send();
                },
                serviceCommandError => Util.PostError(pListener, serviceCommandError)
            );

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

                if (payload != null && command.HttpMethod.Equals(ServiceCommand.TypePost))
                {
                    request.Method = HttpMethod.Post;
                    request.Content =
                        new StreamContent(new MemoryStream(Encoding.UTF8.GetBytes(payload.ToString())));
                    request.Content.Headers.ContentType = new MediaTypeHeaderValue("text/xml") {CharSet = "utf-8"};
                }

                var res = httpClient.SendAsync(request).Result;
                if (res.IsSuccessStatusCode)
                {
                    Util.PostSuccess(command.ResponseListenerValue, res);
                }
                else
                {
                    Util.PostError(command.ResponseListenerValue, ServiceCommandError.GetError((int)res.StatusCode));
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

        protected void SetCapabilities()
        {

            if (DiscoveryManager.GetInstance().PairingLevel == DiscoveryManager.PairingLevelEnum.On)
            {
                AddCapabilities(TextInputControl.Capabilities.ToList());
                AddCapabilities(MouseControl.Capabilities.ToList());
                AddCapabilities(KeyControl.Capabilities.ToList());
                AddCapabilities(MediaPlayer.Capabilities.ToList());

                AddCapabilities(
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
                AddCapabilities(MediaPlayer.Capabilities.ToList());
                AddCapabilities(new List<string>
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

        public void Next(ResponseListener listener)
        {
            throw new NotImplementedException();
        }

        public void Previous(ResponseListener listener)
        {
            throw new NotImplementedException();
        }

        protected override void UpdateCapabilities()
        {
            var capabilities = new List<String>();

            if (DiscoveryManager.GetInstance().PairingLevel == DiscoveryManager.PairingLevelEnum.On)
            {
                foreach (var s in TextInputControl.Capabilities)
                {
                    capabilities.Add(s);
                }
                foreach (var s in MouseControl.Capabilities)
                {
                    capabilities.Add(s);
                }
                foreach (var s in KeyControl.Capabilities)
                {
                    capabilities.Add(s);
                }
                foreach (var s in MediaPlayer.Capabilities)
                {
                    capabilities.Add(s);
                }

                capabilities.Add(PowerControl.Off);

                capabilities.Add(MediaControl.Play);
                capabilities.Add(MediaControl.Pause);
                capabilities.Add(MediaControl.Stop);
                capabilities.Add(MediaControl.Rewind);
                capabilities.Add(MediaControl.FastForward);
                capabilities.Add(MediaControl.Duration);
                capabilities.Add(MediaControl.Position);
                capabilities.Add(MediaControl.Seek);

                capabilities.Add(Launcher.Application);
                capabilities.Add(Launcher.ApplicationClose);
                capabilities.Add(Launcher.ApplicationList);
                capabilities.Add(Launcher.Browser);
                capabilities.Add(Launcher.Hulu);
                capabilities.Add(Launcher.Netflix);
                capabilities.Add(Launcher.NetflixParams);
                capabilities.Add(Launcher.YouTube);
                capabilities.Add(Launcher.YouTubeParams);
                capabilities.Add(Launcher.AppStore);

                capabilities.Add(Launcher.AppStore);

                capabilities.Add(TvControl.ChannelUp);
                capabilities.Add(TvControl.ChannelDown);
                capabilities.Add(TvControl.ChannelGet);
                capabilities.Add(TvControl.ChannelList);
                capabilities.Add(TvControl.ChannelSubscribe);
                capabilities.Add(TvControl.Get_3D);
                capabilities.Add(TvControl.Set_3D);
                capabilities.Add(TvControl.Subscribe_3D);

                capabilities.Add(ExternalInputControl.PickerLaunch);
                capabilities.Add(ExternalInputControl.PickerClose);

                capabilities.Add(VolumeControl.VolumeGet);
                capabilities.Add(VolumeControl.VolumeUpDown);
                capabilities.Add(VolumeControl.MuteGet);
                capabilities.Add(VolumeControl.MuteSet);

                if (ServiceDescription.ModelNumber.Equals("4.0"))
                {
                    capabilities.Add(Launcher.AppStoreParams);
                }
            }
            else
            {
                foreach (var s in MediaPlayer.Capabilities)
                {
                    capabilities.Add(s);
                }

                capabilities.Add(MediaControl.Play);
                capabilities.Add(MediaControl.Pause);
                capabilities.Add(MediaControl.Stop);
                capabilities.Add(MediaControl.Rewind);
                capabilities.Add(MediaControl.FastForward);

                capabilities.Add(Launcher.YouTube);
                capabilities.Add(Launcher.YouTubeParams);
            }
            SetCapabilities(capabilities);
        }
    }
}