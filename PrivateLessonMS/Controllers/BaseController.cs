using Microsoft.AspNet.Identity.Owin;
using Newtonsoft.Json;
using PrivateLessonMS.Helper;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Web;
using System.Web.Mvc;

namespace PrivateLessonMS.Controllers
{
    public class BaseController : Controller
    {
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;
        public ApplicationSignInManager SignInManager
        {
            get
            {
                return _signInManager ?? HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
            }
            private set
            {
                _signInManager = value;
            }
        }
        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }
        public string Language { get { return CultureHelper.GetNeutralCulture(Thread.CurrentThread.CurrentCulture.Name); } }

        protected override IAsyncResult BeginExecuteCore(AsyncCallback callback, object state)
        {
            string cultureName = null;

            // Attempt to read the culture cookie from Request
            HttpCookie cultureCookie = Request.Cookies["_culture"];
            if (cultureCookie != null)
                cultureName = cultureCookie.Value;
            else
                cultureName = Request.UserLanguages != null && Request.UserLanguages.Length > 0 ?
                        Request.UserLanguages[0] :  // obtain it from HTTP header AcceptLanguages
                        null;
            // Validate culture name
            cultureName = CultureHelper.GetImplementedCulture(cultureName); // This is safe

            // Modify current thread's cultures            

            System.Globalization.CultureInfo ci = CultureInfo.CreateSpecificCulture(cultureName);// new System.Globalization.CultureInfo("ar-EG", false);

            DateTimeFormatInfo d = new DateTimeFormatInfo();
            d.DateSeparator = " ";
            d.FullDateTimePattern = "dd MMM yyy HH:mm:ss tt";
            //d.ShortDatePattern = "dd MMM yyy";
            //d.LongDatePattern = "dd MMM yyy";
            d.TimeSeparator = ":";
            d.LongTimePattern = "HH:mm:ss tt";
            //d.SetAllDateTimePatterns(new string[] { "dd MMM yyy HH:mm:ss tt", "dd MMM yyy" },'Y');

            ci.DateTimeFormat = d;


            //SetMonthsName(ci);

            Thread.CurrentThread.CurrentCulture = ci;
            Thread.CurrentThread.CurrentUICulture = ci;
            var call = ci.OptionalCalendars;

            return base.BeginExecuteCore(callback, state);
        }



        public ActionResult getMessage(Enum status, string meesage, string action, string controller, int? id = null)
        {
            TempData["Message"] = string.IsNullOrEmpty(meesage) ? "تمت العملية بنجاح" : meesage;
            TempData["Status"] = status;
            return RedirectToAction(action, controller, new { id = id });
        }
        public ActionResult getMessage(Enum status, string meesage, string action, string controller, string id)
        {
            TempData["Message"] = string.IsNullOrEmpty(meesage) ? "تمت العملية بنجاح" : meesage;
            TempData["Status"] = status;
            return RedirectToAction(action, controller, new { id = id });
        }
        public ActionResult getMessage(Enum status, string meesage, string action, string controller)
        {
            TempData["Message"] = string.IsNullOrEmpty(meesage) ? "تمت العملية بنجاح" : meesage;
            TempData["Status"] = status;
            return RedirectToAction(action, controller);
        }
        public ActionResult getMessage(Enum status, string meesage, string returnUrl)
        {
            TempData["Message"] = string.IsNullOrEmpty(meesage) ? "تمت العملية بنجاح" : meesage;
            TempData["Status"] = status;
            return Redirect(returnUrl);
        }

        public string SendNotification(dynamic obj, string deviceToken, int type, string title)
        {
            try
            {
                string NotificationMessage = "";
                if (obj != null)
                {
                    NotificationMessage = JsonConvert.SerializeObject(obj);
                }
                string AppID = "AAAAmvu4dyI:APA91bGCxkdEnbYRdS91cQMQTZOldfCwRXc-IHoX-YT6D9NxfCWNUvc-6D-23s0TRoMs4UYU0tL5_ZJzubo8Fz4kQ_jMciqsGWp1xJa-8U4NjjYfs3SJbHRsum9zfj5CaGShzpva54SP";
                string senderId = "665648133922";
                const string contentType = "application/json";
                ServicePointManager.DefaultConnectionLimit = 1000;
                CookieContainer cookies = new CookieContainer();
                HttpWebRequest webRequest = WebRequest.Create("https://fcm.googleapis.com/fcm/send") as HttpWebRequest;
                WebHeaderCollection headerCollection = webRequest.Headers;
                ServicePointManager.Expect100Continue = true;
                ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls
                            | SecurityProtocolType.Tls11
                            | SecurityProtocolType.Tls12;
                webRequest.Method = "POST";
                webRequest.Headers["Authorization"] = "Key=" + AppID;
                webRequest.Headers.Add(string.Format("Sender: id={0}", senderId));

                string Data = "{\r\n\"to\":\"" + deviceToken + "\",\r\n \"data\" : {\r\n  \"sound\" : \"default\"";
                if (obj != null)
                {
                    foreach (var item in obj.GetType().GetProperties())
                    {
                        Data = Data + ",\r\n  \"" + item.Name + "\" : \"" + item.GetValue(obj) + "\"";
                    }
                }
                Data = Data + ",\r\n  \"title\" : \"" + title + "\",\r\n  \"content_available\" : true,\r\n  \"type\" : \"" + type.ToString() + "\",\r\n  \"priority\" : \"high\"\r\n }\r\n";
                Data = Data + ",\r\n \"notification\" : {\r\n  \"sound\" : \"default\"";
                if (obj != null)
                {
                    foreach (var item in obj.GetType().GetProperties())
                    {
                        Data = Data + ",\r\n  \"" + item.Name + "\" : \"" + item.GetValue(obj) + "\"";
                    }
                }
                Data = Data + ",\r\n  \"title\" : \"" + title + "\",\r\n  \"content_available\" : true,\r\n  \"type\" : \"" + type.ToString() + "\",\r\n  \"priority\" : \"high\"\r\n }\r\n";

                Data = Data + "}";

                webRequest.ContentType = contentType;
                webRequest.CookieContainer = cookies;
                webRequest.ContentLength = Data.Length;
                webRequest.SendChunked = true;
                webRequest.UserAgent = "Mozilla/5.0 (Windows; U; Windows NT 5.1; en-US; rv:1.9.0.1) Gecko/2008070208 Firefox/3.0.1";
                webRequest.Accept = "text/html,application/xhtml+xml,application/json,application/xml;q=0.9,*;q=0.8";
                webRequest.Referer = "https://accounts.craigslist.org";
                StreamWriter requestWriter = new StreamWriter(webRequest.GetRequestStream());
                requestWriter.Write(Data);
                requestWriter.Flush();
                StreamReader responseReader = new StreamReader(webRequest.GetResponse().GetResponseStream());
                string responseData = responseReader.ReadToEnd();


                responseReader.Close();
                using (System.IO.StreamWriter file =
                   new System.IO.StreamWriter(@"d:\WriteLines2" + ".txt", true))
                {
                    file.WriteLine("notification :" + Data);
                    file.WriteLine();
                    file.WriteLine("response :" + responseData);
                    file.WriteLine();


                }
                if (responseData.Contains("\"success\":1"))
                    return "1";
                else

                    return "0";

            }
            catch (Exception ex)
            {
                return ex.Message.ToString();
            }
        }
    }
}