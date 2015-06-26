#region Copyright Notice
/*
 * ConnectSdk.Windows
 * AppState.cs
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
namespace ConnectSdk.Windows.Service.Capability
{
    /// <summary>
    /// Helper class used with the AppStateListener to return the current state of an app.
    /// </summary>
    public class AppState
    {
        /// <summary>
        /// Whether the app is currently running.
        /// </summary>
        public bool Running;

        /// <summary>
        /// Whether the app is currently visible.
        /// </summary>
        public bool Visible;

        public AppState(bool running, bool visible)
        {
            Running = running;
            Visible = visible;
        }
    }
}