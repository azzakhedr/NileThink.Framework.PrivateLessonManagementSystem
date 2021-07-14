using Newtonsoft.Json;
using NileThink.Framework.PrivateLessonManagementSystem.Web.Helper;
using NileThink.Framework.PrivateLessonManagementSystem.Web.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Http;
using System.Web.Http.Description;



namespace NileThink.Framework.PrivateLessonManagementSystem.Web.Controllers.Api
{
    [RoutePrefix("api/v1/Zoom")]
    public class ZoomController : ApiController
    {
       
        [AllowAnonymous]
        [Route("CreateWebinar")]
        [ResponseType(typeof(string))]
        [ResponseCodes(HttpStatusCode.OK)]
        [HttpGet]
        public IHttpActionResult CreateWebinar()
        {
            CallApiCreateWebinarsnRequest();

            return this.ResponseOK(new ResponseViewModel(HttpStatusCode.OK, "هنالك خطأ في البيانات", true, null));

        }
        // GET: Zoom

        public string CallApiCreateWebinarsnRequest()
        {
            try
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
                var webinar = new WebinarModel()
                {
                    agenda = "Zoom Webinar",
                    duration = "60",
                    password = "",
                    start_time = DateTime.Parse("2020-09-20T06:59:00Z"),
                    timezone = "Asia/Riyadh",
                    topic = "Zoom Webinar",
                    type = 5,
                    recurrence = new Recurrence()
                    {
                        end_date_time = DateTime.Parse("2020-09-22T06:59:00Z"),
                        repeat_interval = 1,
                        type = 1
                    },
                    settings = new Settings()
                    {
                        host_video = "true",
                        panelists_video = "true",
                        practice_session = "true",
                        hd_video = "true",
                        approval_type = 0,
                        registration_type = 2,
                        audio = "both",
                        auto_recording = "none",
                        enforce_login = "false",
                        close_registration = "true",
                        show_share_button = "true",
                        allow_multiple_devices = "false",
                        registrants_email_notification = "true"
                    }

                };

                string postData = JsonConvert.SerializeObject(webinar);

                string url =string.Format("{0}{1}", ConfigurationManager.AppSettings["ZoomApiUrl"], "users/"+ ConfigurationManager.AppSettings["ZoomUserId"] + "/webinars");

                byte[] byteArray = Encoding.UTF8.GetBytes(postData);

                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                request.ContentType = "application/json";
                request.Method = "POST";
                request.Headers.Add("Authorization", ConfigurationManager.AppSettings["ZoomAuthorization"]);
                request.ContentLength = byteArray.Length;
                Stream dataStream = request.GetRequestStream();
                // Write the data to the request stream.
                dataStream.Write(byteArray, 0, byteArray.Length);
                // Close the Stream object.
                dataStream.Close();
                string html = string.Empty;
                WebResponse response = request.GetResponse();
                using (Stream stream = response.GetResponseStream())
                using (StreamReader reader = new StreamReader(stream))
                {
                    html = reader.ReadToEnd();
                }

                return html;
            }
            catch (Exception ex)
            {
                return "";

            }

        }
       
        
    }
}