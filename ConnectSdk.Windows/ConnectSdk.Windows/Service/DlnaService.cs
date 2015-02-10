using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using Windows.Data.Json;
using ConnectSdk.Windows.Core;
using ConnectSdk.Windows.Etc.Helper;
using ConnectSdk.Windows.Service.Capability;
using ConnectSdk.Windows.Service.Capability.Listeners;
using ConnectSdk.Windows.Service.Command;
using ConnectSdk.Windows.Service.Config;
using ConnectSdk.Windows.Service.Sessions;

namespace ConnectSdk.Windows.Service
{
    public class DlnaService : DeviceService, IMediaControl, IMediaPlayer
    {
        // ReSharper disable InconsistentNaming
        public static string ID = "DLNA";
        private const string DATA = "XMLData";
        private const string ACTION = "SOAPAction";
        private const string ACTION_CONTENT = "\"urn:schemas-upnp-org:service:AVTransport:1#%s\"";
        // ReSharper restore InconsistentNaming

        string controlUrl;

        public DlnaService(ServiceDescription serviceDescription, ServiceConfig serviceConfig)
            : base(serviceDescription, serviceConfig)
        {
        }

        public new static JsonObject DiscoveryParameters()
        {
            var ps = new JsonObject();

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


        public override void SetServiceDescription(ServiceDescription serviceDescriptionParam)
        {
            serviceDescription = serviceDescriptionParam;
            var sb = new StringBuilder();
            var serviceList = serviceDescription.ServiceList;

            if (serviceList == null) return;
            foreach (Core.Upnp.Service.Service service in serviceList)
            {
                if (!service.ServiceType.Contains("AVTransport")) continue;
                sb.Append(service.BaseUrl);
                sb.Append(service.ControlUrl);
                break;
            }
            controlUrl = sb.ToString();
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
            const string method = "Stop";
            const string instanceId = "0";

            var payload = GetMethodBody(instanceId, method);

            var request = new ServiceCommand(this, method, payload, listener);
            request.send();
        }


        public void DisplayMedia(string url, string mimeType, string title, string description, string iconSrc, ResponseListener listener)
        {
            var stopDisplayMediaListener = new ResponseListener();
            stopDisplayMediaListener.Success += (sender, args) =>
            {
                const string instanceId = "0";
                var mediaElements = mimeType.Split('/');
                var mediaType = mediaElements[0];
                var mediaFormat = mediaElements[1];

                if (string.IsNullOrEmpty(mediaType) || string.IsNullOrEmpty(mediaFormat))
                {
                    Util.PostError(listener, new ServiceCommandError(0, "You must provide a valid mimeType (audio/*,  video/*, etc)", null));
                    return;
                }

                mediaFormat = "mp3".Equals(mediaFormat) ? "mpeg" : mediaFormat;
                var mMimeType = string.Format("{0}/{1}", mediaType, mediaFormat);

                var playDisplayMediaListener = new ResponseListener();
                playDisplayMediaListener.Success += (sender2, args2) =>
                {
                    const string playMethod = "Play";

                    var parameters = new Dictionary<string, string> {{"Speed", "1"}};

                    var payload = GetMethodBody(instanceId, playMethod, parameters);

                    var playResponseListener = new ResponseListener();

                    playDisplayMediaListener.Success += (o, eventArgs) =>
                    {
                        var launchSession = new LaunchSession {Service = this, SessionType = LaunchSessionType.Media};
                        Util.PostSuccess(listener, new MediaLaunchObject(launchSession, this));
                    };

                    playDisplayMediaListener.Error += (o, eventArgs) =>
                    {
                        if (listener != null)
                        {
                            listener.OnError(eventArgs);
                        }
                    };
                    var playrequest = new ServiceCommand(this, playMethod, payload, playResponseListener);
                    playrequest.send();
                };
                playDisplayMediaListener.Error += (sender2, args2) =>
                {
                    if (listener != null)
                    {
                        stopDisplayMediaListener.OnError(args2);
                    }
                };
                const string method = "SetAVTransportURI";
                var httpMessage = GetSetAvTransportUriBody(method, instanceId, url, mMimeType, title);

                var request = new ServiceCommand(this, method, httpMessage, playDisplayMediaListener);
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
            DisplayMedia(url, mimeType, title, description, iconSrc, listener);
        }

        public void PlayMedia(string url, string mimeType, string title, string description, string iconSrc, bool shouldLoop, ResponseListener listener)
        {
            DisplayMedia(url, mimeType, title, description, iconSrc, listener);

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
            const string method = "Play";
            const string instanceId = "0";

            var parameters = new Dictionary<string, string> {{"Speed", "1"}};

            var payload = GetMethodBody(instanceId, method, parameters);

            var request = new ServiceCommand(this, method, payload, listener);
            request.send();
        }

        public void Pause(ResponseListener listener)
        {
            const string method = "Pause";
            const string instanceId = "0";

            var payload = GetMethodBody(instanceId, method);

            var request = new ServiceCommand(this, method, payload, listener);
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
            const string method = "Seek";
            const string instanceId = "0";

            var second = (position / 1000) % 60;
            var minute = (position / (1000 * 60)) % 60;
            var hour = (position / (1000 * 60 * 60)) % 24;

            var time = string.Format("{0}:{1}:{2}", hour, minute, second);

            var parameters = new Dictionary<string, string> {{"Unit", "REL_TIME"}, {"Target", time}};

            var payload = GetMethodBody(instanceId, method, parameters);

            var request = new ServiceCommand(this, method, payload, listener);
            request.send();
        }

        private void GetPositionInfo(ResponseListener listener)
        {
            const string method = "GetPositionInfo";
            const string instanceId = "0";

            var payload = GetMethodBody(instanceId, method);

            var responseListener = new ResponseListener();

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
            var request = new ServiceCommand(this, method, payload, responseListener);
            request.send();
        }

        public void GetDuration(ResponseListener listener)
        {
            var responseListener = new ResponseListener();

            responseListener.Success += (sender, args) =>
            {
                var strDuration = parseData((string)args, "TrackDuration");

                var milliTimes = ConvertStrTimeFormatToLong(strDuration) * 1000;

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
            var responseListener = new ResponseListener();

            responseListener.Success += (sender, args) =>
            {
                var strDuration = parseData((string)args, "RelTime");

                var milliTimes = ConvertStrTimeFormatToLong(strDuration) * 1000;

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

        public static JsonObject GetSetAvTransportUriBody(string method, string instanceId, string mediaURL, string mime, string title)
        {
            const string action = "SetAVTransportURI";
            var metadata = GetMetadata(mediaURL, mime, title);

            var sb = new StringBuilder();
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

            var obj = new JsonObject();
            try
            {
                obj.Add(DATA, JsonValue.CreateStringValue(sb.ToString()));
                obj.Add(ACTION, JsonValue.CreateStringValue(string.Format(ACTION_CONTENT, method)));
            }
            catch
            {

            }

            return obj;
        }

        private JsonObject GetMethodBody(string instanceId, string method)
        {
            return GetMethodBody(instanceId, method, null);
        }

        public static JsonObject GetMethodBody(string instanceId, string method, Dictionary<string, string> parameters)
        {
            var sb = new StringBuilder();

            sb.Append("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
            sb.Append("<s:Envelope s:encodingStyle=\"http://schemas.xmlsoap.org/soap/encoding/\" xmlns:s=\"http://schemas.xmlsoap.org/soap/envelope/\">");

            sb.Append("<s:Body>");
            sb.Append("<u:" + method + " xmlns:u=\"urn:schemas-upnp-org:service:AVTransport:1\">");
            sb.Append("<InstanceID>" + instanceId + "</InstanceID>");

            if (parameters != null)
            {
                foreach (var entry in parameters)
                {
                    var key = entry.Key;
                    var value = entry.Value;

                    sb.Append("<" + key + ">");
                    sb.Append(value);
                    sb.Append("</" + key + ">");
                }
            }

            sb.Append("</u:" + method + ">");
            sb.Append("</s:Body>");
            sb.Append("</s:Envelope>");

            var obj = new JsonObject();
            try
            {
                obj.Add(DATA, JsonValue.CreateStringValue(sb.ToString()));
                obj.Add(ACTION, JsonValue.CreateStringValue(string.Format(ACTION_CONTENT, method)));
            }
            catch
            {

            }

            return obj;
        }

        public static string GetMetadata(string mediaURL, string mime, string title)
        {
            const string id = "1000";
            const string parentID = "0";
            const string restricted = "0";
            string objectClass = null;
            var sb = new StringBuilder();

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

        public override void SendCommand(ServiceCommand command)
        {
            var httpClient = new HttpClient();

            var payload = (JsonObject)command.Payload;

            var request = HttpMessage.GetDlnaHttpPost(controlUrl, command.Target);
            request.Headers.Add(ACTION, payload.GetNamedString(ACTION));
            try
            {
                request.Content =
                    new StreamContent(new MemoryStream(Encoding.UTF8.GetBytes(payload.GetNamedString(DATA))));
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
                    var message = response.Content.ReadAsStringAsync().Result;
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

        protected void SetCapabilities()
        {
            AppendCapabilites(new List<string>{
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

        public LaunchSession DecodeLaunchSession(string type, JsonObject sessionObj)
        {
            if (type != "dlna") return null;
            
            var launchSession = LaunchSession.LaunchSessionFromJsonObject(sessionObj);
            launchSession.Service = this;

            return launchSession;
        }

        //private string parseData(string response, string key)
        //{
        //    string startTag = "<" + key + ">";
        //    string endTag = "</" + key + ">";

        //    int start = response.IndexOf(startTag);
        //    int end = response.IndexOf(endTag);

        //    string data = response.Substring(start + startTag.Length, end);

        //    return data;
        //}

        private static long ConvertStrTimeFormatToLong(string strTime)
        {
            var tokens = strTime.Split(':');
            long time = 0;

            foreach (string token in tokens)
            {
                time *= 60;
                time += int.Parse(token);
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

        public override bool IsConnectable()
        {
            return true;
        }

        public override bool IsConnected()
        {
            return connected;
        }

        public override void Connect()
        {
            //  TODO:  Fix this for roku.  Right now it is using the InetAddress reachable function.  Need to use an HTTP Method.
            //		mServiceReachability = DeviceServiceReachability.getReachability(serviceDescription.getIpAddress(), this);
            //		mServiceReachability.start();

            connected = true;

            ReportConnected(true);
        }

        public override void Disconnect()
        {
            connected = false;

            if (mServiceReachability != null)
                mServiceReachability.Stop();

            if (Listener != null)
                Listener.OnDisconnect(this, null);
        }

        public override void OnLoseReachability(DeviceServiceReachability reachability)
        {
            if (connected)
            {
                Disconnect();
            }
            else
            {
                mServiceReachability.Stop();
            }
        }
    }
}