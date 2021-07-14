using Newtonsoft.Json;
using NileThink.Framework.PrivateLessonManagementSystem.BLL.ViewModels;
using PrivateLessonMS.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Hosting;
using System.Xml.Serialization;

namespace PrivateLessonMS.Helper
{
    public static class Functions
    {
        public static loginUser GetUser()
        {
            loginUser u = new loginUser();
            if (HttpContext.Current.Session["User"] != null)
            {
                var user = (ApplicationUser)HttpContext.Current.Session["User"];
                u.email = user.Email;
                u.photo = string.IsNullOrEmpty(user.photo) ? "default.png" : user.photo;
                u.id = user.Id;
                u.role = user.Roles.FirstOrDefault() != null ? user.Roles.FirstOrDefault().RoleId : "";
                u.fullname = string.IsNullOrEmpty(user.fullname) ? user.first_name + " " + user.last_name : user.fullname;
            }
            return u;
        }




        public static string FixBase64ForImage(string Image)
        {
            System.Text.StringBuilder sbText = new System.Text.StringBuilder(Image, Image.Length);
            sbText.Replace("\r\n", String.Empty); sbText.Replace(" ", String.Empty);
            return sbText.ToString();
        }
        public static string[] Extensions = ConfigurationManager.AppSettings["ImageFormats"].Split(',');
        public static string getVideoId(string link)
        {
            return Regex.Match(link, @"youtu(?:\.be|be\.com)/(?:.*v(?:/|=)|(?:.*/)?)([a-zA-Z0-9-_]+)").Groups[1].Value;
        }
        public static string toEnglishNumber(string input)
        {
            string EnglishNumbers = "";
            if (!string.IsNullOrEmpty(input))
            {
                for (int i = 0; i < input.Length; i++)
                {
                    if (Char.IsDigit(input[i]))
                    {
                        EnglishNumbers += char.GetNumericValue(input, i);
                    }
                    else
                    {
                        EnglishNumbers += input[i].ToString();
                    }
                }
            }
            return EnglishNumbers;
        }
        public static string SerializeUrl(string title)
        {
            title = Regex.Replace(title, @"[^0-9a-zA-Z\._]", title);
            return title.Replace(" ", "_");
        }
        public static Tuple<bool, string> ValidateImage(HttpPostedFileBase file, bool restrict = true)
        {
            if (file == null) return Tuple.Create(false, "لا يوجد صورة");
            if (!IsImage(file)) return Tuple.Create(false, "يرجى ان يتم اختيار صورة صحيحة [" + string.Join(",", Extensions) + "]");
            if (file.ContentLength <= 0) return Tuple.Create(false, "لا يمكن قبول ملف حجمه صفر");
            if (restrict)
            {
                string maxImageSize = ConfigurationManager.AppSettings["MaxImageSize"];
                int size = int.Parse(maxImageSize.Replace("mb", "")) * 1048576;

                if (file.ContentLength > size)
                    return Tuple.Create(false, "حجم الملف يجب ان يكون أقل من أو يساوي   " + maxImageSize);

                Image img = Image.FromStream(file.InputStream);
                string minWidth = ConfigurationManager.AppSettings["MinImageWidth"];
                string minHeight = ConfigurationManager.AppSettings["MinImageHeight"];
                int w = int.Parse(minWidth.Replace("px", ""));
                int h = int.Parse(minHeight.Replace("px", ""));
                //if (img.Width < w || img.Height < h)
                //    return Tuple.Create(false, "Image dimensions should be at least " + minWidth + " by " + minHeight);
                img.Dispose();
                file.InputStream.Position = 0;
            }
            return Tuple.Create(true, "");
        }
        public static bool IsImage(HttpPostedFileBase file)
        {
            if (file.ContentType.Contains("image")) return true;
            return Extensions.Any(item => file.FileName.ToLower().EndsWith(item, StringComparison.OrdinalIgnoreCase));
        }
        public static string SaveTempFile(HttpPostedFileBase file, string path = "~/Temp")
        {
            string tempFolder = HttpContext.Current.Server.MapPath(path);
            string fileName = Guid.NewGuid() + ".jpeg";
            string savePath = Path.Combine(tempFolder, fileName);

            if (!Directory.Exists(tempFolder)) Directory.CreateDirectory(tempFolder);

            MemoryStream target = new MemoryStream();
            file.InputStream.CopyTo(target);
            byte[] fileData = target.ToArray();

            Image img = Image.FromStream(file.InputStream);
            int width = 500;
            int height = (int)(img.Height * width / (decimal)img.Width);

            GetCroppedImage(fileData, new Size(width, height), 0, 0, ImageFormat.Jpeg, savePath);
            img.Dispose();
            return fileName;
        }
        public static byte[] GetCroppedImage(byte[] originalBytes, Size size, int x, int y, ImageFormat format, string saveTo = null)
        {
            using (var streamOriginal = new MemoryStream(originalBytes))
            using (var imgOriginal = Image.FromStream(streamOriginal))
            {
                //get original width and height of the incoming image
                var originalWidth = imgOriginal.Width; // 2560
                var originalHeight = imgOriginal.Height; // 1440

                //get the percentage difference in size of the dimension that will change the least
                var percWidth = (size.Width / (float)originalWidth); // 0.2
                var percHeight = (size.Height / (float)originalHeight); // 0.2
                var percentage = Math.Max(percHeight, percWidth); // 0.2

                //get the ideal width and height for the resize (to the next whole number)
                var width = (int)Math.Max(originalWidth * percentage, size.Width); // 512,600
                var height = (int)Math.Max(originalHeight * percentage, size.Height); // 288,377

                //actually resize it
                using (var resizedBmp = new Bitmap(width, height))
                {
                    using (var graphics = Graphics.FromImage(resizedBmp))
                    {
                        graphics.Clear(Color.White);
                        graphics.InterpolationMode = InterpolationMode.Default;
                        graphics.DrawImage(imgOriginal, 0, 0, width, height);
                    }

                    //work out the coordinates of the top left pixel for cropping
                    x = Math.Max(x, (width - size.Width) / 2); // 0
                    y = Math.Max(y, (height - size.Height) / 2); // 0

                    //create the cropping rectangle
                    var rectangle = new Rectangle(x, y, size.Width, size.Height); // 0, 0, 600, 377

                    //crop
                    using (var croppedBmp = resizedBmp.Clone(rectangle, resizedBmp.PixelFormat))
                    using (var ms = new MemoryStream())
                    {
                        //get the codec needed
                        var imgCodec = ImageCodecInfo.GetImageEncoders().First(c => c.FormatID == format.Guid);

                        //make a paramater to adjust quality
                        var codecParams = new EncoderParameters(1);

                        //reduce to quality of 80 (from range of 0 (max compression) to 100 (no compression))
                        codecParams.Param[0] = new EncoderParameter(Encoder.Quality, 100L);

                        //save to the memorystream - convert it to an array and send it back as a byte[]
                        croppedBmp.Save(ms, imgCodec, codecParams);
                        if (!string.IsNullOrEmpty(saveTo))
                            croppedBmp.Save(saveTo);
                        return ms.ToArray();
                    }
                }
            }
        }
        public static string GetAnonymousValue(object obj, string key)
        {
            if (obj != null)
            {
                Type type = obj.GetType();
                PropertyInfo info = type.GetProperty(key);
                return info != null ? info.GetValue(obj, null).ToString().ToLower() : "";
            }
            return "";
        }
        public static List<string> SaveMultiFile(HttpPostedFileBase[] file, string path = "~/resources/advs")
        {
            List<string> images = new List<string>();
            foreach (var item in file)
            {
                string tempFolder = HttpContext.Current.Server.MapPath(path);
                string fileName = Guid.NewGuid() + ".jpeg";
                string savePath = Path.Combine(tempFolder, fileName);

                if (!Directory.Exists(tempFolder)) Directory.CreateDirectory(tempFolder);

                MemoryStream target = new MemoryStream();
                item.InputStream.CopyTo(target);
                byte[] fileData = target.ToArray();

                Image img = Image.FromStream(item.InputStream);
                int width = 1200;
                int height = (int)(img.Height * width / (decimal)img.Width);

                GetCroppedImage(fileData, new Size(width, height), 0, 0, ImageFormat.Jpeg, savePath);
                img.Dispose();
                images.Add(fileName);
            }
            return images;

        }
        public static int GetAnOtherDays(DateTime oldDate, DateTime newDate, int years)
        {
            int days = 0;

            oldDate = oldDate.AddYears(years);
            DateTime oldDate1 = oldDate.AddMonths(1);

            while ((oldDate1.Month <= newDate.Month && oldDate1.Year <= newDate.Year) ||
                (oldDate1.Month > newDate.Month && oldDate1.Year < newDate.Year))
            {
                days += ((TimeSpan)(oldDate1 - oldDate)).Days - 30;
                oldDate = oldDate.AddMonths(1);
                oldDate1 = oldDate.AddMonths(1);
            }

            return days;
        }
        public static int GetLeapDays(DateTime oldDate, DateTime newDate)
        {
            int days = 0;

            while (oldDate.Year < newDate.Year)
            {
                if (DateTime.IsLeapYear(oldDate.Year)) days += 1;
                oldDate = oldDate.AddYears(1);
            }

            return days;
        }
        //public static string GetFlag(string code = "SA")
        //{
        //    string file = HttpContext.Current.Server.MapPath("~/Country.json");
        //    //deserialize JSON from file  
        //    string Json = System.IO.File.ReadAllText(file);
        //    // JavaScriptSerializer ser = new JavaScriptSerializer();
        //    List<CounrtyModel> list = JsonConvert.DeserializeObject<List<CounrtyModel>>(Json);
        //    var name = list.Where(w => w.code == code).FirstOrDefault();
        //    if (name == null)
        //        name = list.Where(w => w.code == "SA").FirstOrDefault();
        //    return name.flag;
        //}
        //public static string GetCode(string code = "SA")
        //{
        //    string file = HttpContext.Current.Server.MapPath("~/Country.json");
        //    //deserialize JSON from file  
        //    string Json = System.IO.File.ReadAllText(file);
        //    // JavaScriptSerializer ser = new JavaScriptSerializer();
        //    List<CounrtyModel> list = JsonConvert.DeserializeObject<List<CounrtyModel>>(Json);
        //    var flag = list.Where(w => w.code == code).FirstOrDefault().code;
        //    return flag;
        //}
        //public static string GetCountry(string code = "SA")
        //{
        //    string file = HttpContext.Current.Server.MapPath("~/Country.json");
        //    //deserialize JSON from file  
        //    string Json = System.IO.File.ReadAllText(file);
        //    // JavaScriptSerializer ser = new JavaScriptSerializer();
        //    List<CounrtyModel> list = JsonConvert.DeserializeObject<List<CounrtyModel>>(Json);
        //    var name = list.Where(w => w.code == code).FirstOrDefault();
        //    if (name == null)
        //        name = list.Where(w => w.code == "SA").FirstOrDefault();
        //    return name.name;
        //}
        public static string GetBase64(string imagePath)
        {
            if (string.IsNullOrEmpty(imagePath) || !System.IO.File.Exists(HttpContext.Current.Server.MapPath("~/resources/Question/" + imagePath)))
                return "";
            using (Image image = Image.FromFile(HttpContext.Current.Server.MapPath("~/resources/Question/" + imagePath)))
            {
                using (MemoryStream m = new MemoryStream())
                {
                    image.Save(m, image.RawFormat);
                    byte[] imageBytes = m.ToArray();

                    // Convert byte[] to Base64 String
                    string base64String = Convert.ToBase64String(imageBytes);
                    return base64String;
                }
            }

        }

