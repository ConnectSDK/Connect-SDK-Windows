using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using Windows.Data.Json;
using ConnectSdk.Windows.Core;
using ConnectSdk.Windows.Discovery;
using ConnectSdk.Windows.Etc.Helper;
using ConnectSdk.Windows.Service.Capability;
using ConnectSdk.Windows.Service.Capability.Listeners;
using ConnectSdk.Windows.Service.Command;
using ConnectSdk.Windows.Service.Config;
using ConnectSdk.Windows.Service.Sessions;

namespace ConnectSdk.Windows.Service
{
    public class DlnaService : DeviceService, IMediaControl, IMediaPlayer, IVolumeControl, IPlayListControl 
    {
        // ReSharper disable InconsistentNaming
        public static string ID = "DLNA";
        protected static string SUBSCRIBE = "SUBSCRIBE";
        protected static string UNSUBSCRIBE = "UNSUBSCRIBE";

        private const string DATA = "XMLData";
        private const string ACTION = "SOAPAction";
        private const string ACTION_CONTENT = "\"urn:schemas-upnp-org:service:AVTransport:1#{0}\"";

        public static string AV_TRANSPORT_URN = "urn:schemas-upnp-org:service:AVTransport:1";
        public static string CONNECTION_MANAGER_URN = "urn:schemas-upnp-org:service:ConnectionManager:1";
        public static string RENDERING_CONTROL_URN = "urn:schemas-upnp-org:service:RenderingControl:1";

        protected static string AV_TRANSPORT = "AVTransport";
        protected static string CONNECTION_MANAGER = "ConnectionManager";
        protected static string RENDERING_CONTROL = "RenderingControl";
        protected static string GROUP_RENDERING_CONTROL = "GroupRenderingControl";

        public static string PLAY_STATE = "playState";
        // ReSharper restore InconsistentNaming

        private readonly string controlUrl;

        public DlnaService(ServiceDescription serviceDescription, ServiceConfig serviceConfig)
            : base(serviceDescription, serviceConfig)
        {
            
        }

        public DlnaService(ServiceDescription serviceDescription, ServiceConfig serviceConfig, string controlUrl)
            : base(serviceDescription, serviceConfig)
        {
            this.controlUrl = controlUrl;
        }

        public new static DiscoveryFilter DiscoveryFilter()
        {
            return new DiscoveryFilter(ID, "urn:schemas-upnp-org:device:MediaRenderer:1");
        }


        //public void Stop(ResponseListener listener)
        //{
        //    const string method = "Stop";
        //    const string instanceId = "0";

        //    JsonObject payload = GetMethodBody(instanceId, method);

        //    var request = new ServiceCommand(this, method, payload, listener);
        //    request.Send();
        //}


        #region Media Control

        public IMediaControl GetMediaControl()
        {
            return this;
        }

        public CapabilityPriorityLevel GetMediaControlCapabilityLevel()
        {
            return CapabilityPriorityLevel.Normal;
        }

        public void Play(ResponseListener<object> listener)
        {
            const string method = "Play";
            const string instanceId = "0";

            var parameters = new Dictionary<string, string> { { "Speed", "1" } };

            var payload = GetMethodBody(AV_TRANSPORT_URN, instanceId, method, parameters);

            var request = new ServiceCommand<object>(this, method, payload, listener);
            request.Send();
        }

        public void Pause(ResponseListener<object> listener)
        {
            const string method = "Pause";
            const string instanceId = "0";

            var payload = GetMethodBody(AV_TRANSPORT_URN, instanceId, method, null);

            var request = new ServiceCommand<object>(this, method, payload, listener);
            request.Send();
        }

        public void Stop(ResponseListener<object> listener)
        {
            const string method = "Stop";
            const string instanceId = "0";

            var payload = GetMethodBody(AV_TRANSPORT_URN, method, instanceId, null);

            var request = new ServiceCommand<object>(this, method, payload, listener);
            request.Send();
        }

        public void Rewind(ResponseListener<object> listener)
        {
            Util.PostError(listener, ServiceCommandError.NotSupported());
        }

        public void FastForward(ResponseListener<object> listener)
        {
            Util.PostError(listener, ServiceCommandError.NotSupported());
        }

