#region Copyright Notice
/*
 * ConnectSdk.Windows
 * WebAppSession.cs
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
using Windows.Data.Json;
using ConnectSdk.Windows.Core;
using ConnectSdk.Windows.Service.Capability;
using ConnectSdk.Windows.Service.Capability.Listeners;
using ConnectSdk.Windows.Service.Command;

namespace ConnectSdk.Windows.Service.Sessions
{
   public class WebAppSession : IMediaControl, IMediaPlayer
    {
        public WebAppSession(LaunchSession launchSession, DeviceService service)
        {
            LaunchSession = launchSession;
            Service = service;
        }

        public LaunchSession LaunchSession { get; set; }

        public DeviceService Service { get; set; }

        public IWebAppSessionListener WebAppSessionListener { get; set; }

        public IServiceSubscription SubscribeWebAppStatus(ResponseListener listener)
        {
            if (listener != null)
                listener.OnError(ServiceCommandError.NotSupported());

            return null;
        }

        public void Connect(ResponseListener connectionListener)
        {
            Util.PostError(connectionListener, ServiceCommandError.NotSupported());
        }

        public void Join(ResponseListener connectionListener)
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
        public void PinWebApp(String webAppId, ResponseListener listener)
        {
            Util.PostError(listener, ServiceCommandError.NotSupported());
        }

        /// <summary>
        /// UnPin  the web app on the launcher.
        /// </summary>
        /// <param name="webAppId">NSString webAppId to be unpinned</param>
        /// <param name="listener"></param>
        public void UnpinWebApp(String webAppId, ResponseListener listener)
        {
            Util.PostError(listener, ServiceCommandError.NotSupported());
        }

        /// <summary>
        /// To check if the web app is pinned or not.
        /// </summary>
        /// <param name="webAppId">NSString webAppId that is checked</param>
        /// <param name="listener"></param>
        public void IsWebAppPinned(String webAppId, ResponseListener listener)
        {
            Util.PostError(listener, ServiceCommandError.NotSupported());
        }

        public void Close(ResponseListener listener)
        {
            if (listener != null)
                listener.OnError(ServiceCommandError.NotSupported());
        }

        public void SendMessage(String message, ResponseListener listener)
        {
            if (listener != null)
                listener.OnError(ServiceCommandError.NotSupported());
        }

        public void SendMessage(JsonObject message, ResponseListener listener)
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


        public void GetMediaInfo(ResponseListener listener)
        {
            Util.PostError(listener, ServiceCommandError.NotSupported());
        }


        public IServiceSubscription SubscribeMediaInfo(ResponseListener listener)
        {
            listener.OnError(ServiceCommandError.NotSupported());
            return null;
        }

        public void Play(ResponseListener listener)
        {
            IMediaControl mediaControl = null;

            if (Service != null)
                mediaControl = Service as IMediaControl;

            if (mediaControl != null)
                mediaControl.Play(listener);
            else if (listener != null)
                listener.OnError(ServiceCommandError.NotSupported());
        }

        public void Pause(ResponseListener listener)
        {
            IMediaControl mediaControl = null;

            if (Service != null)
                mediaControl = Service as IMediaControl;

            if (mediaControl != null)
                mediaControl.Pause(listener);
            else if (listener != null)
                listener.OnError(ServiceCommandError.NotSupported());
        }

        public void Stop(ResponseListener listener)
        {
            IMediaControl mediaControl = null;

            if (Service != null)
                mediaControl = Service as IMediaControl;

            if (mediaControl != null)
                mediaControl.Stop(listener);
            else if (listener != null)
                listener.OnError(ServiceCommandError.NotSupported());
        }

        public void Rewind(ResponseListener listener)
        {
            IMediaControl mediaControl = null;

            if (Service != null)
                mediaControl = Service as IMediaControl;

            if (mediaControl != null)
                mediaControl.Rewind(listener);
            else if (listener != null)
                listener.OnError(ServiceCommandError.NotSupported());
        }

        public void FastForward(ResponseListener listener)
        {
            IMediaControl mediaControl = null;

            if (Service != null)
                mediaControl = Service as IMediaControl;

            if (mediaControl != null)
                mediaControl.FastForward(listener);
            else if (listener != null)
                listener.OnError(ServiceCommandError.NotSupported());
        }

        public void Previous(ResponseListener listener)
        {
            IMediaControl mediaControl = null;

            if (Service != null)
                mediaControl = Service as IMediaControl;

            if (mediaControl != null)
                mediaControl.Previous(listener);
            else if (listener != null)
                listener.OnError(ServiceCommandError.NotSupported());
        }

        public void Next(ResponseListener listener)
        {
            IMediaControl mediaControl = null;

            if (Service != null)
                mediaControl = Service as IMediaControl;

            if (mediaControl != null)
                mediaControl.Next(listener);
            else if (listener != null)
                listener.OnError(ServiceCommandError.NotSupported());
        }

        public void Seek(long position, ResponseListener listener)
        {
            IMediaControl mediaControl = null;

            if (Service != null)
                mediaControl = Service as IMediaControl;

            if (mediaControl != null)
                mediaControl.Seek(position, listener);
            else if (listener != null)
                listener.OnError(ServiceCommandError.NotSupported());
        }

        public virtual void GetDuration(ResponseListener listener)
        {
            IMediaControl mediaControl = null;

            if (Service != null)
                mediaControl = Service as IMediaControl;

            if (mediaControl != null)
                mediaControl.GetDuration(listener);
            else if (listener != null)
                listener.OnError(ServiceCommandError.NotSupported());
        }

        public virtual void GetPosition(ResponseListener listener)
        {
            IMediaControl mediaControl = null;

            if (Service != null)
                mediaControl = Service as IMediaControl;

            if (mediaControl != null)
                mediaControl.GetPosition(listener);
            else if (listener != null)
                listener.OnError(ServiceCommandError.NotSupported());
        }

        public virtual void GetPlayState(ResponseListener listener)
        {
            IMediaControl mediaControl = null;

            if (Service != null)
                mediaControl = Service as IMediaControl;

            if (mediaControl != null)
                mediaControl.GetPlayState(listener);
            else if (listener != null)
                listener.OnError(ServiceCommandError.NotSupported());
        }

        public virtual IServiceSubscription SubscribePlayState(ResponseListener listener)
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


        public void CloseMedia(LaunchSession launchSession, ResponseListener listener)
        {
            Util.PostError(listener, ServiceCommandError.NotSupported());
        }

        public void DisplayImage(String url, String mimeType, String title, String description, String iconSrc,
            ResponseListener listener)
        {
            Util.PostError(listener, ServiceCommandError.NotSupported());
        }

        public void DisplayImage(MediaInfo mediaInfo, ResponseListener listener)
        {
            Util.PostError(listener, ServiceCommandError.NotSupported());
        }

        public void PlayMedia(String url, String mimeType, String title, String description, String iconSrc,
            bool shouldLoop, ResponseListener listener)
        {
            Util.PostError(listener, ServiceCommandError.NotSupported());
        }

        public void PlayMedia(MediaInfo mediaInfo, bool shouldLoop, ResponseListener listener)
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
    }
}