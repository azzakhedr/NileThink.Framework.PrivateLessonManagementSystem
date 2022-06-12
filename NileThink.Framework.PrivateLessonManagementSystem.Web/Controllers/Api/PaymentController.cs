//using Microsoft.ApplicationInsights.Extensibility.Implementation;
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
using System.Web;
using PrivateLessonMS.Resources;

namespace NileThink.Framework.PrivateLessonManagementSystem.Web.Controllers.Api
{
    [RoutePrefix("api/v1/Payment")]
    public class PaymentController : BaseController
    {
        StudentBLL _studentBll = new StudentBLL();
        PaymentBLL _paymet = new PaymentBLL();
        RequestCourceBLL _request = new RequestCourceBLL();
        ScheduleLessonsBLL _lesson = new ScheduleLessonsBLL();

        TeacherBLL _TeacherBll = new TeacherBLL();
        NotificationBLL _notificationBLL = new NotificationBLL();
        CommonController _comm = new CommonController();
        PackageBLL _package = new PackageBLL();
        MembershipPackageBLL _membpackage = new MembershipPackageBLL();


        [Authorize]
        [Route("AddRequestPayment")]
        [ResponseType(typeof(string))]
        [ResponseCodes(HttpStatusCode.OK)]
        [HttpGet]
        public IHttpActionResult AddRequestPayment(int requestId, string paymentBrand)
        {
            string Lang = lang;
            try
            {
                var Coursre = _request.GetRequestDetailsBytId(requestId);
                if (Coursre != null)
                {
                    var totalprice = CalculateVat(float.Parse(Coursre.totalPrice.ToString()));
                    var responseData = GetCheckoutRequest(totalprice, requestId, Coursre.studentId, null, paymentBrand);

                    PaymentModelResult model = new PaymentModelResult();


                    if (responseData != null)
                    {
                        using (StreamWriter writetext = new StreamWriter(HttpContext.Current.Server.MapPath("~/write.txt"), true))
                        {
                            writetext.WriteLine("----------------------------------- Payment responseData " + DateTime.Now + " -----------------");
                            foreach (var group in responseData)
                            {
                                writetext.WriteLine("Key: {0} Value: {1}", group.Key, group.Value);
                            }
                            foreach (var group in responseData["result"])
                            {
                                writetext.WriteLine("Key: {0} Value: {1}", group.Key, group.Value);
                            }
                            writetext.WriteLine("responseData : " + responseData);
                            writetext.WriteLine("----------------------------------- Payment responseData" + DateTime.Now + " -----------------");
                        }

                        foreach (var group in responseData)
                        {
                            if (group.Key == "id")
                            {
                                model.checkout_id = group.Value;

                            }
                            if (group.Key == "ndc")
                            {
                                model.ndc = group.Value;

                            }

                            if (group.Key == "result")
                            {
                                foreach (var group2 in responseData["result"])
                                {
                                    if (group2.Key == "description")
                                    {
                                        model.description = group2.Value;
                                        if (group2.Value == "successfully created checkout")
                                        {
                                            model.status = 1;
                                            //  db.SaveChanges();
                                        }
                                        else
                                        {
                                            model.status = -1;
                                        }

                                    }
                                    if (group.Key == "code")
                                    {
                                        model.code = group2.Value;
                                    }


                                }
                            }

                        }


                        return this.ResponseOK(new ResponseViewModel(HttpStatusCode.OK, "", true, model));
                    }
                    return this.ResponseOK(new ResponseViewModel(HttpStatusCode.OK, Resource.ErrorOccure, false, null));


                }
                return this.ResponseOK(new ResponseViewModel(HttpStatusCode.OK, Resource.ErrorOccure, false, null));
            }
            catch
            {
                return this.ResponseOK(new ResponseViewModel(HttpStatusCode.OK, Resource.ErrorOccure, false, null));
            }


        }
        public float CalculateVat(float Price)
        {
            var Vat = ConfigurationManager.AppSettings["Vat"];
            return Price + (Price / 100) * float.Parse(Vat);
        }

