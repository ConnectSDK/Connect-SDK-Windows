using System;
using MyRemote.ConnectSDK.Service.Command;

namespace MyRemote.ConnectSDK.Service.Capability.Listeners
{
    public class ResponseListener
    {
        public EventHandler<object> Success;
        public EventHandler<ServiceCommandError> Error;

        public void OnSuccess(object obj)
        {
            if (Success != null)
                Success(this, new LoadEventArgs(obj));
        }

        public void OnError(ServiceCommandError obj)
        {
            if (Error != null)
                Error(this, obj);
        }
    }
}