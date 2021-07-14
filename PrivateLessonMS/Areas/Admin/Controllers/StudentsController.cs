
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
    public class StudentsController : BaseController
    {
        // GET: Admin/Students
        EducationLevelBLL _educationLevel = new EducationLevelBLL();
        StudentNewRequestBLL _stNewReq = new StudentNewRequestBLL();
        Membership _mem = new Membership();
        InvitesBLL _inviteBll = new InvitesBLL();
        StudentBLL _stu = new StudentBLL();
        RatingBLL _rateBll = new RatingBLL();
        CourseBLL _cour = new CourseBLL();
        InvitesBLL _invite = new InvitesBLL();
        TeacherBLL _teaBll = new TeacherBLL();
        public ActionResult Index(string name, int? course_id, string email, string mobile)
        {

            var levels = _educationLevel.GetEducationLevels().AsEnumerable();

            //var item = _mem.GetUsers(null, "1", null, null, name, email, mobile, course_id).Select(s => new TeacherListAdmin()
            //{
            //    absher = s.absher,
            //    cdate = s.cdate,
            //    email = s.Email,
            //    fullname = !string.IsNullOrEmpty(s.fullname) ? s.fullname : (s.first_name + " " + s.last_name),
            //    id = s.Id,
            //    mobile = s.PhoneNumber,
            //    photo = s.photo,
            //    status = s.status,
            //    specialization = levels.FirstOrDefault(w => w.id + "" == s.education_level_text) != null ? levels.FirstOrDefault(w => w.id + "" == s.education_level_text).name : "---"

            //});

            var item = _mem.GetUsers(null, "1", null, null, name, email, mobile).Select(s => new TeacherListAdmin()
            {
                absher = s.absher,
                cdate = s.cdate,
                email = s.Email,
                fullname = !string.IsNullOrEmpty(s.fullname) ? s.fullname : (s.first_name + " " + s.last_name),
                id = s.Id,
                mobile = s.PhoneNumber,
                photo = s.photo,
                status = s.status,
                specialization = levels.FirstOrDefault(w => w.id + "" == s.education_level_text) != null ? levels.FirstOrDefault(w => w.id + "" == s.education_level_text).name : "---"

            });


            return View(item.OrderByDescending(o => o.cdate).ToList());

        }

        public ActionResult Edit(string id)
        {

            var student = _stu.Student_GetByUserId(id);
            var item = _mem.GetUsers(id, null, null, null).FirstOrDefault();
            if (item == null)
                return getMessage(Enums.MStatus.check, "", "Index", "Students");
            ViewBag.Courses = _cour.getcourses(null, id, null, null, null).Select(s => new CourseList()
            {
                id = s.id,
                cdate = s.cdate + "",
                status = s.status,
                teacher_name = s.teacher_name,
                title = s.title,
                teacher_id = s.teacher_id,


            }).OrderByDescending(o => o.cdate).ToList();

            //ViewBag.levels = db.education_level.ToList();
            if (student.educationLevel != null)
            {
                var levels = student.educationLevel.Select(c => c.name).ToList();
                if (levels.Count() > 0)
                    ViewBag.eduLevels = string.Join(",", levels);
                else
                    ViewBag.eduLevels = "---";
            }
            return View(item);

        }
        public async Task<ActionResult> Requests()
        {

            var items = _cour.getcourses(null, null, null, null, 0).Select(s => new NewRequestList()
            {
                cdate = s.cdate + "",
                id = s.id,
                //period = s.period,
                start_date = s.start_date,
                start_time = s.start_date.HasValue ? s.start_date.Value.TimeOfDay : DateTime.Now.TimeOfDay,
                status = s.status,
                student_name = s.student_name,
                //details = s.,
                // teaching_mechanism = s.teaching_mechanism,
                // material = s.material
            }).OrderByDescending(o => o.id).ToList();
            return View(items);

        }
        public ActionResult EditRequest(int? id)
        {

            var c = _cour.getcoursesBasic(null, null, id, null, null);
            ViewBag.dates = c != null ? _teaBll.GetTeacherAvailabilty(c.FirstOrDefault().TeacherId) : null;
            return View(c);

        }
        [HttpPost]
        public ActionResult EditRequest(EditCourse model)
        {

            _stNewReq.UpdateRequest(model);
            return getMessage(Enums.MStatus.check, "", "Requests", "Students");

        }
        public ActionResult deactive(string id)
        {
            using (var context = new ApplicationDbContext())
            {
                var UserManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(context));
                var checkUser = UserManager.FindById(id);
                //if (checkUser != null)
                //    return getMessage(Enums.MStatus.danger, "البريد الالكتروني مستخدم مسبقاً", "Create", "Manager");
                checkUser.status = -1;
                UserManager.Update(checkUser);
                return getMessage(Enums.MStatus.check, "", "Index", "Students");
            }

        }

        //public ActionResult deactiveRequest(int? id)
        //{


        //    _stNewReq.deactiveRequest(id);

        //    //var fcm = db.AspNetUsers.Where(w => w.Id == item.std_id).AsEnumerable().Select(s => s.fcm).ToList();
        //    //if (fcm.Count() > 0)
        //    //{
        //    //    string msg = "لقد تم رفض طلبك الذي قمت بارساله";
        //    //    Functions.SendNotification(fcm, msg, "token", item.Id, 1, item.std_id);
        //    //}
        //    return getMessage(Enums.MStatus.check, "", "Index", "Students");

        //}

        public ActionResult DeleteRate(int? id)
        {

            _rateBll.DeleteRate(id);
            return getMessage(Enums.MStatus.check, "", "Rates", "Students");

        }


        public ActionResult Active(string id)
        {

            using (var context = new ApplicationDbContext())
            {
                var UserManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(context));
                var checkUser = UserManager.FindById(id);
                //if (checkUser != null)
                //    return getMessage(Enums.MStatus.danger, "البريد الالكتروني مستخدم مسبقاً", "Create", "Manager");
                checkUser.status = 1;
                UserManager.Update(checkUser);
                return getMessage(Enums.MStatus.check, "", "Index", "Students");
            }
        }
        public ActionResult RemoveFromCourse(int? id, int std)
        {


            _cour.UpdateCourseStatus(std, id, 2);
            return getMessage(Enums.MStatus.check, "", "Edit", "Students", std);

        }
        public ActionResult AcceptInCourse(int? id, int std)
        {


            _cour.UpdateCourseStatus(std, id, 1);
            return getMessage(Enums.MStatus.check, "", "Edit", "Students", std);

        }
        public ActionResult Delete(string id)
        {

            _stu.DeleteStudent(id);
            return getMessage(Enums.MStatus.check, "", "Index", "Students");

        }
        public ActionResult Rates()
        {


            var items = _rateBll.GetRateType(1).Select(s => new RateModel()
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

        //public ActionResult Invites(int? id)
        //{
        //    var user_id = Functions.GetUser().id;

        //    var c = _invite.GetInvite().OrderByDescending(o => o.id).ToList();
        //    return View(c);

        //}

        //public ActionResult Invites_Std()
        //{

        //    ViewBag.Courses = _cour.getcourses().Select(s => new keyValue() { id = s.id, value = s.title }).ToList();
        //    return View();


        //}
        //[HttpPost]
        //public async Task<ActionResult> Invites_Std(Invite model, int? type)
        //{
        //    var user_id = Functions.GetUser().id;

        //    model.cdate = DateTime.Now;
        //    model.send_by = user_id;
        //    model.status = 0;
        //    model.invite_type = 0; //0 for student without reg 1 for student with reg
        //    _inviteBll.InsertInvite(model);
        //    if (type != 1)
        //    {
        //        emailInviteS mail = new emailInviteS()
        //        {
        //            from = Functions.GetUser().fullname,
        //            msg = "لقد تم دعوتك من قبل المعلم " + Functions.GetUser().fullname + "للمشاركة في درس في تطبيق درس خصوصي نتمن القيام بتحميل التطبيق الخاص بدرس خصوصي من الروابط التالية  ",
        //            title = "دعوة طالب للمشاركة في درس خصوصي",
        //            password = "",
        //            username = "",
        //            to = model.email,
        //            cdate = string.Format("{0:MMM/dd/yyyy}", DateTime.Now)
        //        };
        //        new MailerController().InviteS(mail).Deliver();
        //    }

        //    if (type == 1)
        //    {
        //        using (var context = new ApplicationDbContext())
        //        {
        //            var UserManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(context));
        //            var checkUser = UserManager.FindByName(model.email);
        //            if (checkUser != null)
        //                return getMessage(Enums.MStatus.check, "البريد الالكتروني مستخدم مسبقاً", "Invites_Std", "Students");
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
        //                await UserManager.AddToRoleAsync(user.Id, "Student");
        //            emailInviteS mail = new emailInviteS()
        //            {
        //                from = Functions.GetUser().fullname,
        //                msg = "لقد تم دعوتك من قبل الادارة " + Functions.GetUser().fullname + "للمشاركة في درس في تطبيق درس خصوصي نتمن القيام بتحميل التطبيق الخاص بدرس خصوصي من الروابط التالية  ",
        //                title = "دعوة طالب للمشاركة في درس خصوصي",
        //                password = pass,
        //                username = model.email,
        //                to = model.email,
        //                cdate = string.Format("{0:MMM/dd/yyyy}", DateTime.Now)
        //            };
        //            new MailerController().InviteS(mail).Deliver();

        //        }
        //    }

        //    return getMessage(Enums.MStatus.check, "", "Invites", "Students");
        //}
    }
}