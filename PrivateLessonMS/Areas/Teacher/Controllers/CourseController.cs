
using NileThink.Framework.PrivateLessonManagementSystem.BLL.BussinessLayer;
using NileThink.Framework.PrivateLessonManagementSystem.BLL.ViewModels;
using NileThink.Framework.PrivateLessonManagementSystem.DAL.Models;
using PrivateLessonMS.Controllers;
using PrivateLessonMS.Helper;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Xml;

namespace PrivateLessonMS.Areas.Teacher.Controllers
{
    ////[Authorize(Roles = "2")]
    public class CourseController : BaseController
    {
        // GET: Teacher/Course

        Membership _mem = new Membership();
        TeacherBLL _teach = new TeacherBLL();

        SpecializationBLL _spec = new SpecializationBLL();
        EducationLevelBLL _eduLevel = new EducationLevelBLL();
        BankAccountBLL _bankAccount = new BankAccountBLL();
        StudentNewRequestBLL _newReq = new StudentNewRequestBLL();
        RatingBLL _rate = new RatingBLL();
        InvitesBLL _invite = new InvitesBLL();
        CourseBLL _courseBll = new CourseBLL();
        AbsharDataBLL _absharData = new AbsharDataBLL();
        // GET: Teacher/Requests
        StudentBLL _stu = new StudentBLL();
        public ActionResult Index()
        {
            var user_id = Functions.GetUser().id;

            var items = _courseBll.getcoursesBasic(user_id,null,null,null,null).AsEnumerable().Select(s => new CourseListAdmin()
            {
                cdate = s.cdate + "",
                id = s.Id,
                //lectures_count = s.lectures_count,
                period = s.period,
                //photo = s.photo,
                start_date = s.StartDate,
                start_time = s.StartDate.HasValue ? s.StartDate.Value.TimeOfDay : DateTime.Now.TimeOfDay,
                status = s.status,
                title = s.title,
                teacher_name = s.TeacherFullName,
                //total_std = s.total_std
            }).OrderByDescending(o => o.id).ToList();
            return View(items);

        }
        public ActionResult Edit(int? id)
        {

            List<sp_get_courses_Result> c = _courseBll.getcoursesBasic(null, null, id, null, null);
            return View(c);

        }
        public ActionResult EditReport(int? cid, int std)
        {
            var user_id = Functions.GetUser().id;

            var c = _rate.GetRateLst(std, cid, null).FirstOrDefault();
            var stdrate = _stu.Student_GetById(std);
            ViewBag.fullname = stdrate.fullName;
            ViewBag.email = stdrate.email;
            ViewBag.phonenumber = stdrate.mobile;
            return View(c);

        }

        [HttpPost]
        public ActionResult EditReport(Rating model)
        {
            var user_id = Functions.GetUser().id;
            var teacher = _teach.Teacher_GetByUserId(user_id);
            var _rateObj = new RatingVM()
            {
                createdDate = DateTime.Now,
                comment = model.Comment,
                courseId = model.CourseID,
                rate = model.Rate,
                studentId = model.StudentId,
                teacherId = teacher.teacherId,
                report = model.Report,
                rateType = 1
            };
            _rate.AddRating(_rateObj);
            return getMessage(Enums.MStatus.check, "", "Edit", "Course", model.CourseID);

        }


      
        public ActionResult Calendar()
        {
            var user_id = Functions.GetUser().id;
            var teac = _teach.Teacher_GetByUserId(user_id);

            var today = _courseBll.GetTeacherCourseTime(teac.teacherId, true).Select(s => new todayLesson()
            {
                id = s.LessonId,
                course_id = s.RequestId,
                course_name = s.Subject,

                sch_date = s.StartDate,
                sch_time = s.StartDate != null ? s.StartDate.Value.TimeOfDay : new TimeSpan(),

                link = s.ConferanceZoom,
                status = s.Status

            }).OrderBy(o => o.sch_date).ToList();
            ViewBag.course = _courseBll.GetTeacherCourseTime(teac.teacherId ,false).Select(s => new todayLesson()
            {
                id = s.LessonId,
                course_id = s.RequestId,
                course_name = s.Subject,
              
                sch_date = s.StartDate,
                sch_time =s.StartDate!=null? s.StartDate.Value.TimeOfDay:new TimeSpan(),
           
                link = s.ConferanceZoom,
                status = s.Status

            }).OrderBy(o => o.sch_date).ToList();
            //Course c = db.Courses.Include(i => i.Course_Times).Where(w => w.Id == id).FirstOrDefault();
            return View(today);

        }