        #endregion


        #region playlist

        public IPlayListControl GetPlaylistControl()
        {
            return this;
        }

        public CapabilityPriorityLevel GetPlaylistControlCapabilityLevel()
        {
            return CapabilityPriorityLevel.Normal;
        }

        public void Previous(ResponseListener<object> listener)
        {
            const string method = "Previous";
            const string instanceId = "0";

            var payload = GetMethodBody(AV_TRANSPORT_URN, method, instanceId, null);

            var request = new ServiceCommand<object>(this, method, payload, listener);
            request.Send();
        }

        public void Next(ResponseListener<object> listener)
        {
            const string method = "Next";
            const string instanceId = "0";

            var payload = GetMethodBody(AV_TRANSPORT_URN, method, instanceId, null);

            var request = new ServiceCommand<object>(this, method, payload, listener);
            request.Send();
        }

        public void JumpToTrack(long index, ResponseListener<object> listener)
        {
            // DLNA requires start index from 1. 0 is a special index which means the end of media.
            ++index;
            Seek("TRACK_NR", index.ToString(), listener);
        }

        public void SetPlayMode(PlayMode playMode, ResponseListener<object> listener)
        {
            const string method = "SetPlayMode";
            const string instanceId = "0";
            string mode;

            switch (playMode)
            {
                case PlayMode.RepeatAll:
                    mode = "REPEAT_ALL";
                    break;
                case PlayMode.RepeatOne:
                    mode = "REPEAT_ONE";
                    break;
                case PlayMode.Shuffle:
                    mode = "SHUFFLE";
                    break;
                default:
                    mode = "NORMAL";
                    break;
            }

            var parameters = new Dictionary<String, String> { { "NewPlayMode", mode } };

            var payload = GetMethodBody(AV_TRANSPORT_URN, method, instanceId, parameters);

            var request = new ServiceCommand<object>(this, method, payload, listener);
            request.Send();
        }

        public void Seek(long position, ResponseListener<object> listener)
        {
            long second = (position / 1000) % 60;
            long minute = (position / (1000 * 60)) % 60;
            long hour = (position / (1000 * 60 * 60)) % 24;

            string time = string.Format("{0}:{1}:{2}", hour, minute, second);
            Seek("REL_TIME", time, listener);
        }


        private void GetPositionInfo(ResponseListener<object> listener)
        {
            const string method = "GetPositionInfo";
            const string instanceId = "0";

            string payload = GetMethodBody(AV_TRANSPORT_URN, instanceId, method, null);

            var responseListener = new ResponseListener<object>
                (
                loadEventArgs =>
                {
                    if (listener != null)
                    {
                        listener.OnSuccess(loadEventArgs);
                    }
                },
                serviceCommandError =>
                {
                    if (listener != null)
                        listener.OnError(serviceCommandError);
                }
                );


            var request = new ServiceCommand<object>(this, method, payload, responseListener);
            request.Send();
        }

        public void GetDuration(ResponseListener<long> listener)
        {
            var responseListener = new ResponseListener<object>(
                loadEventArgs =>
                {
                    var strDuration = Util.ParseData((string)loadEventArgs, "TrackDuration");
                    //string trackMetaData = Util.ParseData((string)args, "TrackMetaData");

                    var milliTimes = Util.ConvertStrTimeFormatToLong(strDuration) * 1000;

                    if (listener != null)
                    {
                        listener.OnSuccess(milliTimes);
                    }
                },
                serviceCommandError =>
                {
                    if (listener != null)
                    {
                        listener.OnError(serviceCommandError);
                    }
                }
                );
            GetPositionInfo(responseListener);
        }

        public void GetPosition(ResponseListener<long> listener)
        {
            var responseListener = new ResponseListener<object>(
                loadEventArgs =>
                {
                    string strDuration = Util.ParseData((string)loadEventArgs, "RelTime");

                    long milliTimes = Util.ConvertStrTimeFormatToLong(strDuration) * 1000;

                    if (listener != null)
                    {
                        listener.OnSuccess(milliTimes);
                    }
                },
                serviceCommandError =>
                {
                    if (listener != null)
                    {
                        listener.OnError(serviceCommandError);
                    }
                }
                );
            

            GetPositionInfo(responseListener);
        }

