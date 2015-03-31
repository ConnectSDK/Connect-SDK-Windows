using System;
using Windows.Data.Json;
using ConnectSdk.Windows.Core;
using ConnectSdk.Windows.Service.Capability;
using ConnectSdk.Windows.Service.Capability.Listeners;
using ConnectSdk.Windows.Service.Command;

namespace ConnectSdk.Windows.Service.Sessions
{
   public class WebAppSession : IMediaControl, IMediaPlayer
    {
        public WebAppSession(LaunchSession launchSession, DeviceService<object> service)
        {
            LaunchSession = launchSession;
            Service = service;
        }

        public LaunchSession LaunchSession { get; set; }

        public DeviceService<object> Service { get; set; }

        public IWebAppSessionListener WebAppSessionListener { get; set; }

        public IServiceSubscription<object> SubscribeWebAppStatus(ResponseListener<object> listener)
        {
            if (listener != null)
                listener.OnError(ServiceCommandError.NotSupported());

            return null;
        }

        public void Connect(ResponseListener<object> connectionListener)
        {
            Util.PostError(connectionListener, ServiceCommandError.NotSupported());
        }

        public void Join(ResponseListener<object> connectionListener)
        {
            Util.PostError(connectionListener, ServiceCommandError.NotSupported());
        }

        public void DisconnectFromWebApp()
        {
        }

        /// <summary>
        /// Pin the web app on the launcher.
        /// </summary>
        /// <param name="webAppId">NSString webAppId to be pinned.</param>
        /// <param name="listener"></param>
        public void PinWebApp(String webAppId, ResponseListener<object> listener)
        {
            Util.PostError(listener, ServiceCommandError.NotSupported());
        }

        /// <summary>
        /// UnPin  the web app on the launcher.
        /// </summary>
        /// <param name="webAppId">NSString webAppId to be unpinned</param>
        /// <param name="listener"></param>
        public void UnpinWebApp(String webAppId, ResponseListener<object> listener)
        {
            Util.PostError(listener, ServiceCommandError.NotSupported());
        }

        /// <summary>
        /// To check if the web app is pinned or not.
        /// </summary>
        /// <param name="webAppId">NSString webAppId that is checked</param>
        /// <param name="listener"></param>
        public void IsWebAppPinned(String webAppId, ResponseListener<bool> listener)
        {
            Util.PostError(listener, ServiceCommandError.NotSupported());
        }

        public void Close(ResponseListener<object> listener)
        {
            if (listener != null)
                listener.OnError(ServiceCommandError.NotSupported());
        }

        public void SendMessage(String message, ResponseListener<object> listener)
        {
            if (listener != null)
                listener.OnError(ServiceCommandError.NotSupported());
        }

        public void SendMessage(JsonObject message, ResponseListener<object> listener)
        {
            if (listener != null)
            {
                listener.OnError(ServiceCommandError.NotSupported());
            }
        }

        public IMediaControl GetMediaControl()
        {
            return null;
        }

        public CapabilityPriorityLevel GetMediaControlCapabilityLevel()
        {
            return CapabilityPriorityLevel.VeryLow;
        }


        public void GetMediaInfo(ResponseListener<MediaInfo> listener)
        {
            Util.PostError(listener, ServiceCommandError.NotSupported());
        }


        public IServiceSubscription<MediaInfo> SubscribeMediaInfo(ResponseListener<MediaInfo> listener)
        {
            listener.OnError(ServiceCommandError.NotSupported());
            return null;
        }

        public void Play(ResponseListener<object> listener)
        {
            IMediaControl mediaControl = null;

            if (Service != null)
                mediaControl = Service as IMediaControl;

            if (mediaControl != null)
                mediaControl.Play(listener);
            else if (listener != null)
                listener.OnError(ServiceCommandError.NotSupported());
        }

        public void Pause(ResponseListener<object> listener)
        {
            IMediaControl mediaControl = null;

            if (Service != null)
                mediaControl = Service as IMediaControl;

            if (mediaControl != null)
                mediaControl.Pause(listener);
            else if (listener != null)
                listener.OnError(ServiceCommandError.NotSupported());
        }

        public void Stop(ResponseListener<object> listener)
        {
            IMediaControl mediaControl = null;

            if (Service != null)
                mediaControl = Service as IMediaControl;

            if (mediaControl != null)
                mediaControl.Stop(listener);
            else if (listener != null)
                listener.OnError(ServiceCommandError.NotSupported());
        }

        public void Rewind(ResponseListener<object> listener)
        {
            IMediaControl mediaControl = null;

            if (Service != null)
                mediaControl = Service as IMediaControl;

            if (mediaControl != null)
                mediaControl.Rewind(listener);
            else if (listener != null)
                listener.OnError(ServiceCommandError.NotSupported());
        }