        //public static string SendNotification(List<string> deviceId, string device, string message, string send_type, int? course, int? opp)
        //{
        //    string key = getOption("fcm");
        //    string postData = "";
        //    // string SERVER_API_KEY = "";
        //    string SERVER_API_KEY = key;
        //    var SENDER_ID = "#";
        //    var value = message;
        //    WebRequest tRequest;
        //    tRequest = WebRequest.Create("https://fcm.googleapis.com/fcm/send");
        //    tRequest.Method = "post";
        //    tRequest.ContentType = "application/json";
        //    tRequest.Headers.Add(string.Format("Authorization: key={0}", SERVER_API_KEY));
        //    tRequest.Headers.Add(string.Format("Sender: id={0}", SENDER_ID));
        //    // string receivers = string.Join("\",\"", deviceId);
        //    if (!string.IsNullOrEmpty(send_type) && send_type.Equals("topic"))
        //    {
        //        postData = "{\"collapse_key\":\"score_update\",\"time_to_live\":108,\"delay_while_idle\":true,\"notification\": { \"body\" : " + "\"" + value + "\",\"time\": " + "\"" + System.DateTime.Now.ToString() + "\"},\"to\":\"/topics/dev_cash\",\"priority\":\"high\"}";
        //    }
        //    else if (deviceId.FirstOrDefault() != null && send_type.Equals("token"))
        //    {
        //        string receivers = string.Join("\",\"", deviceId);
        //        postData = "{\"collapse_key\":\"score_update\",\"time_to_live\":108,\"delay_while_idle\":true,\"notification\": { \"body\" : " + "\"" + value + "\",\"time\": " + "\"" + System.DateTime.Now.ToString() + "\"},\"registration_ids\":[\"" + receivers + "\"],\"priority\":\"high\"}";
        //    }
        //    Byte[] byteArray = System.Text.Encoding.UTF8.GetBytes(postData);
        //    tRequest.ContentLength = byteArray.Length;
        //    Stream dataStream = tRequest.GetRequestStream();
        //    dataStream.Write(byteArray, 0, byteArray.Length);
        //    dataStream.Close();
        //    WebResponse tResponse = tRequest.GetResponse();
        //    dataStream = tResponse.GetResponseStream();
        //    StreamReader tReader = new StreamReader(dataStream);
        //    String sResponseFromServer = tReader.ReadToEnd();
        //    tReader.Close();
        //    dataStream.Close();
        //    tResponse.Close();
        //    return JsonConvert.SerializeObject(postData);
        //}
        //public static string getOption(string name)
        //{
        //    using (var db = new MhanaDbEntities())
        //    {
        //        var conf = db.Settings.Where(w => w.option_key == name).FirstOrDefault();
        //        if (conf != null)
        //            return !string.IsNullOrEmpty(conf.option_value) ? conf.option_value : "";
        //        return "";
        //    }
        //}
        //// [CacheFilterAttribute(Duration = 60)]
        //public static List<string> GetAllCity()
        //{
        //    string file = HttpContext.Current.Server.MapPath("~/saudi_regions.json");
        //    //deserialize JSON from file  
        //    string Json = System.IO.File.ReadAllText(file);
        //    // JavaScriptSerializer ser = new JavaScriptSerializer();
        //    List<CityListModel> list = JsonConvert.DeserializeObject<List<CityListModel>>(Json);
        //    //List<CityList> list = JsonConvert.DeserializeObject<List<CityList>>(Json);
        //    var items = list.OrderBy(o => o.city_name).Select(s => s.city_name).Distinct().ToList();
        //    return items;

