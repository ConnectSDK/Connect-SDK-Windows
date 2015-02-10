using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using Windows.Data.Json;
using ConnectSdk.Windows.Core;
using MyRemote.ConnectSDK.Core;
using MyRemote.ConnectSDK.Etc.Helper;
using MyRemote.ConnectSDK.Service.Capability;
using MyRemote.ConnectSDK.Service.Capability.Listeners;
using MyRemote.ConnectSDK.Service.Command;
using MyRemote.ConnectSDK.Service.Config;
using MyRemote.ConnectSDK.Service.Sessions;

namespace MyRemote.ConnectSDK.Service
{
    public class DlnaService : DeviceService, IMediaControl, IMediaPlayer
    {
        // ReSharper disable InconsistentNaming
        public static string ID = "DLNA";
        private static string DATA = "XMLData";
        private static string ACTION = "SOAPAction";
        private static string ACTION_CONTENT = "\"urn:schemas-upnp-org:service:AVTransport:1#%s\"";
        // ReSharper restore InconsistentNaming

        string controlURL;

        public DlnaService(ServiceDescription serviceDescription, ServiceConfig serviceConfig)
            : base(serviceDescription, serviceConfig)
        {
        }

        public static JsonObject discoveryParameters()
        {
            JsonObject ps = new JsonObject();

            try
            {
                ps.Add("serviceId", JsonValue.CreateStringValue(ID));
                ps.Add("filter", JsonValue.CreateStringValue("urn:schemas-upnp-org:device:MediaRenderer:1"));
            }
            catch (Exception e)
            {

            }

            return ps;
        }


        public override void SetServiceDescription(ServiceDescription serviceDescription)
        {
            ServiceDescription = serviceDescription;
            StringBuilder sb = new StringBuilder();
            var serviceList = serviceDescription.ServiceList;

            if (serviceList != null)
            {
                for (int i = 0; i < serviceList.Count; i++)
                {
                    if (serviceList[i].ServiceType.Contains("AVTransport"))
                    {
                        sb.Append(serviceList[i].BaseUrl);
                        sb.Append(serviceList[i].ControlUrl);
                        break;
                    }
                }
                controlURL = sb.ToString();
            }
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
            return CapabilityPriorityLevel.NORMAL;
        }


        public void Stop(ResponseListener listener)
        {
            string method = "Stop";
            string instanceId = "0";

            JsonObject payload = getMethodBody(instanceId, method);

            ServiceCommand request = new ServiceCommand(this, method, payload, listener);
            request.send();
        }


        public void displayMedia(string url, string mimeType, string title, string description, string iconSrc, ResponseListener listener)
        {
            ResponseListener stopDisplayMediaListener = new ResponseListener();
            stopDisplayMediaListener.Success += (sender, args) =>
            {
                string instanceId = "0";
                string[] mediaElements = mimeType.Split('/');
                string mediaType = mediaElements[0];
                string mediaFormat = mediaElements[1];

                if (mediaType == null || mediaType.Length == 0 || mediaFormat == null || mediaFormat.Length == 0)
                {
                    Util.PostError(listener, new ServiceCommandError(0, "You must provide a valid mimeType (audio/*,  video/*, etc)", null));
                    return;
                }

                mediaFormat = "mp3".Equals(mediaFormat) ? "mpeg" : mediaFormat;
                string mMimeType = string.Format("{0}/{1}", mediaType, mediaFormat);

                ResponseListener playDisplayMediaListener = new ResponseListener();
                playDisplayMediaListener.Success += (sender2, args2) =>
                {
                    string playMethod = "Play";

                    Dictionary<string, string> parameters = new Dictionary<string, string>();
                    parameters.Add("Speed", "1");

                    JsonObject payload = getMethodBody(instanceId, playMethod, parameters);

                    ResponseListener playResponseListener = new ResponseListener();

                    playDisplayMediaListener.Success += (o, eventArgs) =>
                    {
                        LaunchSession launchSession = new LaunchSession();
                        launchSession.Service = this;
                        launchSession.SessionType = LaunchSessionType.Media;
                        Util.PostSuccess(listener, new MediaLaunchObject(launchSession, this));
                    };

                    playDisplayMediaListener.Error += (o, eventArgs) =>
                    {
                        if (listener != null)
                        {
                            listener.OnError(eventArgs);
                        }
                    };
                    ServiceCommand playrequest = new ServiceCommand(this, playMethod, payload, playResponseListener);
                    playrequest.send();
                };
                playDisplayMediaListener.Error += (sender2, args2) =>
                {
                    if (listener != null)
                    {
                        stopDisplayMediaListener.OnError(args2);
                    }
                };
                string method = "SetAVTransportURI";
                JsonObject httpMessage = getSetAVTransportURIBody(method, instanceId, url, mMimeType, title);

                ServiceCommand request = new ServiceCommand(this, method, httpMessage, playDisplayMediaListener);
                request.send();
            };

            stopDisplayMediaListener.Error += (sender, args) =>
            {
                throw new Exception(args.ToString());
            };
            Stop(stopDisplayMediaListener);
        }