        public void FastForward(ResponseListener<object> listener)
        {
            IMediaControl mediaControl = null;

            if (Service != null)
                mediaControl = Service as IMediaControl;

            if (mediaControl != null)
                mediaControl.FastForward(listener);
            else if (listener != null)
                listener.OnError(ServiceCommandError.NotSupported());
        }

        public void Previous(ResponseListener<object> listener)
        {
            IMediaControl mediaControl = null;

            if (Service != null)
                mediaControl = Service as IMediaControl;

            if (mediaControl != null)
                mediaControl.Previous(listener);
            else if (listener != null)
                listener.OnError(ServiceCommandError.NotSupported());
        }

        public void Next(ResponseListener<object> listener)
        {
            IMediaControl mediaControl = null;

            if (Service != null)
                mediaControl = Service as IMediaControl;

            if (mediaControl != null)
                mediaControl.Next(listener);
            else if (listener != null)
                listener.OnError(ServiceCommandError.NotSupported());
        }

        public void Seek(long position, ResponseListener<object> listener)
        {
            IMediaControl mediaControl = null;

            if (Service != null)
                mediaControl = Service as IMediaControl;

            if (mediaControl != null)
                mediaControl.Seek(position, listener);
            else if (listener != null)
                listener.OnError(ServiceCommandError.NotSupported());
        }

        public void GetDuration(ResponseListener<long> listener)
        {
            IMediaControl mediaControl = null;

            if (Service != null)
                mediaControl = Service as IMediaControl;

            if (mediaControl != null)
                mediaControl.GetDuration(listener);
            else if (listener != null)
                listener.OnError(ServiceCommandError.NotSupported());
        }

        public void GetPosition(ResponseListener<long> listener)
        {
            IMediaControl mediaControl = null;

            if (Service != null)
                mediaControl = Service as IMediaControl;

            if (mediaControl != null)
                mediaControl.GetPosition(listener);
            else if (listener != null)
                listener.OnError(ServiceCommandError.NotSupported());
        }

        public void GetPlayState(ResponseListener<PlayStateStatus> listener)
        {
            IMediaControl mediaControl = null;

            if (Service != null)
                mediaControl = Service as IMediaControl;

            if (mediaControl != null)
                mediaControl.GetPlayState(listener);
            else if (listener != null)
                listener.OnError(ServiceCommandError.NotSupported());
        }

        public IServiceSubscription<PlayStateStatus> SubscribePlayState(ResponseListener<PlayStateStatus> listener)
        {
            IMediaControl mediaControl = null;

            if (Service != null)
                mediaControl = Service as IMediaControl;

            if (mediaControl != null)
                return mediaControl.SubscribePlayState(listener);
            if (listener != null)
                listener.OnError(ServiceCommandError.NotSupported());

            return null;
        }


       public void PlayMedia(string url, string mimeType, string title, string description, string iconSrc, bool shouldLoop,
           ResponseListener<MediaLaunchObject> listener)
       {
           Util.PostError(listener, ServiceCommandError.NotSupported());
       }

       public void CloseMedia(LaunchSession launchSession, ResponseListener<object> listener)
        {
            Util.PostError(listener, ServiceCommandError.NotSupported());
        }

        public void DisplayImage(String url, String mimeType, String title, String description, String iconSrc,
            ResponseListener<LaunchSession> listener)
        {
            Util.PostError(listener, ServiceCommandError.NotSupported());
        }

        public void DisplayImage(MediaInfo mediaInfo, ResponseListener<LaunchSession> listener)
        {
            Util.PostError(listener, ServiceCommandError.NotSupported());
        }

        public void PlayMedia(String url, String mimeType, String title, String description, String iconSrc,
            bool shouldLoop, ResponseListener<LaunchSession> listener)
        {
            Util.PostError(listener, ServiceCommandError.NotSupported());
        }

        public void PlayMedia(MediaInfo mediaInfo, bool shouldLoop, ResponseListener<LaunchSession> listener)
        {
            Util.PostError(listener, ServiceCommandError.NotSupported());
        }

        public IMediaPlayer GetMediaPlayer()
        {
            return null;
        }

        public CapabilityPriorityLevel GetMediaPlayerCapabilityLevel()
        {
            return CapabilityPriorityLevel.VeryLow;
        }

       public void DisplayImage(string url, string mimeType, string title, string description, string iconSrc,
           ResponseListener<MediaLaunchObject> listener)
       {
           Util.PostError(listener, ServiceCommandError.NotSupported());
       }

       //public IWebAppSessionListener GetWebAppSessionListener()
        //{
        //    return webAppListener;
        //}

        //public void SetWebAppSessionListener(IWebAppSessionListener listener)
        //{
        //    webAppListener = listener;
        //}

    }
}