        protected void Seek(String unit, String target, ResponseListener<object> listener)
        {
            const string method = "Seek";
            const string instanceId = "0";

            var parameters = new Dictionary<String, String> { { "Unit", unit }, { "Target", target } };

            var payload = GetMethodBody(AV_TRANSPORT_URN, method, instanceId, parameters);

            var request = new ServiceCommand<object>(this, method, payload, listener);
            request.Send();
        }

        public static string GetMethodBody(String serviceUrn, string instanceId, string method, Dictionary<string, string> parameters)
        {
            var sb = new StringBuilder();

            sb.Append("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
            sb.Append(
                "<s:Envelope s:encodingStyle=\"http://schemas.xmlsoap.org/soap/encoding/\" xmlns:s=\"http://schemas.xmlsoap.org/soap/envelope/\">");

            sb.Append("<s:Body>");
            sb.Append("<u:" + method + " xmlns:u=\"" + serviceUrn + "\">");
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

            return sb.ToString();
        }

        public static string GetMetadata(string mediaUrl, string mime, string title)
        {
            const string id = "1000";
            const string parentId = "0";
            const string restricted = "0";
            string objectClass = null;
            var sb = new StringBuilder();

            sb.Append("&lt;DIDL-Lite xmlns=&quot;urn:schemas-upnp-org:metadata-1-0/DIDL-Lite/&quot; ");
            sb.Append("xmlns:upnp=&quot;urn:schemas-upnp-org:metadata-1-0/upnp/&quot; ");
            sb.Append("xmlns:dc=&quot;http://purl.org/dc/elements/1.1/&quot;&gt;");

            sb.Append("&lt;item id=&quot;" + id + "&quot; parentID=&quot;" + parentId + "&quot; restricted=&quot;" +
                      restricted + "&quot;&gt;");
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
            sb.Append("&lt;res protocolInfo=&quot;http-get:*:" + mime + ":DLNA.ORG_OP=01&quot;&gt;" + mediaUrl +
                      "&lt;/res&gt;");
            sb.Append("&lt;upnp:class&gt;" + objectClass + "&lt;/upnp:class&gt;");

            sb.Append("&lt;/item&gt;");
            sb.Append("&lt;/DIDL-Lite&gt;");

            return sb.ToString();
        }

        public void GetPlayState(ResponseListener<PlayStateStatus> listener)
        {
            const string method = "GetTransportInfo";
            const string instanceId = "0";

            var payload = GetMethodBody(AV_TRANSPORT_URN, method, instanceId, null);

            var responseListener = new ResponseListener<object>(
                loadEventArgs =>
                {
                    var transportState = Util.ParseData((String)loadEventArgs, "CurrentTransportState");
                    var status = (PlayStateStatus)Enum.Parse(typeof(PlayStateStatus), transportState);

                    Util.PostSuccess(listener, status);
                },
                serviceCommandError =>
                {
                    Util.PostError(listener, serviceCommandError);
                }
                );

            var request = new ServiceCommand<object>(this, method, payload, responseListener);

            request.Send();
        }

        public IServiceSubscription<PlayStateStatus> SubscribePlayState(ResponseListener<PlayStateStatus> listener)
        {
            var request = new UrlServiceSubscription<PlayStateStatus>(this, PLAY_STATE, null, null);
            request.AddListener(listener);
            AddSubscription(request);
            return request;

/*
            if (listener != null)
                listener.OnError(ServiceCommandError.NotSupported());

            return null;
*/
        }


        // ReSharper disable once UnusedParameter.Local
        private void AddSubscription(UrlServiceSubscription<PlayStateStatus> subscription)
        {
            // no server capability in winrt yet
            throw new NotSupportedException();
        }

        public void Unsubscribe(UrlServiceSubscription<PlayStateStatus> subscription)
        {
            // no server capability in winrt yet
            throw new NotSupportedException();
        }

        #endregion

        #region Media Player

        public IMediaPlayer GetMediaPlayer()
        {
            return this;
        }

        public CapabilityPriorityLevel GetMediaPlayerCapabilityLevel()
        {
            return CapabilityPriorityLevel.Normal;
        }

        public void GetMediaInfo(ResponseListener<MediaInfo> listener)
        {

            var responseListener = new ResponseListener<object>
            (
            loadEventArg =>
            {
                throw new NotImplementedException();
                /*
                var positionInfoXml = args as string;
                var trackMetaData = Util.ParseData(positionInfoXml, "TrackMetaData");

                var info = DlnaMediaInfoParser.GetMediaInfo(trackMetaData);
                if (listener != null)
                {
                    listener.OnSuccess(info);
                }
                */
            },
            serviceCommandError =>
            {
                if (listener != null)
                    listener.OnError(serviceCommandError);
            }
            );

 
            GetPosition(responseListener);
        }

        public IServiceSubscription<MediaInfo> SubscribeMediaInfo(ResponseListener<MediaInfo> listener)
        {
            var request = new UrlServiceSubscription<MediaInfo>(this, "info", null, null);
            request.AddListener(listener);
            AddSubscription(request);
            return request;
        }

        public void DisplayMedia(string url, string mimeType, string title, string description, string iconSrc,
             ResponseListener<LaunchSession> listener)
        {

            const string instanceId = "0";
            var mediaElements = mimeType.Split('/');
            var mediaType = mediaElements[0];
            var mediaFormat = mediaElements[1];

            if (string.IsNullOrEmpty(mediaType) || string.IsNullOrEmpty(mediaFormat))
            {
                Util.PostError(listener,
                    new ServiceCommandError(0, null));
                return;
            }

            mediaFormat = "mp3".Equals(mediaFormat) ? "mpeg" : mediaFormat;
            var mMimeType = String.Format("{0}/{1}", mediaType, mediaFormat);


            var responseListener = new ResponseListener<object>
            (
            loadEventArg =>
            {
                const string playMethod = "Play";

                var playParameters = new Dictionary<String, String> { { "Speed", "1" } };

                var playPayload = GetMethodBody(AV_TRANSPORT_URN, playMethod, "0", playParameters);

                var playResponseListener = new ResponseListener<object>
                (
                loadEventArg1 =>
                {
                    var launchSession = new LaunchSession { Service = this, SessionType = LaunchSessionType.Media };

                    if (listener != null)
                        listener.OnSuccess(launchSession);
                    //Util.PostSuccess(listener, new MediaLaunchObject(launchSession, this, this));
                },
                serviceCommandError1 =>
                {
                    Util.PostError(listener, serviceCommandError1);
                }
                );

                var playRequest = new ServiceCommand<object>(this, playMethod, playPayload, playResponseListener);
                playRequest.Send();
            },
            serviceCommandError =>
            {
                throw new Exception(serviceCommandError.ToString());
            }
            );

    
            const string setTransportMethod = "SetAVTransportURI";
            var metadata = GetMetadata(url, mMimeType, title);

            var setTransportParams = new Dictionary<String, String> {{"CurrentURI", url}, {"CurrentURIMetaData", metadata}};

            var setTransportPayload = GetMethodBody(AV_TRANSPORT_URN, setTransportMethod, instanceId, setTransportParams);

            var setTransportRequest = new ServiceCommand(this, setTransportMethod, setTransportPayload, responseListener);
            setTransportRequest.Send();

        }

        public void DisplayImage(string url, string mimeType, string title, string description, string iconSrc,
            ResponseListener<LaunchSession> listener)
        {
            DisplayMedia(url, mimeType, title, description, iconSrc, listener);
        }

        public void DisplayImage(MediaInfo mediaInfo, ResponseListener<LaunchSession> listener)
        {
            var imageInfo = mediaInfo.AllImages[0];
            var iconSrc = imageInfo.Url;

            DisplayImage(mediaInfo.Url, mediaInfo.MimeType, mediaInfo.Title, mediaInfo.Description, iconSrc, listener);
        }

        public void PlayMedia(string url, string mimeType, string title, string description, string iconSrc,
            bool shouldLoop, ResponseListener<LaunchSession> listener)
        {
            DisplayMedia(url, mimeType, title, description, iconSrc, listener);
        }

        public void PlayMedia(MediaInfo mediaInfo, bool shouldLoop, ResponseListener<LaunchSession>  listener)
        {
            var imageInfo = mediaInfo.AllImages[0];
            var iconSrc = imageInfo.Url;

            PlayMedia(mediaInfo.Url, mediaInfo.MimeType, mediaInfo.Title, mediaInfo.Description, iconSrc, shouldLoop, listener);
        }

        public void CloseMedia(LaunchSession launchSession, ResponseListener<object> listener)
        {
            var service = launchSession.Service as DlnaService<T>;
            if (service != null)
                service.Stop(listener);
        }

        #endregion

        #region volume control

        public IVolumeControl GetVolumeControl()
        {
            return this;
        }

        public CapabilityPriorityLevel GetVolumeControlCapabilityLevel()
        {
            return CapabilityPriorityLevel.Normal;
        }

        public void VolumeUp(ResponseListener<object> listener)
        {

            var responseListener = new ResponseListener<object>
            (
            loadEventArg =>
            {
                var volume = (float)loadEventArg;
                if (volume >= 1.0)
                {
                    if (listener != null)
                    {
                        listener.OnSuccess(loadEventArg);
                    }
                }
                else
                {
                    var newVolume = (float)(volume + 0.01);

                    if (newVolume > 1.0)
                        newVolume = (float)1.0;

                    SetVolume(newVolume, listener);

                    Util.PostSuccess(listener, null);
                }
            },
            serviceCommandError =>
            {
                if (listener != null)
                    listener.OnError(serviceCommandError);
            }
            );

            GetVolume(responseListener);
        }

        public void VolumeDown(ResponseListener<object> listener)
        {
            var responseListener = new ResponseListener<object>
            (
            loadEventArg =>
            {
                var volume = (float)loadEventArg;
                if (volume <= 0.0)
                {
                    if (listener != null)
                    {
                        listener.OnSuccess(loadEventArg);
                    }
                }
                else
                {
                    var newVolume = (float)(volume - 0.01);

                    if (newVolume > 1.0)
                        newVolume = (float)1.0;

                    SetVolume(newVolume, listener);

                    Util.PostSuccess(listener, null);
                }
            },
            serviceCommandError =>
            {
                if (listener != null)
                    listener.OnError(serviceCommandError);
            }
            );

            GetVolume(responseListener);
        }

        public void SetVolume(float volume, ResponseListener<object> listener)
        {
            const string method = "SetVolume";
            const string instanceId = "0";
            const string channel = "Master";
            var value = ((int)(volume * 100)).ToString();

            var parameters = new Dictionary<string, string> { { "Channel", channel }, { "DesiredVolume", value } };

            var payload = GetMethodBody(RENDERING_CONTROL_URN, instanceId, method, parameters);

            var request = new ServiceCommand(this, method, payload, listener);
            request.Send();
        }

        public void GetVolume(ResponseListener<float> listener)
        {
            const string method = "GetVolume";
            const string instanceId = "0";
            const string channel = "Master";
            
            var parameters = new Dictionary<string, string> { { "Channel", channel } };

            var payload = GetMethodBody(RENDERING_CONTROL_URN, instanceId, method, parameters);


            var responseListener = new ResponseListener<object>
            (
            loadEventArg =>
            {
                var currentVolume = Util.ParseData((String)loadEventArg, "CurrentVolume");
                var iVolume = 0;
                try
                {
                    iVolume = int.Parse(currentVolume);
                }
                // ReSharper disable once EmptyGeneralCatchClause
                catch
                {

                }
                var fVolume = (float)(iVolume / 100.0);

                Util.PostSuccess(listener, fVolume);
            },
            serviceCommandError =>
            {
                if (listener != null)
                    listener.OnError(serviceCommandError);
            }
            );

            var request = new ServiceCommand(this, method, payload, responseListener);
            request.Send();

        }

        public void SetMute(bool isMute, ResponseListener<bool> listener)
        {
            const string method = "SetVolume";
            const string instanceId = "0";
            const string channel = "Master";
            var muteStatus = (isMute) ? 1 : 0;


            var parameters = new Dictionary<string, string> { { "Channel", channel }, {"DesiredMute", muteStatus.ToString() }};

            var payload = GetMethodBody(RENDERING_CONTROL_URN, instanceId, method, parameters);

            var request = new ServiceCommand(this, method, payload, listener);
            request.Send();
        }

        public void GetMute(ResponseListener<object> listener)
        {
            const string method = "GetMute";
            const string instanceId = "0";
            const string channel = "Master";

            var parameters = new Dictionary<string, string> { { "Channel", channel } };

            var payload = GetMethodBody(RENDERING_CONTROL_URN, instanceId, method, parameters);


            var responseListener = new ResponseListener<object>
            (
            loadEventArg =>
            {
                var currentMute = Util.ParseData((String)loadEventArg, "CurrentMute");
                var isMute = bool.Parse(currentMute);

                Util.PostSuccess(listener, isMute);
            },
            serviceCommandError =>
            {
                if (listener != null)
                    listener.OnError(args);
            }
            );

            var request = new ServiceCommand<object>(this, method, payload, responseListener);
            request.Send();
        }

        public IServiceSubscription<float> SubscribeVolume(ResponseListener<float> listener)
        {
            // winrt does not support server
            throw new NotSupportedException();
        }

        public IServiceSubscription<bool> SubscribeMute(ResponseListener<bool> listener)
        {
            // winrt does not support server
            throw new NotSupportedException();
        }

        #endregion

        public static JsonObject DiscoveryParameters()
        {
            var ps = new JsonObject();

            try
            {
                ps.Add("serviceId", JsonValue.CreateStringValue(ID));
                ps.Add("filter", JsonValue.CreateStringValue("urn:schemas-upnp-org:device:MediaRenderer:1"));
            }
            // ReSharper disable once EmptyGeneralCatchClause
            catch 
            {
            }

            return ps;
        }


        public void SetServiceDescription(ServiceDescription serviceDescriptionParam)
        {
            var serviceList = serviceDescriptionParam.ServiceList;

            if (serviceList == null) return;
            foreach (Discovery.Provider.ssdp.Service service in serviceList)
            {
                if (service.ServiceType.Contains(AV_TRANSPORT))
                {
                }
                else if ((service.ServiceType.Contains(RENDERING_CONTROL)) && !(service.ServiceType.Contains(GROUP_RENDERING_CONTROL)))
                {
                }
                else if ((service.ServiceType.Contains(CONNECTION_MANAGER)))
                {
                }
            }
        }

        public static JsonObject GetSetAvTransportUriBody(string method, string instanceId, string mediaUrl, string mime,
            string title)
        {
            const string action = "SetAVTransportURI";
            var metadata = GetMetadata(mediaUrl, mime, title);

            var sb = new StringBuilder();
            sb.Append("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
            sb.Append(
                "<s:Envelope s:encodingStyle=\"http://schemas.xmlsoap.org/soap/encoding/\" xmlns:s=\"http://schemas.xmlsoap.org/soap/envelope/\">");

            sb.Append("<s:Body>");
            sb.Append("<u:" + action + " xmlns:u=\"urn:schemas-upnp-org:service:AVTransport:1\">");
            sb.Append("<InstanceID>" + instanceId + "</InstanceID>");
            sb.Append("<CurrentURI>" + mediaUrl + "</CurrentURI>");
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
            // ReSharper disable once EmptyGeneralCatchClause
            catch
            {
            }

            return obj;
        }

        public void SendCommand(ServiceCommand<object> command)
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
                    Util.PostError(command.ResponseListenerValue,
                        ServiceCommandError.GetError((int)response.StatusCode));
                }
            }
            // ReSharper disable once EmptyGeneralCatchClause
            catch
            {
            }
        }

