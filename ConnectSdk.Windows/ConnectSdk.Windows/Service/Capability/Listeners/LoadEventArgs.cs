using System;
using MyRemote.ConnectSDK.Service.Command;

namespace MyRemote.ConnectSDK.Service.Capability.Listeners
{
    public class LoadEventArgs : EventArgs
    {
        public ServiceCommandError Load { get; set; }

        public LoadEventArgs(object obj)
        {
            try
            {
                var load = obj as ServiceCommandError;
                Load = load ?? new ServiceCommandError(0, "", obj);
            }
            catch
            {
                Load = new ServiceCommandError(0, "", obj);
            }
        }
    }
}