        //}
        //  [CacheFilterAttribute(Duration = 60)]
        //public static List<string> GetDistricts(string city)
        //{
        //    string file = HttpContext.Current.Server.MapPath("~/saudi_regions.json");
        //    //deserialize JSON from file  
        //    string Json = System.IO.File.ReadAllText(file);
        //    // JavaScriptSerializer ser = new JavaScriptSerializer();
        //    List<CityListModel> list = JsonConvert.DeserializeObject<List<CityListModel>>(Json);
        //    //List<CityList> list = JsonConvert.DeserializeObject<List<CityList>>(Json);
        //    var items = list.Where(w => w.city_name == city).OrderBy(o => o.district_name).Select(s => s.district_name).ToList();
        //    return items;
        //}
        //// [CacheFilterAttribute(Duration = 60)]
        //public static List<string> GetAllDistricts()
        //{
        //    string file = HttpContext.Current.Server.MapPath("~/saudi_regions.json");
        //    //deserialize JSON from file  
        //    string Json = System.IO.File.ReadAllText(file);
        //    // JavaScriptSerializer ser = new JavaScriptSerializer();
        //    List<CityListModel> list = JsonConvert.DeserializeObject<List<CityListModel>>(Json);
        //    //List<CityList> list = JsonConvert.DeserializeObject<List<CityList>>(Json);
        //    var items = list.OrderBy(o => o.district_name).Select(s => s.district_name).ToList();
        //    return items;
        //}
        public static void SaveFile(byte[] content, string path)
        {
            string filePath = GetFileFullPath(path);
            if (!Directory.Exists(Path.GetDirectoryName(filePath)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(filePath));
            }

            //Save file
            using (FileStream str = File.Create(filePath))
            {
                str.Write(content, 0, content.Length);
            }
        }
        private static string GetFileFullPath(string path)
        {
            string relName = path.StartsWith("~") ? path : path.StartsWith("/") ? string.Concat("~", path) : path;
            string filePath = relName.StartsWith("~") ? HostingEnvironment.MapPath(relName) : relName;
            return filePath;
        }
        public static bool IsBase64Encoded(String str)
        {
            try
            {
                // If no exception is caught, then it is possibly a base64 encoded string
                byte[] data = Convert.FromBase64String(str);
                // The part that checks if the string was properly padded to the
                // correct length was borrowed from d@anish's solution
                return (str.Replace(" ", "").Length % 4 == 0);
            }
            catch
            {
                // If exception is caught, then it is not a base64 encoded string
                return false;
            }
        }
        //public async Task<string> UploadSingleFile()
        //{
        //    string url = HttpContext.Current.Server.MapPath("~/resources/advs");
        //    var streamProvider = new MultipartFormDataStreamProvider(url);
        //    await  HttpContext.Current.Request.ReadAsMultipartAsync(streamProvider);

