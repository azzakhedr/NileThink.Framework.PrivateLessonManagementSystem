using Microsoft.AspNet.Identity.Owin;
using PrivateLessonMS.Helper;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Mvc;

namespace PrivateLessonMS.Controllers
{
    public class BaseController : Controller
    {
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;
        public ApplicationSignInManager SignInManager
        {
            get
            {
                return _signInManager ?? HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
            }
            private set
            {
                _signInManager = value;
            }
        }
        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }
        public string Language { get { return CultureHelper.GetNeutralCulture(Thread.CurrentThread.CurrentCulture.Name); } }

        protected override IAsyncResult BeginExecuteCore(AsyncCallback callback, object state)
        {
            string cultureName = null;

            // Attempt to read the culture cookie from Request
            HttpCookie cultureCookie = Request.Cookies["_culture"];
            if (cultureCookie != null)
                cultureName = cultureCookie.Value;
            else
                cultureName = Request.UserLanguages != null && Request.UserLanguages.Length > 0 ?
                        Request.UserLanguages[0] :  // obtain it from HTTP header AcceptLanguages
                        null;
            // Validate culture name
            cultureName = CultureHelper.GetImplementedCulture(cultureName); // This is safe

            // Modify current thread's cultures            

            System.Globalization.CultureInfo ci = CultureInfo.CreateSpecificCulture(cultureName);// new System.Globalization.CultureInfo("ar-EG", false);

            DateTimeFormatInfo d = new DateTimeFormatInfo();
            d.DateSeparator = " ";
            d.FullDateTimePattern = "dd MMM yyy HH:mm:ss tt";
            //d.ShortDatePattern = "dd MMM yyy";
            //d.LongDatePattern = "dd MMM yyy";
            d.TimeSeparator = ":";
            d.LongTimePattern = "HH:mm:ss tt";
            //d.SetAllDateTimePatterns(new string[] { "dd MMM yyy HH:mm:ss tt", "dd MMM yyy" },'Y');

            ci.DateTimeFormat = d;


            //SetMonthsName(ci);

            Thread.CurrentThread.CurrentCulture = ci;
            Thread.CurrentThread.CurrentUICulture = ci;
            var call = ci.OptionalCalendars;

            return base.BeginExecuteCore(callback, state);
        }



        public ActionResult getMessage(Enum status, string meesage, string action, string controller, int? id = null)
        {
            TempData["Message"] = string.IsNullOrEmpty(meesage) ? "تمت العملية بنجاح" : meesage;
            TempData["Status"] = status;
            return RedirectToAction(action, controller, new { id = id });
        }
        public ActionResult getMessage(Enum status, string meesage, string action, string controller, string id)
        {
            TempData["Message"] = string.IsNullOrEmpty(meesage) ? "تمت العملية بنجاح" : meesage;
            TempData["Status"] = status;
            return RedirectToAction(action, controller, new { id = id });
        }
        public ActionResult getMessage(Enum status, string meesage, string action, string controller)
        {
            TempData["Message"] = string.IsNullOrEmpty(meesage) ? "تمت العملية بنجاح" : meesage;
            TempData["Status"] = status;
            return RedirectToAction(action, controller);
        }
        public ActionResult getMessage(Enum status, string meesage, string returnUrl)
        {
            TempData["Message"] = string.IsNullOrEmpty(meesage) ? "تمت العملية بنجاح" : meesage;
            TempData["Status"] = status;
            return Redirect(returnUrl);
        }


    }
}