using ActionMailer.Net.Mvc;
using NileThink.Framework.PrivateLessonManagementSystem.BLL.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace PrivateLessonMS.Controllers
{
   public class MailerController : MailerBase
    {
        // GET: Mailer
        public MailerController()
        {
            From = "info@privatelessonforyou.com";
        }
        public ActionResult Index()
        {
            return View();
        }
        //public EmailResult NewStd(AdminAddUser model)
        //{
        //    To.Add(model.to);
        //    Subject = "تفعيل حساب جديد";
        //    return Email("NewStd", model);
        //}
        //public EmailResult NewAdmin(AdminAddUser model)
        //{
        //    To.Add(model.to);
        //    Subject = "تفعيل حساب جديد";
        //    return Email("NewAdmin", model);
        //}
        public EmailResult RegEmail(RegEmail model)
        {
            string message = "شكراً لتسجيلك معنا ك "+model.type+" نرجو منك اكمال باقي تفاصيل الملف الشخصي بك ليتم قبولك بالنظام";
            To.Add(model.to);
            model.message = model.message;
            Subject = "تسجيل مستخدم جديد";
            return Email("RegEmail", model);
        }
        public EmailResult InviteT(emailInviteT model)
        {          
            To.Add(model.to);           
            Subject = "دعوة معلم لتطبيق درس خصوصي";
            return Email("inviteT", model);
        }

        public EmailResult InviteS(emailInviteS model)
        {
            To.Add(model.to);
            Subject = "دعوة طالب لتطبيق درس خصوصي";
            return Email("inviteS", model);
        }
        public EmailResult EmailRequest(EmailContract model)
        {
            To.Add(model.to);
            Subject = "طلب درس خصوصي";
            return Email("EmailRequest", model);
        }
        public EmailResult foregetPassword(ChangeUserModel model)
        {
            To.Add(model.to);
            Subject = "تغيير كلمة المرور";
            return Email("foregetPassword", model);
        }
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
    }
}