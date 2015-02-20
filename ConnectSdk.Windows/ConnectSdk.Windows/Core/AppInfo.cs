using Windows.Data.Json;

namespace ConnectSdk.Windows.Core
{
    /// <summary>
    /// Normalized reference object for information about a DeviceService's app. This object will, in most cases, be used to launch apps.
    /// In some cases, all that is needed to launch an app is the app id.
    /// </summary>
    public class AppInfo : IJsonSerializable
    {
        private string id;
	    private string name;
        private string ip;
        private string port;

        public AppInfo(string id)
        {
            this.id = id;
        }

        /// <summary>
        /// Gets or sets the ID of the app on the first screen device. Format is different depending on the platform. (ex. youtube.leanback.v4, 0000001134, netflix, etc).
        /// </summary>
        public string Id
        {
            get { return id; }
            set { id = value; }
        }

        /// <summary>
        /// Gets or sets the user-friendly name of the app (ex. YouTube, Browser, Netflix, etc).
        /// </summary>
        public string Name
        {
            get { return name; }
            set { name = value.Trim(); }
        }

        /// <summary>
        /// Gets or sets the raw data from the first screen device about the app.
        /// </summary>
        public JsonObject RawData { get; set; }

        public string Url
        {
            get { return string.Format("http://{0}:{1}/udap/api/data?target=appicon_get&auid={2}&appname={3}", ip, port, id, name); }
        }

        public void SetUrl(string ipParam, string portParam)
        {
            ip = ipParam;
            port = portParam;
        }

        public JsonObject ToJsonObject()
        {
            var obj = new JsonObject
            {
                {"name", JsonValue.CreateStringValue(name)},
                {"id", JsonValue.CreateStringValue(id)}
            };
            return obj;
        }
    }
}

	
