using System;
using System.Collections.Generic;
using Windows.Data.Json;
using Windows.UI.Core;
using Windows.UI.Popups;
using ConnectSdk.Windows.Service.Capability;
using ConnectSdk.Windows.Service.Capability.Listeners;
using ConnectSdk.Windows.Service.Sessions;

namespace ConnectSdk.Demo.Demo
{
    public partial class Model
    {
        private const String WebOsId = "webOS TV";
        private string webAppId;
        private WebAppSessionListener webAppListener;
        private WebAppSession mWebAppSession;
        private bool isLaunched;
        private ResponseListener connectionListener;

        private void SetWebAppControls()
        {
            if (selectedDevice != null)
            {
                LaunchWebAppCommand.Enabled = selectedDevice.HasCapability(WebAppLauncher.Launch);
                JoinWebAppCommand.Enabled = selectedDevice.HasCapability(WebAppLauncher.Join);
                SendMessageCommand.Enabled = selectedDevice.HasCapability(WebAppLauncher.MessageSend);
                SendJsonCommand.Enabled = selectedDevice.HasCapability(WebAppLauncher.MessageSendJson);
                LeaveWebAppCommand.Enabled = selectedDevice.HasCapability(WebAppLauncher.Disconnect);
                CloseWebAppCommand.Enabled = selectedDevice.HasCapability(WebAppLauncher.Close);
                PinWebAppCommand.Enabled = selectedDevice.HasCapability(WebAppLauncher.Pin);
                UnPinWebAppCommand.Enabled = selectedDevice.HasCapability(WebAppLauncher.Pin);

                if (!isLaunched)
                {
                    CloseWebAppCommand.Enabled = false;
                    LeaveWebAppCommand.Enabled = false;
                    SendMessageCommand.Enabled = false;
                    SendJsonCommand.Enabled = false;
                }
                else
                {
                    LaunchWebAppCommand.Enabled = true;
                }

                WebAppResponseMessage = "";

                if (selectedDevice.GetServiceByName(WebOsId) != null)
                {
                    webAppId = "WebAppTester";
                }

                if (selectedDevice.HasCapability(WebAppLauncher.Pin))
                {
                    SubscribeIfWebAppIsPinned();
                }
                else
                {
                    PinWebAppCommand.Enabled = false;
                    UnPinWebAppCommand.Enabled = false;
                }
            }

            webAppListener = new WebAppSessionListener
                (
                (session, message) =>
                {
                    var str = LoadEventArgs.GetValue<string>(message);
                    var json = LoadEventArgs.GetValue<JsonObject>(message);
                    App.MainDispatcher.RunAsync(CoreDispatcherPriority.High, () =>
                    {
                        if (str != null)
                            WebAppResponseMessage += str + "\n";
                        else if (json != null)
                            WebAppResponseMessage += json.Stringify() + "\n";
                    });
                },
                session =>
                {
                    JoinWebAppCommand.Enabled = selectedDevice.HasCapability(WebAppLauncher.Join);
                    SendMessageCommand.Enabled = false;
                    SendJsonCommand.Enabled = false;
                    CloseWebAppCommand.Enabled = false;
                    LeaveWebAppCommand.Enabled = false;

                    mWebAppSession = null;
                    isLaunched = false;
                }
                );

            connectionListener = new ResponseListener
                (
                loadEventArg =>
                {
                    if (selectedDevice == null) return;

                    if (selectedDevice.HasCapability(WebAppLauncher.MessageSendJson))
                        SendJsonCommand.Enabled = true;
                    if (selectedDevice.HasCapability(WebAppLauncher.MessageSend))
                        SendMessageCommand.Enabled = true;

                    LeaveWebAppCommand.Enabled = selectedDevice.HasCapability(WebAppLauncher.Disconnect);
                    CloseWebAppCommand.Enabled = true;
                    LaunchWebAppCommand.Enabled = true;
                    isLaunched = true;
                },
                serviceCommandError =>
                {
                    SendJsonCommand.Enabled = false;
                    SendMessageCommand.Enabled = false;
                    CloseWebAppCommand.Enabled = false;
                    LaunchWebAppCommand.Enabled = true;
                    isLaunched = false;

                    if (mWebAppSession != null)
                    {
                        mWebAppSession.WebAppSessionListener = null;
                        mWebAppSession.Close(null);
                    }
                }
                );
        }

        private void SubscribeIfWebAppIsPinned()
        {
            if (webAppId == null) return;

            var listener = new ResponseListener
                (
                loadEventArg =>
                {
                    var status = LoadEventArgs.GetValueBool(loadEventArg);
                    UpdatePinButton(status);
                },
                serviceCommandError =>
                {

                }
                );
            webAppLauncher.SubscribeIsWebAppPinned(webAppId, listener);
        }


