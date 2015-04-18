#region Copyright Notice
/*
 * ConnectSdk.Windows
 * IMediaControl.cs
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
using ConnectSdk.Windows.Service.Capability.Listeners;
using ConnectSdk.Windows.Service.Command;

namespace ConnectSdk.Windows.Service.Capability
{
    public interface IMediaControl
    {
        IMediaControl GetMediaControl();
        CapabilityPriorityLevel GetMediaControlCapabilityLevel();

        void Play(ResponseListener listener);
        void Pause(ResponseListener listener);
        void Stop(ResponseListener listener);
        void Rewind(ResponseListener listener);
        void FastForward(ResponseListener listener);

        void Seek(long position, ResponseListener listener);
        void GetDuration(ResponseListener listener);
        void GetPosition(ResponseListener listener);

        void GetPlayState(ResponseListener listener);
        IServiceSubscription SubscribePlayState(ResponseListener listener);

        void Next(ResponseListener listener);
        void Previous(ResponseListener listener);
    }
}