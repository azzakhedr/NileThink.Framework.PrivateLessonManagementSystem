using Microsoft.ApplicationInsights.Extensibility.Implementation;
using NileThink.Framework.PrivateLessonManagementSystem.BLL.BussinessLayer;
using NileThink.Framework.PrivateLessonManagementSystem.BLL.ViewModels;
using NileThink.Framework.PrivateLessonManagementSystem.Web.Helper;
using NileThink.Framework.PrivateLessonManagementSystem.Web.Models;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using System.Web.Script.Serialization;
using Newtonsoft.Json;
using System.Text.RegularExpressions;

namespace NileThink.Framework.PrivateLessonManagementSystem.Web.Controllers.Api
{
    [RoutePrefix("api/v1/Payment")]
    public class PaymentController : ApiController
    {
        StudentBLL _studentBll = new StudentBLL();
        PaymentBLL _paymet = new PaymentBLL();
        RequestCourceBLL _request = new RequestCourceBLL();
        ScheduleLessonsBLL _lesson = new ScheduleLessonsBLL();
        [Authorize]
        [Route("AddRequestPayment")]
        [ResponseType(typeof(string))]
        [ResponseCodes(HttpStatusCode.OK)]
        [HttpGet]
        public IHttpActionResult AddRequestPayment(int requestId)
        {
            try
            {
                var Coursre = _request.GetRequestDetailsBytId(requestId);
                if (Coursre != null)
                {
                    var totalprice = CalculateVat(float.Parse(Coursre.totalPrice.ToString()));
                    var request = GetCheckoutRequest(totalprice, requestId, Coursre.studentId);

                    if (request != null)
                    {
                        try
                        {



                            return this.ResponseOK(new ResponseViewModel(HttpStatusCode.OK, "تم ارسال طلبك بنجاح", true, new InviteResponse { checkoutId = request.id }));
                        }
                        catch
                        {
                            return this.ResponseOK(new ResponseViewModel(HttpStatusCode.OK, "هنالك خطأ في البيانات", false, null));
                        }
                    }
                    return this.ResponseOK(new ResponseViewModel(HttpStatusCode.OK, "هنالك خطأ في البيانات", false, null));
                }
                return this.ResponseOK(new ResponseViewModel(HttpStatusCode.OK, "هنالك خطأ في البيانات", false, null));
            }
            catch
            {
                return this.ResponseOK(new ResponseViewModel(HttpStatusCode.OK, "هنالك خطأ في البيانات", false, null));
            }


        }
        public float CalculateVat(float Price)
        {
            var Vat = ConfigurationManager.AppSettings["Vat"];
            return Price + (Price / 100) * float.Parse(Vat);
        }