        private void LaunchWebAppCommandExecute(object obj)
        {
            var listener = new ResponseListener
                (
                loadEventArg =>
                {
                    var webappSession = LoadEventArgs.GetValue<WebAppSession>(loadEventArg);

                    webappSession.WebAppSessionListener = webAppListener;
                    isLaunched = true;
                    DisconnectMediaPlayerSession();
                    mWebAppSession = webappSession;
                    if (selectedDevice.HasAnyCapability(new List<string>
                    {
                        WebAppLauncher.MessageSend, 
                        WebAppLauncher.MessageReceive,
                        WebAppLauncher.MessageReceiveJson,
                        WebAppLauncher.MessageSendJson
                    }))
                    {
                        webappSession.Connect(connectionListener);
                    }
                    else
                    {
                        connectionListener.OnSuccess(webappSession.LaunchSession);
                    }

                },
                serviceCommandError =>
                {
                    var msg =
                        new MessageDialog(
                            "Error launching webapp");
                    msg.ShowAsync();
                }
                );
            webAppLauncher.LaunchWebApp(webAppId, listener);
        }

        private void JoinWebAppCommandExecute(object obj)
        {
            var listener = new ResponseListener
                (
                loadEventArg =>
                {
                    if (selectedDevice == null) return;
                    
                    var webappSession = LoadEventArgs.GetValue<WebAppSession>(loadEventArg);

                    webappSession.WebAppSessionListener = webAppListener;
                    mWebAppSession = webappSession;

                    SendMessageCommand.Enabled = true;
                    LaunchWebAppCommand.Enabled = false;
                    LeaveWebAppCommand.Enabled = selectedDevice.HasCapability(WebAppLauncher.Disconnect);

                    if (selectedDevice.HasCapability(WebAppLauncher.MessageSendJson)) SendJsonCommand.Enabled = true;
                    if (selectedDevice.HasCapability(WebAppLauncher.Close)) CloseWebAppCommand.Enabled = true;

                    isLaunched = true;
                    DisconnectMediaPlayerSession();
                },
                serviceCommandError =>
                {
                    var msg =
                        new MessageDialog(
                            "Error joining webapp");
                    msg.ShowAsync();
                }
                );
            webAppLauncher.JoinWebApp(webAppId, listener);

        }

        private void SendMessageCommandExecute(object obj)
        {
            var listener = new ResponseListener
                (
                loadEventArg =>
                {

                },
                serviceCommandError =>
                {

                }
                );
            mWebAppSession.SendMessage("This is an Universal App test message", listener);
        }

        private void SendJsonCommandExecute(object obj)
        {
            var message = new JsonObject
            {
                {"type", JsonValue.CreateStringValue("message")},
                {"contents", JsonValue.CreateStringValue("This is a test message")}
            };

            var par = new JsonObject
            {
                {"someParam1", JsonValue.CreateStringValue("The content & format of this JSON block can be anything")},
                {"someParam2", JsonValue.CreateStringValue("The only limit ... is yourself")}
            };

            message.Add("params", par);

            var listener = new ResponseListener
                (
                loadEventArg =>
                {

                },
                serviceCommandError =>
                {

                }
                );
            mWebAppSession.SendMessage(message, listener);
        }

        private void LeaveWebAppCommandExecute(object obj)
        {
            if (mWebAppSession != null)
            {
                mWebAppSession.WebAppSessionListener = null;
                mWebAppSession.DisconnectFromWebApp();
                mWebAppSession = null;

                LaunchWebAppCommand.Enabled = true;
                JoinWebAppCommand.Enabled = selectedDevice.HasCapability(WebAppLauncher.Join);
                SendJsonCommand.Enabled = false;
                SendMessageCommand.Enabled = false;
                LeaveWebAppCommand.Enabled = false;
                CloseWebAppCommand.Enabled = false;

                isLaunched = false;
            }
        }

        private void CloseWebAppCommandExecute(object obj)
        {
            CloseWebAppCommand.Enabled = false;
            SendMessageCommand.Enabled = false;
            SendJsonCommand.Enabled = false;
            LeaveWebAppCommand.Enabled = false;
            isLaunched = false;
            mWebAppSession.WebAppSessionListener = null;
            

            var listener = new ResponseListener
                (
                loadEventArg =>
                {
                    LaunchWebAppCommand.Enabled = true;
                },
                serviceCommandError =>
                {
                    LaunchWebAppCommand.Enabled = true;
                }
                );
            mWebAppSession.Close(listener);
        }

        private void PinWebAppCommandExecute(object obj)
        {
            if (selectedDevice == null) return;
            
            var listener = new ResponseListener
                (
                loadEventArg =>
                {
                    
                },
                serviceCommandError =>
                {
                    
                }
                );

            webAppLauncher.PinWebApp(webAppId, listener);
        }

        private void UnPinWebAppCommandExecute(object obj)
        {
            if (webAppId == null) return;  
 
            var listener = new ResponseListener
                (
                loadEventArg =>
                {

                },
                serviceCommandError =>
                {

                }
                );
            webAppLauncher.UnPinWebApp(webAppId, listener);

        }

        public void UpdatePinButton(bool status)
        {
            if (status)
            {
                PinWebAppCommand.Enabled = false;
                UnPinWebAppCommand.Enabled = true;
            }
            else
            {
                PinWebAppCommand.Enabled = true;
                UnPinWebAppCommand.Enabled = false;
            }
        }

        private void DisconnectMediaPlayerSession()
        {
            if (launchSession != null)
            {
                launchSession.Close(null);
                launchSession = null;
                isPlaying = isPlayingImage = false;
            }
        }
    }
}
