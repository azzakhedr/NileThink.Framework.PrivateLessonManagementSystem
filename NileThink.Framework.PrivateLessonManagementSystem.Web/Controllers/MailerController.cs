using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web;
using System.Web.Mvc;
using ActionMailer.Net.Mvc;
using NileThink.Framework.PrivateLessonManagementSystem.Web.Models;

namespace NileThink.Framework.PrivateLessonManagementSystem.Web.Controllers
{
    public class MailerController
    {
        // GET: Mailer
        //public MailerController()
        //{
        //    From = "info@privatelessonforyou.com";
        //}
        //public ActionResult Index()
        //{
        //    return View();
        //}


        //public EmailResult RegEmail(RegisterEmail model)
        //{
        //    string message = "شكراً لتسجيلك معنا ك " + model.type + " نرجو منك اكمال باقي تفاصيل الملف الشخصي بك ليتم قبولك بالنظام";
        //    To.Add(model.to);
        //    model.message = model.message;
        //    Subject = "تسجيل مستخدم جديد";
        //    return Email("RegEmail", model);
        //}
        //public EmailResult InviteT(EmailInviteSender model)
        //{
        //    To.Add(model.to);
        //    Subject = "دعوة معلم لتطبيق درس خصوصي";
        //    return Email("inviteT", model);
        //}

        //public EmailResult InviteS(EmailInviteSender model)
        //{
        //    To.Add(model.to);
        //    Subject = "دعوة طالب لتطبيق درس خصوصي";
        //    return Email("inviteS", model);
        //}
        //public EmailResult EmailRequest(EmailContract model)
        //{
        //    To.Add(model.to);
        //    Subject = "طلب درس خصوصي";
        //    return Email("EmailRequest", model);
        //}
        //public EmailResult foregetPassword(ChangeUserModel model)
        //{
        //    //To.Add(model.to);
        //    //Subject = "تغيير كلمة المرور";
        //    //return Email("foregetPassword", model);
        //}
        //public EmailResult contact(Contact model)
        //{
        //    string to = Functions.getOption("contact_email");
        //    to = !string.IsNullOrEmpty(to) ? to : "aks.mizo@gmail.com";
        //    To.Add(to);
        //    if (!string.IsNullOrEmpty(model.email))
        //        ReplyTo.Add(model.email);
        //    Subject = "رسالة من مستخدم";
        //    return Email("contact", model);
        //}



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