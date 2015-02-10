using System;
using Windows.Data.Json;
using ConnectSdk.Windows.Core;
using MyRemote.ConnectSDK.Core;

namespace MyRemote.ConnectSDK.Service.Config
{
    public class ServiceConfig
    {
        public static string KeyClass = "class";
        public static string KeyLastDetect = "lastDetection";
        public static string KeyUuid = "UUID";
        private string serviceUuid;
        private double lastDetected = double.MaxValue;

        readonly bool connected;
        readonly bool wasConnected;

        private IServiceConfigListener listener;

        public ServiceConfig(string serviceUuid)
        {
            if (serviceUuid == null) throw new ArgumentNullException("serviceUuid");
            ServiceUuid = serviceUuid;
        }

        public ServiceConfig(ServiceDescription desc)
        {
            ServiceUuid = desc.Uuid;
            connected = false;
            wasConnected = false;
            LastDetected = Util.GetTime();
        }

        public ServiceConfig(ServiceConfig config)
        {
            ServiceUuid = config.ServiceUuid;
            connected = config.connected;
            wasConnected = config.wasConnected;
            LastDetected = config.LastDetected;

            listener = config.listener;
        }

        public ServiceConfig(JsonObject json)
        {
            ServiceUuid = json.GetNamedString(KeyUuid);
            LastDetected = json.GetNamedNumber(KeyLastDetect);
        }

        public string ServiceUuid
        {
            get { return serviceUuid; }
            set { serviceUuid = value; }
        }

        public double LastDetected
        {
            get { return lastDetected; }
            set { lastDetected = value; }
        }

        public IServiceConfigListener Listener
        {
            get { return listener; }
            set { listener = value; }
        }

        public static ServiceConfig GetConfig(JsonObject json)
        {
            ServiceConfig newServiceClass = null;
            var className = json.GetNamedString(KeyClass);
            if (className.Equals("ServiceConfig", StringComparison.OrdinalIgnoreCase))
            {
                newServiceClass =
                    (ServiceConfig)Activator.CreateInstance(typeof(ServiceConfig), new object[] { json });
            }
            if (className.Equals("NetcastTVServiceConfig", StringComparison.OrdinalIgnoreCase))
            {
                newServiceClass =
                    (NetcastTvServiceConfig)Activator.CreateInstance(typeof(NetcastTvServiceConfig), new object[] { json });
            }

            return newServiceClass;
        }


        public override string ToString()
        {
            return ServiceUuid;
        }

        public void Detect()
        {
            LastDetected = Util.GetTime();
        }

        public virtual JsonObject ToJsonObject()
        {
            var jsonObj = new JsonObject();

            try
            {
                jsonObj.Add(KeyClass, JsonValue.CreateStringValue(GetType().Name));
                jsonObj.Add(KeyLastDetect, JsonValue.CreateNumberValue(lastDetected));
                jsonObj.Add(KeyUuid, JsonValue.CreateStringValue(serviceUuid));
            }
            catch
            {
                //e.printStackTrace();
            }

            return jsonObj;
        }


    }
}
