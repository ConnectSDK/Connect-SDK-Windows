using ConnectSdk.Windows.Service.Command;

namespace ConnectSdk.Windows.Service.Capability.Listeners
{
    public interface IErrorListener
    {
        /// <summary>
        /// Method to return the error that was generated. Will pass an error object with a helpful status code and error message.
        /// </summary>
        /// <param name="error"></param>
        void OnError(ServiceCommandError error);
    }
}
