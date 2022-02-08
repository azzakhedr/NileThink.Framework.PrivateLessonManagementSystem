using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Newtonsoft.Json;
using NileThink.Framework.PrivateLessonManagementSystem.DAL.Models;
using NileThink.Framework.PrivateLessonManagementSystem.Web.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace NileThink.Framework.PrivateLessonManagementSystem.Web.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }
        [HttpGet]
        public async Task<ActionResult> CheckIAM(string id)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                    return View();


                var item = StateCertificate.getCer(id);
                foreach (var x in item)
                {
                    ViewBag.txt = item[0];
                }
                HttpClient client = new HttpClient();
                var values = new Dictionary<string, string>
                {
                };
                var content = new FormUrlEncodedContent(values);

                //var xxx = StateCertificate.Encrypt2();                              
                var response = await client.PostAsync(item[0], content);
                var responseString = await response.Content.ReadAsStringAsync();
                Response.Write(responseString);
                //Response.Write("Cer 2 :" + xxx);
            }
            catch (Exception e)
            {
                Response.Write("EXCEPTION: " + e.Message + "\n");


            }
            //String timeStamp = GetTimestamp(DateTime.Now);
            //Response.Redirect("https://iambeta.elm.sa/authservice/authorize?scope=openid&response_type=id_token&response_mode=form_post&client_id=16352727&redirect_uri=http://privatelessonforyou.com/Home/callback&nonce" + Guid.NewGuid() + "&ui_locales=ar&prompt=login&max_age=" + timeStamp);
            String timeStamp = GetTimestamp(DateTime.Now);
            ViewBag.Time = timeStamp;
            return View();
        }
        String GetTimestamp(DateTime value)
        {
            return value.ToString("yyyyMMddHHmmss");
        }
        public async Task<ActionResult> CheckIAMTest(string id)
        {
            return RedirectToAction("CallBack", new { id = id });
        }
        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
        [HttpPost]
        public ActionResult CallBack(FormCollection form, string id)
        {
            try
            {
                //Response.Write("<p style='padding:15px;'> id : " + id + "</p>");
                //var re = Request;
                //var headers = Request.Headers;
                //foreach (var key in Request.Headers.AllKeys)
                //{
                //    Response.Write("<p style='padding:15px;'>"+key + ": " + Request.Headers[key] + "</p>");
                //}
                //string[] keys = Request.Form.AllKeys;
                //for (int i = 0; i < keys.Length; i++)
                //{
                //    Response.Write(keys[i] + ": " + Request.Form[keys[i]] + "<br>");
                //}
                foreach (var key in form.AllKeys)
                {
                    using (System.IO.StreamWriter file = new System.IO.StreamWriter(System.Web.HttpContext.Current.Server.MapPath("~/debug.txt"), true))
                        file.WriteLine(key + ": " + Request.Form[key]);

                    if (key == "id_token")
                    {
                        var decoded = ConvertFromBase64String(Request.Form[key]);
                        if (decoded != null)
                        {
                            var data = Encoding.UTF8.GetString(decoded);
                            using (System.IO.StreamWriter file = new System.IO.StreamWriter(System.Web.HttpContext.Current.Server.MapPath("~/debug.txt"), true))
                                file.WriteLine("Data : " + data);
                        }
                    }

                }

                //using (var db = new MhanaDbEntities())
                //{
                //    var current_user = db.AspNetUsers.Where(w => w.Id == id).FirstOrDefault();
                //    if (current_user != null)
                //    {
                //        current_user.absher = 1;
                //        current_user.absher_no = "";
                //        db.Entry(current_user).State = EntityState.Modified;
                //        db.SaveChanges();
                //    }
                //}


                foreach (var key in form.AllKeys)
                {
                    //Response.Write("<p style='padding:15px;'>" + key + ": " + Request.Form[key] + "</p>");

                    if (key == "id_token")
                    {


                        var decoded = ConvertFromBase64String(Request.Form[key]);
                        var data = Encoding.UTF8.GetString(decoded);
                        // Response.Write("<p style='padding:15px;'> My Data : " + data + "</p>");
                        if (data.Contains("englishName"))
                        {
                            // var data = Encoding.ASCII.GetString(decoded);

                            //   var items = data.Split('}');

                            //  if (items.Count() > 1)
                            //  {
                            //data = "[" + items[0] + "}," + items[1] + "}" + "]";
                            //var myData = items[1] + "}";
                            var myDataObject = JsonConvert.DeserializeObject<AbsherData>(data);
                            dynamic parsedJson = JsonConvert.DeserializeObject(data);
                            // Response.Write(myDataObject.arabicName);
                            string user_id = id;

                            using (var db = new MhanaDevEntities())
                            {
                                var current_user = db.AspNetUsers.Where(w => w.Id == id).FirstOrDefault();
                                if (current_user != null)
                                {
                                    current_user.absher = 1;
                                    current_user.absher_no = myDataObject.sub;
                                    current_user.fullname = myDataObject.arabicName;
                                    current_user.gender = myDataObject.gender == "male" ? "ذكر" : "أنثى";
                                    current_user.first_name = myDataObject.arabicFirstName;
                                    current_user.last_name = myDataObject.arabicFamilyName;
                                    //current_user.dob = DateTime.Parse(myDataObject.dob);
                                    db.Entry(current_user).State = EntityState.Modified;
                                    db.SaveChanges();
                                }

                                if (myDataObject != null)
                                {
                                    myDataObject.cdate = DateTime.Now;
                                    myDataObject.mhana_user_id = user_id;
                                    db.AbsherDatas.Add(myDataObject);
                                    db.SaveChanges();
                                }
                            }
                            ViewBag.success = 1;

                            // }
                        }

                    }
                    if (key == "state")
                    {

                        //Response.Write("<p style='padding:15px;'> My State : " + Request.Form[key] + "</p>");
                        // var decoded = ConvertFromBase64String(Request.Form[key]);
                        //   var data = Encoding.UTF8.GetString(decoded);
                        //   Response.Write(data);
                    }

                }
                ViewBag.succwss = 1;
                Response.Redirect("mhanna://success");
                return View();
            }
            catch (Exception ex)
            {
                using (System.IO.StreamWriter file = new System.IO.StreamWriter(System.Web.HttpContext.Current.Server.MapPath("~/debug.txt"), true))
                    file.WriteLine(ex.ToString());

                return View();
            }
        }

        [HttpGet]
        public ActionResult CallBack(string id)
        {
            //string yy = "FlPRzW6vGPk+Mk371TXTTGWzf+S7CNrkKAXIZxiE3KQ=";

            //var ydecoded = ConvertFromBase64String(yy);
            //var ydata = Encoding.UTF8.GetString(ydecoded);
            //Response.Write(ydata);
            //using (System.IO.StreamWriter file = new System.IO.StreamWriter(System.Web.HttpContext.Current.Server.MapPath("~/debug.txt"), true))
            //    file.WriteLine("Session Get: " + Convert.ToString(Session["user_id"]));
            // string xx = "eyJhbGciOiJSUzI1NiJ9.eyJlbmdsaXNoTmFtZSI6Ik1pc2hhcmkgTW9oYW1tZWQgTXVhdGggS2hhbGlkIiwiYXJhYmljRmF0aGVyTmFtZSI6ItmF2K3ZhdivIiwiZW5nbGlzaEZhdGhlck5hbWUiOiJNb2hhbW1lZCIsInN1YiI6IjEwMjAxOTEzNTciLCJnZW5kZXIiOiJNYWxlIiwiaXNzIjoiaHR0cHM6XC9cL3d3dy5pYW0uZ292LnNhXC91c2VyYXV0aCIsImNhcmRJc3N1ZURhdGVHcmVnb3JpYW4iOiJUaHUgTm92IDE2IDAwOjAwOjAwIEFTVCAyMDE3IiwiZW5nbGlzaEdyYW5kRmF0aGVyTmFtZSI6Ik11YXRoIiwidXNlcmlkIjoiMTAyMDE5MTM1NyIsImlkVmVyc2lvbk5vIjoiNCIsImFyYWJpY05hdGlvbmFsaXR5Ijoi2KfZhNi52LHYqNmK2Kkg2KfZhNiz2LnZiNiv2YrYqSIsImFyYWJpY05hbWUiOiLZhdi02KfYsdmKINmF2K3ZhdivINmF2LnYp9iwINiu2KfZhNivIiwiYXJhYmljRmlyc3ROYW1lIjoi2YXYtNin2LHZiiIsIm5hdGlvbmFsaXR5Q29kZSI6IjExMyIsImlxYW1hRXhwaXJ5RGF0ZUhpanJpIjoiMTQ0OFwvMDlcLzExIiwibGFuZyI6ImFyIiwiZXhwIjoxNTk3NjI3NzY2LCJpYXQiOjE1OTc2Mjc2MTYsImlxYW1hRXhwaXJ5RGF0ZUdyZWdvcmlhbiI6IlRodSBGZWIgMTggMDA6MDA6MDAgQVNUIDIwMjciLCJpZEV4cGlyeURhdGVHcmVnb3JpYW4iOiJUaHUgRmViIDE4IDAwOjAwOjAwIEFTVCAyMDI3IiwianRpIjoiaHR0cHM6XC9cL2lhbWJldGEuZWxtLnNhLDUyZTUxZjUwLWRkNDMtNDNhNS1hZmFlLWJkMWQ0ODNlOTZiNiIsImlzc3VlTG9jYXRpb25BciI6IiIsImRvYkhpanJpIjoiMTQwMlwvMDFcLzAxIiwiY2FyZElzc3VlRGF0ZUhpanJpIjoiMTQzOVwvMDJcLzI3IiwiZW5nbGlzaEZpcnN0TmFtZSI6Ik1pc2hhcmkiLCJpc3N1ZUxvY2F0aW9uRW4iOiIiLCJhcmFiaWNHcmFuZEZhdGhlck5hbWUiOiLZhdi52KfYsCIsImF1ZCI6Imh0dHA6XC9cL3ByaXZhdGVsZXNzb25mb3J5b3UuY29tXC8iLCJuYmYiOjE1OTc2Mjc0NjYsIm5hdGlvbmFsaXR5IjoiU2F1ZGkgQXJhYmlhIiwiZG9iIjoiV2VkIE9jdCAyOCAwMDowMDowMCBBU1QgMTk4MSIsImVuZ2xpc2hGYW1pbHlOYW1lIjoiS2hhbGlkIiwiaWRFeHBpcnlEYXRlSGlqcmkiOiIxNDQ4XC8wOVwvMTEiLCJhc3N1cmFuY2VfbGV2ZWwiOiIiLCJhcmFiaWNGYW1pbHlOYW1lIjoi2K7Yp9mE2K8ifQ.IAPT--2SeMCLTgmAg_hDAdR5_jGGMzgm1ug0-Xoiggnx3ZXJmMsZZJuIjwRt29Dn2kOKmXEtvOg9pDJjUiZCjTOdhMy7P-g4RzcLSZ8pUHcmgqMEH2NfSagOWCb3zkQRjokmCYIHRtcbdXmqgjI2LrOUnyUEpqvkooopV-Xr78egiFCOjE-Myv0QZAcp6wCzY3nUe41zLyBN8U_95knfPaCHrwwwpb20BpbgYIUW26R0Qd-jE-PqK4qz8tCN-4EZwCigWDArQQeaVHH_KVePrSgIXKW6qCssrpCcfNsjou64T9Z0PVV5gwwdkg5nGj-iqQZP59KStVJ73Jb8CXiriA";
            string xx = "eyJhbGciOiJSUzI1NiJ9.eyJzdWIiOiIxMDI3NzY1NzE2IiwiZW5nbGlzaE5hbWUiOiJCQURFUiAgICAgICAgICAgSEFNQUQgICAgICAgICAgIEEgICAgICAgICAgICAgICBBTE1VUUJJTCAgICAgICAiLCJhcmFiaWNGYXRoZXJOYW1lIjoi2KjZhiDYrdmF2K8gICAgICAgICAiLCJlbmdsaXNoRmF0aGVyTmFtZSI6IkhBTUFEICAgICAgICAgICIsImdlbmRlciI6Ik1hbGUiLCJpc3MiOiJodHRwczpcL1wvd3d3LmlhbS5nb3Yuc2FcL3VzZXJhdXRoIiwiY2FyZElzc3VlRGF0ZUdyZWdvcmlhbiI6IlNhdCBTZXAgMjYgMDM6MDA6MDAgQVNUIDIwMjAiLCJlbmdsaXNoR3JhbmRGYXRoZXJOYW1lIjoiQSAgICAgICAgICAgICAgIiwidXNlcmlkIjoiMTAyNzc2NTcxNiIsImlkVmVyc2lvbk5vIjoiNCIsImFyYWJpY05hdGlvbmFsaXR5Ijoi2KfZhNi52LHYqNmK2Kkg2KfZhNiz2LnZiNiv2YrYqSAgICAiLCJhcmFiaWNOYW1lIjoi2KjYr9ixICAgICAgICAgICAgINio2YYg2K3ZhdivICAgICAgICAgINio2YYg2LnYqNiv2KfZhNix2K3ZhdmGICAgINin2YTZhdmC2KjZhCAgICAgICAgICIsImFyYWJpY0ZpcnN0TmFtZSI6Itio2K_YsSAgICAgICAgICAgICIsIm5hdGlvbmFsaXR5Q29kZSI6IjExMyIsImlxYW1hRXhwaXJ5RGF0ZUhpanJpIjoiMTQ1MlwvMDJcLzA1IiwiZXhwIjoxNjI3Mjk0MTY0LCJsYW5nIjoiYXIiLCJpYXQiOjE2MjcyOTQwMTQsImp0aSI6Imh0dHBzOlwvXC9pYW0uZWxtLnNhLGNhZjFiMmQ0LTJiMDYtNGEyZS1iZjA4LTBmYWI5NTQ1OGE4ZSIsImlxYW1hRXhwaXJ5RGF0ZUdyZWdvcmlhbiI6IlRodSBKdW4gMDYgMDM6MDA6MDAgQVNUIDIwMzAiLCJpZEV4cGlyeURhdGVHcmVnb3JpYW4iOiJUaHUgSnVuIDA2IDAzOjAwOjAwIEFTVCAyMDMwIiwiaXNzdWVMb2NhdGlvbkFyIjoi2KPYrdmI2KfZhCDYp9mE2LHZitin2LYgMTIt2LHYrNin2YQgICAgICAgICAgIiwiZG9iSGlqcmkiOiIxNDAwXC8xMFwvMDkiLCJjYXJkSXNzdWVEYXRlSGlqcmkiOiIxNDQyXC8wMlwvMDkiLCJlbmdsaXNoRmlyc3ROYW1lIjoiQkFERVIgICAgICAgICAgIiwiaXNzdWVMb2NhdGlvbkVuIjoiIiwiYXJhYmljR3JhbmRGYXRoZXJOYW1lIjoi2KjZhiDYudio2K_Yp9mE2LHYrdmF2YYgICAiLCJhdWQiOiJodHRwczpcL1wvcHJpdmF0ZWxlc3NvbmZvcnlvdS5jb21cLyIsIm5iZiI6MTYyNzI5Mzg2NCwibmF0aW9uYWxpdHkiOiJTYXVkaSBBcmFiaWEgICAgICAgICIsImRvYiI6IlR1ZSBBdWcgMTkgMDM6MDA6MDAgQVNUIDE5ODAiLCJlbmdsaXNoRmFtaWx5TmFtZSI6IkFMTVVRQklMICAgICAgICIsImlkRXhwaXJ5RGF0ZUhpanJpIjoiMTQ1MlwvMDJcLzA1IiwicHJlZmVycmVkTGFuZyI6IkVOIiwiYXNzdXJhbmNlX2xldmVsIjoiIiwiYXJhYmljRmFtaWx5TmFtZSI6Itin2YTZhdmC2KjZhCAgICAgICAgICJ9.f7chWZxjDmRHIEGxwvzTwhUrI8AfqX7M97A20o6a12VapSGbb_wxAWw3-APEPg_YWpJ-J8wuUlW9VLcJxWReGBJRT3vuGv5T2x9vcazTs5enBaql-vnAKkP4e9hxM7hLPavvj2g1JvsfHCCUcVhYU3k0q8ERekXjH3iAMI7ZUuI1hx6zQzAa69aVs-b2PtmkmHNVbw86jqOtrP7TUbDqXbyfyqcRteqcZ7_orCiv6ffHWktxouGklhaQz-G0I-9MlzwabOKzuoQw2wykiO19sj2qvNwgXlitsVGQb0A1zgOXb5JdU34hfixb97iQYvI9i_N7mZvqNEW4oEezV5gWLA";

            var decoded = ConvertFromBase64String(xx);
            var data = Encoding.UTF8.GetString(decoded);

            if (data.Contains("RS256"))
            {
                // var data = Encoding.ASCII.GetString(decoded);
                var items = data.Split('}');
                data = "[" + items[0] + "}," + items[1] + "}" + "]";
                var myData = items[1] + "}";
                var myDataObject = JsonConvert.DeserializeObject<AbsherData>(myData);
                dynamic parsedJson = JsonConvert.DeserializeObject(data);
                // Response.Write(parsedJson);
                // string user_id = Convert.ToString(Session["user_id"]);
                using (var db = new MhanaDevEntities())
                {
                    //User
                    var current_user = db.AspNetUsers.Where(w => w.Id == id).FirstOrDefault();
                    if (current_user != null)
                    {
                        current_user.absher = 1;
                        current_user.absher_no = myDataObject.sub;
                        db.Entry(current_user).State = EntityState.Modified;
                    }
                    myDataObject.mhana_user_id = id;
                    db.AbsherDatas.Add(myDataObject);
                    db.SaveChanges();
                }
                //   Response.Write(JsonConvert.SerializeObject(parsedJson, Newtonsoft.Json.Formatting.Indented));
            }
            //ViewBag.success = 1;
            return View();
        }

        private static byte[] ConvertFromBase64String(string input)
        {

            var items = input.Split('.');
            if (items.Count() > 0)
            {
                input = items[1];
            }

            //var l = input.Length;
            //input = l % 4 == 1 && input[l - 1] == '='? input.Substring(0, l - 1) : input;




            //string working = input.Replace("-", "+").Replace("_", "/").Replace(".","");
            //while (working.Length % 4 != 0)
            //{
            //    working += '=';
            //}
            //while (working.Length % 3 != 0)
            //{
            //    working += '=';
            //}
            try
            {
                return Convert.FromBase64String(input);
            }
            catch (Exception)
            {
                // .Net core 2.1 bug
                // https://github.com/dotnet/corefx/issues/30793
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

                try
                {
                    return Convert.FromBase64String(input.Replace("-", "+").Replace("_", "/").Replace(".", "") + "=");
                }
                catch (Exception) { }

                try
                {
                    return Convert.FromBase64String(input.Replace("-", "+").Replace("_", "/").Replace(".", "") + "==");
                }
                catch (Exception) { }


                try
                {
                    return Convert.FromBase64String(input.Replace("-", "+").Replace("_", "/").Replace(".", ""));
                }
                catch (Exception) { }



                return null;
            }
        }
        public ActionResult PaymentNotify(string id = null)
        {
            try
            {
                //var id = Request.QueryString["id"];
                // ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
                using (StreamWriter writetext = new StreamWriter(HttpContext.Server.MapPath("~/write.txt"), true))
                {
                    writetext.WriteLine("----------------------------------- Response" + DateTime.Now + " -----------------");
                    writetext.WriteLine(" Response : " + Request.Url.PathAndQuery);
                    writetext.WriteLine("----------------------------------- Response" + DateTime.Now + "  -----------------");
                }
                var source = Request.QueryString["id"];
                //using (StreamWriter writetext = new StreamWriter(HttpContext.Server.MapPath("~/write.txt"), true))
                //{
                //    writetext.WriteLine("----------------------------------- checkcontent -----------------");
                //    writetext.WriteLine("content : " + id);
                //    writetext.WriteLine("----------------------------------- checkcontent -----------------");
                //}
                using (StreamWriter writetext = new StreamWriter(HttpContext.Server.MapPath("~/write.txt"), true))
                {
                    writetext.WriteLine("----------------------------------- source " + DateTime.Now + "  -----------------");
                    writetext.WriteLine("source : " + source);
                    writetext.WriteLine("----------------------------------- source " + DateTime.Now + "  -----------------");
                }


                return View();

            }
            catch (Exception ex)
            {
                using (StreamWriter writetext = new StreamWriter(HttpContext.Server.MapPath("~/write.txt"), true))
                {
                    writetext.WriteLine("----------------------------------- EX value" + DateTime.Now + "  -----------------");
                    writetext.WriteLine("EX : " + ex.Message);
                    writetext.WriteLine("----------------------------------- EX value" + DateTime.Now + " -----------------");
                }

                return View();
            }
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
                string AppID = "AAAAGPsw9V4:APA91bF1ul41JTPpLr0Ft0Zr7o2LJCWkdmSesTX4e1wIxl9s-XZcKwxZxsCgMDLAzBS87b9Io7ooxNypudDCbrXxOOxdds7bwhXgQiQIW6q5QbUMZNQADVhykek4et6pmtd7Qqev6nx6";
                //
                string senderId = "107293504862";
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
                //| SecurityProtocolType.Ssl3;
                //deviceToken = "cKf915AOTViyFciNjCNnNp:APA91bFZRerfFynFUeoxW7nxNos_YG_sXyr2a5oWVpUuMVUFX9XTyAOMULfgxf-1F8c9PKpVRYQdUrqP0eVtEIN7DktPQ8QG38BMM352hxAOg60rA7ImfVAJpew3TlNv-43AoXEmPcqa";
                webRequest.Method = "POST";
                webRequest.Headers["Authorization"] = "Key=" + AppID;
                webRequest.Headers.Add(string.Format("Sender: id={0}", senderId));

                string Data = "{\r\n\"to\":\"" + deviceToken + "\",\r\n \"data\" : {\r\n  \"sound\" : \"default\"";
                //string Data = "{\r\n\"to\":\"" + deviceToken + "\",\r\n \"data\" : {\r\n  \"sound\" : \"default\",\r\n  \"body\" : " + NotificationMessage + ",\r\n  \"title\" : \"" + title + "\",\r\n  \"content_available\" : true,\r\n  \"type\" : \"" + type.ToString() + "\"\r\n  \"priority\" : \"high\"\r\n }\r\n}";
                //string Data = "{\r\n\"to\":\"" + deviceToken + "\",\r\n \"data\" : {\r\n  \"sound\" : \"default\",\r\n  \"body\" :  \"test body\",\r\n  \"title\" : \""+ title + "\",\r\n  \"content_available\" : true,\r\n  \"priority\" : \"high\"\r\n }\r\n}";
                if (obj != null)
                {
                    foreach (var item in obj.GetType().GetProperties())
                    {
                        Data = Data + ",\r\n  \"" + item.Name + "\" : \"" + item.GetValue(obj) + "\"";
                    }
                }
                Data = Data + ",\r\n  \"title\" : \"" + title + "\",\r\n  \"content_available\" : true,\r\n  \"type\" : \"" + type.ToString() + "\"\r\n  \"priority\" : \"high\"\r\n }\r\n}";

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
                //webRequest.GetResponse().Close();
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


                //var client = new RestClient("https://fcm.googleapis.com/fcm/send");
                //client.Timeout = -1;
                //var request = new RestRequest(Method.POST);
                //request.AddHeader("Authorization", "Key=AAAAGPsw9V4:APA91bF1ul41JTPpLr0Ft0Zr7o2LJCWkdmSesTX4e1wIxl9s-XZcKwxZxsCgMDLAzBS87b9Io7ooxNypudDCbrXxOOxdds7bwhXgQiQIW6q5QbUMZNQADVhykek4et6pmtd7Qqev6nx6");
                //request.AddHeader("Content-Type", "application/json");
                //request.AddParameter("application/json", "{\r\n\"to\":\"cKf915AOTViyFciNjCNnNp:APA91bFZRerfFynFUeoxW7nxNos_YG_sXyr2a5oWVpUuMVUFX9XTyAOMULfgxf-1F8c9PKpVRYQdUrqP0eVtEIN7DktPQ8QG38BMM352hxAOg60rA7ImfVAJpew3TlNv-43AoXEmPcqa\",\r\n \"notification\" : {\r\n  \"sound\" : \"default\",\r\n  \"body\" :  \"test body\",\r\n  \"title\" : \"test title\",\r\n  \"content_available\" : true,\r\n  \"priority\" : \"high\"\r\n },\r\n \"data\" : {\r\n  \"sound\" : \"default\",\r\n  \"body\" :  \"test body\",\r\n  \"title\" : \"test title\",\r\n  \"content_available\" : true,\r\n  \"priority\" : \"high\"\r\n }\r\n}", ParameterType.RequestBody);
                //IRestResponse response = client.Execute(request);
                //Console.WriteLine(response.Content);
            }
            catch (Exception ex)
            {
                return ex.Message.ToString();
            }
        }
    }

}




