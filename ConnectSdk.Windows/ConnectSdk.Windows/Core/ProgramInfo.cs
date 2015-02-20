using System;

namespace ConnectSdk.Windows.Core
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

        protected bool Equals(ProgramInfo other)
        {
            return string.Equals(Id, other.Id) && string.Equals(Name, other.Name) && Equals(ChannelInfo, other.ChannelInfo) && Equals(RawData, other.RawData);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (Id != null ? Id.GetHashCode() : 0);
                hashCode = (hashCode*397) ^ (Name != null ? Name.GetHashCode() : 0);
                hashCode = (hashCode*397) ^ (ChannelInfo != null ? ChannelInfo.GetHashCode() : 0);
                hashCode = (hashCode*397) ^ (RawData != null ? RawData.GetHashCode() : 0);
                return hashCode;
            }
        }

        public override bool Equals(Object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((ProgramInfo) obj);
        }
    }
}