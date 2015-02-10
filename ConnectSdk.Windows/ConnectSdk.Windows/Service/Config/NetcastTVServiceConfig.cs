using System;
using Windows.Data.Json;

namespace MyRemote.ConnectSDK.Service.Config
{
    public class NetcastTvServiceConfig : ServiceConfig
    {
        public static string KeyPairing = "pairingKey";

        public NetcastTvServiceConfig(string serviceUuid) :
            base(serviceUuid)
        {
        }

        public NetcastTvServiceConfig(string serviceUuid, string pairingKey)
            : base(serviceUuid)
        {
            PairingKey = pairingKey;
        }

        public NetcastTvServiceConfig(JsonObject json)
            : base(json)
        {
            PairingKey = json.GetNamedString(KeyPairing);
        }

        public string PairingKey { get; set; }

        public override JsonObject ToJsonObject()
        {
            var jsonObj = base.ToJsonObject();

            try
            {
                jsonObj.Add(KeyPairing, JsonValue.CreateStringValue(PairingKey));
            }
            catch (Exception e)
            {
                throw e;
            }

            return jsonObj;
        }

    }
}