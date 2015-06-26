#region Copyright Notice
/*
 * ConnectSdk.Windows
 * ResponseListener.cs
 * 
 * Copyright (c) 2015, https://github.com/sdaemon
 * Created by Sorin S. Serban on 22-4-2015,
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
using ConnectSdk.Windows.Service.Command;
using ConnectSdk.Windows.Service.Sessions;

namespace ConnectSdk.Windows.Service.Capability.Listeners
{
    public class ResponseListener
    {
       
        // funcs to be sent as parameters
        private readonly Action<object> onSuccessFunc;
        private readonly Action<ServiceCommandError> onErrorFunc;

        public ResponseListener() { }

        /// <summary>
        /// Specific constructor with actions to be executed on events
        /// </summary>
        /// <param name="onSuccess">The action to be executed on success</param>
        /// <param name="onError">The action to be executed on error</param>
        public ResponseListener(Action<object> onSuccess, Action<ServiceCommandError> onError)
        {
            onSuccessFunc = onSuccess;
            onErrorFunc = onError;
        }

        public void OnSuccess(object obj)
        {
            if (onSuccessFunc != null)
                onSuccessFunc(new LoadEventArgs(obj));
        }

        public void OnError(ServiceCommandError obj)
        {
            if (onErrorFunc != null)
                onErrorFunc(obj);
        }

    }

    public class WebAppSessionListener : IWebAppSessionListener
    {

        // funcs to be sent as parameters
        private readonly Action<object, object> onReceiveMessageFunc;
        private readonly Action<object> onWebAppSessionDisconnect;

        public WebAppSessionListener() { }

        /// <summary>
        /// Specific constructor with actions to be executed on events
        /// </summary>
        /// <param name="onReceiveMessageFunc">The action to be executed on receive message</param>
        /// <param name="onWebAppSessionDisconnect">The action to be executed on disconnect</param>
        public WebAppSessionListener(Action<object, object> onReceiveMessageFunc, Action<object> onWebAppSessionDisconnect)
        {
            this.onReceiveMessageFunc = onReceiveMessageFunc;
            this.onWebAppSessionDisconnect = onWebAppSessionDisconnect;
        }

        public void OnReceiveMessage(WebAppSession webAppSession, object message)
        {
            if (onReceiveMessageFunc != null)
                onReceiveMessageFunc(new LoadEventArgs(webAppSession), new LoadEventArgs(message));
        }

        public void OnWebAppSessionDisconnect(WebAppSession webAppSession)
        {
            if (onWebAppSessionDisconnect != null)
                onWebAppSessionDisconnect(webAppSession);
        }
    }
}