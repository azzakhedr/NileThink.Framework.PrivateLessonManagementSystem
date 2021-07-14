using NileThink.Framework.PrivateLessonManagementSystem.BLL.BussinessLayer;
using NileThink.Framework.PrivateLessonManagementSystem.BLL.ViewModels;
using NileThink.Framework.PrivateLessonManagementSystem.Web.Models;
using PrivateLessonMS.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
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

            ViewBag.Students =_stu.GetActiveStudents();
            var complainLst = _rateBll.GetRateLst(student_id, teacher_id, course_id);
            return View(complainLst);
        }
        public ActionResult deactive(long id)
        {
            _teac.DeactiveTeacher(id);
            return getMessage(Enums.MStatus.check, "", "Index", "Students");


        }
    }
}