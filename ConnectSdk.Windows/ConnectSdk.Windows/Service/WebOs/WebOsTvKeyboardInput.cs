using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Windows.Data.Json;
using ConnectSdk.Windows.Core;
using ConnectSdk.Windows.Service.Capability.Listeners;
using ConnectSdk.Windows.Service.Command;
using UnicodeEncoding = System.Text.UnicodeEncoding;

namespace ConnectSdk.Windows.Service.WebOs
{
    class WebOsTvKeyboardInput
    {

        WebOstvService service;
        bool waiting;
        List<String> toSend;

        static String KEYBOARD_INPUT = "ssap://com.webos.service.ime/registerRemoteKeyboard";
        static String ENTER = "ENTER";
        static String DELETE = "DELETE";

        bool canReplaceText = false;

        public WebOsTvKeyboardInput(WebOstvService service)
        {
            this.service = service;
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
            toSend.Add(ENTER);
            if (!waiting)
            {
                SendData();
            }
        }

        public void SendDel()
        {
            if (toSend.Count == 0)
            {
                toSend.Add(DELETE);
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

            if (typeTest.Equals(ENTER))
            {
                toSend.RemoveAt(0);
                uri = "ssap://com.webos.service.ime/sendEnterKey";
            }
            else if (typeTest.Equals(DELETE))
            {
                uri = "ssap://com.webos.service.ime/deleteCharacters";

                int count = 0;
                while (toSend.Count > 0 && toSend[0].Equals(DELETE))
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

                while (toSend.Count > 0 && !(toSend[0].Equals(DELETE) || toSend[0].Equals(ENTER)))
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

            var responseListener = new ResponseListener();
            responseListener.Success += (sender, o) =>
            {
                waiting = false;
                if (toSend.Count > 0)
                    SendData();
            };

            responseListener.Error += (sender, o) =>
            {
                throw new NotImplementedException();
            };

            var request = new ServiceCommand(service, uri, payload, responseListener);
            request.Send();
        }

        public UrlServiceSubscription connect(ResponseListener listener)
        {
            var responseListener = new ResponseListener();
            responseListener.Success += (sender, o) =>
            {
                var jsonObj = (JsonObject)o;

                var keyboard = parseRawKeyboardData(jsonObj);

                Util.PostSuccess(listener, keyboard);
            };
            responseListener.Error += (sender, o) => Util.PostError(listener, o);

            var subscription = new UrlServiceSubscription(service, KEYBOARD_INPUT, null, true,
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

            var keyboard = new TextInputStatusInfo();
            keyboard.RawData = rawData;

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