        public string CallApiRequest(float? amount, int? request_id, int? getway = 0, string paymentBrand = "")
        {
            try
            {
                string EntityId = "";
                if (paymentBrand == "VISA" || paymentBrand == "MASTER")
                {

                    EntityId = ConfigurationManager.AppSettings["HyperPayEntityIdVISA"].ToString();
                }
                else
                {

                    EntityId = ConfigurationManager.AppSettings["HyperPayEntityIdMADA"].ToString();
                }

                ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
                string postData = EntityId +
                   "&amount=" + String.Format("{0:0.00}", amount) +
                   "&currency=SAR" +
                   "&paymentType=DB" +
                   "&notificationUrl=" + ConfigurationManager.AppSettings["HyperPayNotify"] + "/" + request_id;



                string url = ConfigurationManager.AppSettings["HyperPayUrl"];// "https://test.oppwa.com/v1/checkouts";

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
        public IHttpActionResult PaymentStatus(string checkoutId, int requestId, string paymentBrand)

        {
            string Lang = lang;
            try
            {
                Regex successPattern = new Regex(@"(000\.000\.|000\.100\.1|000\.[36])");
                Regex successManuelPattern = new Regex(@"(000\.400\.0[^3]|000\.400\.100)");
                Regex pendingPattern = new Regex(@"(000\.200)");
                bool match1success = false;
                bool match2success = false;
                bool matchpendingsuccess = false; ;
                double out_amount = 0;
                var Coursre = _request.GetRequestDetailsBytId(requestId);
                if (Coursre != null)
                {
                    // var ZoomMeetings = GenerateZoom(requestId, DateTime.Now, DateTime.Now.AddHours(4));
                    //var t = ZoomUrl(99371489528);
                    var student = _studentBll.Student_GetById(Coursre.studentId);
                    // if (student.userId != User.Identity.GetUserId()) return this.ResponseUnauthorized(new ResponseViewModel(HttpStatusCode.Unauthorized, "", false, null));
                    var totalprice = CalculateVat(float.Parse(Coursre.totalPrice.ToString()));
                    var responseData = statusrequest(checkoutId, paymentBrand);
                    // var responseData = GetRequestStatus(checkoutId, paymentBrand);

                    PaymentModelResult model = new PaymentModelResult();
                    if (responseData != null)
                    {
                        foreach (var group in responseData)
                        {
                            if (group.Key == "ndc")
                            {
                                model.ndc = group.Value;
                                model.checkout_id = group.Value;
                            }
                            if (group.Key == "amount")
                            {
                                out_amount = Convert.ToDouble(group.Value);
                            }
                            if (group.Key == "result")
                            {
                                foreach (var group2 in responseData["result"])
                                {
                                    if (group2.Key == "description")
                                    {
                                        model.description = group2.Value;


                                    }
                                    if (group2.Key == "code")
                                    {
                                        model.code = group2.Value;

                                        Match match1 = successPattern.Match(group2.Value);
                                        Match match2 = successManuelPattern.Match(group2.Value);
                                        Match matchpending = pendingPattern.Match(group2.Value);
                                        if (match1.Success)
                                        {
                                            match1success = true;
                                        }
                                        if (match2.Success)
                                        {
                                            match2success = true;
                                        }
                                        if (matchpending.Success)
                                        {
                                            matchpendingsuccess = true;
                                        }

                                    }


                                }
                            }

                        }

                        if (match1success || match2success)
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
                                        MeetingId = ZoomMeeting.id


                                    };
                                    var lessonid = _lesson.AddScheduleLesson(lesson);

                                }

                                var updateId = _request.RequestCourceUpdateStatus(requestId, 2, "Confirmed");

                                _paymet.InsertPaymentWalletTransaction(requestId, Coursre.teacherId, decimal.Parse(Coursre.totalPrice.ToString()));

                                var lst = _notificationBLL.GetNotificationToken(Coursre.studentId, 1);


                                var teacher = _TeacherBll.Teacher_GetById(Coursre.teacherId);
                                //var student = studentBLL.Student_GetById(Coursre.studentId);
                                dynamic returndata = new
                                {
                                    CourseId = Coursre.requestId,
                                    teacherId = Coursre.teacherId,
                                    teacherName = teacher.fullName,
                                    studentId = Coursre.studentId,
                                    studentName = student.fullName,
                                    subject = Coursre.subject,
                                    type = 4
                                };
                                string NotificationMessage = JsonConvert.SerializeObject(returndata);
                                var title = "تمت عملية الدفع بنجاح";
                                _notificationBLL.InsertUserNotification(new NotificationVM() { course_id = Coursre.requestId, details = NotificationMessage, title = title, typeId = 1, userId = Coursre.studentId, user_type = 1 });
                                foreach (var item in lst)
                                {
                                    _comm.SendNotification(returndata, item.Token, 1, title);

                                }
                                return this.ResponseOK(new ResponseViewModel(HttpStatusCode.OK, model.description, true, model));
                            }
                        }
                        if (matchpendingsuccess)
                        {
                            var updateId = _request.RequestCourceUpdateStatus(requestId, 3, Resource.Pending);
                            // insert teacher amount -- entity amount
                            return this.ResponseOK(new ResponseViewModel(HttpStatusCode.OK, model.description, true, model));
                        }
                        else
                        {
                            return this.ResponseOK(new ResponseViewModel(HttpStatusCode.OK, model.description, false, model));

                        }
                        return this.ResponseOK(new ResponseViewModel(HttpStatusCode.OK, model.description, false, model));

                    }

                    return this.ResponseOK(new ResponseViewModel(HttpStatusCode.OK, Resource.ErrorOccure, false, null));
                }
                return this.ResponseOK(new ResponseViewModel(HttpStatusCode.OK, Resource.NoCourseFound, false, null));
            }
            catch (Exception ex)
            {
                return this.ResponseOK(new ResponseViewModel(HttpStatusCode.OK, ex.Message, false, null));
            }

        }
        public Dictionary<string, dynamic> GetCheckoutRequest(float? amount, int? request_id, int studentId, int? gateway = 0, string paymentBrand = "")
        {
            var Student = _studentBll.Student_GetById(studentId);
            string email, firstname, surname, street, city, state;

            email = Student.email;
            firstname = Student.firstName;
            surname = Student.lastName;
            street = !string.IsNullOrEmpty(Student.streetNo) ? Student.streetNo : "unknown";
            city = string.IsNullOrEmpty(Student.city) ? Student.city : "unknown";
            state = string.IsNullOrEmpty(Student.district) ? Student.district : "unknown";

            //ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
            Dictionary<string, dynamic> responseData;
            string EntityId = "";
            var notifTrl = ConfigurationManager.AppSettings["ProjectURL"].ToString() + "/Home/PaymentNotify/" + request_id;

            StringBuilder data = new StringBuilder();

            if (paymentBrand == "VISA" || paymentBrand == "MASTER")
            {
                data.Append(ConfigurationManager.AppSettings["HyperPayEntityIdVISA"].ToString());

            }
            else
            {
                data.Append(ConfigurationManager.AppSettings["HyperPayEntityIdMADA"].ToString());

            }
            data.Append("&notificationUrl=" + notifTrl);
            data.Append("&amount=" + String.Format("{0:0.00}", amount));
            data.Append("&currency=SAR");
            data.Append("&paymentType=DB");
            data.Append("&merchantTransactionId=" + Guid.NewGuid());
            data.Append("&customer.surname=" + surname);
            data.Append("&billing.street1=" + street);
            data.Append("&billing.city=" + city);
            data.Append("&billing.state=" + state);
            data.Append("&billing.country=SA");
            data.Append("&billing.postcode=" + "21577");
            data.Append("&customer.email=" + email);
            data.Append("&customer.givenName=" + firstname);


            string url = ConfigurationManager.AppSettings["HyperPayUrl"].ToString();
            byte[] buffer = Encoding.ASCII.GetBytes(data.ToString());
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(url);
            request.Method = "POST";
            //////////////////////////////////////Alllli////////////////////////////////////
            request.Headers["Authorization"] = ConfigurationManager.AppSettings["HyperPayAuthorization"].ToString();

            // request.Headers["Authorization"] = "Bearer OGFjN2E0Yzc3MDY4MjJjODAxNzA3NzcxYjVmMDBhNWR8aFo5Y2NIZkRYRQ==";
            ////////////////////////////////////////Allli//////////////////////////////////////////////
            request.ContentType = "application/x-www-form-urlencoded";
            Stream PostData = request.GetRequestStream();
            PostData.Write(buffer, 0, buffer.Length);
            PostData.Close();
            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            {
                Stream dataStream = response.GetResponseStream();
                StreamReader reader = new StreamReader(dataStream);
                var s = new System.Web.Script.Serialization.JavaScriptSerializer();
                responseData = s.Deserialize<Dictionary<string, dynamic>>(reader.ReadToEnd());
                reader.Close();
                dataStream.Close();
            }
            return responseData;
        }
        public string GetRequestStatus(string checkout_id, string paymentBrand)
        {
            try
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;

                string EntityId = "";
                if (paymentBrand == "VISA" || paymentBrand == "MASTER")
                {

                    EntityId = ConfigurationManager.AppSettings["HyperPayEntityIdVISA"].ToString();
                }
                else
                {
                    EntityId = ConfigurationManager.AppSettings["HyperPayEntityIdMADA"].ToString();
                }
                string url = ConfigurationManager.AppSettings["HyperPayUrl"] + "/" + checkout_id;// + "/payment?" + EntityId;



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

        public Dictionary<string, dynamic> statusrequest(string checkout_id, string PaymentBrand)
        {
            Dictionary<string, dynamic> responseData;
            string data = "";
            if (PaymentBrand == "VISA" || PaymentBrand == "MASTER")
            {

                data = ConfigurationManager.AppSettings["HyperPayEntityIdVISA"].ToString();
            }
            else
            {
                data = ConfigurationManager.AppSettings["HyperPayEntityIdMADA"].ToString();
            }
            ///////////////////////////////////////////////////////////////////////////////////////////
            string url = ConfigurationManager.AppSettings["HyperPayUrl"] + "/" + checkout_id + "/payment?" + data;



            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(url);
            request.Method = "GET";
            //////////////////////////////Alllli///////////////////////////////////////
            request.Headers["Authorization"] = ConfigurationManager.AppSettings["HyperPayAuthorization"].ToString();

            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            {
                Stream dataStream = response.GetResponseStream();
                StreamReader reader = new StreamReader(dataStream);
                var s = new JavaScriptSerializer();
                responseData = s.Deserialize<Dictionary<string, dynamic>>(reader.ReadToEnd());
                reader.Close();
                dataStream.Close();
            }
            return responseData;
        }

        #region ---------- package Payment ----------
        [Authorize]
        [Route("AddPackagePayment")]
        [ResponseType(typeof(string))]
        [ResponseCodes(HttpStatusCode.OK)]
        [HttpGet]
        public IHttpActionResult AddPackagePayment(long TeacherPackId, string paymentBrand)
        {
            string Lang = lang;
            try
            {
                var package = _package.GetTeacherPackById(TeacherPackId);
                if (package != null)
                {
                    var totalprice = CalculateVat(float.Parse(package.fees.ToString()));
                    var responseData = GetCheckoutPayment(totalprice, package.teacher_id, null, paymentBrand);

                    PaymentModelResult model = new PaymentModelResult();


                    if (responseData != null)
                    {
                        using (StreamWriter writetext = new StreamWriter(HttpContext.Current.Server.MapPath("~/write.txt"), true))
                        {
                            writetext.WriteLine("----------------------------------- Payment responseData " + DateTime.Now + " -----------------");
                            foreach (var group in responseData)
                            {
                                writetext.WriteLine("Key: {0} Value: {1}", group.Key, group.Value);
                            }
                            foreach (var group in responseData["result"])
                            {
                                writetext.WriteLine("Key: {0} Value: {1}", group.Key, group.Value);
                            }
                            writetext.WriteLine("responseData : " + responseData);
                            writetext.WriteLine("----------------------------------- Payment responseData" + DateTime.Now + " -----------------");
                        }

                        foreach (var group in responseData)
                        {
                            if (group.Key == "id")
                            {
                                model.checkout_id = group.Value;
                                _package.UpdatePackageCheckOutId(package.id, model.checkout_id);

                            }
                            if (group.Key == "ndc")
                            {
                                model.ndc = group.Value;

                            }

                            if (group.Key == "result")
                            {
                                foreach (var group2 in responseData["result"])
                                {
                                    if (group2.Key == "description")
                                    {
                                        model.description = group2.Value;
                                        if (group2.Value == "successfully created checkout")
                                        {
                                            model.status = 1;
                                            //  db.SaveChanges();
                                        }
                                        else
                                        {
                                            model.status = -1;
                                        }

                                    }
                                    if (group.Key == "code")
                                    {
                                        model.code = group2.Value;
                                    }


                                }
                            }

                        }


                        return this.ResponseOK(new ResponseViewModel(HttpStatusCode.OK, "", true, model));
                    }
                    return this.ResponseOK(new ResponseViewModel(HttpStatusCode.OK, Resource.ErrorOccure, false, null));


                }
                return this.ResponseOK(new ResponseViewModel(HttpStatusCode.OK, Resource.ErrorOccure, false, null));
            }
            catch (Exception e)
            {
                return this.ResponseOK(new ResponseViewModel(HttpStatusCode.OK, Resource.ErrorOccure, false, null));
            }


        }

        public Dictionary<string, dynamic> GetCheckoutPayment(float? amount, int teacherId, int? gateway = 0, string paymentBrand = "")
        {
            var package = _package.GetCurrentPackageDataNotPaid(teacherId);
            var teacher = package.Teacher;
            string email, firstname, surname, street, city, state;

            email = teacher.Email;
            firstname = teacher.FirstName;
            surname = teacher.LastName;
            street =// !string.IsNullOrEmpty(teacher.streetNo) ? teacher.streetNo :
                "unknown";
            city = string.IsNullOrEmpty(teacher.CityName) ? teacher.CityName : "unknown";
            state = string.IsNullOrEmpty(teacher.District) ? teacher.District : "unknown";

            //ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
            Dictionary<string, dynamic> responseData;
            string EntityId = "";
            var notifTrl = ConfigurationManager.AppSettings["ProjectURL"].ToString() + "/Home/PaymentNotify/" + package.id;

            StringBuilder data = new StringBuilder();

            if (paymentBrand == "VISA" || paymentBrand == "MASTER")
            {
                data.Append(ConfigurationManager.AppSettings["HyperPayEntityIdVISA"].ToString());

            }
            else
            {
                data.Append(ConfigurationManager.AppSettings["HyperPayEntityIdMADA"].ToString());

            }
            data.Append("&notificationUrl=" + notifTrl);
            data.Append("&amount=" + String.Format("{0:0.00}", amount));
            data.Append("&currency=SAR");
            data.Append("&paymentType=DB");
            data.Append("&merchantTransactionId=" + Guid.NewGuid());
            data.Append("&customer.surname=" + surname);
            data.Append("&billing.street1=" + street);
            data.Append("&billing.city=" + city);
            data.Append("&billing.state=" + state);
            data.Append("&billing.country=SA");
            data.Append("&billing.postcode=" + "21577");
            data.Append("&customer.email=" + email);
            data.Append("&customer.givenName=" + firstname);


            string url = ConfigurationManager.AppSettings["HyperPayUrl"].ToString();
            byte[] buffer = Encoding.ASCII.GetBytes(data.ToString());
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(url);
            request.Method = "POST";
            //////////////////////////////////////Alllli////////////////////////////////////
            request.Headers["Authorization"] = ConfigurationManager.AppSettings["HyperPayAuthorization"].ToString();

            // request.Headers["Authorization"] = "Bearer OGFjN2E0Yzc3MDY4MjJjODAxNzA3NzcxYjVmMDBhNWR8aFo5Y2NIZkRYRQ==";
            ////////////////////////////////////////Allli//////////////////////////////////////////////
            request.ContentType = "application/x-www-form-urlencoded";
            Stream PostData = request.GetRequestStream();
            PostData.Write(buffer, 0, buffer.Length);
            PostData.Close();
            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            {
                Stream dataStream = response.GetResponseStream();
                StreamReader reader = new StreamReader(dataStream);
                var s = new System.Web.Script.Serialization.JavaScriptSerializer();
                responseData = s.Deserialize<Dictionary<string, dynamic>>(reader.ReadToEnd());
                reader.Close();
                dataStream.Close();
            }
            return responseData;
        }


        [Authorize]
        [Route("PaymentPackageStatus")]
        [HttpGet]
        [ResponseType(typeof(PaymentModelResult))]
        public IHttpActionResult PaymentPackageStatus(long TeacherPackId, string paymentBrand)

        {
            string Lang = lang;
            try
            {
                Regex successPattern = new Regex(@"(000\.000\.|000\.100\.1|000\.[36])");
                Regex successManuelPattern = new Regex(@"(000\.400\.0[^3]|000\.400\.100)");
                Regex pendingPattern = new Regex(@"(000\.200)");
                bool match1success = false;
                bool match2success = false;
                bool matchpendingsuccess = false; ;
                double out_amount = 0;
                var package = _package.GetTeacherPackById(TeacherPackId);
                if (package != null)
                {
                    // var ZoomMeetings = GenerateZoom(requestId, DateTime.Now, DateTime.Now.AddHours(4));
                    //var t = ZoomUrl(99371489528);
                    //var teacher = _studentBll.Student_GetById(Coursre.studentId);
                    // if (student.userId != User.Identity.GetUserId()) return this.ResponseUnauthorized(new ResponseViewModel(HttpStatusCode.Unauthorized, "", false, null));
                    var totalprice = CalculateVat(float.Parse(package.fees.ToString()));
                    var responseData = statusrequest(package.checkout_id, paymentBrand);
                    // var responseData = GetRequestStatus(checkoutId, paymentBrand);

                    PaymentModelResult model = new PaymentModelResult();
                    if (responseData != null)
                    {
                        foreach (var group in responseData)
                        {
                            if (group.Key == "ndc")
                            {
                                model.ndc = group.Value;
                                model.checkout_id = group.Value;
                            }
                            if (group.Key == "amount")
                            {
                                out_amount = Convert.ToDouble(group.Value);
                            }
                            if (group.Key == "result")
                            {
                                foreach (var group2 in responseData["result"])
                                {
                                    if (group2.Key == "description")
                                    {
                                        model.description = group2.Value;


                                    }
                                    if (group2.Key == "code")
                                    {
                                        model.code = group2.Value;

                                        Match match1 = successPattern.Match(group2.Value);
                                        Match match2 = successManuelPattern.Match(group2.Value);
                                        Match matchpending = pendingPattern.Match(group2.Value);
                                        if (match1.Success)
                                        {
                                            match1success = true;
                                        }
                                        if (match2.Success)
                                        {
                                            match2success = true;
                                        }
                                        if (matchpending.Success)
                                        {
                                            matchpendingsuccess = true;
                                        }

                                    }


                                }
                            }

                        }

                        if (match1success || match2success)
                        {
                            var payment = new PaymentVM()
                            {
                                CheckOut = package.checkout_id,
                                PaymentStatus = "1",
                                TotalPrice = totalprice
                            };

                            var id = _paymet.AddPackagePayment(payment, package.id);
                            // check if payment success


                        }
                        if (matchpendingsuccess)
                        {
                            var payment = new PaymentVM()
                            {
                                CheckOut = package.checkout_id,
                                PaymentStatus = "3",
                                TotalPrice = totalprice
                            };
                            var id = _paymet.AddPackagePayment(payment, package.id);
                            //var updateId = _request.RequestCourceUpdateStatus(requestId, 3, "Pending");
                            // insert teacher amount -- entity amount
                            return this.ResponseOK(new ResponseViewModel(HttpStatusCode.OK, model.description, true, model));
                        }
                        else
                        {
                            return this.ResponseOK(new ResponseViewModel(HttpStatusCode.OK, model.description, false, model));

                        }
                        return this.ResponseOK(new ResponseViewModel(HttpStatusCode.OK, model.description, false, model));

                    }

                    return this.ResponseOK(new ResponseViewModel(HttpStatusCode.OK, Resource.ErrorOccure, false, null));
                }
                return this.ResponseOK(new ResponseViewModel(HttpStatusCode.OK, Resource.NoCourseFound, false, null));
            }
            catch (Exception ex)
            {
                return this.ResponseOK(new ResponseViewModel(HttpStatusCode.OK, ex.Message, false, null));
            }

        }

        #endregion

        #region RequestPaymentTemp

        [Authorize]
        [Route("AddRequestPaymentTemp")]
        [ResponseType(typeof(string))]
        [ResponseCodes(HttpStatusCode.OK)]
        [HttpGet]
        public IHttpActionResult AddRequestPaymentTemp(int requestId, string paymentBrand)
        {
            string Lang = lang;
            try
            {
                var Coursre = _request.GetRequestDetailsBytIdTemp(requestId);
                if (Coursre != null)
                {
                    var totalprice = CalculateVat(float.Parse(Coursre.totalPrice.ToString()));
                    var responseData = GetCheckoutRequest(totalprice, requestId, Coursre.studentId, null, paymentBrand);

                    PaymentModelResult model = new PaymentModelResult();


                    if (responseData != null)
                    {
                        using (StreamWriter writetext = new StreamWriter(HttpContext.Current.Server.MapPath("~/write.txt"), true))
                        {
                            writetext.WriteLine("----------------------------------- Payment responseData " + DateTime.Now + " -----------------");
                            foreach (var group in responseData)
                            {
                                writetext.WriteLine("Key: {0} Value: {1}", group.Key, group.Value);
                            }
                            foreach (var group in responseData["result"])
                            {
                                writetext.WriteLine("Key: {0} Value: {1}", group.Key, group.Value);
                            }
                            writetext.WriteLine("responseData : " + responseData);
                            writetext.WriteLine("----------------------------------- Payment responseData" + DateTime.Now + " -----------------");
                        }

                        foreach (var group in responseData)
                        {
                            if (group.Key == "id")
                            {
                                model.checkout_id = group.Value;

                            }
                            if (group.Key == "ndc")
                            {
                                model.ndc = group.Value;

                            }

                            if (group.Key == "result")
                            {
                                foreach (var group2 in responseData["result"])
                                {
                                    if (group2.Key == "description")
                                    {
                                        model.description = group2.Value;
                                        if (group2.Value == "successfully created checkout")
                                        {
                                            model.status = 1;
                                            //  db.SaveChanges();
                                        }
                                        else
                                        {
                                            model.status = -1;
                                        }

                                    }
                                    if (group.Key == "code")
                                    {
                                        model.code = group2.Value;
                                    }


                                }
                            }

                        }


                        return this.ResponseOK(new ResponseViewModel(HttpStatusCode.OK, "", true, model));
                    }
                    return this.ResponseOK(new ResponseViewModel(HttpStatusCode.OK, Resource.ErrorOccure, false, null));


                }
                return this.ResponseOK(new ResponseViewModel(HttpStatusCode.OK, Resource.ErrorOccure, false, null));
            }
            catch
            {
                return this.ResponseOK(new ResponseViewModel(HttpStatusCode.OK, Resource.ErrorOccure, false, null));
            }


        }


        // GET api/<controller>
        [Authorize]
        [Route("PaymentStatusTemp")]
        [HttpGet]
        [ResponseType(typeof(PaymentModelResult))]
        public IHttpActionResult PaymentStatusTemp(string checkoutId, int requestId, string paymentBrand)
        {
            string Lang = lang;
            try
            {
                Regex successPattern = new Regex(@"(000\.000\.|000\.100\.1|000\.[36])");
                Regex successManuelPattern = new Regex(@"(000\.400\.0[^3]|000\.400\.100)");
                Regex pendingPattern = new Regex(@"(000\.200)");
                bool match1success = false;
                bool match2success = false;
                bool matchpendingsuccess = false; ;
                double out_amount = 0;
                var Coursre = _request.GetRequestDetailsBytIdTemp(requestId);
                if (Coursre != null)
                {
                    // var ZoomMeetings = GenerateZoom(requestId, DateTime.Now, DateTime.Now.AddHours(4));
                    //var t = ZoomUrl(99371489528);
                    var student = _studentBll.Student_GetById(Coursre.studentId);
                    // if (student.userId != User.Identity.GetUserId()) return this.ResponseUnauthorized(new ResponseViewModel(HttpStatusCode.Unauthorized, "", false, null));
                    var totalprice = CalculateVat(float.Parse(Coursre.totalPrice.ToString()));
                    var responseData = statusrequest(checkoutId, paymentBrand);
                    // var responseData = GetRequestStatus(checkoutId, paymentBrand);

                    PaymentModelResult model = new PaymentModelResult();
                    if (responseData != null)
                    {
                        foreach (var group in responseData)
                        {
                            if (group.Key == "ndc")
                            {
                                model.ndc = group.Value;
                                model.checkout_id = group.Value;
                            }
                            if (group.Key == "amount")
                            {
                                out_amount = Convert.ToDouble(group.Value);
                            }
                            if (group.Key == "result")
                            {
                                foreach (var group2 in responseData["result"])
                                {
                                    if (group2.Key == "description")
                                    {
                                        model.description = group2.Value;


                                    }
                                    if (group2.Key == "code")
                                    {
                                        model.code = group2.Value;

                                        Match match1 = successPattern.Match(group2.Value);
                                        Match match2 = successManuelPattern.Match(group2.Value);
                                        Match matchpending = pendingPattern.Match(group2.Value);
                                        if (match1.Success)
                                        {
                                            match1success = true;
                                        }
                                        if (match2.Success)
                                        {
                                            match2success = true;
                                        }
                                        if (matchpending.Success)
                                        {
                                            matchpendingsuccess = true;
                                        }

                                    }


                                }
                            }

                        }

                        if (match1success || match2success)
                        {
                            var payment = new PaymentVM()
                            {
                                CheckOut = checkoutId,
                                PaymentStatus = "2",
                                TotalPrice = totalprice
                            };

                            // new payment temp  to actual
                            var Payment_request_id = _paymet.RequestCourseAddpayment(Coursre.requestId);
                            var id = _paymet.AddCoursePayment(payment, Payment_request_id);
                            // check if payment success

                            var dates = _request.GetRequestDatesByRequestId(Payment_request_id);
                            if (dates != null)
                            {
                                foreach (var item in dates)
                                {
                                    // azza zoom change //
                                    if (item.idFk > 0)

                                    {
                                        var SchedualLessonDateZoom = _request.GetSchedualLessonDateZoom(item.idFk.Value);
                                        var lesson = new ScheduleLessonsVM()
                                        {
                                            requestId = Payment_request_id,
                                            requestDateId = item.id,
                                            startDate = SchedualLessonDateZoom.StartDate,
                                            endDate = SchedualLessonDateZoom.EndDate,
                                            conferanceZoom = SchedualLessonDateZoom.ConferanceZoom,
                                            studentZoom = SchedualLessonDateZoom.StudentZoom,
                                            MeetingId = SchedualLessonDateZoom.MeetingId


                                        };
                                        var lessonid = _lesson.AddScheduleLesson(lesson);
                                    }
                                    else
                                    {
                                        var ZoomMeeting = GenerateZoom(Payment_request_id, item.startDate.Value, item.endDate.Value);
                                        var lesson = new ScheduleLessonsVM()
                                        {
                                            requestId = Payment_request_id,
                                            requestDateId = item.id,
                                            startDate = ZoomMeeting.start_time,
                                            endDate = ZoomMeeting.start_time.AddMinutes(ZoomMeeting.duration),
                                            conferanceZoom = ZoomMeeting.start_url,
                                            studentZoom = ZoomMeeting.join_url,
                                            MeetingId = ZoomMeeting.id


                                        };
                                        var lessonid = _lesson.AddScheduleLesson(lesson);

                                    }


                                }

                                //var updateId = _request.RequestCourceUpdateStatus(requestId, 2, "Confirmed");

                                _paymet.InsertPaymentWalletTransaction(Payment_request_id, Coursre.teacherId, decimal.Parse(Coursre.totalPrice.ToString()));

                                var lst = _notificationBLL.GetNotificationToken(Coursre.studentId, 1);


                                var teacher = _TeacherBll.Teacher_GetById(Coursre.teacherId);
                                //var student = studentBLL.Student_GetById(Coursre.studentId);
                                dynamic returndata = new
                                {
                                    CourseId = Payment_request_id,
                                    teacherId = Coursre.teacherId,
                                    teacherName = teacher.fullName,
                                    studentId = Coursre.studentId,
                                    studentName = student.fullName,
                                    subject = Coursre.subject,
                                    type = 4
                                };
                                string NotificationMessage = JsonConvert.SerializeObject(returndata);
                                var title = "تمت عملية الدفع بنجاح";
                                _notificationBLL.InsertUserNotification(new NotificationVM() { course_id = Payment_request_id, details = NotificationMessage, title = title, typeId = 1, userId = Coursre.studentId, user_type = 1 });
                                foreach (var item in lst)
                                {
                                    _comm.SendNotification(returndata, item.Token, 1, title);

                                }
                                return this.ResponseOK(new ResponseViewModel(HttpStatusCode.OK, model.description, true, model));
                            }
                        }
                        if (matchpendingsuccess)
                        {
                            //var updateId = _request.RequestCourceUpdateStatus(Payment_request_id, 3, Resource.Pending);
                            // insert teacher amount -- entity amount
                            return this.ResponseOK(new ResponseViewModel(HttpStatusCode.OK, model.description, true, model));
                        }
                        else
                        {
                            return this.ResponseOK(new ResponseViewModel(HttpStatusCode.OK, model.description, false, model));

                        }
                        return this.ResponseOK(new ResponseViewModel(HttpStatusCode.OK, model.description, false, model));

                    }

                    return this.ResponseOK(new ResponseViewModel(HttpStatusCode.OK, Resource.ErrorOccure, false, null));
                }
                return this.ResponseOK(new ResponseViewModel(HttpStatusCode.OK, Resource.NoCourseFound, false, null));
            }
            catch (Exception ex)
            {
                return this.ResponseOK(new ResponseViewModel(HttpStatusCode.OK, ex.Message, false, null));
            }

        }




        [AllowAnonymous]
        [Route("PaymentStatusTempCheckPayment")]
        [HttpGet]
        [ResponseType(typeof(PaymentModelResult))]
        public IHttpActionResult PaymentStatusTempCheckPayment(string checkoutId, int requestId)
        {
            string Lang = lang;
            try
            {

                var Coursre = _request.GetRequestDetailsBytIdTemp(requestId);
                if (Coursre != null)
                {
                    // var ZoomMeetings = GenerateZoom(requestId, DateTime.Now, DateTime.Now.AddHours(4));
                    //var t = ZoomUrl(99371489528);
                    var student = _studentBll.Student_GetById(Coursre.studentId);
                    // if (student.userId != User.Identity.GetUserId()) return this.ResponseUnauthorized(new ResponseViewModel(HttpStatusCode.Unauthorized, "", false, null));
                    var totalprice = CalculateVat(float.Parse(Coursre.totalPrice.ToString()));

                    // var responseData = GetRequestStatus(checkoutId, paymentBrand);

                    PaymentModelResult model = new PaymentModelResult();

                    var payment = new PaymentVM()
                    {
                        CheckOut = checkoutId,
                        PaymentStatus = "2",
                        TotalPrice = totalprice
                    };

                    // new payment temp  to actual
                    var Payment_request_id = _paymet.RequestCourseAddpayment(Coursre.requestId);
                    var id = _paymet.AddCoursePayment(payment, Payment_request_id);
                    // check if payment success

                    var dates = _request.GetRequestDatesByRequestId(Payment_request_id);
                    if (dates != null)
                    {
                        foreach (var item in dates)
                        {
                            // azza zoom change //
                            if (item.idFk > 0)

                            {
                                var SchedualLessonDateZoom = _request.GetSchedualLessonDateZoom(item.idFk.Value);
                                var lesson = new ScheduleLessonsVM()
                                {
                                    requestId = Payment_request_id,
                                    requestDateId = item.id,
                                    startDate = SchedualLessonDateZoom.StartDate,
                                    endDate = SchedualLessonDateZoom.EndDate,
                                    conferanceZoom = SchedualLessonDateZoom.ConferanceZoom,
                                    studentZoom = SchedualLessonDateZoom.StudentZoom,
                                    MeetingId = SchedualLessonDateZoom.MeetingId


                                };
                                var lessonid = _lesson.AddScheduleLesson(lesson);
                            }
                            else
                            {
                                var ZoomMeeting = GenerateZoom(Payment_request_id, item.startDate.Value, item.endDate.Value);
                                var lesson = new ScheduleLessonsVM()
                                {
                                    requestId = Payment_request_id,
                                    requestDateId = item.id,
                                    startDate = ZoomMeeting.start_time,
                                    endDate = ZoomMeeting.start_time.AddMinutes(ZoomMeeting.duration),
                                    conferanceZoom = ZoomMeeting.start_url,
                                    studentZoom = ZoomMeeting.join_url,
                                    MeetingId = ZoomMeeting.id


                                };
                                var lessonid = _lesson.AddScheduleLesson(lesson);

                            }


                        }

                        //var updateId = _request.RequestCourceUpdateStatus(requestId, 2, "Confirmed");

                        _paymet.InsertPaymentWalletTransaction(Payment_request_id, Coursre.teacherId, decimal.Parse(Coursre.totalPrice.ToString()));

                        var lst = _notificationBLL.GetNotificationToken(Coursre.studentId, 1);


                        var teacher = _TeacherBll.Teacher_GetById(Coursre.teacherId);
                        //var student = studentBLL.Student_GetById(Coursre.studentId);
                        dynamic returndata = new
                        {
                            CourseId = Payment_request_id,
                            teacherId = Coursre.teacherId,
                            teacherName = teacher.fullName,
                            studentId = Coursre.studentId,
                            studentName = student.fullName,
                            subject = Coursre.subject,
                            type = 4
                        };
                        string NotificationMessage = JsonConvert.SerializeObject(returndata);
                        var title = "تمت عملية الدفع بنجاح";
                        _notificationBLL.InsertUserNotification(new NotificationVM() { course_id = Payment_request_id, details = NotificationMessage, title = title, typeId = 1, userId = Coursre.studentId, user_type = 1 });
                        foreach (var item in lst)
                        {
                            _comm.SendNotification(returndata, item.Token, 1, title);

                        }
                        return this.ResponseOK(new ResponseViewModel(HttpStatusCode.OK, model.description, true, model));
                    }


                    return this.ResponseOK(new ResponseViewModel(HttpStatusCode.OK, Resource.SavedSuccessfully, false, null));
                }
                return this.ResponseOK(new ResponseViewModel(HttpStatusCode.OK, Resource.NoCourseFound, false, null));
            }
            catch (Exception ex)
            {
                return this.ResponseOK(new ResponseViewModel(HttpStatusCode.OK, ex.Message, false, null));
            }

        }

        #endregion

        #region ---------- package Payment membership ----------
        [Authorize]
        [Route("AddPackageMembershipPayment")]
        [ResponseType(typeof(string))]
        [ResponseCodes(HttpStatusCode.OK)]
        [HttpGet]
        public IHttpActionResult AddPackageMembershipPayment(long TeacherPackId, string paymentBrand)
        {
            string Lang = lang;
            try
            {
                var package = _membpackage.GetTeacherMembershipById(TeacherPackId);
                if (package != null)
                {
                    var totalprice = CalculateVat(float.Parse(package.cost.ToString()));
                    var responseData = GetCheckoutMembershipPayment(totalprice, package.teacher_id, TeacherPackId, null, paymentBrand);

                    PaymentModelResult model = new PaymentModelResult();


                    if (responseData != null)
                    {
                        using (StreamWriter writetext = new StreamWriter(HttpContext.Current.Server.MapPath("~/write.txt"), true))
                        {
                            writetext.WriteLine("----------------------------------- Payment responseData " + DateTime.Now + " -----------------");
                            foreach (var group in responseData)
                            {
                                writetext.WriteLine("Key: {0} Value: {1}", group.Key, group.Value);
                            }
                            foreach (var group in responseData["result"])
                            {
                                writetext.WriteLine("Key: {0} Value: {1}", group.Key, group.Value);
                            }
                            writetext.WriteLine("responseData : " + responseData);
                            writetext.WriteLine("----------------------------------- Payment responseData" + DateTime.Now + " -----------------");
                        }

                        foreach (var group in responseData)
                        {
                            if (group.Key == "id")
                            {

                                model.checkout_id = group.Value;
                                var outObj = _membpackage.UpdateMerchantTeacherMembership(package.id, model.checkout_id);
                                if (outObj < 0)
                                {
                                    return this.ResponseOK(new ResponseViewModel(HttpStatusCode.OK, Resource.PaidBefore, false, null));
                                }

                            }
                            if (group.Key == "ndc")
                            {
                                model.ndc = group.Value;

                            }

                            if (group.Key == "result")
                            {
                                foreach (var group2 in responseData["result"])
                                {
                                    if (group2.Key == "description")
                                    {
                                        model.description = group2.Value;
                                        if (group2.Value == "successfully created checkout")
                                        {
                                            model.status = 1;
                                            //  db.SaveChanges();
                                        }
                                        else
                                        {
                                            model.status = -1;
                                        }

                                    }
                                    if (group.Key == "code")
                                    {
                                        model.code = group2.Value;
                                    }


                                }
                            }

                        }


                        return this.ResponseOK(new ResponseViewModel(HttpStatusCode.OK, "", true, model));
                    }
                    return this.ResponseOK(new ResponseViewModel(HttpStatusCode.OK, Resource.ErrorOccure, false, null));


                }
                return this.ResponseOK(new ResponseViewModel(HttpStatusCode.OK, Resource.ErrorOccure, false, null));
            }
            catch (Exception e)
            {
                return this.ResponseOK(new ResponseViewModel(HttpStatusCode.OK, Resource.ErrorOccure, false, null));
            }


        }



        [Authorize]
        [Route("PaymentMembershipPackageStatus")]
        [HttpGet]
        [ResponseType(typeof(PaymentModelResult))]
        public IHttpActionResult PaymentMembershipPackageStatus(long TeacherPackId, string paymentBrand)

        {
            string Lang = lang;
            try
            {
                Regex successPattern = new Regex(@"(000\.000\.|000\.100\.1|000\.[36])");
                Regex successManuelPattern = new Regex(@"(000\.400\.0[^3]|000\.400\.100)");
                Regex pendingPattern = new Regex(@"(000\.200)");
                bool match1success = false;
                bool match2success = false;
                bool matchpendingsuccess = false; ;
                double out_amount = 0;
                var package = _membpackage.GetTeacherMembershipById(TeacherPackId);
                if (package != null)
                {
                    // var ZoomMeetings = GenerateZoom(requestId, DateTime.Now, DateTime.Now.AddHours(4));
                    //var t = ZoomUrl(99371489528);
                    //var teacher = _studentBll.Student_GetById(Coursre.studentId);
                    // if (student.userId != User.Identity.GetUserId()) return this.ResponseUnauthorized(new ResponseViewModel(HttpStatusCode.Unauthorized, "", false, null));
                    var totalprice = CalculateVat(float.Parse(package.cost.ToString()));
                    var responseData = statusrequest(package.merchant_id, paymentBrand);
                    // var responseData = GetRequestStatus(checkoutId, paymentBrand);

                    PaymentModelResult model = new PaymentModelResult();
                    if (responseData != null)
                    {
                        foreach (var group in responseData)
                        {
                            if (group.Key == "ndc")
                            {
                                model.ndc = group.Value;
                                model.checkout_id = group.Value;
                            }
                            if (group.Key == "amount")
                            {
                                out_amount = Convert.ToDouble(group.Value);
                            }
                            if (group.Key == "result")
                            {
                                foreach (var group2 in responseData["result"])
                                {
                                    if (group2.Key == "description")
                                    {
                                        model.description = group2.Value;


                                    }
                                    if (group2.Key == "code")
                                    {
                                        model.code = group2.Value;

                                        Match match1 = successPattern.Match(group2.Value);
                                        Match match2 = successManuelPattern.Match(group2.Value);
                                        Match matchpending = pendingPattern.Match(group2.Value);
                                        if (match1.Success)
                                        {
                                            match1success = true;
                                        }
                                        if (match2.Success)
                                        {
                                            match2success = true;
                                        }
                                        if (matchpending.Success)
                                        {
                                            matchpendingsuccess = true;
                                        }

                                    }


                                }
                            }

                        }

                        if (match1success || match2success)
                        {
                            var payment = new PaymentVM()
                            {
                                CheckOut = package.merchant_id,
                                PaymentStatus = "1",
                                TotalPrice = totalprice
                            };

                            _membpackage.PayTeacherMembership(TeacherPackId);
                            // check if payment success


                        }
                        if (matchpendingsuccess)
                        {
                            var payment = new PaymentVM()
                            {
                                CheckOut = package.merchant_id,
                                PaymentStatus = "3",
                                TotalPrice = totalprice
                            };
                            //_membpackage.PayTeacherMembership(TeacherPackId);
                            var id = _paymet.AddPackagePayment(payment, package.id);
                            //var updateId = _request.RequestCourceUpdateStatus(requestId, 3, "Pending");
                            // insert teacher amount -- entity amount
                            return this.ResponseOK(new ResponseViewModel(HttpStatusCode.OK, model.description, true, model));
                        }
                        else
                        {
                            return this.ResponseOK(new ResponseViewModel(HttpStatusCode.OK, model.description, false, model));

                        }
                        return this.ResponseOK(new ResponseViewModel(HttpStatusCode.OK, model.description, false, model));

                    }

                    return this.ResponseOK(new ResponseViewModel(HttpStatusCode.OK, Resource.ErrorOccure, false, null));
                }
                return this.ResponseOK(new ResponseViewModel(HttpStatusCode.OK, Resource.NoCourseFound, false, null));
            }
            catch (Exception ex)
            {
                return this.ResponseOK(new ResponseViewModel(HttpStatusCode.OK, ex.Message, false, null));
            }

        }


        public Dictionary<string, dynamic> GetCheckoutMembershipPayment(float? amount, int TeacherId, long TeacherPackId, int? gateway = 0, string paymentBrand = "")
        {

            var teacher = _TeacherBll.GetTeacherById(TeacherId);
            string email, firstname, surname, street, city, state;

            email = teacher.Email;
            firstname = teacher.FirstName;
            surname = teacher.LastName;
            street =// !string.IsNullOrEmpty(teacher.streetNo) ? teacher.streetNo :
                "unknown";
            city = string.IsNullOrEmpty(teacher.CityName) ? teacher.CityName : "unknown";
            state = string.IsNullOrEmpty(teacher.District) ? teacher.District : "unknown";

            //ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
            Dictionary<string, dynamic> responseData;
            string EntityId = "";
            var notifTrl = ConfigurationManager.AppSettings["ProjectURL"].ToString() + "/Home/PaymentNotify/" + TeacherPackId;

            StringBuilder data = new StringBuilder();

            if (paymentBrand == "VISA" || paymentBrand == "MASTER")
            {
                data.Append(ConfigurationManager.AppSettings["HyperPayEntityIdVISA"].ToString());

            }
            else
            {
                data.Append(ConfigurationManager.AppSettings["HyperPayEntityIdMADA"].ToString());

            }
            data.Append("&notificationUrl=" + notifTrl);
            data.Append("&amount=" + String.Format("{0:0.00}", amount));
            data.Append("&currency=SAR");
            data.Append("&paymentType=DB");
            data.Append("&merchantTransactionId=" + Guid.NewGuid());
            data.Append("&customer.surname=" + surname);
            data.Append("&billing.street1=" + street);
            data.Append("&billing.city=" + city);
            data.Append("&billing.state=" + state);
            data.Append("&billing.country=SA");
            data.Append("&billing.postcode=" + "21577");
            data.Append("&customer.email=" + email);
            data.Append("&customer.givenName=" + firstname);


            string url = ConfigurationManager.AppSettings["HyperPayUrl"].ToString();
            byte[] buffer = Encoding.ASCII.GetBytes(data.ToString());
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(url);
            request.Method = "POST";
            //////////////////////////////////////Alllli////////////////////////////////////
            request.Headers["Authorization"] = ConfigurationManager.AppSettings["HyperPayAuthorization"].ToString();

            // request.Headers["Authorization"] = "Bearer OGFjN2E0Yzc3MDY4MjJjODAxNzA3NzcxYjVmMDBhNWR8aFo5Y2NIZkRYRQ==";
            ////////////////////////////////////////Allli//////////////////////////////////////////////
            request.ContentType = "application/x-www-form-urlencoded";
            Stream PostData = request.GetRequestStream();
            PostData.Write(buffer, 0, buffer.Length);
            PostData.Close();
            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            {
                Stream dataStream = response.GetResponseStream();
                StreamReader reader = new StreamReader(dataStream);
                var s = new System.Web.Script.Serialization.JavaScriptSerializer();
                responseData = s.Deserialize<Dictionary<string, dynamic>>(reader.ReadToEnd());
                reader.Close();
                dataStream.Close();
            }
            return responseData;
        }

        #endregion
    }

}