        protected override void UpdateCapabilities()
        {
            var capabilities = new List<String>
            {
                MediaPlayer.DisplayImage,
                MediaPlayer.PlayVideo,
                MediaPlayer.PlayAudio,
                MediaPlayer.PlayPlaylist,
                MediaPlayer.Close,
                MediaPlayer.MetaDataTitle,
                MediaPlayer.MetaDataMimeType,
                MediaPlayer.MediaInfoGet,
                MediaPlayer.MediaInfoSubscribe,
                MediaControl.Play,
                MediaControl.Pause,
                MediaControl.Stop,
                MediaControl.Seek,
                MediaControl.Position,
                MediaControl.Duration,
                MediaControl.PlayState,
                MediaControl.PlayStateSubscribe,
                MediaControl.Previous,
                PlaylistControl.Next,
                PlaylistControl.Previous,
                PlaylistControl.JumpToTrack,
                PlaylistControl.SetPlayMode,
                VolumeControl.VolumeSet,
                VolumeControl.VolumeGet,
                VolumeControl.VolumeUpDown,
                VolumeControl.VolumeSubscribe,
                VolumeControl.MuteGet,
                VolumeControl.MuteSet,
                VolumeControl.MuteSubscribe
            };

            SetCapabilities(capabilities);
        }

