using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.ModelBinding;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.OAuth;
using NileThink.Framework.PrivateLessonManagementSystem.Web.Models;
using NileThink.Framework.PrivateLessonManagementSystem.Web.Providers;
using NileThink.Framework.PrivateLessonManagementSystem.Web.Results;
using System.IO;
using System.Web.Hosting;
using System.Web.Http.Description;
using NileThink.Framework.PrivateLessonManagementSystem.Web.Helper;
using System.Net;
using System.Linq;
using System.Data.Entity;
using System.Configuration;
using NileThink.Framework.PrivateLessonManagementSystem.Web;
using NileThink.Framework.PrivateLessonManagementSystem.BLL.BussinessLayer;
using NileThink.Framework.PrivateLessonManagementSystem.BLL.ViewModels;
using System.Globalization;



using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;

using System.Drawing;
using System.Drawing.Imaging;

namespace NileThink.Framework.PrivateLessonManagementSystem.Web.Controllers.Api
{


    public class BaseController : ApiController
    {
        protected string lang
        {
            get
            {

                var re = Request;
                var headers = re.Headers;
                string lang = "ar";
                if (headers.Contains("lang"))
                {
                    lang = headers.GetValues("lang").First();
                }
                else if (headers.Contains("Lang"))
                {
                    lang = headers.GetValues("Lang").First();
                }




                if (lang.ToLower() == "ar")
                {
                    var cultureInfo = new CultureInfo("ar-EG");
                    System.Threading.Thread.CurrentThread.CurrentCulture = cultureInfo;
                    CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
                    CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;

                }
                else
                {
                    var cultureInfo = new CultureInfo("en-US");
                    System.Threading.Thread.CurrentThread.CurrentCulture = cultureInfo;
                    CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
                    CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;

                }
                return lang;
            }
        }

        protected string Encrypt(string clearText)
        {
            string EncryptionKey = "MAKV2SPBNI99212";
            byte[] clearBytes = Encoding.Unicode.GetBytes(clearText);
            using (Aes encryptor = Aes.Create())
            {
                Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(EncryptionKey, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
                encryptor.Key = pdb.GetBytes(32);
                encryptor.IV = pdb.GetBytes(16);
                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(clearBytes, 0, clearBytes.Length);
                        cs.Close();
                    }
                    clearText = Convert.ToBase64String(ms.ToArray());
                }
            }
            return clearText;
        }
        protected string Decrypt(string cipherText)
        {
            string EncryptionKey = "MAKV2SPBNI99212";
            byte[] cipherBytes = Convert.FromBase64String(cipherText);
            using (Aes encryptor = Aes.Create())
            {
                Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(EncryptionKey, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
                encryptor.Key = pdb.GetBytes(32);
                encryptor.IV = pdb.GetBytes(16);
                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateDecryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(cipherBytes, 0, cipherBytes.Length);
                        cs.Close();
                    }
                    cipherText = Encoding.Unicode.GetString(ms.ToArray());
                }
            }
            return cipherText;
        }

    }
}