        //    return new FileResult
        //    {
        //        FileNames = streamProvider.FileData.Select(entry => entry.LocalFileName),
        //        Names = streamProvider.FileData.Select(entry => entry.Headers.ContentDisposition.FileName),
        //        ContentTypes = streamProvider.FileData.Select(entry => entry.Headers.ContentType.MediaType),
        //        Description = streamProvider.FormData["description"],
        //        CreatedTimestamp = DateTime.UtcNow,
        //        UpdatedTimestamp = DateTime.UtcNow,
        //        DownloadLink = "TODO, will implement when file is persisited"
        //    };
        //}
        //public static string SendNotification(List<string> deviceId, string message, string send_type, int? adv_id, int? ntype = 0, string user_id = null)
        //{
        //    string postData = "";
        //    // string SERVER_API_KEY = "";
        //    //
        //    string SERVER_API_KEY = "AAAAB0gkcr8:APA91bE0SMyOrHj19QE6bOQ3eCXfWwg0IIw6LT96-aqeiQTJD6_Q7Ik6lMcTcGITvGORkx4wqu4o7etXa1KRq19SZQD2gU21rRKAOH1zH5EeYRybiF0mvhwLU8lethWHMpcNojA0z8FC";
        //    var SENDER_ID = "#";
        //    var value = message;
        //    WebRequest tRequest;
        //    tRequest = WebRequest.Create("https://fcm.googleapis.com/fcm/send");
        //    tRequest.Method = "post";
        //    tRequest.ContentType = "application/json";
        //    tRequest.Headers.Add(string.Format("Authorization: key={0}", SERVER_API_KEY));
        //    tRequest.Headers.Add(string.Format("Sender: id={0}", SENDER_ID));
        //    var data = new object();
        //    if (send_type == "token" && !string.IsNullOrEmpty(deviceId.FirstOrDefault()))
        //    {

