using System;
using Windows.Data.Json;
using Windows.Security.Cryptography.Certificates;

namespace ConnectSdk.Windows.Service.Config
{
    public class WebOstvServiceConfig : ServiceConfig
    {

        // ReSharper disable InconsistentNaming
        public static String KEY_CLIENT_KEY = "clientKey";
        public static String KEY_CERT = "serverCertificate";
        // ReSharper restore InconsistentNaming
        private String clientKey;
        private Certificate cert;


        public string PairingKey { get; set; }

        public WebOstvServiceConfig(String serviceUuid)
            : base(serviceUuid)
        {
        }

        public WebOstvServiceConfig(String serviceUuid, String clientKey) :
            base(serviceUuid)
        {
            this.clientKey = clientKey;
            cert = null;
        }

        public WebOstvServiceConfig(String serviceUuid, String clientKey, Certificate cert) :
            base(serviceUuid)
        {
            this.clientKey = clientKey;
            this.cert = cert;
        }

        public WebOstvServiceConfig(String serviceUuid, String clientKey, String cert) :
            base(serviceUuid)
        {
            this.clientKey = clientKey;
            this.cert = loadCertificateFromPEM(cert);
        }

        public WebOstvServiceConfig(JsonObject json) :
            base(json)
        {

            clientKey = json.GetNamedString(KEY_CLIENT_KEY);
            cert = null; // TODO: loadCertificateFromPEM(json.optString(KEY_CERT));
        }

        public String GetClientKey()
        {
            return clientKey;
        }

        public void SetClientKey(String pclientKey)
        {
            clientKey = pclientKey;
        }

        public Certificate GetServerCertificate()
        {
            return cert;
        }

        public void SetServerCertificate(Certificate pcert)
        {
            cert = pcert;
        }

        public void SetServerCertificate(String pcert)
        {
            cert = loadCertificateFromPEM(pcert);
        }

        public String GetServerCertificateInString()
        {
            return exportCertificateToPEM(cert);
        }

        // ReSharper disable once UnusedParameter.Local
        private String exportCertificateToPEM(Certificate pcert)
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

        // ReSharper disable once UnusedParameter.Local
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

        public override JsonObject ToJsonObject()
        {
            var jsonObj = base.ToJsonObject();

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
