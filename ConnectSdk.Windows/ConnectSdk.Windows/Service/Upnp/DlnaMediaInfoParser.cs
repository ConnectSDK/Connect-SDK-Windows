#region Copyright Notice
/*
 * ConnectSdk.Windows
 * DlnaMediaInfoParser.cs
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
using System.Collections.Generic;
using System.Text;
using ConnectSdk.Windows.Core;

namespace ConnectSdk.Windows.Service.Upnp
{
    public class DlnaMediaInfoParser
    {
        // ReSharper disable InconsistentNaming
        private const string APOS = "&amp;apos;";
        private const string LT = "&lt;";
        private const string GT = "&gt;";
        private const string TITLE = "dc:title";
        private const string CREATOR = "dc:creator";
        private const string THUMBNAIL = "upnp:albumArtURI";
        private const string ALBUM = "upnp:album";
        private const string GENRE = "upnp:genre";
        private const string RADIOTITLE = "r:streamContent";
        // ReSharper restore InconsistentNaming


        private static String GetData(String str, String data)
        {
            if (str.Contains(ToEndTag(data)))
            {
                int startInd = str.IndexOf(ToStartTag(data), StringComparison.Ordinal)
                               + ToStartTag(data).Length;
                int endInd = str.IndexOf(ToEndTag(data), StringComparison.Ordinal);
                return (ToString(str.Substring(startInd, endInd)));
            }

            if (str.Contains(LT)) return "";

            throw new NotImplementedException();

            //XmlPullParser parser = Xml.newPullParser();

            //try
            //{
            //    parser.setFeature(XmlPullParser.FEATURE_PROCESS_NAMESPACES, false);
            //    parser.setInput(new StringReader(str));

            //    int eventType = parser.nextTag();
            //    while (eventType != XmlPullParser.END_DOCUMENT)
            //    {
            //        if (eventType == XmlPullParser.START_TAG)
            //        {
            //            String name = parser.getName();
            //            if (name.equals(data))
            //            {
            //                eventType = parser.next();
            //                if (eventType == XmlPullParser.TEXT)
            //                    return parser.getText();
            //            }
            //        }
            //        eventType = parser.next();
            //    }
            //}
            //catch (XmlPullParserException e)
            //{
            //    e.printStackTrace();
            //}
            //catch (IOException e)
            //{
            //    e.printStackTrace();
            //}
        }

        public static MediaInfo GetMediaInfo(String str)
        {
            var url = GetUrl(str);
            var title = GetTitle(str);
            var mimeType = GetMimeType(str);
            var description = GetArtist(str) + "\n" + GetAlbum(str);
            var iconUrl = GetThumbnail(str);

            var list = new List<ImageInfo> {new ImageInfo(iconUrl)};
            var info = new MediaInfo(url, mimeType, title, description, list);

            return info;
        }

        public static String GetTitle(String str)
        {
            return GetData(str, !GetData(str, RADIOTITLE).Equals("") ? RADIOTITLE : TITLE);
        }

        public static String GetArtist(String str)
        {
            return GetData(str, CREATOR);
        }

        public static String GetAlbum(String str)
        {
            return GetData(str, ALBUM);
        }

        public static String GetGenre(String str)
        {
            return GetData(str, GENRE);
        }

        public static String GetThumbnail(String str)
        {
            var res = GetData(str, THUMBNAIL);
            //res = java.net.URLDecoder.decode(res);
            return res;
        }

        public static String GetMimeType(String str)
        {
            if (!str.Contains("protocolInfo")) return "";
            var startInd = str.IndexOf("*:", StringComparison.Ordinal) + 2;
            var endInd = str.Substring(startInd).IndexOf(":", StringComparison.Ordinal) + startInd;
            return str.Substring(startInd, endInd);
        }

        public static String GetUrl(String str)
        {
            if (!str.Contains(LT)) return GetData(str, "res");
            if (!str.Contains(ToEndTag("res"))) return "";
            var startInd = str.Substring(str.IndexOf(LT + "res", StringComparison.Ordinal)).IndexOf(GT, StringComparison.Ordinal)
                           + str.IndexOf(LT + "res", StringComparison.Ordinal) + GT.Length;
            var endInd = str.IndexOf(ToEndTag("res"), StringComparison.Ordinal);
            return str.Substring(startInd, endInd);
            //return java.net.URLDecoder.decode(str.Substring(startInd, endInd));
        }

        private static String ToStartTag(String str)
        {
            return (LT + str + GT);
        }

        private static String ToEndTag(String str)
        {
            return ToStartTag("/" + str);
        }

        private static String ToString(String text)
        {
            var sb = new StringBuilder();
            if (text.Contains(APOS))
            {
                sb.Append(text.Substring(0, text.IndexOf(APOS, StringComparison.Ordinal)));
                sb.Append("'");
                sb.Append(text.Substring(text.IndexOf(APOS, StringComparison.Ordinal) + APOS.Length));
            }
            else
                return text;

            return sb.ToString();
        }
    }
}