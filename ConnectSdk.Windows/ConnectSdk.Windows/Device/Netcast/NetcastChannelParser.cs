using Windows.Data.Json;
using ConnectSdk.Windows.Core;

namespace ConnectSdk.Windows.Device.Netcast
{
    public class NetcastChannelParser
    {
        public JsonArray ChannelArray;
        public JsonObject Channel;

        public string Value;

        // ReSharper disable InconsistentNaming
        public string CHANNEL_TYPE = "chtype";
        public string MAJOR = "major";
        public string MINOR = "minor";
        public string DISPLAY_MAJOR = "displayMajor";
        public string DISPLAY_MINOR = "displayMinor";
        public string SOURCE_INDEX = "sourceIndex";
        public string PHYSICAL_NUM = "physicalNum";
        public string CHANNEL_NAME = "chname";
        public string PROGRAM_NAME = "progName";
        public string AUDIO_CHANNEL = "audioCh";
        public string INPUT_SOURCE_NAME = "inputSourceName";
        public string INPUT_SOURCE_TYPE = "inputSourceType";
        public string LABEL_NAME = "labelName";
        public string INPUT_SOURCE_INDEX = "inputSourceIdx";
        // ReSharper restore InconsistentNaming

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

                string channelNumber = !channelRawData.ContainsKey("channelNumber") 
                    ? channelRawData.GetNamedString("channelNumber") 
                    : string.Format("{0}-{1}", majorNumber, minorNumber);

                channelInfo.ChannelName = channelName;
                channelInfo.ChannelId = channelId;
                channelInfo.ChannelNumber = channelNumber;
                channelInfo.MajorNumber = majorNumber;
                channelInfo.MinorNumber = minorNumber;

            }
            catch
            {
                //TODO: get some analysis here
                throw;
            }

            return channelInfo;
        }
    }
}