using System;

namespace ConnectSdk.Windows.Service.Sessions
{
    public interface IWebAppSessionListener
    {
        /// <summary>
        /// This method is called when a message is received from a web app.
        /// </summary>
        /// <param name="webAppSession">WebAppSession that corresponds to the web app that sent the message</param>
        /// <param name="message">Object from the web app, either an String or a JSONObject</param>
        void OnReceiveMessage(WebAppSession webAppSession, Object message);

        /// <summary>
        /// This method is called when a web app's communication channel (WebSocket, etc) has become disconnected.
        /// </summary>
        /// <param name="webAppSession">WebAppSession that became disconnected</param>
        void OnWebAppSessionDisconnect(WebAppSession webAppSession);
      
    }
}