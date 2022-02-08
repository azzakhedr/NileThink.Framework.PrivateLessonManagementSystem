using NileThink.Framework.PrivateLessonManagementSystem.DAL.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Web;


namespace NileThink.Framework.PrivateLessonManagementSystem.Web
{
    public static class StateCertificate
    {

        public static X509Certificate2 LoadCertificate(StoreLocation storeLocation, string certificateName)
        {
            X509Store store = new X509Store(storeLocation);
            store.Open(OpenFlags.ReadOnly);
            X509Certificate2Collection certCollection =
               store.Certificates;
            X509Certificate2 cert =
               certCollection.Cast<X509Certificate2>().FirstOrDefault
               (c => !string.IsNullOrEmpty(c.Subject) && c.Subject.Contains(certificateName));
            if (cert == null)
                Console.WriteLine("NO Certificate named " +
                   certificateName + " was found in your certificate store");
            store.Close();
            return cert;
        }
        //public static List<string> getAllCertificate2(StoreLocation storeLocation)
        //{
        //    X509Store store = new X509Store(storeLocation);
        //    store.Open(OpenFlags.ReadOnly);
        //    X509Certificate2Collection certCollection =
        //       store.Certificates;

        //    List<string> xx = new List<string>();
        //    foreach (var item in certCollection)
        //    {
        //        xx.Add(" Subject : " + item.Subject);
        //        xx.Add(" SubjectName: " + item.SubjectName);
        //        xx.Add(" FriendlyName : " + item.FriendlyName);
        //        xx.Add(" IssuerName : " + item.IssuerName);
        //    }
        //    return xx;

        //}
        public static List<string> getAllCertificate()
        {
            List<string> xx = new List<string>();
            foreach (StoreLocation storeLocation in (StoreLocation[])
            Enum.GetValues(typeof(StoreLocation)))
            {
                foreach (StoreName storeName in (StoreName[])
                    Enum.GetValues(typeof(StoreName)))
                {
                    X509Store store = new X509Store(storeName, storeLocation);

                    try
                    {
                        store.Open(OpenFlags.OpenExistingOnly);

                        xx.Add(string.Format("Yes    {0,4}  {1}, {2}",
                             store.Certificates.Count, store.Name, store.Location));

                    }
                    catch (CryptographicException)
                    {
                        xx.Add(string.Format("No           {0}, {1}",
                            store.Name, store.Location));
                    }
                }
                // Console.WriteLine();

            }
            return xx;
        }
        public static string Encrypt(X509Certificate2 x509, string stringToEncrypt)
        {
            if (x509 == null || string.IsNullOrEmpty(stringToEncrypt))
                throw new Exception("A x509 certificate and string for encryption must be provided");

            RSACryptoServiceProvider rsa = (RSACryptoServiceProvider)x509.PublicKey.Key;
            byte[] bytestoEncrypt = ASCIIEncoding.ASCII.GetBytes(stringToEncrypt);
            byte[] encryptedBytes = rsa.Encrypt(bytestoEncrypt, false);
            return Convert.ToBase64String(encryptedBytes);
        }

        public static string Decrypt(X509Certificate2 x509, string stringTodecrypt)
        {
            if (x509 == null || string.IsNullOrEmpty(stringTodecrypt))
                throw new Exception("A x509 certificate and string for decryption must be provided");

            if (!x509.HasPrivateKey)
                throw new Exception("x509 certicate does not contain a private key for decryption");

            RSACryptoServiceProvider rsa = (RSACryptoServiceProvider)x509.PrivateKey;
            byte[] bytestodecrypt = Convert.FromBase64String(stringTodecrypt);
            byte[] plainbytes = rsa.Decrypt(bytestodecrypt, false);
            System.Text.ASCIIEncoding enc = new System.Text.ASCIIEncoding();
            return enc.GetString(plainbytes);
        }




        static byte[] Sign(string text, string certSubject)