        public LaunchSession DecodeLaunchSession(string type, JsonObject sessionObj)
        {
            if (type != "dlna") return null;

            var launchSession = LaunchSession.LaunchSessionFromJsonObject(sessionObj);
            launchSession.Service = this;

            return launchSession;
        }

        // ReSharper disable once UnusedMember.Local
        private bool IsXmlEncoded(string xml)
        {
            if (xml == null || xml.Length < 4)
            {
                return false;
            }
            return xml.Trim().Substring(0, 4).Equals("&lt;");
        }

        public override bool IsConnectable()
        {
            return true;
        }

        public override bool IsConnected()
        {
            return Connected;
        }

        public override void Connect()
        {
            //  TODO:  Fix this for roku.  Right now it is using the InetAddress reachable function.  Need to use an HTTP Method.
            //		mServiceReachability = DeviceServiceReachability.getReachability(serviceDescription.getIpAddress(), this);
            //		mServiceReachability.start();

            Connected = true;

            ReportConnected(true);
        }

        public override void Disconnect()
        {
            Connected = false;

            if (ServiceReachability != null)
                ServiceReachability.Stop();

            if (Listener != null)
                Listener.OnDisconnect(this, null);
        }

        // ReSharper disable once UnusedMember.Local
        private void GetDeviceCapabilities(ResponseListener<object> listener)
        {
            const string method = "GetDeviceCapabilities";
            const string instanceId = "0";

            var payload = GetMethodBody(AV_TRANSPORT_URN, method, instanceId, null);



            var responseListener = new ResponseListener<object>
                (
                (loadEventArg) =>
                {
                    if (listener != null)
                    {
                        listener.OnSuccess(loadEventArg);
                    }
                },
                (serviceCommandError) =>
                {
                    if (listener != null)
                    {
                        listener.OnError(serviceCommandError);
                    }
                }
                );

            var request = new ServiceCommand<object>(this, method, payload,responseListener);
            request.Send();
        }

        // ReSharper disable once UnusedMember.Local
        private void GetProtocolInfo(ResponseListener<object> listener)
        {
            const string method = "GetProtocolInfo";
            const string instanceId = "0";

            var payload = GetMethodBody(AV_TRANSPORT_URN, method, instanceId, null);


            var responseListener = new ResponseListener<object>
            (
            loadEventArg =>
            {
                if (listener != null)
                {
                    listener.OnSuccess(loadEventArg);
                }
            },
            serviceCommandError =>
            {
                if (listener != null)
                {
                    listener.OnError(serviceCommandError);
                }
            }
            );

            var request = new ServiceCommand<object>(this, method, payload, responseListener);
            request.Send();
        }


        public override void OnLoseReachability(DeviceServiceReachability reachability)
        {
            if (Connected)
            {
                Disconnect();
            }
            else
            {
                ServiceReachability.Stop();
            }
        }

        public void SubscribeServices()
        {
            // no server capability in winrt yet
            throw new NotSupportedException();
        }

        public void ResubscribeServices()
        {
            // no server capability in winrt yet
            throw new NotSupportedException();
        }

        public void UnsubscribeServices()
        {
            // no server capability in winrt yet
            throw new NotSupportedException();
        }
    }
}