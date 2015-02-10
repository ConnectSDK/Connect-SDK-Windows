using System;
using System.Net.Http;

namespace ConnectSdk.Windows.Etc.Helper
{
    public class HttpMessage
    {
        // ReSharper disable InconsistentNaming
        public static string CONTENT_TYPE_HEADER = "Content-Type";
        public static string CONTENT_TYPE_APPLICATION_PLIST = "application/x-apple-binary-plist";
        public static string CONTENT_TYPE = "text/xml; charset=utf-8";
        public static string UDAP_USER_AGENT = "UDAP/2.0";
        public static string LG_ELECTRONICS = "LG Electronics";
        public static string USER_AGENT = "User-Agent";
        public static string SOAP_ACTION = "\"urn:schemas-upnp-org:service:AVTransport:1#%s\"";
        public static string SOAP_HEADER = "Soapaction";
        // ReSharper restore InconsistentNaming

        public static HttpRequestMessage GetHttpPost(string uri)
        {
            var post = new HttpRequestMessage(HttpMethod.Post, uri);
            post.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("text/xml")
            {
                CharSet = "utf-8"
            };

            return post;
        }

        public static HttpRequestMessage GetUdapHttpPost(string uri)
        {
            HttpRequestMessage post = GetHttpPost(uri);
            post.Headers.Add("User-Agent", UDAP_USER_AGENT);

            return post;
        }

        public static HttpRequestMessage GetDlnaHttpPost(string uri, string action)
        {
            var soapAction = "\"urn:schemas-upnp-org:service:AVTransport:1#" + action + "\"";

            var post = GetHttpPost(uri);
            post.Headers.Add("Soapaction", soapAction);

            return post;
        }

        public static HttpRequestMessage GetHttpGet(string url)
        {
            return new HttpRequestMessage(HttpMethod.Get, url);
        }

        public static HttpRequestMessage GetUdapHttpGet(string uri)
        {
            var get = GetHttpGet(uri);
            get.Headers.Add("User-Agent", UDAP_USER_AGENT);
            return get;
        }

        public static HttpRequestMessage GetHttpDelete(string url)
        {
            return new HttpRequestMessage(HttpMethod.Delete, url);
        }

        public static string Encode(string str)
        {
            return Uri.EscapeUriString(str);
        }

        public static string Decode(string str)
        {
            return Uri.UnescapeDataString(str);
        }
    }
}