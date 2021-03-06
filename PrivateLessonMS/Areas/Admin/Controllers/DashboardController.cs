using Newtonsoft.Json;
using NileThink.Framework.PrivateLessonManagementSystem.BLL.BussinessLayer;
using NileThink.Framework.PrivateLessonManagementSystem.BLL.ViewModels;
using NileThink.Framework.PrivateLessonManagementSystem.DAL.Models;
using NileThink.Framework.PrivateLessonManagementSystem.Web.Models;
using PrivateLessonMS.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace PrivateLessonMS.Areas.Admin.Controllers
{
    ////[Authorize(Roles = "3")]
    public class DashboardController : BaseController
    {

        CourseBLL _courseBll = new CourseBLL();
        RatingBLL _rateBll = new RatingBLL();
        Membership _mem = new Membership();
        StudentBLL _stu = new StudentBLL();
        TeacherBLL _teac = new TeacherBLL();

        PackageBLL _pack = new PackageBLL();
        NotificationBLL _notificationBLL = new NotificationBLL();
        public ActionResult Index()
        {
            var AllCourses = _courseBll.CourseData(null, null);
            ViewBag.courses_all = AllCourses;
            ViewBag.courses = AllCourses;
            ViewBag.std = (_mem.UsersWithRoles(null, "1", null));
            ViewBag.teacher = _mem.UsersWithRoles(null, "2", null);
            ViewBag.std_active = _mem.UsersWithRoles(null, "1", 1);
            ViewBag.teacher_active = _mem.UsersWithRoles(null, "2", 1);

            ViewBag.requests = _courseBll.RequestData(null, null, null, null, null).Count();

            ViewBag.NewT = _mem.GetUsers(null, "2", 1, 0).OrderByDescending(o => o.Id).ToList();
            ViewBag.RequestData = _courseBll.getcourses(null, null, null, null, 0).OrderByDescending(o => o.id).ToList();
            return View();
        }

        public ActionResult ComplainLst(int? course_id, int? student_id, int? teacher_id)
        {
            ViewBag.Courses = _courseBll.getcourses(null, null, null, null, null).Select(s => new CourseList()
            {
                id = s.id,
                cdate = s.cdate + "",
                status = s.status,
                teacher_name = s.teacher_name,
                title = s.title,
                teacher_id = s.teacher_id,


            }).OrderByDescending(o => o.cdate).ToList();


            ViewBag.Teachers = _teac.GetActiveTeachers();

            ViewBag.Students = _stu.GetActiveStudents();
            var complainLst = _rateBll.GetRateLst(student_id, teacher_id, course_id);
            return View(complainLst);
        }
        public ActionResult deactive(long id)
        {
            _teac.DeactiveTeacher(id);
            return getMessage(Enums.MStatus.check, "", "Index", "Students");


        }

        public ActionResult Packages()
        {
            var lst = _pack.GetAllPackages(null, null);
            return View(lst);
        }

        public ActionResult Edit(int Id)
        {
            var lst = _pack.GetPackageById(Id);
            return View(lst);
        }
        [HttpPost]
        public ActionResult Edit(tbl_packages data)
        {

            _pack.UpdatePackage(data);
            return getMessage(Enums.MStatus.check, "", "Packages", "Dashboard");
        }





        public ActionResult SendNotificationFcm()
        {
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> SendNotificationFcm(int? Type, string subject, string Title)
        {

            var lst = _notificationBLL.GetUserNotificationToken( 0);

            foreach (var item in lst)
            {
                dynamic returndata = new
                {
                    user_id = item.user_id,
                    subject = subject,
                    type = 10// رسالة من لوحة التحكم
                };
                string NotificationMessage = JsonConvert.SerializeObject(returndata);



                _notificationBLL.InsertUserNotification(new NotificationVM() { details = NotificationMessage, title = Title, typeId = 10, userId = item.user_id, user_type = item.user_type });
                if (!String.IsNullOrEmpty(item.Token))
                {
                    SendNotification(returndata, item.Token, 10, Title);
                }

            }
            return getMessage(Enums.MStatus.check, "", "SendNotificationFcm", "Dashboard");
        }
    }
}