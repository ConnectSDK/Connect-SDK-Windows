#region Copyright Notice
/*
 * ConnectSdk.Windows
 * DeviceServiceReachability.cs
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
using System.Threading;
using System.Threading.Tasks;

namespace ConnectSdk.Windows.Etc.Helper
{
    public class DeviceServiceReachability
    {
        private const int Timeout = 10000;
        private Task testThread;
        public string IpAddress { get; set; }
        public IDeviceServiceReachabilityListener Listener { get; set; }

        public DeviceServiceReachability(CancellationTokenSource cancelationTokenParam)
        {
            cancelationToken = cancelationTokenParam;
        }

        public DeviceServiceReachability(string ipAddress, CancellationTokenSource cancelationTokenParam)
        {
            IpAddress = ipAddress;
            cancelationToken = cancelationTokenParam;
        }

        public DeviceServiceReachability(string ipAddress, IDeviceServiceReachabilityListener listener, CancellationTokenSource cancelationTokenParam)
        {
            IpAddress = ipAddress;
            Listener = listener;
            cancelationToken = cancelationTokenParam;
        }


        public static DeviceServiceReachability GetReachability(string ipAddress, IDeviceServiceReachabilityListener listener)
        {
            return new DeviceServiceReachability(ipAddress, listener, cancelationToken);
        }


        public bool IsRunning()
        {
            return testThread != null && testThread.Status == TaskStatus.Running;
        }

        private static CancellationTokenSource cancelationToken;

        public void Start()
        {
            if (IsRunning())
                return;

            Task.Factory.StartNew(delegate
            {
                try
                {
                    while (true)
                    {
                        //todo: get a ping implementation from somewhere
                        //if (!ipAddress.IsReachable(TIMEOUT))
                        //    Unreachable();
                        testThread.Wait(Timeout);
                    }
                }
                catch (Exception)
                {
                    Unreachable();
                }
            }, cancelationToken);

        }

        public void Stop()
        {
            if (!IsRunning())
                return;
            cancelationToken.Cancel();
            testThread = null;
        }

        private void Unreachable()
        {
            Stop();

            if (Listener != null)
                Listener.OnLoseReachability(this);
        }
    }
}
