
using Microsoft.Ajax.Utilities;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using NileThink.Framework.PrivateLessonManagementSystem.BLL.BussinessLayer;
using NileThink.Framework.PrivateLessonManagementSystem.BLL.ViewModels;
using NileThink.Framework.PrivateLessonManagementSystem.DAL.Models;
using PrivateLessonMS.Controllers;
using PrivateLessonMS.Helper;
using PrivateLessonMS.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace PrivateLessonMS.Areas.Admin.Controllers
{
    ////[Authorize(Roles = "3")]
    public class TeachersController : BaseController
    {

        Membership _mem = new Membership();
        TeacherBLL _teach = new TeacherBLL();

        SpecializationBLL _spec = new SpecializationBLL();
        EducationLevelBLL _eduLevel = new EducationLevelBLL();
        BankAccountBLL _bankAccount = new BankAccountBLL();
        StudentNewRequestBLL _newReq = new StudentNewRequestBLL();
        RatingBLL _rate = new RatingBLL();
        InvitesBLL _invite = new InvitesBLL();
        CourseBLL _course = new CourseBLL();
        AbsharDataBLL _absharData = new AbsharDataBLL();
        public ActionResult Index()
        {

            var item = _mem.GetUsers(null, "2", 1, null).Select(s => new TeacherListAdmin()
            {
                absher = s.absher,
                cdate = s.cdate,
                email = s.Email,
                fullname = s.first_name + "" + s.last_name,
                id = s.Id,
                mobile = s.PhoneNumber,
                photo = s.photo,
                status = s.status,

            }).OrderByDescending(o => o.cdate).ToList();
            return View(item);

        }

        public ActionResult Create()
        {
            return View();
        }

        public ActionResult Edit(string id)
        {
            var teacerData = _teach.Teacher_GetByUserId(id);
            //TeacherRegisterResponseModel
            var TT = _teach.GetTeacherAvailabilty(teacerData.teacherId);

            //TT = db.Teacher_Times.Where(w => w.user_id == user_id).Select(s => new TeacherTimes() { day_number = s.day_number, from = s.from_time, to = s.to_time }).ToList();
            ViewBag.specializations = _spec.GetSpecializations().Select(s => new keyValue() { id = s.id, value = s.name }).ToList();
            ViewBag.branch_specialization = _spec.GetBranchSpecializations(null).Select(s => new keyValue() { id = s.id, value = s.name }).ToList();
            ViewBag.education_level = _eduLevel.GetEducationSubLevels(null, null).Select(s => new keyValue() { id = s.id, value = s.name }).ToList();
            ViewBag.banks = _bankAccount.GetBankAccount(id).OrderByDescending(o => o.Id).ToList();
            ViewBag.absherData = _absharData.GetAbsharData(id);
            ViewBag.Courses = _course.getcourses(id, null, null, null, null).OrderByDescending(o => o.cdate).ToList();
            ViewBag.tTimes = TT;
            var user =_teach.Teacher_GetByUserId(id);
            if (user != null)
            {
               


                return View(user);
            }

            return RedirectToAction("Index", "Dashboard", new { area = "Admin" });

        }

        public ActionResult Requests()
        {

            var items = _course.getcourses(null,null,null,null,0).AsEnumerable().Select(s => new NewRequestList()
            {
                cdate = s.cdate + "",
                id = s.id,
              //  period = s.period,
                start_date = s.start_date,
                start_time = s.start_date.HasValue ? s.start_date.Value.TimeOfDay : DateTime.Now.TimeOfDay,
                status = s.status,
                student_name = s.student_name,
               // details = s.details,
               // teaching_mechanism = s.teaching_mechanism,
                teacher_name = s.teacher_name,
              //  material = s.material
            }).OrderByDescending(o => o.id).ToList();
            return View(items);

        }

        public ActionResult EditRequest(int? id)
        {

            var c = _course.getcourses(null,null,id,null,null).FirstOrDefault();
           // ViewBag.banks = _bankAccount.GetBankAccount(c.teacher_id).OrderByDescending(o => o.Id).ToList();
            return View(c);

        }
        [HttpPost]
        public ActionResult EditRequest(EditCourse model)
        {

            _newReq.UpdateRequest(model);

            return getMessage(Enums.MStatus.check, "", "Requests", "Teachers");


        }

        public ActionResult Rates()
        {


            var items = _rate.GetRateType(2).Select(s => new RateModel()
            {
                id = s.Id,
                cdate = DateTime.Now,
                comment = s.Comment,
                course_title = s.title,
                rate = s.Rate,
                course_id = s.CourseID,
                std_id = s.student_id,
                student_name = s.stu_fullname,
                teacher_name = s.teach_fullname,
                photo = "/resources/users/" + s.photo

            }).OrderByDescending(o => o.id).ToList();
            return View(items);

        }
        //public ActionResult Invites_Teacher(int? id)
        //{

        //    ViewBag.Courses = _course.getcourses(null, null, null, null, 1).Select(s => new keyValue() { id = s.id, value = s.title }).ToList();
        //    return View();



        //}
        //[HttpPost]
        //public async Task<ActionResult> Invites_Teacher(Invite model, int? type)
        //{
        //    var user_id = Functions.GetUser().id;

        //    model.cdate = DateTime.Now;
        //    model.send_by = user_id;
        //    model.status = 0;
        //    model.invite_type = 1; //0 for student without reg 1 for student with reg
        //    //_invite.InsertInvite(model);
        //    if (type != 1)
        //    {
        //        var mail = new emailInviteT()
        //        {
        //            cdate = DateTime.Now + "",
        //            from = User.Identity.Name,
        //            to = model.email,
        //            title = "دعوة الانضمام الى تطبيق درس خصوصي",
        //            msg = "لقد تمت دعوتك للانضمام الى تطبيق درس خصوصي من قبل المعلم " + model.email + "يمكنك الآن الانضمام الى التطبيق وتسجيل بياناتك من خلال الروابط المرفقة الخاصة بتطبيق الأيفون والأندرويد"
        //        };
        //        new MailerController().InviteT(mail).Deliver();
        //    }

        //    if (type == 1)
        //    {
        //        using (var context = new ApplicationDbContext())
        //        {
        //            var UserManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(context));
        //            var checkUser = UserManager.FindByName(model.email);
        //            if (checkUser != null)
        //                return getMessage(Enums.MStatus.check, "البريد الالكتروني مستخدم مسبقاً", "Create", "Users");
        //            var user = new ApplicationUser
        //            {
        //                cdate = DateTime.Now,
        //                Email = model.email,
        //                fullname = model.std_name,
        //                status = 0,
        //                UserName = model.email,
        //                is_complete = 0
        //            };

        //            string pass = System.Web.Security.Membership.GeneratePassword(8, 3);
        //            var x = await UserManager.CreateAsync(user, pass);
        //            if (model.invite_type == 1)
        //                await UserManager.AddToRoleAsync(user.Id, "Teacher");
        //            var mail = new emailInviteT()
        //            {
        //                cdate = DateTime.Now + "",
        //                from = User.Identity.Name,
        //                to = model.email,
        //                title = "دعوة الانضمام الى تطبيق درس خصوصي",
        //                msg = "لقد تمت دعوتك للانضمام الى تطبيق درس خصوصي من قبل المعلم " + model.email + "يمكنك الآن الانضمام الى التطبيق وتسجيل بياناتك من خلال الروابط المرفقة الخاصة بتطبيق الأيفون والأندرويد",
        //                username = model.email,
        //                password = pass
        //            };
        //            new MailerController().InviteT(mail).Deliver();

        //        }
        //    }

        //    return getMessage(Enums.MStatus.check, "", "Index", "Invites");
        //}

        public ActionResult deactive(string id)
        {

            _mem.deactive(id);

            return getMessage(Enums.MStatus.check, "", "Index", "Teachers");

        }
        public ActionResult Active(string id)
        {
            //var checkUser = db.AspNetUsers.Where(w => w.Id == id).FirstOrDefault();
            //if (checkUser != null)
            //{
            //    checkUser.status = 1;
            //    checkUser.absher = 1;
            //    db.Entry(checkUser).State = EntityState.Modified;
            //    db.SaveChanges();
            //TeacherWiziQ tw = new TeacherWiziQ()
            //{
            //    email = checkUser.Email,
            //    name = checkUser.fullname,
            //    about_the_teacher = checkUser.details,
            //    password = "123456"
            //};
            //var x = WiziQ.WizIQClass.AddTeacher(tw);
            //if (!string.IsNullOrEmpty(x) && x.Substring(0, 20).Contains("ok"))
            //{
            //    rsp resultObject = Functions.DeserializeXML<rsp>(x);
            //    checkUser.wiziq_id = resultObject.add_teacher.teacher_id + "";
            //    db.Entry(checkUser).State = EntityState.Modified;
            //    db.SaveChanges();
            //}
            //using (System.IO.StreamWriter file = new System.IO.StreamWriter(Server.MapPath("~/debug.txt"), true))
            //    file.WriteLine("/n" + x);

            // }
            _teach.ActiveTeacher(id);
            return getMessage(Enums.MStatus.check, "", "Index", "Teachers");



        }

        public ActionResult Delete(string id)
        {
            _teach.DeleteTeacher(id);


            return getMessage(Enums.MStatus.check, "", "Index", "Teachers");

        }

    }
}