        //public ActionResult ActiveToday(int? id)
        //{
        //    var user_id = Functions.GetUser().id;
        //    using (var db = new MhanaDbEntities())
        //    {
        //        var check_user = db.AspNetUsers.Where(w => w.Id == user_id).FirstOrDefault();

        //        //if(string.IsNullOrEmpty(check_user.wiziq_id))
        //        //{
        //        //    TeacherWiziQ Tw = new TeacherWiziQ()
        //        //    {
        //        //        about_the_teacher = check_user.details,
        //        //        can_schedule_class = "true",
        //        //        email = check_user.Email,
        //        //        is_active = "true",
        //        //        name = check_user.fullname,
        //        //        mobile_number = "",
        //        //        phone_number = "",
        //        //        password = "123456"
        //        //    };
        //        //    string result = WiziQ.WizIQClass.AddTeacher(Tw);
        //        //    using (System.IO.StreamWriter file =new System.IO.StreamWriter(Server.MapPath("~/debug.txt"), true))
        //        //        file.WriteLine(result);
        //        //    if (!string.IsNullOrEmpty(result)&& result.Substring(0,20).Contains("ok"))
        //        //    {
        //        //        rsp resultObject = Functions.DeserializeXML<rsp>(result);
        //        //        check_user.wiziq_id = resultObject.add_teacher.teacher_id + "";
        //        //        db.Entry(check_user).State = EntityState.Modified;
        //        //        db.SaveChanges();
        //        //    }                   
        //        //}
        //        var wiziq_id = db.WiziqIDs.Where(w => w.status == 0).FirstOrDefault();
        //        var course_time = db.Course_Times.Where(w => w.id == id).FirstOrDefault();
        //        var _course = course_time.Course;

        //        string atten_url = "";
        //        string wiziqId = Functions.CreateWiziq(_course, course_time);
        //        using (System.IO.StreamWriter file = new System.IO.StreamWriter(System.Web.HttpContext.Current.Server.MapPath("~/debug.txt"), true))
        //            file.WriteLine("wiziqId : " + wiziqId);
        //        if (!string.IsNullOrEmpty(wiziqId))
        //        {
        //            atten_url = Functions.AddStudentToWiziq(_course, wiziqId);
        //            using (System.IO.StreamWriter file = new System.IO.StreamWriter(System.Web.HttpContext.Current.Server.MapPath("~/debug.txt"), true))
        //                file.WriteLine("atten_url : " + atten_url);
        //        }
        //        else
        //        {
        //            return getMessage(Enums.MStatus.remove, "هنالك خطأ في البيانات", "Calendar", "Course");                 
        //        }


        //       // if (string.IsNullOrEmpty(wiziq_id.wiziq_id))
        //       //     {

