using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Newtonsoft.Json;
using NileThink.Framework.PrivateLessonManagementSystem.BLL.BussinessLayer;
using NileThink.Framework.PrivateLessonManagementSystem.BLL.ViewModels;
using PrivateLessonMS.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace PrivateLessonMS.Controllers
{
    public class HomeController : Controller
    {
       
        public ActionResult Index()
        {
            ViewBag.Title = "Home Page";

            return View();
        }
        public ActionResult LogOff()
        {
            Response.Cache.SetCacheability(HttpCacheability.NoCache);
            Response.Cache.SetExpires(DateTime.Now.AddSeconds(-1));
            Response.Cache.SetNoStore();
            Session.Clear();
            Session.RemoveAll();
            Session.Abandon();
            return RedirectToAction("Login", "Account", new { area = "" });
        }

        public async Task<ActionResult> ResetPassword(string userid)
        {

            return View();
        }
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ResetPassword(ResetPasswordViewModel model)
        {

            if (!ModelState.IsValid)
            {
                return View(model);
            }
            using (var context = new ApplicationDbContext())
            {
                var UserManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(context));
                var user = await UserManager.FindByIdAsync(model.id);
                if (user == null)
                {
                    TempData["Message"] = "حسابك موقوف يرجى مراحعة الادارة ";
                    TempData["Status"] = "danger";
                    return RedirectToAction("ResetPassword", "Home", new { area = "" });
                }

                if (!string.IsNullOrEmpty(model.Code) && user.Email == model.Email)
                {
                    //var data=await  UserManager.ResetPasswordAsync(user.Id,model.Code,model.Password);
                    await UserManager.RemovePasswordAsync(user.Id);
                    await UserManager.AddPasswordAsync(user.Id, model.Password);
                    //if(data.Succeeded)
                    //  {
                    TempData["Message"] = "لقد تم تغيير كلمة المرور بنجاح ";
                    TempData["Status"] = "success";
                    return RedirectToAction("ResetPassword", "Home", new { area = "" });
                    //}
                    //else
                    //{
                    //    TempData["Message"] = "خطأ في اللينك ";
                    //    TempData["Status"] = "danger";
                    //    return RedirectToAction("ResetPassword", "Home", new { area = "" });
                    //}

                }
                TempData["Message"] = "انتهت صلاحية الرابط المرسل على بريدك يرجى طلب رابط جديد";
                TempData["Status"] = "danger";
                return RedirectToAction("ResetPassword", "Home", new { area = "" });
            }

        }


       
    }
}
