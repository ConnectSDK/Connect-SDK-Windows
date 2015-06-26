#region Copyright Notice
/*
 * ConnectSdk.Windows
 * WebOsTvServiceConfig.cs
 * 
 * Copyright (c) 2015, https://github.com/sdaemon
 * Created by Sorin S. Serban on 22-4-2015,
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
using Windows.Data.Json;
using Windows.Security.Cryptography.Certificates;

namespace ConnectSdk.Windows.Service.Config
{
    public class WebOsTvServiceConfig : ServiceConfig
    {
        public static String KeyClientKey = "clientKey";
        public static String KeyCert = "serverCertificate";

        public string PairingKey { get; set; }

        public string ClientKey { get; set; }

        public Certificate Cert { get; set; }

        public WebOsTvServiceConfig(String serviceUuid)
            : base(serviceUuid)
        {
        }

        public WebOsTvServiceConfig(String serviceUuid, String clientKey) :
            base(serviceUuid)
        {
            ClientKey = clientKey;
            Cert = null;
        }

        public WebOsTvServiceConfig(String serviceUuid, String clientKey, Certificate cert) :
            base(serviceUuid)
        {
            ClientKey = clientKey;
            Cert = cert;
        }

        public WebOsTvServiceConfig(String serviceUuid, String clientKey, String cert) :
            base(serviceUuid)
        {
            ClientKey = clientKey;
            Cert = null;
            //Cert = loadCertificateFromPEM(cert);
        }

        public WebOsTvServiceConfig(JsonObject json) :
            base(json)
        {

            ClientKey = json.GetNamedString(KeyClientKey);
            Cert = null; // TODO: loadCertificateFromPEM(json.optString(KEY_CERT));
        }

        public void SetServerCertificate(String cert)
        {
            Cert = null;
            //Cert = loadCertificateFromPEM(cert);
        }

        public String GetServerCertificateInString()
        {
            return null;
            //return ExportCertificateToPEM(Cert);
        }

        //private String ExportCertificateToPem(Certificate cert)
        //{
        //    //try {
        //    //    if ( cert == null ) 
        //    //        return null;
        //    //    return Base64.encodeToString(cert.getEncoded(), Base64.DEFAULT);
        //    //} catch (CertificateEncodingException e) {
        //    //    e.printStackTrace();
        //    //    return null;
        //    //}
        //    return null;
        //}

        //private Certificate LoadCertificateFromPem(String pemString)
        //{
        //    //CertificateFactory certFactory;
        //    //try {
        //    //    certFactory = CertificateFactory.getInstance("X.509");
        //    //    ByteArrayInputStream inputStream = new ByteArrayInputStream(pemString.getBytes("US-ASCII"));

        //    //    return (X509Certificate)certFactory.generateCertificate(inputStream);
        //    //} catch (CertificateException e) {
        //    //    e.printStackTrace();
        //    //    return null;
        //    //} catch (UnsupportedEncodingException e) {
        //    //    e.printStackTrace();
        //    //    return null;
        //    //}
        //    return null;
        //}

        public override JsonObject ToJsonObject()
        {
            var jsonObj = base.ToJsonObject();

            try
            {
                jsonObj.Add(KeyClientKey, JsonValue.CreateStringValue(ClientKey));
                jsonObj.Add(KeyCert, JsonValue.CreateStringValue(""));
                //jsonObj.Add(KeyCert, JsonValue.CreateStringValue(exportCertificateToPEM(Cert)));
            }
            catch (Exception e)
            {
                throw e;
            }

            return jsonObj;
        }

    }
}
