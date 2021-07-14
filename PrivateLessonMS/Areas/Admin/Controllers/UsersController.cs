using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using NileThink.Framework.PrivateLessonManagementSystem.BLL.BussinessLayer;
using NileThink.Framework.PrivateLessonManagementSystem.BLL.ViewModels;
using PrivateLessonMS.Controllers;
using PrivateLessonMS.Helper;
using PrivateLessonMS.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace PrivateLessonMS.Areas.Admin.Controllers
{
    ////[Authorize(Roles = "3")]
    public class UsersController : BaseController
    {
        Membership _mem = new Membership();
        public ActionResult Index()
        {

            var items = _mem.GetUsers(null, "3", null, null);
            return View(items != null ? items.OrderByDescending(o => o.cdate) : null);

        }

        public ActionResult Create()
        {
            return View();
        }
        [HttpPost]
        public async Task<ActionResult> Create(adminModel model, HttpPostedFileBase photo_file)
        {
            using (var context = new ApplicationDbContext())
            {
                var UserManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(context));
                var checkUser = UserManager.FindByName(model.email);
                if (checkUser != null)
                    return getMessage(Enums.MStatus.check, "البريد الالكتروني مستخدم مسبقاً", "Create", "Users");
                if (photo_file != null)
                {
                    Tuple<bool, string> imgValidation = Functions.ValidateImage(photo_file);
                    if (!imgValidation.Item1)
                    {
                        TempData["Message"] = imgValidation.Item2;
                        TempData["Status"] = "danger";
                        return View(model);
                    }
                    string res = Functions.SaveTempFile(photo_file, "~/resources/users");
                    model.photo = res;
                }
                var user = new ApplicationUser
                {
                    cdate = DateTime.Now,
                    Email = model.email,
                    fullname = model.fullname,
                    PhoneNumber = model.phoneNumber,
                    status = (int)Enums.Status.Visible,
                    UserName = model.email,
                    photo = model.photo,
                    absher_no = model.absher_no

                };

                string pass = System.Web.Security.Membership.GeneratePassword(8, 3);
                var x = await UserManager.CreateAsync(user, pass);
                await UserManager.AddToRoleAsync(user.Id, "3");
                //AdminAddUser mail = new AdminAddUser()
                //{
                //    fullname = model.fullname,
                //    userName = model.UserName,
                //    Password = pass,
                //    to = model.Email,
                //    type = "كمدير نظام"
                //};
                //new Molen.Controllers.MailerController().NewAdmin(mail).Deliver();
                return getMessage(Enums.MStatus.check, "", "Create", "Users");
            }
        }
        public ActionResult Edit(string id)
        {

            var item = _mem.GetUsers(id, null, null, null).FirstOrDefault();
            return View(item);

        }
        [HttpPost]
        public ActionResult Edit(adminModel model, HttpPostedFileBase photo_file)
        {
            using (var context = new ApplicationDbContext())
            {
                var UserManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(context));
                var checkUser = UserManager.FindById(model.id);
                if (photo_file != null)
                {
                    Tuple<bool, string> imgValidation = Functions.ValidateImage(photo_file);
                    if (!imgValidation.Item1)
                    {
                        TempData["Message"] = imgValidation.Item2;
                        TempData["Status"] = "danger";
                        return View();
                    }
                    string res = Functions.SaveTempFile(photo_file, "~/resources/users");
                    model.photo = res;
                }

                checkUser.PhoneNumber = model.phoneNumber;
                checkUser.fullname = model.fullname;
                checkUser.photo = model.photo;
                checkUser.absher_no = model.absher_no;
                UserManager.Update(checkUser);
                return getMessage(Enums.MStatus.check, "", "Index", "Users");
            }
        }

        public ActionResult deactive(string id)
        {

            using (var context = new ApplicationDbContext())
            {
                var UserManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(context));
                var checkUser = UserManager.FindById(id);
                checkUser.status = -1;
                UserManager.Update(checkUser);
                return getMessage(Enums.MStatus.check, "", "Index", "Users");
            }
        }
        public ActionResult Active(string id)
        {

            using (var context = new ApplicationDbContext())
            {
                var UserManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(context));
                var checkUser = UserManager.FindById(id);
                checkUser.status = 1;
                UserManager.Update(checkUser);
                return getMessage(Enums.MStatus.check, "", "Index", "Users");
            }
        }


        public async Task<ActionResult> Delete(string id)
        {
            var user = await UserManager.FindByIdAsync(id);
            await UserManager.DeleteAsync(user);


            return getMessage(Enums.MStatus.check, "", "Index", "Users");

        }

        public ActionResult Change_password()
        {
            return View();
        }
        [HttpPost]
        public async Task<ActionResult> Change_password(ChangePasswordViewModel model)
        {
            var u = Functions.GetUser();
            using (var context = new ApplicationDbContext())
            {
                var UserManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(context));
                var user = await UserManager.FindByNameAsync(u.email);
                if (user != null && UserManager.CheckPassword(user, model.OldPassword))
                {
                    var result = await UserManager.ChangePasswordAsync(user.Id, model.OldPassword, model.NewPassword);
                    return getMessage(Enums.MStatus.check, "", "Change_password", "Users");
                }
            }
            return getMessage(Enums.MStatus.remove, "", "Change_password", "Users");
        }
    }
}