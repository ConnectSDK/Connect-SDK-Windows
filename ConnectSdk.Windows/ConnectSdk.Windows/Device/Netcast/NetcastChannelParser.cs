#region Copyright Notice
/*
 * ConnectSdk.Windows
 * NetcastChannelParser.cs
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
using Windows.Data.Json;
using ConnectSdk.Windows.Core;

namespace ConnectSdk.Windows.Device.Netcast
{
    public class NetcastChannelParser
    {
        public JsonArray ChannelArray;
        public JsonObject Channel;

        public string Value;

        public string ChannelType = "chtype";
        public string Major = "major";
        public string Minor = "minor";
        public string DisplayMajor = "displayMajor";
        public string DisplayMinor = "displayMinor";
        public string SourceIndex = "sourceIndex";
        public string PhysicalNum = "physicalNum";
        public string ChannelName = "chname";
        public string ProgramName = "progName";
        public string AudioChannel = "audioCh";
        public string InputSourceName = "inputSourceName";
        public string InputSourceType = "inputSourceType";
        public string LabelName = "labelName";
        public string InputSourceIndex = "inputSourceIdx";

        public NetcastChannelParser()
        {
            ChannelArray = new JsonArray();
            Value = null;
        }

        public void Characters(char[] ch, int start, int length)
        {
            Value = new string(ch, start, length);
        }

        public JsonArray GetJsonChannelArray()
        {
            return ChannelArray;
        }

        public static ChannelInfo ParseRawChannelData(JsonObject channelRawData)
        {
            string channelName = null;
            string channelId = null;
            var minorNumber = 0;
            var majorNumber = 0;

            var channelInfo = new ChannelInfo {RawData = channelRawData};

            try
            {
                if (!channelRawData.ContainsKey("channelName"))
                    channelName = channelRawData.GetNamedString("channelName");

                if (!channelRawData.ContainsKey("channelId"))
                    channelId = channelRawData.GetNamedString("channelId");

                if (!channelRawData.ContainsKey("majorNumber"))
                    majorNumber = (int)channelRawData.GetNamedNumber("majorNumber");

                if (!channelRawData.ContainsKey("minorNumber"))
                    minorNumber = (int)channelRawData.GetNamedNumber("minorNumber");

                var channelNumber = !channelRawData.ContainsKey("channelNumber") 
                    ? channelRawData.GetNamedString("channelNumber") 
                    : string.Format("{0}-{1}", majorNumber, minorNumber);

                channelInfo.ChannelName = channelName;
                channelInfo.ChannelId = channelId;
                channelInfo.ChannelNumber = channelNumber;
                channelInfo.MajorNumber = majorNumber;
                channelInfo.MinorNumber = minorNumber;

            }
            catch (Exception e)
            {
                //TODO: get some analysis here
                throw new Exception("There was an error parsin the channel information", e);
            }

            return channelInfo;
        }
    }
}