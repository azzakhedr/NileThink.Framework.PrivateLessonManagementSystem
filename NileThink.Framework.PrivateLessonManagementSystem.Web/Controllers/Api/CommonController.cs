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
using Newtonsoft.Json;

namespace NileThink.Framework.PrivateLessonManagementSystem.Web.Controllers.Api
{
    public class CommonController : BaseController
    {
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
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
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
