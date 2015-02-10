using System;
using System.Collections.Generic;
using Windows.Data.Json;
using ConnectSdk.Windows.Service.Capability.Listeners;

namespace ConnectSdk.Windows.Service.Sessions
{
    public class LaunchSession
    {
        public static LaunchSession LaunchSessionForAppId(string appId)
        {
            var launchSession = new LaunchSession { AppId = appId };

            return launchSession;
        }

        public static LaunchSession LaunchSessionFromJsonObject(JsonObject json)
        {
            var launchSession = new LaunchSession();
            try
            {
                launchSession.FromJsonObject(json);
            }
            // ReSharper disable once EmptyGeneralCatchClause
            catch (Exception)
            {

            }

            return launchSession;
        }

        public void Close(ResponseListener listener)
        {
            Service.CloseLaunchSession(this, listener);
        }

        public JsonObject ToJsonObject()
        {
            var obj = new JsonObject
            {
                {"appId", JsonValue.CreateStringValue(AppId)},
                {"sessionId", JsonValue.CreateStringValue(SessionId)},
                {"name", JsonValue.CreateStringValue(AppName)},
                {"sessionType", JsonValue.CreateStringValue(SessionType.ToString())}
            };

            if (Service != null) obj.Add("serviceName", JsonValue.CreateStringValue(Service.ServiceName));

            if (RawData == null) return obj;
            // ReSharper disable once AssignNullToNotNullAttribute
            if (RawData is JsonObject) obj.Add("rawData", JsonValue.CreateStringValue(RawData as string));
            var list = RawData as List<object>;
            if (list != null)
            {
                var arr = new JsonArray();
                foreach (var item in list)
                {
                    arr.Add(JsonValue.CreateStringValue(item.ToString()));
                }
                obj.Add("rawData", arr);
            }
            if (RawData is string) obj.Add("rawData", JsonValue.CreateStringValue(RawData as string));

            return obj;
        }


        public string AppId { get; set; }

        public string AppName { get; set; }

        public string SessionId { get; set; }

        public object RawData { get; set; }

        public DeviceService Service { get; set; }

        public LaunchSessionType SessionType { get; set; }

        public void FromJsonObject(JsonObject obj)
        {
            AppId = obj.GetNamedString("appId");
            SessionId = obj.GetNamedString("sessionId");
            AppName = obj.GetNamedString("name");
            SessionType =
                (LaunchSessionType)Enum.Parse(typeof(LaunchSessionType), obj.GetNamedString("sessionType"));
            RawData = obj.GetNamedString("rawData");
        }

    }
}