        public void DisplayImage(string url, string mimeType, string title, string description, string iconSrc, ResponseListener listener)
        {
            displayMedia(url, mimeType, title, description, iconSrc, listener);
        }

        public void PlayMedia(string url, string mimeType, string title, string description, string iconSrc, bool shouldLoop, ResponseListener listener)
        {
            displayMedia(url, mimeType, title, description, iconSrc, listener);

        }

        public void CloseMedia(LaunchSession launchSession, ResponseListener listener)
        {
            if (launchSession.Service is DlnaService)
                ((DlnaService)launchSession.Service).Stop(listener);
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
            string method = "Play";
            string instanceId = "0";

            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add("Speed", "1");

            JsonObject payload = getMethodBody(instanceId, method, parameters);

            ServiceCommand request = new ServiceCommand(this, method, payload, listener);
            request.send();
        }

        public void Pause(ResponseListener listener)
        {
            string method = "Pause";
            string instanceId = "0";

            JsonObject payload = getMethodBody(instanceId, method);

            ServiceCommand request = new ServiceCommand(this, method, payload, listener);
            request.send();
        }

        public void Rewind(ResponseListener listener)
        {
            Util.PostError(listener, ServiceCommandError.NotSupported());
        }

        public void FastForward(ResponseListener listener)
        {
            Util.PostError(listener, ServiceCommandError.NotSupported());
        }

        public void Seek(long position, ResponseListener listener)
        {
            string method = "Seek";
            string instanceId = "0";

            long second = (position / 1000) % 60;
            long minute = (position / (1000 * 60)) % 60;
            long hour = (position / (1000 * 60 * 60)) % 24;

            string time = string.Format("{0}:{1}:{2}", hour, minute, second);

            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add("Unit", "REL_TIME");
            parameters.Add("Target", time);

            JsonObject payload = getMethodBody(instanceId, method, parameters);

            ServiceCommand request = new ServiceCommand(this, method, payload, listener);
            request.send();
        }

        private void getPositionInfo(ResponseListener listener)
        {
            string method = "GetPositionInfo";
            string instanceId = "0";

            JsonObject payload = getMethodBody(instanceId, method);

            ResponseListener responseListener = new ResponseListener();

            responseListener.Success += (sender, args) =>
            {
                if (listener != null)
                {
                    listener.OnSuccess(args);
                }
            };

            responseListener.Error += (sender, args) =>
            {
                if (listener != null)
                    listener.OnError(args);
            };
            ServiceCommand request = new ServiceCommand(this, method, payload, responseListener);
            request.send();
        }

        public void GetDuration(ResponseListener listener)
        {
            ResponseListener responseListener = new ResponseListener();

            responseListener.Success += (sender, args) =>
            {
                string strDuration = parseData((string)args, "TrackDuration");

                long milliTimes = convertStrTimeFormatToLong(strDuration) * 1000;

                if (listener != null)
                {
                    listener.OnSuccess(milliTimes);
                }
            };

            responseListener.Error += (sender, args) =>
            {
                if (listener != null)
                {
                    listener.OnError(args);
                }
            };

        }

        public void GetPosition(ResponseListener listener)
        {
            ResponseListener responseListener = new ResponseListener();

            responseListener.Success += (sender, args) =>
            {
                string strDuration = parseData((string)args, "RelTime");

                long milliTimes = convertStrTimeFormatToLong(strDuration) * 1000;

                if (listener != null)
                {
                    listener.OnSuccess(milliTimes);
                }
            };

            responseListener.Error += (sender, args) =>
            {
                if (listener != null)
                {
                    listener.OnError(args);
                }
            };
        }

