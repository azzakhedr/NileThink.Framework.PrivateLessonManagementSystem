
using Microsoft.AspNet.Identity;
using NileThink.Framework.PrivateLessonManagementSystem.BLL.BussinessLayer;
using PrivateLessonMS.Controllers;
using PrivateLessonMS.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace PrivateLessonMS.Areas.Teacher.Controllers
{
    ////[Authorize(Roles = "2")]

    public class DashboardController : BaseController
    {
        CourseBLL _courseBll = new CourseBLL();
        Membership _mem = new Membership();
        ScheduleLessonsBLL _schLesson = new ScheduleLessonsBLL();
        RequestCourceBLL _reqCourse = new RequestCourceBLL();
        TeacherBLL _teac = new TeacherBLL();
        RatingBLL _rateBll = new RatingBLL();
        public ActionResult Index()
        {

            var user_id = Functions.GetUser().id;
            var teac = _teac.Teacher_GetByUserId(user_id);
            //var courses = _courseBll.getcourses(user_id, null, null, null, null);
            ViewBag.courses = _courseBll.CourseData(teac.teacherId, null);
            ViewBag.online = _courseBll.CourseData(teac.teacherId, 0);
            ViewBag.onsite = _courseBll.CourseData(teac.teacherId, 1);
            ViewBag.both = _courseBll.CourseData(teac.teacherId, 2);

            var teacherId = _teac.Teacher_GetByUserId(user_id).teacherId;
            ViewBag.requests = _courseBll.RequestData(user_id, null, null, null, null).Count();
            var userData = _mem.GetUsers(user_id, "2", null, null).FirstOrDefault();
            var income = userData.total_income;
            ViewBag.payments = income.HasValue ? income : 0;

            ViewBag.education_level_text = userData.education_level_text;// db.AspNetUsers.Where(w => w.AspNetRoles.FirstOrDefault() != null && w.AspNetRoles.FirstOrDefault().Id == "3" && !string.IsNullOrEmpty(w.education_level_text)).Select(s => s.education_level_text).ToList();
            var dd = DateTime.Now.AddMonths(-1);

            ViewBag.course = _reqCourse.GetRequestDatesByTeacherId(teacherId).OrderBy(o => o.sch_date).ToList();
            return View();

        }

        public ActionResult HowWork()
        {
            return View();
        }

        public ActionResult ComplainLst()
        {
            var user_id = Functions.GetUser().id;
            //var teac = _teac.Teacher_GetByUserId(user_id);

            var teacherId = _teac.Teacher_GetByUserId(user_id).teacherId;
            var complainLst = _rateBll.GetRateLst(null, teacherId, null);
            return View(complainLst);


        }
    }
}