        //        var receivers = deviceId.ToArray();

        //        //receivers
        //        data = GetData(receivers, null, message, adv_id);

        //    }
        //    else if (send_type != "token")
        //    {
        //        List<string> x = new List<string>();
        //        var xx = x.ToArray();
        //        data = GetData(xx, "/topics/mhanna", message, adv_id);

        //    }
        //    string json = JsonConvert.SerializeObject(data);

        //    Byte[] byteArray = System.Text.Encoding.UTF8.GetBytes(json);
        //    tRequest.ContentLength = byteArray.Length;
        //    Stream dataStream = tRequest.GetRequestStream();
        //    dataStream.Write(byteArray, 0, byteArray.Length);
        //    dataStream.Close();
        //    WebResponse tResponse = tRequest.GetResponse();
        //    dataStream = tResponse.GetResponseStream();
        //    StreamReader tReader = new StreamReader(dataStream);
        //    String sResponseFromServer = tReader.ReadToEnd();
        //    tReader.Close();
        //    dataStream.Close();
        //    tResponse.Close();

        //    using (var db = new MhanaDbEntities())
        //    {
        //        var notiy = new Notification()
        //        {
        //            rel_id = adv_id + "",
        //            cdate = DateTime.Now,
        //            details = message,
        //            type = ntype,
        //            is_read = 0,
        //            user_id = user_id,
        //            title = "تطبيق درس خصوصي"
        //        };
        //        db.Notifications.Add(notiy);
        //        db.SaveChanges();
        //    }
        //    return JsonConvert.SerializeObject(json);

        //}
        private static object GetData(string[] con, string to, string message, int? adv_id)
        {
            if (string.IsNullOrEmpty(to) && con != null)
            {
                var data = new
                {
                    collapse_key = "score_update",
                    time_to_live = 108,
                    delay_while_idle = true,
                    // condition = con,
                    // to = "/topics/dev_molen",
                    //"[fNzL1u7KYM8:APA91bE3velIbSW8QwhvCQXMlKv_YH2YUnfu-c1LeNi74vRsJAgDI9uUAz9pzXQGaNCVBbi2b3mL05qO1iJ9aoaNcD4iXIfNA-gfDzmkZM92q7jI5JsJIXFXFq-mgNmb7yjdnF1D4zPB,fA5n4gCyaYs:APA91bE4qoxxBiy7xljm745ur6QASFk77luwypvdWg9fXyhMDSSMMA79ZnGdQdqXy4-bR0YZ2PfQ3PCIr1hzElu_NpSpGGpzibvCg2C3epQbghjEHHTede6RI3hwxOdwo6f5SUZs284U,ff9yuTjRUbo:APA91bH9KBCi7mtbjWsdrV9tDYv3K6BWP8K5uhTg51E_stmUEyNtlrQS0iItnBOxIPz0iEl4M7XFh8gK_8x2LPce1dID1XPWxSElTGYpoJvfxmPN-PikAlAQ6vTxtXqOpEV3ly5U7T8o,dETC4Xtk65Y:APA91bHke1O3YQAProJOeKKf2txl_gIe8HhP4Bxpu8kjZue6ZD-gUsOOVnV1X6mNqgJcWnHgwVrjIqfNqLQLB_ViUBGqncjSZREIdebep_nXNSVCkBhHjpbRCnfGBChe4vIXjbdOqNOF,fdShLkGhcX0:APA91bE7XfhskoR4gJYJn8johnLC2BTne7lMkBYWqnMLaPqifMa3S3tBYFBiBewqCd-U7kSTvev2_hM6kPsEIY2VnfLIsioWm66E5u1rLqUkhBnk1n1M6D5ntJ84av4Lc7bT7Mb08z3d,duXQ8f8BE2E:APA91bGYdksqSMrpCK47ds0DEDNt20sjFsrg7GNrrcoF5d5Mja8bONSaXVzPtZGGbij2zVbnLkzSrcBFoO9AsOySpnLaVAvh761Y8pTDb3Gjh9zsFYGjXbz9DqwYmDlI8DO3AUcGGaUN,d0ULQHX-uUI:APA91bEdKmomYZqWq2QR9OskM6Hg-zJoiVJ0qbJhd7ZlsCrRME9zzsPW39_HX9-98ZOYogUbMJa9tbyRp6f_N0QK2pSfIXlFqZY65O73JyjWw6_4fTppKICKAo1dPeFj9yfQaMBkAqza]",
                    registration_ids = con,
                    priority = "high",
                    notification = new
                    {
                        body = message,
                        title = "تطبيق درس خصوصي",
                        time = System.DateTime.Now.ToString(),
                        Params = new
                        {
                            rel_id = adv_id,
                            //is_molen = is_molen

                        }
                    },
                    data = new
                    {
                        Params = new
                        {
                            rel_id = adv_id,
                            // is_molen = is_molen
                        }
                    }
                };
                return data;
            }
            //else if (!string.IsNullOrEmpty(con))
            //{
            //    var data = new
            //    {
            //        collapse_key = "score_update",
            //        time_to_live = 108,
            //        delay_while_idle = true,
            //        condition = con,
            //        // to = "/topics/dev_molen",
            //        priority = "high",
            //        notification = new
            //        {
            //            body = message,
            //            title = "تطبيق عطني",
            //            time = System.DateTime.Now.ToString(),
            //            Params = new
            //            {
            //                adv_id = adv_id,
            //                is_molen = false
            //            }
            //        },
            //        data = new
            //        {
            //            Params = new
            //            {
            //                adv_id = adv_id,
            //                is_molen = false
            //            }
            //        }
            //    };
            //    return data;
            //}
            else
            {
                var data = new
                {
                    collapse_key = "score_update",
                    time_to_live = 108,
                    delay_while_idle = true,
                    // condition = con,
                    to = to,
                    priority = "high",
                    notification = new
                    {
                        body = message,
                        title = "تطبيق درس خصوصي",
                        time = System.DateTime.Now.ToString(),
                        Params = new
                        {
                            rel_id = adv_id,
                        }
                    },
                    data = new
                    {
                        Params = new
                        {
                            rel_id = adv_id,

                        }
                    }
                };
                return data;
            }

        }
        public static string UploadVideo(HttpPostedFileBase file)
        {
            string tempFolder = HttpContext.Current.Server.MapPath("~/resources/advs");
            string fileName = Guid.NewGuid() + ".mp4";
            string savePath = Path.Combine(tempFolder, fileName);
            file.SaveAs(savePath);
            return fileName;
        }

