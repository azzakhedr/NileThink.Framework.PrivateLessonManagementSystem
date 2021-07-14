using NileThink.Framework.PrivateLessonManagementSystem.BLL.BussinessLayer;
using NileThink.Framework.PrivateLessonManagementSystem.BLL.ViewModels;
using NileThink.Framework.PrivateLessonManagementSystem.DAL.Models;
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

    public class CoursesController : BaseController
    {
        // GET: Admin/Courses
        EducationLevelBLL _educationLevel = new EducationLevelBLL();
        StudentNewRequestBLL _stNewReq = new StudentNewRequestBLL();
        Membership _mem = new Membership();
        InvitesBLL _inviteBll = new InvitesBLL();
        StudentBLL _stu = new StudentBLL();
        RatingBLL _rateBll = new RatingBLL();
        CourseBLL _cour = new CourseBLL();
        InvitesBLL _invite = new InvitesBLL();
        TeacherBLL _teac = new TeacherBLL();
        public ActionResult Index()
        {

            var levels = _educationLevel.GetEducationLevels().AsEnumerable();
            var item = _cour.getcourses(null, null, null, null, null).Select(s => new CourseList()
            {
                cdate = s.cdate + "",
                id = s.id,
                start_date = s.start_date,
                status = s.status,

                title = s.title,
                teacher_name = s.teacher_name,
                teacher_id = s.teacher_id,
                student_name = s.student_name
            }).OrderByDescending(o => o.id).ToList();
            return View(item);

        }
        //public ActionResult Calendar()
        //{
        //    var today=_t

        //    var today = db.Course_Times.Where(w => w.sch_date.HasValue && w.sch_date.Value.Day == DateTime.Now.Day && w.sch_date.Value.Month == DateTime.Now.Month && w.sch_date.Value.Year == DateTime.Now.Year).Select(s => new todayLesson()
        //    {
        //        id = s.id,
        //        course_id = s.course_id,
        //        course_name = s.Course.title,
        //        lesson_number = "5",
        //        sch_date = s.sch_date,
        //        sch_time = s.sch_time,
        //        total_std = s.Course.total_std,
        //        link = s.teacher_link,
        //        status = s.status

        //    }).OrderBy(o => o.sch_date).ToList();
        //    var dd = DateTime.Now.AddMonths(-1);
        //    ViewBag.course = db.Course_Times.Where(w => w.sch_date.HasValue && w.sch_date >= dd).Select(s => new todayLesson()
        //    {
        //        id = s.id,
        //        course_id = s.course_id,
        //        course_name = s.Course.title,
        //        lesson_number = "5",
        //        sch_date = s.sch_date,
        //        sch_time = s.sch_time,
        //        total_std = s.Course.total_std,
        //        link = s.teacher_link,
        //        status = s.status

        //    }).OrderBy(o => o.sch_date).ToList();
        //    //Course c = db.Courses.Include(i => i.Course_Times).Where(w => w.Id == id).FirstOrDefault();
        //    return View(today);

        //}
        public ActionResult Create()
        {
            return View();
        }
//        [HttpPost]
//        public ActionResult Create(CreateCourse model, int? request_id=0)
//        {
//            RequestCourceVM Request = new RequestCourceVM() { 
            
//            comment=model.details,courceStatus="1",createdAt=DateTime.Now,pricePerHour=double.Parse(model.cost.ToString()),
//           // latitude=model.l
//requestId=request_id.Value,requestStatus=1,teacherId=model.teacher_id,teachingMechanism=model.teaching_mechanism,teachingMechanismStatus,
//            };
            
//            List<RequestCourceDates> ct = new List<RequestCourceDates>();

       
//            if (model.schedual != null)
//            {
//                foreach (var item in model.schedual)
//                {
//                    RequestCourceDates _ct = new RequestCourceDates()
//                    {
//                        startDate = item.Date.Add(item.TimeOfDay),
//                        endDate = item.Date.Add(item.TimeOfDay)

//                    };
//                    ct.Add(_ct);
//                }
//            }

//            RequestCource c = new RequestCource()
//            {
//                Subject = model.title,
//                //allow_sound = (model.live_type == "both" || model.live_type == "audio") ? true : false,
//                //allow_video = (model.live_type == "both" || model.live_type == "video") ? true : false,
//                CreatedAt = DateTime.Now,
//                PricePerHour = model.cost,
//                Comment = model.details,
//                //lectures_count = model.lectures_count,
//                //period = model.period,
//                //start_date = model.start_date,
//                //start_time = model.start_date.HasValue ? model.start_date.Value.TimeOfDay : DateTime.Now.TimeOfDay,
//                TeacherId = model.teacher_id,
//                Status = 1,
//                //te = model.teaching_mechanism,
//                //total_std = model.total_std,
//                re = ct

//            };
//            ScheduleLesson xx = new ScheduleLesson()
//            {
   
//                a = (model.live_type == "both" || model.live_type == "audio") ? true : false,
//                allow_video = (model.live_type == "both" || model.live_type == "video") ? true : false,
//                cdate = DateTime.Now,
//                cost = model.cost,
//                details = model.details,
//                lectures_count = model.lectures_count,
//                period = model.period,
//                start_date = model.start_date,
//                start_time = model.start_date.HasValue ? model.start_date.Value.TimeOfDay : DateTime.Now.TimeOfDay,
//                teacher_id = model.teacher_id,
//                status = 1,
//                te = model.teaching_mechanism,
//                total_std = model.total_std,
//                Course_Times = ct

//            };
//            db.Courses.Add(c);
//            db.SaveChanges();
//            return getMessage(Enums.MStatus.check, "", "Index", "Course");


//        }
        public ActionResult Edit(int? id)
        {

            List<sp_get_courses_Result> c = _cour.getcoursesBasic(null,null,id,null,null);
            return View(c);

        }
        [HttpPost]
        public ActionResult Edit(int? id, byte? status)
        {
            _cour.UpdateCourseStatus(null, id, status);

            return getMessage(Enums.MStatus.check, "", "Index", "Courses");


        }

        //public ActionResult deactive(int? id)
        //{


        //        Course c = db.Courses.Where(w => w.Id == id).FirstOrDefault();
        //        c.status = -1;
        //        db.Entry(c).State = EntityState.Modified;
        //        db.SaveChanges();
        //        return getMessage(Enums.MStatus.check, "", "Index", "Courses");

        //}
        public ActionResult Active(int? id)
        {


            _cour.UpdateCourseStatus(null, id, 1);

            return getMessage(Enums.MStatus.check, "", "Index", "Courses");

        }
    }
}