        {

            //try
            //{            
            // Access Personal (MY) certificate store of current user

            X509Store my = new X509Store(StoreName.My, StoreLocation.LocalMachine);
            my.Open(OpenFlags.ReadOnly);
            // Find the certificate we'll use to sign
            //RSACryptoServiceProvider csp = null;
            foreach (X509Certificate2 cert in my.Certificates)

            {

                if (cert.FriendlyName == certSubject)
                {
                    // We found it.
                    // Get its associated CSP and private key

                    byte[] data2 = Encoding.UTF8.GetBytes(text);
                    X509Certificate2 privateCert = cert;
                    RSACryptoServiceProvider key = new RSACryptoServiceProvider();
                    key.FromXmlString(privateCert.PrivateKey.ToXmlString(true));
                    byte[] sig = key.SignData(data2, CryptoConfig.MapNameToOID("SHA256"));
                    //SHA256 sha256Hash = SHA256.Create();
                    //byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(text));
                    //using (System.IO.StreamWriter file = new System.IO.StreamWriter(System.Web.HttpContext.Current.Server.MapPath("~/debug.txt"), true))
                    //    file.WriteLine("sig : " + Encoding.UTF8.GetString(sig));
                    return sig;

                }

            }
            throw new Exception("No valid cert was found");
            ////////////////////// Failed Code ////////////////
            //byte[] hash;

        }
        public static String GetTimestamp(this DateTime value)
        {
            return value.ToString("yyyyMMddHHmmssfff");
        }
        public static List<string> getCer(string id)
        {
            //ToUnixTimeSeconds()


            // string timeStamp =;

            string timeStamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds() + "";
            //new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds() + "";
            //GetTimestamp(DateTime.Now);
            // .ToUnixTimeSeconds()
            //new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds();
            //DateTimeOffset.UtcNow.ToUnixTimeSeconds()+ "";
            string guid = Guid.NewGuid().ToString();
            string ElmClientId = ConfigurationManager.AppSettings["ElmClientId"].ToString();
            string link = "https://iam.elm.sa/authservice/authorize?scope=openid&response_type=id_token&response_mode=form_post&client_id=" + ElmClientId + "&redirect_uri=https://privatelessonforyou.com/Home/callback/" + id + "&nonce=" + (guid) + "&ui_locales=ar&prompt=login&max_age=" + timeStamp;
            var signature =  Sign(link, ElmClientId); //"25274732"
            string txt = Convert.ToBase64String(signature);
            //string fullurl = "https://iam.elm.sa/authservice/authorize?scope=openid&response_type=id_token&response_mode=form_post&client_id=25274732&redirect_uri=http://privatelessonforyou.com/Home/callback/" + id + "&nonce=" + (guid) + "&ui_locales=ar&prompt=login&max_age=" + timeStamp + "&state=";
            string fullurl = link + "&state=" + HttpUtility.UrlEncode(txt);
            //////////////////////// 
            using (var db = new MhanaDevEntities())
            {
                var _user = db.AspNetUsers.Find(id);
                _user.user_state = txt;
                db.Entry(_user).State = EntityState.Modified;
                db.SaveChanges();
            }
            using (System.IO.StreamWriter file = new System.IO.StreamWriter(System.Web.HttpContext.Current.Server.MapPath("~/debug.txt"), true))
            {
                file.WriteLine(fullurl);
            }
            HttpContext.Current.Response.Redirect(fullurl);
            List<string> result = new List<string>();
            result.Add(fullurl);
            return result;
        }
        private static byte[] ConvertFromBase64String(string input)
        {
            string working = input.Replace("-", "+").Replace("_", "/").Replace(".", "");
            while (working.Length % 4 != 0)
            {
                working += '=';
            }
            try
            {
                return Convert.FromBase64String(working);
            }
            catch (Exception)
            {

                try
                {
                    return Convert.FromBase64String(input.Replace('-', '+').Replace('_', '/'));
                }
                catch (Exception) { }
                try
                {
                    return Convert.FromBase64String(input.Replace('-', '+').Replace('_', '/') + "=");
                }
                catch (Exception) { }
                try
                {
                    return Convert.FromBase64String(input.Replace('-', '+').Replace('_', '/') + "==");
                }
                catch (Exception) { }

                return null;
            }
        }

    }
}