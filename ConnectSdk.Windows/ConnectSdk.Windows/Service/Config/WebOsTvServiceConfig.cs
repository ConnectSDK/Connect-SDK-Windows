#region Copyright Notice
/*
 * ConnectSdk.Windows
 * WebOsTvServiceConfig.cs
 * 
 * Copyright (c) 2015 LG Electronics.
 * Created by Sorin S. Serban on 16-4-2015,
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
 #endregion
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Data.Json;
using Windows.Security.Cryptography.Certificates;

namespace ConnectSdk.Windows.Service.Config
{
    public class WebOSTVServiceConfig : ServiceConfig
    {

        public static String KEY_CLIENT_KEY = "clientKey";
        public static String KEY_CERT = "serverCertificate";
        private String clientKey;
        private Certificate cert;
        private string pairingKey;


        public string PairingKey
        {
            get { return pairingKey; }
            set { pairingKey = value; }
        }

        public WebOSTVServiceConfig(String serviceUUID)
            : base(serviceUUID)
        {
        }

        public WebOSTVServiceConfig(String serviceUUID, String clientKey) :
            base(serviceUUID)
        {
            this.clientKey = clientKey;
            this.cert = null;
        }

        public WebOSTVServiceConfig(String serviceUUID, String clientKey, Certificate cert) :
            base(serviceUUID)
        {
            this.clientKey = clientKey;
            this.cert = cert;
        }

        public WebOSTVServiceConfig(String serviceUUID, String clientKey, String cert) :
            base(serviceUUID)
        {
            this.clientKey = clientKey;
            this.cert = loadCertificateFromPEM(cert);
        }

        public WebOSTVServiceConfig(JsonObject json) :
            base(json)
        {

            clientKey = json.GetNamedString(KEY_CLIENT_KEY);
            cert = null; // TODO: loadCertificateFromPEM(json.optString(KEY_CERT));
        }

        public String getClientKey()
        {
            return clientKey;
        }

        public void setClientKey(String clientKey)
        {
            this.clientKey = clientKey;
        }

        public Certificate getServerCertificate()
        {
            return cert;
        }

        public void setServerCertificate(Certificate cert)
        {
            this.cert = cert;
        }

        public void setServerCertificate(String cert)
        {
            this.cert = loadCertificateFromPEM(cert);
        }

        public String getServerCertificateInString()
        {
            return exportCertificateToPEM(this.cert);
        }

        private String exportCertificateToPEM(Certificate cert)
        {
            //try {
            //    if ( cert == null ) 
            //        return null;
            //    return Base64.encodeToString(cert.getEncoded(), Base64.DEFAULT);
            //} catch (CertificateEncodingException e) {
            //    e.printStackTrace();
            //    return null;
            //}
            return null;
        }

        private Certificate loadCertificateFromPEM(String pemString)
        {
            //CertificateFactory certFactory;
            //try {
            //    certFactory = CertificateFactory.getInstance("X.509");
            //    ByteArrayInputStream inputStream = new ByteArrayInputStream(pemString.getBytes("US-ASCII"));

            //    return (X509Certificate)certFactory.generateCertificate(inputStream);
            //} catch (CertificateException e) {
            //    e.printStackTrace();
            //    return null;
            //} catch (UnsupportedEncodingException e) {
            //    e.printStackTrace();
            //    return null;
            //}
            return null;
        }

        public JsonObject toJSONObject()
        {
            JsonObject jsonObj = base.ToJsonObject();

            try
            {
                jsonObj.Add(KEY_CLIENT_KEY, JsonValue.CreateStringValue(clientKey));
                jsonObj.Add(KEY_CERT, JsonValue.CreateStringValue(exportCertificateToPEM(cert)));
            }
            catch (Exception e)
            {
                throw e;
            }

            return jsonObj;
        }

    }
}
