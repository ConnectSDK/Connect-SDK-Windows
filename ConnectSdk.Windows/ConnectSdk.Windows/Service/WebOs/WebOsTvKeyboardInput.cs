using System;
using System.Collections.Generic;
using System.Text;
using Windows.Data.Json;
using ConnectSdk.Windows.Core;
using ConnectSdk.Windows.Service.Capability.Listeners;
using ConnectSdk.Windows.Service.Command;

namespace ConnectSdk.Windows.Service.WebOs
{
    class WebOsTvKeyboardInput
    {
        readonly WebOstvService service;
        bool waiting;
        readonly List<String> toSend;

        private const String KeyboardInputUrl = "ssap://com.webos.service.ime/registerRemoteKeyboard";
        private const String EnterKey = "ENTER";
        private const String DeleteKey = "DELETE";
        public bool CanReplaceText { get; set; }

        public WebOsTvKeyboardInput(WebOstvService service, bool canReplaceText)
        {
            this.service = service;
            CanReplaceText = canReplaceText;
            waiting = false;

            toSend = new List<String>();
        }


        public void AddToQueue(String input)
        {
            toSend.Add(input);
            if (!waiting)
            {
                SendData();
            }
        }

        public void SendEnter()
        {
            toSend.Add(EnterKey);
            if (!waiting)
            {
                SendData();
            }
        }

        public void SendDel()
        {
            if (toSend.Count == 0)
            {
                toSend.Add(DeleteKey);
                if (!waiting)
                {
                    SendData();
                }
            }
            else
            {
                toSend.RemoveAt(toSend.Count - 1);
            }
        }

        private void SendData()
        {
            waiting = true;

            String uri;
            var typeTest = toSend[0];

            var payload = new JsonObject();

            if (typeTest.Equals(EnterKey))
            {
                toSend.RemoveAt(0);
                uri = "ssap://com.webos.service.ime/sendEnterKey";
            }
            else if (typeTest.Equals(DeleteKey))
            {
                uri = "ssap://com.webos.service.ime/deleteCharacters";

                int count = 0;
                while (toSend.Count > 0 && toSend[0].Equals(DeleteKey))
                {
                    toSend.RemoveAt(0);
                    count++;
                }

                try
                {
                    payload.Add("count", JsonValue.CreateNumberValue(count));
                }
                catch (Exception e)
                {
                    throw e;
                }
            }
            else
            {
                uri = "ssap://com.webos.service.ime/insertText";
                var sb = new StringBuilder();

                while (toSend.Count > 0 && !(toSend[0].Equals(DeleteKey) || toSend[0].Equals(EnterKey)))
                {
                    var text = toSend[0];
                    sb.Append(text);
                    toSend.RemoveAt(0);
                }

                try
                {
                    payload.Add("text", JsonValue.CreateStringValue(sb.ToString()));
                    payload.Add("replace", JsonValue.CreateNumberValue(0));
                }
                catch (Exception e)
                {
                    throw e;
                }
            }

            var responseListener = new ResponseListener
            (
                loadEventArg =>
                {
                    waiting = false;
                    if (toSend.Count > 0)
                        SendData();
                },
                serviceCommandError =>
                {
                    throw new NotImplementedException();
                }
            );

            var request = new ServiceCommand(service, uri, payload, responseListener);
            request.Send();
        }

        public UrlServiceSubscription Connect(ResponseListener listener)
        {

            var responseListener = new ResponseListener
            (
                loadEventArg =>
                {
                    var jsonObj = (JsonObject)loadEventArg;

                    var keyboard = parseRawKeyboardData(jsonObj);

                    Util.PostSuccess(listener, keyboard);
                },
                serviceCommandError => Util.PostError(listener, serviceCommandError)
            );

            var subscription = new UrlServiceSubscription(service, KeyboardInputUrl, null, true,
                responseListener);
            subscription.Send();

            return subscription;
        }

        private TextInputStatusInfo parseRawKeyboardData(JsonObject rawData)
        {
            var focused = false;
            String contentType = null;
            var predictionEnabled = false;
            var correctionEnabled = false;
            var autoCapitalization = false;
            var hiddenText = false;
            var focusChanged = false;

            var keyboard = new TextInputStatusInfo {RawData = rawData};

            try
            {
                if (rawData.ContainsKey("currentWidget"))
                {
                    var currentWidget = rawData.GetNamedObject("currentWidget");
                    focused = currentWidget.GetNamedBoolean("focus");

                    if (currentWidget.ContainsKey("contentType"))
                    {
                        contentType = currentWidget.GetNamedString("contentType");
                    }
                    if (currentWidget.ContainsKey("predictionEnabled"))
                    {
                        predictionEnabled = currentWidget.GetNamedBoolean("predictionEnabled");
                    }
                    if (currentWidget.ContainsKey("correctionEnabled"))
                    {
                        correctionEnabled = currentWidget.GetNamedBoolean("correctionEnabled");
                    }
                    if (currentWidget.ContainsKey("autoCapitalization"))
                    {
                        autoCapitalization = currentWidget.GetNamedBoolean("autoCapitalization");
                    }
                    if (currentWidget.ContainsKey("hiddenText"))
                    {
                        hiddenText = currentWidget.GetNamedBoolean("hiddenText");
                    }
                }
                if (rawData.ContainsKey("focusChanged"))
                    focusChanged = rawData.GetNamedBoolean("focusChanged");

            }
            catch (Exception e)
            {
                throw e;
            }

            keyboard.Focused = focused;
            keyboard.ContentType = contentType;
            keyboard.PredictionEnabled = predictionEnabled;
            keyboard.CorrectionEnabled = correctionEnabled;
            keyboard.AutoCapitalization = autoCapitalization;
            keyboard.HiddenText = hiddenText;
            keyboard.FocusChanged = focusChanged;

            return keyboard;
        }

        //	public void disconnect() {
        //		subscription.unsubscribe();
        //	}
    }

    public enum State
    {
        None,
        Initial,
        Connecting,
        Registering,
        Registered,
        Disconnecting
    }
}
