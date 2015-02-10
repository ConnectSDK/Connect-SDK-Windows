using System;
using ConnectSdk.Windows.Core;

namespace MyRemote.ConnectSDK.Core
{
    /// <summary>
    /// Normalized reference object for information about a TVs program.
    /// </summary>
    public class ProgramInfo
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public ChannelInfo ChannelInfo { get; set; }
        public object RawData { get; set; }

        public override bool Equals(Object obj)
        {
            var info = obj as ProgramInfo;
            if (info != null)
            {
                var pi = info;
                return pi.Id.Equals(info.Id) && pi.Name.Equals(info.Name);
            }
            return base.Equals(obj);
        }
    }
}