        //       //     CourseWiziQ Cw = new CourseWiziQ()
        //       //     {
        //       //         attendee_limit = 2 + "",
        //       //         create_recording = "true",
        //       //         duration = _course.is_test == 1 ? "10" : "60",
        //       //         start_time = course_time.sch_date.Value.Month + "/" + course_time.sch_date.Value.Day + "/" + course_time.sch_date.Value.Year + " " + course_time.sch_date.Value.AddHours(1).Hour + ":" + course_time.sch_date.Value.Minute,
        //       //         title = _course.title + course_time.id,
        //       //         //presenter_email = "mohanna111@gmail.com",
        //       //          presenter_id = wiziq_id.wiziq_id,
        //       //          presenter_name = wiziq_id.email,
        //       //          language_culture_name = "ar-sa",
        //       //             presenter_default_controls = "audio, video",
        //       //             time_zone = "Asia/Riyadh"
        //       //         };
        //       //         string c_response = WiziQ.WizIQClass.Create(Cw);
        //       //         using (System.IO.StreamWriter file = new System.IO.StreamWriter(Server.MapPath("~/debug.txt"), true))
        //       //             file.WriteLine("/n"+c_response + Cw.start_time);
        //       //         if (!string.IsNullOrEmpty(c_response) && c_response.Substring(0, 20).Contains("ok"))
        //       //         {
        //       //             rsp resultObject = Functions.DeserializeXML<rsp>(c_response);
        //       //             course_time.teacher_link = resultObject.create.class_details.presenter_list.presenter.presenter_url;
        //       //             course_time.wiziq_id = resultObject.create.class_details.class_id;
        //       //             course_time.status = 1;
        //       //             _course.course_live_url = resultObject.create.class_details.presenter_list.presenter.presenter_url;
        //       //             db.Entry(course_time).State = EntityState.Modified;
        //       //             db.Entry(_course).State = EntityState.Modified;
        //       //             db.SaveChanges();
        //       //         }
        //       //     }
        //       // var stds_reg = db.Std_Course.Where(w => w.course_id == _course.Id && w.status == 1).ToList();
        //       // var stds = stds_reg.Where(w => w.course_id == _course.Id && w.status == 1).Select(s => new atten_info()
        //       // {
        //       //     lang = "ar-sa",
        //       //     name = string.IsNullOrEmpty(s.AspNetUser.fullname)?(s.AspNetUser.Email): s.AspNetUser.fullname,
        //       //     user_id = s.std_id
        //       // }).ToList();
        //       // string xml_attendance = "";
        //       // foreach(var item in stds)
        //       // {
        //       //     xml_attendance += "<attendee><attendee_id>" + item.user_id + "</attendee_id><screen_name>" + item.name + "</screen_name><language_culture_name>" + item.lang + "</language_culture_name></attendee>";
        //       // }
        //       //var std_response =  WiziQ.WizIQClass.AddAttendees(course_time.wiziq_id, xml_attendance);
        //       // using (System.IO.StreamWriter file = new System.IO.StreamWriter(Server.MapPath("~/debug.txt"), true))
        //       //     file.WriteLine("/n" + std_response);
        //       // if (!string.IsNullOrEmpty(std_response) && std_response.Substring(0, 20).Contains("ok"))
        //       // {
        //       //     rsp resultObject = Functions.DeserializeXML<rsp>(std_response);
        //       //     foreach (var item in resultObject.add_attendees.attendee_list)
        //       //     {
        //       //         var std_time = stds_reg.Where(w => w.std_id == item.attendee_id).FirstOrDefault();
        //       //         if (std_time != null)
        //       //         {
        //       //             std_time.lesson_link = item.attendee_url;
        //       //             db.Entry(std_time).State = EntityState.Modified;

        //       //         }

        //       //     }
        //       //     db.SaveChanges();
        //       // }
        //        return getMessage(Enums.MStatus.check, "", "Calendar", "Course");
        //    }
        //}

        //public ActionResult CancelToday(int? id)
        //{
        //    var user_id = Functions.GetUser().id;

        //    var check_course = db.Course_Times.Where(w => w.id == id).FirstOrDefault();
        //    var cid = check_course.course_id;
        //    if (check_course != null && check_course.Course.teacher_id == user_id)
        //    {

        //        string result = WiziQ.WizIQClass.Cancel(check_course.wiziq_id);
        //        //using (System.IO.StreamWriter file = new System.IO.StreamWriter(Server.MapPath("~/debug.txt"), true))
        //        //    file.WriteLine(result);
        //        //check_course.status = -1;
        //        //check_course.teacher_link = "";
        //        db.Course_Times.Remove(check_course);
        //        db.SaveChanges();

        //    }
        //    return getMessage(Enums.MStatus.check, "", "Edit", "Course", cid);

        //}

    }

    public class courseStdModel
    {
        public string id { get; set; }
        public string fullname { get; set; }
        public string first_name { get; set; }
        public string last_name { get; set; }
        public string email { get; set; }
        public string phonenumber { get; set; }
        public DateTime? cdate { get; set; }
    }
}