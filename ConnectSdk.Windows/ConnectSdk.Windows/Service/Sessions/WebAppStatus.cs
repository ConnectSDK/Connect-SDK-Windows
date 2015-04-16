#region Copyright Notice
/*
 * ConnectSdk.Windows
 * WebAppStatus.cs
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
    public enum WebAppStatus
    {
        /// <summary>
        /// Web app status is unknown
        /// </summary>
        Unknown,
        /// <summary>
        /// Web app is running and in the foreground
        /// </summary>
        Open,
        /// <summary>
        /// Web app is running and in the background
        /// </summary>
        Background,
        /// <summary>
        /// Web app is in the foreground but has not started running yet
        /// </summary>
        Foreground,
        /// <summary>
        /// Web app is not running and is not in the foreground or background
        /// </summary>
        Closed
    }
}