        //public static void ExportData(DataTable _dataTable, string title, string filename)
        //{
        //    using (XLWorkbook wb = new XLWorkbook())
        //    {
        //        wb.Worksheets.Add(_dataTable, title);
        //        wb.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        //        wb.Style.Font.Bold = true;
        //        wb.Author = "molin";
        //        wb.RightToLeft = true;
        //        wb.Worksheets.FirstOrDefault().RightToLeft = true;
        //        HttpContext.Current.Response.Clear();
        //        HttpContext.Current.Response.Buffer = true;
        //        HttpContext.Current.Response.Charset = "";
        //        HttpContext.Current.Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
        //        HttpContext.Current.Response.AddHeader("Content-Disposition", string.Format("attachment;filename={0}", filename + ".xlsx"));
        //        using (MemoryStream MyMemoryStream = new MemoryStream())
        //        {
        //            wb.SaveAs(MyMemoryStream);
        //            MyMemoryStream.WriteTo(HttpContext.Current.Response.OutputStream);
        //            HttpContext.Current.Response.Flush();
        //            HttpContext.Current.Response.End();
        //        }
        //    }


        //}
        public static AttachmentType GetMimeType(this string value)
        {
            if (String.IsNullOrEmpty(value))
                return new AttachmentType
                {
                    FriendlyName = "Unknown",
                    MimeType = "application/octet-stream",
                    Extension = ""
                };

            var data = value.Substring(0, 5);

            switch (data.ToUpper())
            {
                case "IVBOR":
                case "/9J/4":
                    return new AttachmentType
                    {
                        FriendlyName = "Photo",
                        MimeType = "image/png",
                        Extension = ".png"
                    };

                case "AAAAF":
                    return new AttachmentType
                    {
                        FriendlyName = "Video",
                        MimeType = "video/mp4",
                        Extension = ".mp4"
                    };
                case "JVBER":
                    return new AttachmentType
                    {
                        FriendlyName = "Document",
                        MimeType = "application/pdf",
                        Extension = ".pdf"
                    };

                default:
                    return new AttachmentType
                    {
                        FriendlyName = "Unknown",
                        MimeType = string.Empty,
                        Extension = ""
                    };
            }
        }
        //public static string UploadB64File(string photo, string path)
        //{
        //    if (!string.IsNullOrEmpty(photo))
        //    {
        //        string b64 = FixBase64ForImage(photo);
        //        byte[] imageBytes = Convert.FromBase64String(b64);
        //        string tempFolderName = path;
        //        try
        //        {
        //            string fileName = ImageHelper.SaveServiceFile(imageBytes, tempFolderName);
        //            return fileName;
        //        }
        //        catch (Exception ex)
        //        {

        //            ex.ToString();
        //            return "default.png";
        //            //Log an error                        
        //        }
        //    }
        //    return "default.png";
        //}

