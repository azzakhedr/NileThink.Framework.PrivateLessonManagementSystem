
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web;
using System.Web.Mvc;

namespace PrivateLessonMS.Controllers
{
    public class MailerController
    {
        public string SendMail(List<string> ToEmail, string MailSubject, string MailBody)
        {

            try
            {

                var model = new SendMailVM()
                {
                    Email = ConfigurationSettings.AppSettings["Mail"],
                    UserName = ConfigurationSettings.AppSettings["Usermame"],
                    Password = ConfigurationSettings.AppSettings["Password"],
                    Server = ConfigurationSettings.AppSettings["Host"],
                    Port = ConfigurationSettings.AppSettings["Port"],


                };

                var message = new MailMessage();
                if (ToEmail != null && ToEmail.Count() > 0)
                {
                    foreach (var item in ToEmail)
                    {
                        message.To.Add(new MailAddress(item));  // replace with valid value
                    }
                }
                message.From = new MailAddress(model.Email);  // replace with valid value
                message.Subject = MailSubject;
                message.Body = MailBody;
                message.IsBodyHtml = true;

                using (var smtp = new SmtpClient())
                {
                    var credential = new NetworkCredential
                    {
                        UserName = model.UserName,  // replace with valid value
                        Password = model.Password
                    };

                    smtp.Credentials = credential;
                    smtp.Host = model.Server;
                    smtp.Port = Convert.ToInt16(model.Port);
                    smtp.EnableSsl = true;

                    //smtp.UseDefaultCredentials = true;
                    smtp.Send(message);

                }
                return "1";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }

        }

        public class SendMailVM
        {
            public string Subject { get; set; }
            public string Email { get; set; }
            public string Email_Name { get; set; }
            public string User_Name { get; set; }
            public string Password { get; set; }
            public string Server { get; set; }
            public string Domain { get; set; }
            public string Port { get; set; }
            public string UserName { get; set; }
            public string UserEmail { get; set; }
        }
    }
}