        //public void getPlayState(ResponseListener listener)
        //{
        //    throw new NotImplementedException();
        //}

        //public ServiceSubscription subscribePlayState(ResponseListener listener)
        //{
        //    throw new NotImplementedException();
        //}

        public static JsonObject getSetAVTransportURIBody(string method, string instanceId, string mediaURL, string mime, string title)
        {
            string action = "SetAVTransportURI";
            string metadata = getMetadata(mediaURL, mime, title);

            StringBuilder sb = new StringBuilder();
            sb.Append("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
            sb.Append(
                "<s:Envelope s:encodingStyle=\"http://schemas.xmlsoap.org/soap/encoding/\" xmlns:s=\"http://schemas.xmlsoap.org/soap/envelope/\">");

            sb.Append("<s:Body>");
            sb.Append("<u:" + action + " xmlns:u=\"urn:schemas-upnp-org:service:AVTransport:1\">");
            sb.Append("<InstanceID>" + instanceId + "</InstanceID>");
            sb.Append("<CurrentURI>" + mediaURL + "</CurrentURI>");
            sb.Append("<CurrentURIMetaData>" + metadata + "</CurrentURIMetaData>");

            sb.Append("</u:" + action + ">");
            sb.Append("</s:Body>");
            sb.Append("</s:Envelope>");

            JsonObject obj = new JsonObject();
            try
            {
                obj.Add(DATA, JsonValue.CreateStringValue(sb.ToString()));
                obj.Add(ACTION, JsonValue.CreateStringValue(string.Format(ACTION_CONTENT, method)));
            }
            catch (Exception e)
            {

            }

            return obj;
        }

        private JsonObject getMethodBody(string instanceId, string method)
        {
            return getMethodBody(instanceId, method, null);
        }

        public static JsonObject getMethodBody(string instanceId, string method, Dictionary<string, string> parameters)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
            sb.Append("<s:Envelope s:encodingStyle=\"http://schemas.xmlsoap.org/soap/encoding/\" xmlns:s=\"http://schemas.xmlsoap.org/soap/envelope/\">");

            sb.Append("<s:Body>");
            sb.Append("<u:" + method + " xmlns:u=\"urn:schemas-upnp-org:service:AVTransport:1\">");
            sb.Append("<InstanceID>" + instanceId + "</InstanceID>");

            if (parameters != null)
            {
                foreach (var entry in parameters)
                {
                    string key = entry.Key;
                    string value = entry.Value;

                    sb.Append("<" + key + ">");
                    sb.Append(value);
                    sb.Append("</" + key + ">");
                }
            }

            sb.Append("</u:" + method + ">");
            sb.Append("</s:Body>");
            sb.Append("</s:Envelope>");

            JsonObject obj = new JsonObject();
            try
            {
                obj.Add(DATA, JsonValue.CreateStringValue(sb.ToString()));
                obj.Add(ACTION, JsonValue.CreateStringValue(string.Format(ACTION_CONTENT, method)));
            }
            catch (Exception e)
            {

            }

            return obj;
        }

        public static string getMetadata(string mediaURL, string mime, string title)
        {
            string id = "1000";
            string parentID = "0";
            string restricted = "0";
            string objectClass = null;
            StringBuilder sb = new StringBuilder();

            sb.Append("&lt;DIDL-Lite xmlns=&quot;urn:schemas-upnp-org:metadata-1-0/DIDL-Lite/&quot; ");
            sb.Append("xmlns:upnp=&quot;urn:schemas-upnp-org:metadata-1-0/upnp/&quot; ");
            sb.Append("xmlns:dc=&quot;http://purl.org/dc/elements/1.1/&quot;&gt;");

            sb.Append("&lt;item id=&quot;" + id + "&quot; parentID=&quot;" + parentID + "&quot; restricted=&quot;" + restricted + "&quot;&gt;");
            sb.Append("&lt;dc:title&gt;" + title + "&lt;/dc:title&gt;");

            if (mime.StartsWith("image"))
            {
                objectClass = "object.item.imageItem";
            }
            else if (mime.StartsWith("video"))
            {
                objectClass = "object.item.videoItem";
            }
            else if (mime.StartsWith("audio"))
            {
                objectClass = "object.item.audioItem";
            }
            sb.Append("&lt;res protocolInfo=&quot;http-get:*:" + mime + ":DLNA.ORG_OP=01&quot;&gt;" + mediaURL + "&lt;/res&gt;");
            sb.Append("&lt;upnp:class&gt;" + objectClass + "&lt;/upnp:class&gt;");

            sb.Append("&lt;/item&gt;");
            sb.Append("&lt;/DIDL-Lite&gt;");

            return sb.ToString();
        }

