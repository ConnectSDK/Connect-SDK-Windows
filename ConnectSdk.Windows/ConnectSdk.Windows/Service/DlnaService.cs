#region Copyright Notice
/*
 * ConnectSdk.Windows
 * DlnaService.cs
 * 
 * Copyright (c) 2015 LG Electronics.
 * Created by Sorin S. Serban on 20-2-2015,
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
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using Windows.Data.Json;
using ConnectSdk.Windows.Annotations;
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
        private string avTransportUrl;
        private string renderingControlUrl;
        private string connectionControlUrl;

        public DlnaService(ServiceDescription serviceDescription, ServiceConfig serviceConfig)
            : base(serviceDescription, serviceConfig)
        {
            UpdateControlUrl(serviceDescription);
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

        public override CapabilityPriorityLevel GetPriorityLevel(CapabilityMethods clazz)
        {
            if (clazz is MediaPlayer)
                return GetMediaPlayerCapabilityLevel();
            if (clazz is MediaControl)
                return GetMediaControlCapabilityLevel();
            if (clazz is VolumeControl)
                return GetVolumeControlCapabilityLevel();
            if (clazz is PlaylistControl)
                return GetPlaylistControlCapabilityLevel();
            return CapabilityPriorityLevel.NotSupported;

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

        public void Play(ResponseListener listener)
        {
            const string method = "Play";
            const string instanceId = "0";

            var parameters = new Dictionary<string, string> { { "Speed", "1" } };

            var payload = GetMethodBody(AV_TRANSPORT_URN, instanceId, method, parameters);

            var request = new ServiceCommand(this, method, payload, listener);
            request.Send();
        }

        public void Pause(ResponseListener listener)
        {
            const string method = "Pause";
            const string instanceId = "0";

            var payload = GetMethodBody(AV_TRANSPORT_URN, instanceId, method, null);

            var request = new ServiceCommand(this, method, payload, listener);
            request.Send();
        }

        public void Stop(ResponseListener listener)
        {
            const string method = "Stop";
            const string instanceId = "0";

            var payload = GetMethodBody(AV_TRANSPORT_URN, method, instanceId, null);

            var request = new ServiceCommand(this, method, payload, listener);
            request.Send();
        }

        public void Rewind(ResponseListener listener)
        {
            Util.PostError(listener, ServiceCommandError.NotSupported());
        }

        public void FastForward(ResponseListener listener)
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

        public void Previous(ResponseListener listener)
        {
            const string method = "Previous";
            const string instanceId = "0";

            var payload = GetMethodBody(AV_TRANSPORT_URN, method, instanceId, null);

            var request = new ServiceCommand(this, method, payload, listener);
            request.Send();
        }

        public void Next(ResponseListener listener)
        {
            const string method = "Next";
            const string instanceId = "0";

            var payload = GetMethodBody(AV_TRANSPORT_URN, method, instanceId, null);

            var request = new ServiceCommand(this, method, payload, listener);
            request.Send();
        }

        public void JumpToTrack(long index, ResponseListener listener)
        {
            // DLNA requires start index from 1. 0 is a special index which means the end of media.
            ++index;
            Seek("TRACK_NR", index.ToString(), listener);
        }

        public void SetPlayMode(PlayMode playMode, ResponseListener listener)
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

            var request = new ServiceCommand(this, method, payload, listener);
            request.Send();
        }

        public void Seek(long position, ResponseListener listener)
        {
            long second = (position / 1000) % 60;
            long minute = (position / (1000 * 60)) % 60;
            long hour = (position / (1000 * 60 * 60)) % 24;

            string time = string.Format("{0}:{1}:{2}", hour, minute, second);
            Seek("REL_TIME", time, listener);
        }


        private void GetPositionInfo(ResponseListener listener)
        {
            const string method = "GetPositionInfo";
            const string instanceId = "0";

            string payload = GetMethodBody(AV_TRANSPORT_URN, instanceId, method, null);


            var responseListener = new ResponseListener
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
                    listener.OnError(serviceCommandError);
            }
            );


            var request = new ServiceCommand(this, method, payload, responseListener);
            request.Send();
        }

        public void GetDuration(ResponseListener listener)
        {
            var responseListener = new ResponseListener
            (
            loadEventArg =>
            {
                var strDuration = Util.ParseData((string)loadEventArg, "TrackDuration");
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

        public void GetPosition(ResponseListener listener)
        {

            var responseListener = new ResponseListener
            (
            loadEventArg =>
            {
                string strDuration = Util.ParseData((string)loadEventArg, "RelTime");

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

        protected void Seek(String unit, String target, ResponseListener listener)
        {
            const string method = "Seek";
            const string instanceId = "0";

            var parameters = new Dictionary<String, String> { { "Unit", unit }, { "Target", target } };

            var payload = GetMethodBody(AV_TRANSPORT_URN, method, instanceId, parameters);

            var request = new ServiceCommand(this, method, payload, listener);
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

        public void GetPlayState(ResponseListener listener)
        {
            const string method = "GetTransportInfo";
            const string instanceId = "0";

            var payload = GetMethodBody(AV_TRANSPORT_URN, method, instanceId, null);


            var responseListener = new ResponseListener
            (
                loadEventArg =>
                {
                    var transportState = Util.ParseData((String)loadEventArg, "CurrentTransportState");
                    var status = (PlayStateStatus)Enum.Parse(typeof(PlayStateStatus), transportState);

                    Util.PostSuccess(listener, status);
                },
                serviceCommandError => Util.PostError(listener, serviceCommandError));
            var request = new ServiceCommand(this, method, payload, responseListener);

            request.Send();
        }

        public IServiceSubscription SubscribePlayState(ResponseListener listener)
        {
            var request = new UrlServiceSubscription(this, PLAY_STATE, null, null);
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
        private static void AddSubscription(UrlServiceSubscription subscription)
        {
            // no server capability in winrt yet
            throw new NotSupportedException();
        }

        public override void Unsubscribe(UrlServiceSubscription subscription)
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

        public void GetMediaInfo(ResponseListener listener)
        {
            var responseListener = new ResponseListener
            (
                loadEventArg =>
                {
                    //todo: implement this
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
            GetPositionInfo(responseListener);
        }

        public IServiceSubscription SubscribeMediaInfo(ResponseListener listener)
        {
            var request = new UrlServiceSubscription(this, "info", null, null);
            request.AddListener(listener);
            AddSubscription(request);
            return request;
        }

        public void DisplayMedia(string url, string mimeType, string title, string description, string iconSrc,
             ResponseListener listener)
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


            var responseListener = new ResponseListener
            (
                loadEventArg =>
                {
                    const string playMethod = "Play";

                    var playParameters = new Dictionary<String, String> { { "Speed", "1" } };

                    var playPayload = GetMethodBody(AV_TRANSPORT_URN, playMethod, "0", playParameters);


                    var playResponseListener = new ResponseListener
                    (
                        loadEventArg2 =>
                        {
                            var launchSession = new LaunchSession { Service = this, SessionType = LaunchSessionType.Media };

                            Util.PostSuccess(listener, new MediaLaunchObject(launchSession, this, this));
                        },
                        serviceCommandError2 => Util.PostError(listener, serviceCommandError2));

                    var playRequest = new ServiceCommand(this, playMethod, playPayload, playResponseListener);
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
            ResponseListener listener)
        {
            DisplayMedia(url, mimeType, title, description, iconSrc, listener);
        }

        public void DisplayImage(MediaInfo mediaInfo, ResponseListener listener)
        {
            var imageInfo = mediaInfo.AllImages[0];
            var iconSrc = imageInfo.Url;

            DisplayImage(mediaInfo.Url, mediaInfo.MimeType, mediaInfo.Title, mediaInfo.Description, iconSrc, listener);
        }

        public void PlayMedia(string url, string mimeType, string title, string description, string iconSrc,
            bool shouldLoop, ResponseListener listener)
        {
            DisplayMedia(url, mimeType, title, description, iconSrc, listener);
        }

        public void PlayMedia(MediaInfo mediaInfo, bool shouldLoop, ResponseListener listener)
        {
            var imageInfo = mediaInfo.AllImages[0];
            var iconSrc = imageInfo.Url;

            PlayMedia(mediaInfo.Url, mediaInfo.MimeType, mediaInfo.Title, mediaInfo.Description, iconSrc, shouldLoop, listener);
        }

        public void CloseMedia(LaunchSession launchSession, ResponseListener listener)
        {
            var service = launchSession.Service as DlnaService;
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

        public void VolumeUp(ResponseListener listener)
        {

            var responseListener = new ResponseListener
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

        public void VolumeDown(ResponseListener listener)
        {

            var responseListener = new ResponseListener
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

        public void SetVolume(float volume, ResponseListener listener)
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

        public void GetVolume(ResponseListener listener)
        {
            const string method = "GetVolume";
            const string instanceId = "0";
            const string channel = "Master";
            
            var parameters = new Dictionary<string, string> { { "Channel", channel } };

            var payload = GetMethodBody(RENDERING_CONTROL_URN, instanceId, method, parameters);


            var responseListener = new ResponseListener
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

        public void SetMute(bool isMute, ResponseListener listener)
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

        public void GetMute(ResponseListener listener)
        {
            const string method = "GetMute";
            const string instanceId = "0";
            const string channel = "Master";

            var parameters = new Dictionary<string, string> { { "Channel", channel } };

            var payload = GetMethodBody(RENDERING_CONTROL_URN, instanceId, method, parameters);


            var responseListener = new ResponseListener
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
                        listener.OnError(serviceCommandError);
                }
            );

            var request = new ServiceCommand(this, method, payload, responseListener);
            request.Send();
        }

        public IServiceSubscription SubscribeVolume(ResponseListener listener)
        {
            // winrt does not support server
            throw new NotSupportedException();
        }

        public IServiceSubscription SubscribeMute(ResponseListener listener)
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


        public override void SetServiceDescription(ServiceDescription serviceDescriptionParam)
        {
            base.SetServiceDescription(serviceDescriptionParam);

            UpdateControlUrl(serviceDescriptionParam);
        }

        private void UpdateControlUrl(ServiceDescription serviceDescriptionParam)
        {
            var serviceList = serviceDescriptionParam.ServiceList;

            if (serviceList == null) return;
            foreach (Discovery.Provider.ssdp.Service service in serviceList)
            {
                if (!service.BaseUrl.EndsWith("/"))
                {
                    service.BaseUrl += "/";
                }

                if (service.ServiceType.Contains(AV_TRANSPORT))
                {
                    avTransportUrl = MakeControlUrl(service.BaseUrl, service.ControlUrl);
                }
                else if ((service.ServiceType.Contains(RENDERING_CONTROL)) && !(service.ServiceType.Contains(GROUP_RENDERING_CONTROL)))
                {
                    renderingControlUrl = MakeControlUrl(service.BaseUrl, service.ControlUrl);
                }
                else if ((service.ServiceType.Contains(CONNECTION_MANAGER)))
                {
                    connectionControlUrl = MakeControlUrl(service.BaseUrl, service.ControlUrl);
                }
            }
        }

        private String MakeControlUrl(String basePath, String path)
        {
            if (path.StartsWith("/"))
            {
                return basePath + path.Substring(1);
            }
            return basePath + path;
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

        public override void SendCommand(ServiceCommand command)
        {
            var httpClient = new HttpClient();
            var method = command.Target;
            var payload = (string)command.Payload;


            string targetURL = null;
            string serviceURN = null;

            if (payload.Contains(AV_TRANSPORT_URN))
            {
                targetURL = avTransportUrl;
                serviceURN = AV_TRANSPORT_URN;
            }
            else if (payload.Contains(RENDERING_CONTROL_URN))
            {
                targetURL = renderingControlUrl;
                serviceURN = RENDERING_CONTROL_URN;
            }
            else if (payload.Contains(CONNECTION_MANAGER_URN))
            {
                targetURL = connectionControlUrl;
                serviceURN = CONNECTION_MANAGER_URN;
            }

           // var request = HttpMessage.GetDlnaHttpPost(controlUrl, command.Target);

            var request = new HttpRequestMessage(HttpMethod.Post, targetURL);
            
            //request.Headers.Add("Content-Type", "text/xml; charset=utf-8");
            request.Headers.Add("SOAPAction", String.Format("\"{0}#{1}\"", serviceURN, method));
            try
            {
                request.Content =
                    new StreamContent(new MemoryStream(Encoding.UTF8.GetBytes(payload)));
                request.Content.Headers.ContentType = new MediaTypeHeaderValue("text/xml") { CharSet = "utf-8" };
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

        // ReSharper disable once UnusedMember.Local
        private void GetDeviceCapabilities(ResponseListener listener)
        {
            const string method = "GetDeviceCapabilities";
            const string instanceId = "0";

            var payload = GetMethodBody(AV_TRANSPORT_URN, method, instanceId, null);

            var responseListener = new ResponseListener
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

            var request = new ServiceCommand(this, method, payload,responseListener);
            request.Send();
        }

        // ReSharper disable once UnusedMember.Local
        private void GetProtocolInfo(ResponseListener listener)
        {
            const string method = "GetProtocolInfo";
            const string instanceId = "0";

            var payload = GetMethodBody(AV_TRANSPORT_URN, method, instanceId, null);


            var responseListener = new ResponseListener
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

            var request = new ServiceCommand(this, method, payload, responseListener);
            request.Send();
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