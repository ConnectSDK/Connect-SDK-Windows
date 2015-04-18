#region Copyright Notice
/*
 * ConnectSdk.Windows
 * LaunchSessionType.cs
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
namespace ConnectSdk.Windows.Service.Sessions
{
    public enum LaunchSessionType
    {
        /// <summary>
        /// Unknown LaunchSession type, may be unable to close this launch session
        /// </summary>
        Unknown,
        /// <summary>
        /// LaunchSession represents a launched app
        /// </summary>
        App,
        /// <summary>
        /// LaunchSession represents an external input picker that was launched
        /// </summary>
        ExternalInputPicker,
        /// <summary>
        /// LaunchSession represents a media app
        /// </summary>
        Media,
        /// <summary>
        /// LaunchSession represents a web app
        /// </summary>
        WebApp
    }
}