        public string CallApiRequest(float? amount, int? request_id, int? getway = 0)
        {
            try
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
                string postData = ConfigurationManager.AppSettings["HyperPayEntityId"] +
                   "&amount=" + String.Format("{0:0.00}", amount) +
                   "&currency=SAR" +
                   "&paymentType=DB" +
                   "&notificationUrl=" + ConfigurationManager.AppSettings["HyperPayNotify"] + "/" + request_id;

                string url = ConfigurationManager.AppSettings["HyperPayTestUrl"];// "https://test.oppwa.com/v1/checkouts";

                byte[] byteArray = Encoding.UTF8.GetBytes(postData);

                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                request.ContentType = "application/x-www-form-urlencoded";
                request.Method = "POST";
                request.Headers.Add("Authorization", ConfigurationManager.AppSettings["HyperPayAuthorization"]);
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
        // GET api/<controller>
        [Authorize]
        [Route("PaymentStatus")]
        [HttpGet]
        [ResponseType(typeof(PaymentModelResult))]
        public IHttpActionResult PaymentStatus(string checkoutId, int requestId)
        {
            try
            {

                var Coursre = _request.GetRequestDetailsBytId(requestId);
                if (Coursre != null)
                {
                   // var ZoomMeetings = GenerateZoom(requestId, DateTime.Now, DateTime.Now.AddHours(4));
                    //var t = ZoomUrl(99371489528);
                    var student = _studentBll.Student_GetById(Coursre.studentId);
                   // if (student.userId != User.Identity.GetUserId()) return this.ResponseUnauthorized(new ResponseViewModel(HttpStatusCode.Unauthorized, "", false, null));
                    var totalprice = CalculateVat(float.Parse(Coursre.totalPrice.ToString()));
                    var Response = GetRequestStatus(checkoutId);
                    var paymentmodel = new JavaScriptSerializer().Deserialize<PaymentModelResult>(Response);
                    if (paymentmodel != null)
                    {
                        
                        if (paymentmodel.Result.code != null)
                        {
                            Regex successPattern = new Regex(@"(000\.000\.|000\.100\.1|000\.[36])");
                            Regex successManuelPattern = new Regex(@"(000\.400\.0[^3]|000\.400\.100)");
                            Match matchsuccess = successPattern.Match(paymentmodel.Result.code);
                            Match matchManuel = successManuelPattern.Match(paymentmodel.Result.code);
                            Regex pendingPattern = new Regex(@"(000\.200)");
                            Match matchpending = pendingPattern.Match(paymentmodel.Result.code);
                            if (matchpending.Success)
                            {
                                var updateId = _request.RequestCourceUpdateStatus(requestId, 3, "Pending");
                                return this.ResponseOK(new ResponseViewModel(HttpStatusCode.OK, "", true, paymentmodel));
                            }
                            if (matchsuccess.Success || matchManuel.Success)
                            {
                                var payment = new PaymentVM()
                            {
                                CheckOut = checkoutId,
                                PaymentStatus = "1",
                                TotalPrice = totalprice
                            };
                                
                                    var id = _paymet.AddCoursePayment(payment, Coursre.requestId);
                                // check if payment success
                               
                                    var dates = _request.GetRequestDatesByRequestId(requestId);
                                    if (dates != null)
                                    {
                                    foreach (var item in dates)
                                    {
                                        var ZoomMeeting = GenerateZoom(requestId, item.startDate.Value, item.endDate.Value);
                                        var lesson = new ScheduleLessonsVM()
                                        {
                                            requestId = requestId,
                                            requestDateId = item.id,
                                            startDate = ZoomMeeting.start_time,
                                            endDate = ZoomMeeting.start_time.AddMinutes(ZoomMeeting.duration),
                                            conferanceZoom = ZoomMeeting.start_url,
                                            studentZoom = ZoomMeeting.join_url,
                                            MeetingId= ZoomMeeting.id


                                        };
                                        var lessonid = _lesson.AddScheduleLesson(lesson);

                                    }

                                    var updateId = _request.RequestCourceUpdateStatus(requestId, 2, "Confirmed");
                                    return this.ResponseOK(new ResponseViewModel(HttpStatusCode.OK, "", true, paymentmodel));
                                }
                            }
                            return this.ResponseOK(new ResponseViewModel(HttpStatusCode.OK, "هنالك خطأ في البيانات", false, null));
                        }
                        
                    }
                    return this.ResponseOK(new ResponseViewModel(HttpStatusCode.OK, "هنالك خطأ في البيانات", false, null));
                }
                return this.ResponseOK(new ResponseViewModel(HttpStatusCode.OK, "هنالك خطأ في البيانات", false, null));
            }
            catch(Exception ex)
            {
                return this.ResponseOK(new ResponseViewModel(HttpStatusCode.OK, "هنالك خطأ في البيانات", false, null));
            }

        }
        public PaymentModelResult GetCheckoutRequest(float? amount, int? request_id, int studentId, int? gateway = 0)
        {
            var Student = _studentBll.Student_GetById(studentId);
            string email, firstname, surname, street, city, state;

            email = Student.email;
            firstname = Student.firstName;
            surname = Student.lastName;
            street = !string.IsNullOrEmpty(Student.streetNo) ? Student.streetNo : "unknown";
            city = string.IsNullOrEmpty(Student.city) ? Student.city : "unknown";
            state = string.IsNullOrEmpty(Student.district) ? Student.district : "unknown";

            ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
            PaymentModelResult responseData;
            string data = ConfigurationManager.AppSettings["HyperPayEntityId"].ToString() +//  "entityId=8acda4cc77b5a3a30177ce2752532d43" +
                "&amount=" + String.Format("{0:0.00}", amount) +
                "&currency=SAR" +
                "&paymentType=DB" +
                "&notificationUrl=" + ConfigurationManager.AppSettings["HyperPayNotify"] + "/" + request_id +
                "&customer.email=" + email +
                "&billing.street1=" + street +
                "&billing.city=" + city +
                "&billing.state=" + state +
                "&billing.country=SA" +
                "&billing.postcode=11543" +
                "&customer.givenName=" + firstname +
                "&customer.surname=" + surname +
                "&testMode=EXTERNAL" +
                "&merchantTransactionId=" + Guid.NewGuid();

            string url = ConfigurationManager.AppSettings["HyperPayUrl"];
            // string url = " https://oppwa.com/v1/checkouts";
            byte[] buffer = Encoding.ASCII.GetBytes(data);
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(url);
            request.Method = "POST";
            request.Headers["Authorization"] = ConfigurationManager.AppSettings["HyperPayAuthorization"];// "Bearer OGFjZGE0Y2M3N2I1YTNhMzAxNzdjZTI2ZTU1NzJkM2J8WGpicVRxWjRCaw==";
            request.ContentType = "application/x-www-form-urlencoded";
            Stream PostData = request.GetRequestStream();
            PostData.Write(buffer, 0, buffer.Length);
            PostData.Close();
            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            {
                Stream dataStream = response.GetResponseStream();
                StreamReader reader = new StreamReader(dataStream);
                responseData = new JavaScriptSerializer().Deserialize<PaymentModelResult>(reader.ReadToEnd());
                reader.Close();
                dataStream.Close();
            }
            return responseData;
        }
        public string GetRequestStatus(string checkout_id)
        {
            try
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
                string data = ConfigurationManager.AppSettings["HyperPayEntityId"].ToString();  //"entityId=8acda4cc77b5a3a30177ce2752532d43";
                                                                                                // string url = "https://test.oppwa.com/v1/checkouts" + checkout_id + "/payment?" + data;

                string url = ConfigurationManager.AppSettings["HyperPayUrl"]+"/" + checkout_id + "/payment?" + data;



                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                request.ContentType = "application/x-www-form-urlencoded";
                request.Method = "GET";
                request.Headers.Add("Authorization", ConfigurationManager.AppSettings["HyperPayAuthorization"]);
                string html = string.Empty;
                WebResponse response = request.GetResponse();
                using (Stream stream = response.GetResponseStream())
                using (StreamReader reader = new StreamReader(stream))
                {
                    html = reader.ReadToEnd();
                }

                return html;
            }
            catch (WebException ex)
            {

                Console.WriteLine("Error Status Code : {1} {0}", ((HttpWebResponse)ex.Response).StatusCode, (int)((HttpWebResponse)ex.Response).StatusCode);
                using (var stream = ex.Response.GetResponseStream())
                using (var reader = new StreamReader(stream))
                {
                    return reader.ReadToEnd();

                }
            }
        }
       
        public ZoomWebinar GenerateZoom(int requestId, DateTime startDate, DateTime endDate)
        {
            try
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
                var webinar = new WebinarModel()
                {
                    agenda = "Private Lesson",
                    duration = "60",
                    password = "",
                    start_time = startDate,
                    timezone = "Asia/Riyadh",
                    topic = "Private Lesson",
                    type = 5,
                    recurrence = new Recurrence()
                    {
                        end_date_time = endDate,
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

                string url = string.Format("{0}{1}", ConfigurationManager.AppSettings["ZoomApiUrl"], "users/" + ConfigurationManager.AppSettings["ZoomUserId"] + "/webinars");

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