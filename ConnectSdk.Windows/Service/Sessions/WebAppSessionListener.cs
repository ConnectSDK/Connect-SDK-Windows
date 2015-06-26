#region Copyright Notice
/*
 * ConnectSdk.Windows
 * WebAppSessionListener.cs
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

namespace ConnectSdk.Windows.Service.Sessions
{
    public interface IWebAppSessionListener
    {
        /// <summary>
        /// This method is called when a message is received from a web app.
        /// </summary>
        /// <param name="webAppSession">WebAppSession that corresponds to the web app that sent the message</param>
        /// <param name="message">Object from the web app, either an String or a JSONObject</param>
        void OnReceiveMessage(WebAppSession webAppSession, Object message);

        /// <summary>
        /// This method is called when a web app's communication channel (WebSocket, etc) has become disconnected.
        /// </summary>
        /// <param name="webAppSession">WebAppSession that became disconnected</param>
        void OnWebAppSessionDisconnect(WebAppSession webAppSession);
      
    }
}