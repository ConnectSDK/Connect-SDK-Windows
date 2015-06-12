#region Copyright Notice
/*
 * ConnectSdk.Windows
 * LoadEventArgs.cs
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
    public class LoadEventArgs : EventArgs
    {
        public ServiceCommandError Load { get; set; }

        public LoadEventArgs(object obj)
        {
            try
            {
                var load = obj as ServiceCommandError;
                Load = load ?? new ServiceCommandError(0, obj);
            }
            catch
            {
                Load = new ServiceCommandError(0, obj);
            }
        }

        public static T GetValue<T>(object source) where T: class
        {
            var loadEventArgs = source as LoadEventArgs;
            if (loadEventArgs != null) return loadEventArgs.Load.GetPayload() as T;

            return null;
        }
    }
}