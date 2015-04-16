#region Copyright Notice
/*
 * ConnectSdk.Windows
 * ResponseListener.cs
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
using ConnectSdk.Windows.Service.Command;

namespace ConnectSdk.Windows.Service.Capability.Listeners
{
    public class ResponseListener
    {
        //public EventHandler<object> Success;
        //public EventHandler<ServiceCommandError> Error;
        
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
            //if (Success != null)
            //    Success(this, new LoadEventArgs(obj));
            if (onSuccessFunc != null)
                onSuccessFunc(new LoadEventArgs(obj));
        }

        public void OnError(ServiceCommandError obj)
        {
            //if (Error != null)
            //    Error(this, obj);
            if (onErrorFunc != null)
                onErrorFunc(obj);
        }

    }
}