        public override void SendCommand(ServiceCommand mCommand)
        {

            ServiceCommand command = (ServiceCommand)mCommand;
            HttpClient httpClient = new HttpClient();

            JsonObject payload = (JsonObject)command.Payload;

            HttpRequestMessage request = HttpMessage.GetDlnaHttpPost(controlURL, command.Target);
            request.Headers.Add(ACTION, payload.GetNamedString(ACTION));
            try
            {
                request.Content =
                    new StreamContent(new MemoryStream(System.Text.Encoding.UTF8.GetBytes(payload.GetNamedString(DATA))));
            }
            catch (Exception e)
            {
                throw e;
            }



            try
            {

                var response = httpClient.SendAsync(request).Result;
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    string message = response.Content.ReadAsStringAsync().Result;
                    Util.PostSuccess(command.ResponseListenerValue, message);
                }
                else
                {
                    Util.PostError(command.ResponseListenerValue, ServiceCommandError.GetError((int)response.StatusCode));
                }


            }
            catch (Exception e)
            {

            }

        }

        protected void setCapabilities()
        {
            appendCapabilites(new List<string>{
                MediaPlayer.DisplayImage,
                MediaPlayer.DisplayVideo,
                MediaControl.Play,
                MediaPlayer.MetaDataTitle,
                MediaPlayer.MetaDataMimeType,
                MediaControl.Duration,
                MediaControl.Position,
                MediaControl.Seek}
                );
        }

        public override LaunchSession decodeLaunchSession(string type, JsonObject sessionObj)
        {
            if (type == "dlna")
            {
                LaunchSession launchSession = LaunchSession.LaunchSessionFromJsonObject(sessionObj);
                launchSession.Service = this;

                return launchSession;
            }
            return null;
        }

        private string parseData(string response, string key)
        {
            string startTag = "<" + key + ">";
            string endTag = "</" + key + ">";

            int start = response.IndexOf(startTag);
            int end = response.IndexOf(endTag);

            string data = response.Substring(start + startTag.Length, end);

            return data;
        }

        private long convertStrTimeFormatToLong(string strTime)
        {
            string[] tokens = strTime.Split(':');
            long time = 0;

            for (int i = 0; i < tokens.Length; i++)
            {
                time *= 60;
                time += int.Parse(tokens[i]);
            }

            return time;
        }

        public void GetPlayState(ResponseListener listener)
        {
            if (listener != null)
                listener.OnError(ServiceCommandError.NotSupported());
        }

        public IServiceSubscription SubscribePlayState(ResponseListener listener)
        {
            if (listener != null)
                listener.OnError(ServiceCommandError.NotSupported());

            return null;
        }

        public override bool isConnectable()
        {
            return true;
        }

        public override bool isConnected()
        {
            return connected;
        }

        public override void connect()
        {
            //  TODO:  Fix this for roku.  Right now it is using the InetAddress reachable function.  Need to use an HTTP Method.
            //		mServiceReachability = DeviceServiceReachability.getReachability(serviceDescription.getIpAddress(), this);
            //		mServiceReachability.start();

            connected = true;

            reportConnected(true);
        }

        public override void disconnect()
        {
            connected = false;

            if (mServiceReachability != null)
                mServiceReachability.Stop();

            if (Listener != null)
                Listener.OnDisconnect(this, null);
        }

        public void onLoseReachability(DeviceServiceReachability reachability)
        {
            if (connected)
            {
                disconnect();
            }
            else
            {
                mServiceReachability.Stop();
            }
        }
    }
}