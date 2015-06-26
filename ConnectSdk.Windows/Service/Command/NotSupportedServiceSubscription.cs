#region Copyright Notice
/*
 * ConnectSdk.Windows
 * NotSupportedServiceSubscription.cs
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
using System.Collections.Generic;
using ConnectSdk.Windows.Service.Capability.Listeners;

namespace ConnectSdk.Windows.Service.Command
{
    public class NotSupportedServiceSubscription : IServiceSubscription
    {
        private readonly List<ResponseListener> listeners = new List<ResponseListener>();


        public void Unsubscribe()
        {
        }


        public ResponseListener AddListener(ResponseListener listener)
        {
            listeners.Add(listener);

            return listener;
        }

        public List<ResponseListener> GetListeners()
        {
            return listeners;
        }

        public void RemoveListener(ResponseListener listener)
        {
            listeners.Remove(listener);
        }
    }
}