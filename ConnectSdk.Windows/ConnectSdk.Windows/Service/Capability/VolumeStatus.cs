namespace MyRemote.ConnectSDK.Service.Capability
{
    public class VolumeStatus
    {
        public VolumeStatus(bool isMute, float volume)
        {
            IsMute = isMute;
            Volume = volume;
        }

        public bool IsMute { get; set; }

        public float Volume { get; set; }
    }
}