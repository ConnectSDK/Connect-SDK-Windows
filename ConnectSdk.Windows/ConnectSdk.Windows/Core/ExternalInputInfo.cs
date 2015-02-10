using Windows.Data.Json;

namespace ConnectSdk.Windows.Core
{
    /// <summary>
    /// Normalized reference object for information about a DeviceService's external inputs. This object is required to set a DeviceService's external input.
    /// </summary>
    public class ExternalInputInfo
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public bool Connected { get; set; }
        public string IconUrl { get; set; }
        public JsonObject RawData { get; set; }

        public JsonObject ToJsonObject()
        {
            var obj = new JsonObject
            {
                {"id", JsonValue.CreateStringValue(Id)},
                {"name", JsonValue.CreateStringValue(Name)},
                {"connected", JsonValue.CreateBooleanValue(Connected)},
                {"icon", JsonValue.CreateStringValue(IconUrl)},
                {"rawData", JsonValue.CreateStringValue(RawData.ToString())}
            };
            return obj;
        }

        public override bool Equals(object obj)
        {
            var info = obj as ExternalInputInfo;
            if (info != null)
            {
                var eii = info;
                return Id.Equals(eii.Id) && Name.Equals(eii.Name);
            }
            return false;
        }
    }
}