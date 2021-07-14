using Newtonsoft.Json;
using NileThink.Framework.PrivateLessonManagementSystem.Web.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;

namespace NileThink.Framework.PrivateLessonManagementSystem.Web.Helper
{
    public static class GeneralExtention
    {
        public static ZoomWebinar RefreshZoomUrl(long meetingId)
        {
            try
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
                string url = string.Format("{0}/{1}", "https://api.zoom.us/v2/webinars", meetingId);

                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                request.ContentType = "application/x-www-form-urlencoded";
                request.Method = "GET";
                request.Headers.Add("Authorization", ConfigurationManager.AppSettings["ZoomAuthorization"]);
                string html = string.Empty;
                WebResponse response = request.GetResponse();
                using (Stream stream = response.GetResponseStream())
                using (StreamReader reader = new StreamReader(stream))
                {
                    html = reader.ReadToEnd();
                }
                var zoomMeeting = JsonConvert.DeserializeObject<ZoomWebinar>(html);
                return zoomMeeting;
               
            }
            catch (Exception ex)
            {
                return null;
            }

        }
    }
}