        public static T DeserializeXML<T>(string xmlContent)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(T));
            MemoryStream memStream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(xmlContent));
            return (T)serializer.Deserialize(memStream);

        }


        //public static string SerializeXML()
        //{
        //    XmlSerializer serializer = new XmlSerializer(typeof(attendee_list));
        //    MemoryStream memStream = new MemoryStream();
        //    List<attendee_listAttendee> _list = new List<attendee_listAttendee>();
        //    _list.Add(new attendee_listAttendee()
        //    {
        //        attendee_id = "15225",
        //        screen_name = "شاشة عبدالله ",
        //        language_culture_name = "ar-sa"
        //    });
        //    attendee_list all = new attendee_list();
        //    all.attendee = _list.ToArray();
        //    serializer.Serialize(memStream, all);
        //    StreamReader sw = new StreamReader(memStream);           
        //    return sw.ReadToEnd();

        //}


        //public static string CreateWiziq(Course elemnt, Course_Times _ct)
        //{
        //    try
        //    {
        //        using (var db = new MhanaDbEntities())
        //        {
        //            var wiziq_id = db.WiziqIDs.ToList();
        //            foreach (var item in wiziq_id)
        //            {
        //                CourseWiziQ Cw = new CourseWiziQ()
        //                {
        //                    attendee_limit = 10 + "",
        //                    create_recording = "true",
        //                    duration = elemnt.is_test == 1 ? "10" : "60",
        //                    start_time = _ct.sch_date.Value.Month + "/" + _ct.sch_date.Value.Day + "/" + _ct.sch_date.Value.Year + " " + _ct.sch_date.Value.Hour + ":" + _ct.sch_date.Value.Minute,
        //                    title = elemnt.title + _ct.id,
        //                    //presenter_email = "mohanna111@gmail.com",
        //                    presenter_id = item.wiziq_id,
        //                    presenter_name = item.email,
        //                    language_culture_name = "ar-sa",
        //                    presenter_default_controls = "audio,video",
        //                    time_zone = "Asia/Riyadh"
        //                };
        //                string c_response = WiziQ.WizIQClass.Create(Cw);
        //                using (System.IO.StreamWriter file = new System.IO.StreamWriter(System.Web.HttpContext.Current.Server.MapPath("~/debug.txt"), true))
        //                    file.WriteLine("/n" + c_response + Cw.start_time);
        //                if (!string.IsNullOrEmpty(c_response) && c_response.Substring(0, 20).Contains("ok"))
        //                {
        //                    rsp resultObject = Functions.DeserializeXML<rsp>(c_response);
        //                    _ct.teacher_link = resultObject.create.class_details.presenter_list.presenter.presenter_url;
        //                    _ct.wiziq_id = resultObject.create.class_details.class_id;
        //                    _ct.status = 1;
        //                    elemnt.course_live_url = resultObject.create.class_details.presenter_list.presenter.presenter_url;
        //                    db.Entry(_ct).State = EntityState.Modified;
        //                    db.Entry(elemnt).State = EntityState.Modified;
        //                    db.SaveChanges();

        //                    return _ct.wiziq_id;
        //                }

        //            }

        //        }

        //        return "";
        //    }
        //    catch (Exception ex)
        //    {
        //        using (System.IO.StreamWriter file = new System.IO.StreamWriter(System.Web.HttpContext.Current.Server.MapPath("~/debug.txt"), true))
        //            file.WriteLine(ex.ToString());
        //        return "";
        //    }
        //}

        //public static string AddStudentToWiziq(Course elemnt, string wiziq_id)
        //{
        //    try
        //    {
        //        using (var db = new MhanaDbEntities())
        //        {
        //            var stds_reg = db.Std_Course.Where(w => w.course_id == elemnt.Id && w.status == 1).ToList();
        //            var stds = stds_reg.Where(w => w.course_id == elemnt.Id && w.status == 1).Select(s => new atten_info()
        //            {
        //                lang = "ar-sa",
        //                name = string.IsNullOrEmpty(s.AspNetUser.fullname) ? (s.AspNetUser.Email) : s.AspNetUser.fullname,
        //                user_id = s.std_id
        //            }).ToList();
        //            string xml_attendance = "";
        //            foreach (var item in stds)
        //            {
        //                xml_attendance += "<attendee><attendee_id>" + item.user_id + "</attendee_id><screen_name>" + item.name + "</screen_name><language_culture_name>" + item.lang + "</language_culture_name></attendee>";
        //            }
        //            var std_response = WiziQ.WizIQClass.AddAttendees(wiziq_id, xml_attendance);
        //            using (System.IO.StreamWriter file = new System.IO.StreamWriter(System.Web.HttpContext.Current.Server.MapPath("~/debug.txt"), true))
        //                file.WriteLine("/n" + std_response);
        //            string atten_url = "";
        //            if (!string.IsNullOrEmpty(std_response) && std_response.Substring(0, 20).Contains("ok"))
        //            {
        //                rsp resultObject = Functions.DeserializeXML<rsp>(std_response);
        //                foreach (var item in resultObject.add_attendees.attendee_list)
        //                {
        //                    var std_time = stds_reg.Where(w => w.std_id == item.attendee_id).FirstOrDefault();
        //                    if (std_time != null)
        //                    {
        //                        atten_url = item.attendee_url;
        //                        std_time.lesson_link = item.attendee_url;
        //                        db.Entry(std_time).State = EntityState.Modified;

        //                    }

        //                }
        //                db.SaveChanges();
        //            }

        //            return atten_url;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        using (System.IO.StreamWriter file = new System.IO.StreamWriter(System.Web.HttpContext.Current.Server.MapPath("~/debug.txt"), true))
        //            file.WriteLine(ex.ToString());
        //        return "";
        //    }
        //}



        //public static string CreateWiziqTest(StdNewRequest requset)
        //{
        //    try
        //    {
        //        using (var db = new MhanaDbEntities())
        //        {
        //            var wiziq_id = db.WiziqIDs.ToList();
        //            foreach (var item in wiziq_id)
        //            {
        //                CourseWiziQ Cw = new CourseWiziQ()
        //                {
        //                    attendee_limit = 2 + "",
        //                    create_recording = "true",
        //                    duration = "10",
        //                    start_time = DateTime.Now.Month + "/" + DateTime.Now.Day + "/" + DateTime.Now.Year + " " + DateTime.Now.Hour + ":" + DateTime.Now.Minute,
        //                    title = "اتصال تجريبي",
        //                    presenter_email = item.wiziq_id,
        //                    presenter_id = item.email,
        //                    presenter_name = "درس تجريبي",
        //                    language_culture_name = "ar-sa",
        //                    presenter_default_controls = "audio,video",
        //                    time_zone = "Asia/Riyadh"
        //                };
        //                string c_response = WiziQ.WizIQClass.Create(Cw);
        //                using (System.IO.StreamWriter file = new System.IO.StreamWriter(System.Web.HttpContext.Current.Server.MapPath("~/debug.txt"), true))
        //                    file.WriteLine("/n" + c_response + Cw.start_time);
        //                if (!string.IsNullOrEmpty(c_response) && c_response.Substring(0, 20).Contains("ok"))
        //                {
        //                    rsp resultObject = Functions.DeserializeXML<rsp>(c_response);
        //                    //using (System.IO.StreamWriter file = new System.IO.StreamWriter(System.Web.HttpContext.Current.Server.MapPath("~/debug.txt"), true))
        //                    //    file.WriteLine(resultObject.create.class_details.class_id);
        //                    requset.presenter_url = resultObject.create.class_details.presenter_list.presenter.presenter_url;
        //                    requset.wiziq_id = resultObject.create.class_details.class_id;
        //                    // elemnt.course_live_url = resultObject.create.class_details.presenter_list.presenter.presenter_url;
        //                    db.Entry(requset).State = EntityState.Modified;
        //                    db.SaveChanges();
        //                    return resultObject.create.class_details.class_id;
        //                }

        //            }

        //            return "";

        //        }



        //    }
        //    catch (Exception ex)
        //    {
        //        using (System.IO.StreamWriter file = new System.IO.StreamWriter(System.Web.HttpContext.Current.Server.MapPath("~/debug.txt"), true))
        //            file.WriteLine(ex.ToString());
        //        return "";
        //    }
        //}

        //public static string AddStudentToWiziqTest(StdNewRequest requset, string wiziq_id)
        //{
        //    try
        //    {
        //        using (var db = new MhanaDbEntities())
        //        {
        //            var stds = db.AspNetUsers.Where(w => w.Id == requset.std_id).Select(s => new atten_info()
        //            {
        //                lang = "ar-sa",
        //                name = s.fullname,
        //                user_id = s.Id
        //            }).ToList();
        //            string xml_attendance = "";
        //            foreach (var item in stds)
        //            {
        //                xml_attendance += "<attendee><attendee_id>" + item.user_id + "</attendee_id><screen_name>اتصال تجريبي</screen_name><language_culture_name>" + item.lang + "</language_culture_name></attendee>";
        //            }
        //            var std_response = WiziQ.WizIQClass.AddAttendees(wiziq_id, xml_attendance);
        //            using (System.IO.StreamWriter file = new System.IO.StreamWriter(System.Web.HttpContext.Current.Server.MapPath("~/debug.txt"), true))
        //                file.WriteLine("/n" + std_response);
        //            string atten_url = "";
        //            if (!string.IsNullOrEmpty(std_response) && std_response.Substring(0, 20).Contains("ok"))
        //            {
        //                rsp resultObject = Functions.DeserializeXML<rsp>(std_response);
        //                foreach (var item in resultObject.add_attendees.attendee_list)
        //                {
        //                    requset.std_url = item.attendee_url;

        //                }
        //                db.Entry(requset).State = EntityState.Modified;
        //                db.SaveChanges();
        //            }
        //            return atten_url;
        //        }

        //    }
        //    catch (Exception ex)
        //    {
        //        using (System.IO.StreamWriter file = new System.IO.StreamWriter(System.Web.HttpContext.Current.Server.MapPath("~/debug.txt"), true))
        //            file.WriteLine(ex.ToString());
        //        return "";
        //    }
        //}

    }
}

public class actionTypes
{
    public string action { get; set; }
}
public class imp
{
    public int? charge { get; set; }
    public int? withdraw { get; set; }
    public int? advs { get; set; }
}
public class AttachmentType
{
    public string MimeType { get; set; }
    public string FriendlyName { get; set; }
    public string Extension { get; set; }
}


