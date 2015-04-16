#region Copyright Notice
/*
 * ConnectSdk.Windows
 * virtualkeycodes.cs
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
namespace ConnectSdk.Windows.Device.Netcast
{
    public enum VirtualKeycodes
    {
        Power = 1,
        Number0 = 2,
        Number1 = 3,
        Number2 = 4,
        Number3 = 5,
        Number4 = 6,
        Number5 = 7,
        Number6 = 8,
        Number7 = 9,
        Number8 = 10,
        Number9 = 11,

        KeyUp = 12,
        KeyDown = 13,
        KeyLeft = 14,
        KeyRight = 15,

        Ok = 20,
        Home = 21,
        Menu = 22,
        Back = 23, // PREVIOUS_KEY

        VolumeUp = 24,
        VolumeDown = 25,
        Mute = 26,

        ChannelUp = 27,
        ChannelDown = 28,

        Blue = 29,
        Green = 30,
        Red = 31,
        Yellow = 32,

        Play = 33,
        Pause = 34,
        Stop = 35,
        FastForward = 36,
        Rewind = 37,
        SkipForward = 38,
        SkipBackward = 39,
        Record = 40,
        RecordingList = 41,
        Repeat = 42,
        LiveTv = 43,
        Epg = 44,
        CurrentProgramInfo = 45,

        AspectRatio = 46,
        ExternalInput = 47,
        PipSecondaryVideo = 48,
        ShowChangeSubtitle = 49,
        ProgramList = 50,

        TeleText = 51,
        Mark = 52,

        Video3D = 400,
        Audio3Dlr = 401,

        Dash = 402,
        PreviousChannel = 403, // FLASH BACK
        FavoriteChannel = 404,

        QuickMenu = 405,
        TextOption = 406,
        AudioDescription = 407,
        NetcastKey = 408, // SAME WITH HOME MENU
        EnergySaving = 409,
        AvMode = 410,
        Simplink = 411,
        Exit = 412,
        ReservationProgramList = 413,

        PipChannelUp = 414,
        PipChannelDown = 415,
        SwitchingPrimarySecondaryVideo = 416,
        MyApps = 417
    }
}