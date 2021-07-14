
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

namespace PrivateLessonMS.Areas.Teacher.Controllers
{
    ////[Authorize(Roles = "2")]

    public class RequestsController : BaseController
    {
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

            var items = _courseBll.getcoursesBasic( user_id, null,null,null,null).Select(s => new NewRequestList()
            {
                cdate = s.cdate + "",
                id = s.Id,
                //period = s.period,
                start_date = s.StartDate,
                start_time = s.cdate.HasValue ? s.cdate.Value.TimeOfDay : DateTime.Now.TimeOfDay,
                status = s.status,
                student_name = s.StudentsFullName,
                //details = s.details,
                teaching_mechanism = s.TeachingMechanism,
                //material = s.ma
            }).OrderByDescending(o => o.id).ToList();
            return View(items);

        }

        public ActionResult Students(string name, int? course_id, string email, string mobile)
        {

            var user_id = Functions.GetUser().id;
            var levels = _eduLevel.GetEducationLevels().AsEnumerable();
            ViewBag.courses = _courseBll.getcourses(user_id, null, null, null, null).Select(s => new keyValue()
            {
                id = s.id,
                value = s.title
            }).OrderByDescending(o => o.id).ToList();
            var item = _mem.GetUsersData(null, "1", null, course_id,null, name, email, mobile).Select(s => new TeacherListAdmin()
            {
                absher = s.absher,
                cdate = s.cdate,
                email = s.Email,
                fullname = !string.IsNullOrEmpty(s.fullname) ? s.fullname : (s.first_name + " " + s.last_name),
                id = s.Id,
                mobile = s.PhoneNumber,
                photo = s.photo,
                status = s.status,
                //specialization = levels.FirstOrDefault(w => w.id + "" == s.education_level_text) != null ? levels.FirstOrDefault(w => w.id + "" == s.education_level_text).name : "---",
               // course_ids = s.StudentCourses.Where(m => m.course_id.HasValue).Select(x => x.course_id.Value).ToList()
            }).OrderByDescending(o => o.cdate).ToList();


            return View(item);

        }

        public ActionResult Edit(int? id)
        {

           
            var _course = _courseBll.getcoursesBasic(null, null, id, null, null).ToList();
            
            return View(_course);

        }

        public ActionResult StudentInfo(string id)
        {
            var user_id = Functions.GetUser().id;

            var student = _stu.Student_GetByUserId(id);
            var _course = _courseBll.GetStudentCourses(student.id,null,null).Select(s => new keyValue() { id = s.RequestId, value = s.Subject }).ToList();
            ViewBag.course = _course;
            return PartialView(student);

        }

        [HttpPost]
        public ActionResult Edit(EditCourse model)
        {



            _newReq.UpdateRequest(model);
            //try
            //    {
            //        var fcm = db.AspNetUsers.Where(w => w.Id == _request.std_id).AsEnumerable().Select(s => s.fcm).ToList();
            //        if (fcm.Count() > 0)
            //        {
            //            string msg = model.status == 1 ? "لقد تم الموافقة على طلبك من قبل المعلم يرجى قراءة العقد المرسل والموافقة عليه" : "لقد تم رفض طلبك من المعلم";
            //            Functions.SendNotification(fcm, msg, "token", _request.Id, 1, _request.std_id);
            //        }
            //        if (model.status == 1)
            //        {
            //            var _teacher = db.AspNetUsers.FirstOrDefault(c => c.Id == _request.teacher_id);
            //            var _std = db.AspNetUsers.FirstOrDefault(c => c.Id == _request.std_id);


            //            string atten_url = "";
            //            string wiziqId = Functions.CreateWiziqTest(_request);
            //            using (System.IO.StreamWriter file = new System.IO.StreamWriter(System.Web.HttpContext.Current.Server.MapPath("~/debug.txt"), true))
            //                file.WriteLine("wiziqId : " + wiziqId);
            //            if (!string.IsNullOrEmpty(wiziqId))
            //            {
            //                atten_url = Functions.AddStudentToWiziqTest(_request, wiziqId);
            //                using (System.IO.StreamWriter file = new System.IO.StreamWriter(System.Web.HttpContext.Current.Server.MapPath("~/debug.txt"), true))
            //                    file.WriteLine("atten_url : " + atten_url);
            //            }

            //            EmailContract mail = new EmailContract()
            //            {
            //                from = string.IsNullOrEmpty(_teacher.fullname) ? (_teacher.first_name + " " + _teacher.last_name) : _teacher.fullname,
            //                material = _request.material,
            //                details = _request.details,
            //                period = _request.period == 1 ? "يومي" : (_request.period == 7 ? "اسبوعي" : "شهري"),
            //                start_date = _request.start_date,
            //                start_time = _request.start_time,
            //                teaching_mechanism = _request.teaching_mechanism,
            //                is_test = _request.is_test == 1 ? "يحتاج اتصال تجريبي " : "بدون اتصال تجريبي",
            //                cost = _request.teaching_mechanism == "online" ? _teacher.online_cost : _teacher.site_cost,
            //                title = " درس خصوصي",
            //                to = _std.Email,
            //                msg = _request.teacher_notes,
            //                cdate = string.Format("{0:MMM/dd/yyyy}", DateTime.Now),
            //                status = 1

            //            };
            //            new Mhana.Controllers.MailerController().EmailRequest(mail).Deliver();
            //        }
            //    }
            //    catch (Exception ex)
            //    {

            //    }


            return getMessage(Enums.MStatus.check, "", "Index", "Requests");


        }
        //public ActionResult Invites(int? id)
        //{
        //    var user_id = Functions.GetUser().id;

        //    var c = _invite.GetInvite(user_id).OrderByDescending(o => o.id).ToList();
        //    return View(c);

        //}

        //public ActionResult Invites_Std()
        //{
        //    var user_id = Functions.GetUser().id;

        //    ViewBag.Courses = _courseBll.getcourses(user_id, null, null, null, 1).Select(s => new keyValue() { id = s.id, value = s.title }).ToList();
        //    return View();


        //}
        //[HttpPost]
        //public async Task<ActionResult> Invites_Std(Invite model, int? type)
        //{
        //    var user_id = Functions.GetUser().id;



        //    try
        //    {
        //        using (var context = new ApplicationDbContext())
        //        {
        //            var UserManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(context));
        //            var checkUser = UserManager.FindByName(model.email);
        //            var owner_user = UserManager.FindByName(user_id);
        //            if (checkUser == null)
        //            {
        //                var user = new ApplicationUser
        //                {
        //                    cdate = DateTime.Now,
        //                    Email = model.email,
        //                    fullname = "",
        //                    status = 0,
        //                    UserName = model.email,
        //                    is_complete = 0
        //                };

        //                string pass = System.Web.Security.Membership.GeneratePassword(8, 3);

        //                var x = UserManager.Create(user, pass);

        //                UserManager.AddToRole(user.Id, "Student");

        //                emailInviteS mail = new emailInviteS()
        //                {
        //                    from = Functions.GetUser().fullname,
        //                    msg = "لقد تم دعوتك من قبل الطالب " + Functions.GetUser().fullname + "للمشاركة في درس في تطبيق درس خصوصي نتمن القيام بتحميل التطبيق الخاص بدرس خصوصي من الروابط التالية  ",
        //                    title = "دعوة طالب للمشاركة في درس خصوصي",
        //                    password = pass,
        //                    username = model.email,
        //                    to = model.email,
        //                    cdate = string.Format("{0:MMM/dd/yyyy}", DateTime.Now)
        //                };
        //                new MailerController().InviteS(mail).Deliver();
        //            }

        //        }




        //        var __course = db.Course_Times.Where(w => w.course_id == model.course_id).OrderByDescending(o => o.id).FirstOrDefault();
        //        if (!model.course_id.HasValue || string.IsNullOrEmpty(model.email))
        //            return getMessage(Enums.MStatus.remove, "هنالك خطأ في البيانات", "Invites_Std", "Requests");
        //        var invited_user = _mem.GetUsers(null, null, null, null, null, model.email, null, null).FirstOrDefault();

        //        if (invited_user != null)
        //        {
        //            var check_course_user = _courseBll.GetStudentCourses(invited_user.Id, model.course_id, null).FirstOrDefault();
        //            if (check_course_user != null)
        //                return getMessage(Enums.MStatus.remove, "هذا الحساب مسجل مسبقاً في هذا الدرس", "Invites_Std", "Requests");

        //            StudentCourse sc = new StudentCourse()
        //            {
        //                cdate = DateTime.Now,
        //                course_id = model.course_id,
        //                status = 1,
        //                std_id = invited_user.Id,
        //            };



        //            Invite e = new Invite()
        //            {
        //                cdate = DateTime.Now,
        //                send_by = Functions.GetUser().id,
        //                status = 0,
        //                invite_type = 0,//0 for student without reg 1 for student with reg
        //                std_name = model.std_name,
        //                email = model.email
        //            };
        //            _invite.InsertInvite(e);


        //            emailInviteS mail = new emailInviteS()
        //            {
        //                from = Functions.GetUser().fullname,
        //                msg = "لقد تم دعوتك من قبل المعلم " + Functions.GetUser().fullname + "للمشاركة في درس في تطبيق درس خصوصي نتمنى القيام بتحميل التطبيق الخاص بدرس خصوصي من الروابط التالية  ",
        //                title = "دعوة طالب للمشاركة في درس خصوصي",
        //                password = "",
        //                username = "",
        //                to = model.email,
        //                cdate = string.Format("{0:MMM/dd/yyyy}", DateTime.Now)
        //            };
        //            new MailerController().InviteS(mail).Deliver();
        //            //if (!string.IsNullOrEmpty(__course.wiziq_id))
        //            //{
        //            //    string xml_attendance = "";
        //            //    xml_attendance += "<attendee><attendee_id>" + invited_user.Id + "</attendee_id><screen_name>" + model.email + "</screen_name><language_culture_name>ar-sa</language_culture_name></attendee>";
        //            //    var std_response = WiziQ.WizIQClass.AddAttendees(__course.wiziq_id, xml_attendance);
        //            //    string atten_url = "";
        //            //    if (!string.IsNullOrEmpty(std_response) && std_response.Substring(0, 20).Contains("ok"))
        //            //    {
        //            //        rsp resultObject = Functions.DeserializeXML<rsp>(std_response);
        //            //        foreach (var item in resultObject.add_attendees.attendee_list)
        //            //        {
        //            //            // var std_time = sc;
        //            //            if (sc != null)
        //            //            {
        //            //                atten_url = item.attendee_url;
        //            //                sc.lesson_link = item.attendee_url;
        //            //            }

        //            //        }
        //            //    }
        //            //}
        //            _courseBll.InsertStudentCourde(sc);
        //        }

        //        return getMessage(Enums.MStatus.check, "", "Invites", "Requests");


        //    }
        //    catch (Exception EX)
        //    {
        //        using (System.IO.StreamWriter file = new System.IO.StreamWriter(System.Web.HttpContext.Current.Server.MapPath("~/debug.txt"), true))
        //            file.WriteLine("/n" + EX.ToString());
        //        return getMessage(Enums.MStatus.remove, "هنالك خطأ في البيانات", "Invites_Teacher", "Requests");
        //    }

        //}



        //public ActionResult Invites_Teacher(int? id)
        //{

        //    var user_id = Functions.GetUser().id;
        //    ViewBag.Courses = _courseBll.getcourses(user_id, null, null, null, 1).Select(s => new keyValue() { id = s.id, value = s.title }).ToList();
        //    return View();

        //}
        //[HttpPost]
        //public async Task<ActionResult> Invites_Teacher(Invite model, int? type)
        //{
        //    var user_id = Functions.GetUser().id;


        //    var __course = db.Course_Times.Where(w => w.course_id == model.course_id).OrderByDescending(o => o.id).FirstOrDefault();

        //    model.cdate = DateTime.Now;
        //    model.send_by = user_id;
        //    model.status = 0;
        //    model.invite_type = 1; //0 for student without reg 1 for student with reg
        //    _invite.InsertInvite(model);
        //    //if (type != 1)
        //    //{

        //    string atten_url = "";
        //    //if (!string.IsNullOrEmpty(__course.wiziq_id))
        //    //{
        //    //    string xml_attendance = "";
        //    //    xml_attendance += "<attendee><attendee_id>" + Guid.NewGuid().ToString() + "</attendee_id><screen_name>" + model.email + "</screen_name><language_culture_name>ar-sa</language_culture_name></attendee>";
        //    //    var std_response = WiziQ.WizIQClass.AddAttendees(__course.wiziq_id, xml_attendance);

        //    //    if (!string.IsNullOrEmpty(std_response) && std_response.Substring(0, 20).Contains("ok"))
        //    //    {
        //    //        rsp resultObject = Functions.DeserializeXML<rsp>(std_response);
        //    //        foreach (var item in resultObject.add_attendees.attendee_list)
        //    //        {
        //    //            // var std_time = sc;
        //    //            //if (sc != null)
        //    //            //{
        //    //            atten_url = item.attendee_url;
        //    //            // sc.lesson_link = item.attendee_url;
        //    //            // }

        //    //        }
        //    //    }
        //    //}

        //    var mail = new emailInviteT()
        //    {
        //        cdate = DateTime.Now + "",
        //        from = Functions.GetUser().fullname,
        //        to = model.email,
        //        title = "دعوة الانضمام الى تطبيق درس خصوصي",
        //        msg = "لقد تمت دعوتك للانضمام الى تطبيق درس خصوصي من قبل المعلم " + model.email + " يمكنك الآن الانضمام الى التطبيق وتسجيل بياناتك من خلال الروابط المرفقة الخاصة بتطبيق الأيفون والأندرويد  </br> للدخول الى الدرس يمكنك الضغط على الرابط التالي " + atten_url
        //    };
        //    new MailerController().InviteT(mail).Deliver();

        //    //}

        //    //if (type == 1)
        //    //{
        //    //    using (var context = new ApplicationDbContext())
        //    //    {
        //    //        var UserManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(context));
        //    //        var checkUser = UserManager.FindByName(model.email);


        //    //        if (!string.IsNullOrEmpty(__course.wiziq_id))
        //    //        {
        //    //            string xml_attendance = "";
        //    //            xml_attendance += "<attendee><attendee_id>" + invited_user.Id + "</attendee_id><screen_name>" + model.email + "</screen_name><language_culture_name>ar-sa</language_culture_name></attendee>";
        //    //            var std_response = WiziQ.WizIQClass.AddAttendees(__course.wiziq_id, xml_attendance);
        //    //            string atten_url = "";
        //    //            if (!string.IsNullOrEmpty(std_response) && std_response.Substring(0, 20).Contains("ok"))
        //    //            {
        //    //                rsp resultObject = Functions.DeserializeXML<rsp>(std_response);
        //    //                foreach (var item in resultObject.add_attendees.attendee_list)
        //    //                {
        //    //                    // var std_time = sc;
        //    //                    if (sc != null)
        //    //                    {
        //    //                        atten_url = item.attendee_url;
        //    //                        sc.lesson_link = item.attendee_url;
        //    //                    }

        //    //                }
        //    //            }
        //    //        }


        //    //        if (checkUser != null)
        //    //            return getMessage(Enums.MStatus.check, "البريد الالكتروني مستخدم مسبقاً", "Invites_Teacher", "Requests");
        //    //        var user = new ApplicationUser
        //    //        {
        //    //            cdate = DateTime.Now,
        //    //            Email = model.email,
        //    //            fullname = model.std_name,
        //    //            status = 0,
        //    //            UserName = model.email,
        //    //            is_complete = 0
        //    //        };

        //    //        string pass =  System.Web.Security.Membership.GeneratePassword(8, 3);
        //    //        var x = await UserManager.CreateAsync(user, pass);
        //    //        if (model.invite_type == 1)
        //    //            await UserManager.AddToRoleAsync(user.Id, "Teacher");
        //    //        var mail = new emailInviteT()
        //    //        {
        //    //            cdate = DateTime.Now + "",
        //    //            from = User.Identity.Name,
        //    //            to = model.email,
        //    //            title = "دعوة الانضمام الى تطبيق درس خصوصي",
        //    //            msg = "لقد تمت دعوتك للانضمام الى تطبيق درس خصوصي من قبل المعلم " + model.email + "يمكنك الآن الانضمام الى التطبيق وتسجيل بياناتك من خلال الروابط المرفقة الخاصة بتطبيق الأيفون والأندرويد",
        //    //            username = model.email,
        //    //            password = pass
        //    //        };
        //    //        new Mhana.Controllers.MailerController().InviteT(mail).Deliver();

        //    //    }
        //    //}

        //    return getMessage(Enums.MStatus.check, "", "Invites", "Requests");
        //}


        //public ActionResult AddStdToCourse()
        //{
        //    var user_id = Functions.GetUser().id;
        //    ViewBag.stds = _mem.GetUsers(null, "3", null, null).Where(w => w.status != -1).Select(s => new keyValue2() { id = s.Id, value = s.fullname + "( " + s.Email + " )" }).ToList();
        //    ViewBag.Courses = _courseBll.getcourses(user_id, null, null, null, 1).Select(s => new keyValue() { id = s.id, value = s.title }).ToList();
        //    return View();

        //}

        //[HttpPost]
        //public ActionResult AddStdToCourse(int? Course_id, string std_id)
        //{
        //    var user_id = Functions.GetUser().id;

            
        //        var iii = _courseBll.GetStudentCourses(std_id,Course_id,null).FirstOrDefault();
        //        if (iii != null)
        //            return getMessage(Enums.MStatus.remove, "هذا الطالب منضم مسبقاً لهذا الدرس", "AddStdToCourse", "Requests");
        //        var user = _mem.GetUsers(std_id,null,null,null).FirstOrDefault();
        //        var t_course = db.Course_Times.Where(w => w.course_id == Course_id).OrderByDescending(o => o.id).FirstOrDefault();
        //        var std_c = new StudentCourse
                
                
        //        ()
        //        {
        //            course_id = Course_id,
        //            std_id = std_id,
        //            cdate = DateTime.Now,
        //            status = 1,
        //            cost = 50,
        //            invited_by = user_id,
        //        };
        //    //    Invite ii = new Invite()
        //    //    {
        //    //        cdate = DateTime.Now,
        //    //        course_id = Course_id,
        //    //        status = 1,
        //    //        std_name = Functions.GetUser().fullname,
        //    //        invite_type = 0

        //    //    };
        //    //_invite.InsertInvite(ii);


        //    //if (!string.IsNullOrEmpty(t_course.wiziq_id))
        //    //{
        //    //    string xml_attendance = "";
        //    //    xml_attendance += "<attendee><attendee_id>" + user.Id + "</attendee_id><screen_name>" + user.Email + "</screen_name><language_culture_name>ar-sa</language_culture_name></attendee>";
        //    //    var std_response = WiziQ.WizIQClass.AddAttendees(t_course.wiziq_id, xml_attendance);
        //    //    string atten_url = "";
        //    //    if (!string.IsNullOrEmpty(std_response) && std_response.Substring(0, 20).Contains("ok"))
        //    //    {
        //    //        rsp resultObject = Functions.DeserializeXML<rsp>(std_response);
        //    //        foreach (var item in resultObject.add_attendees.attendee_list)
        //    //        {
        //    //            // var std_time = sc;
        //    //            if (std_c != null)
        //    //            {
        //    //                atten_url = item.attendee_url;
        //    //                std_c.lesson_link = item.attendee_url;
        //    //            }

        //    //        }
        //    //    }
        //    //}
        //    _courseBll.InsertStudentCourde(std_c);
        //        //emailInviteS mail = new emailInviteS()
        //        //{
        //        //    from = Functions.GetUser().fullname,
        //        //    msg = "لقد تم دعوتك من قبل المعلم " + Functions.GetUser().fullname + "للمشاركة في درس في تطبيق درس خصوصي نتمن القيام بتحميل التطبيق الخاص بدرس خصوصي من الروابط التالية  ",
        //        //    title = "دعوة طالب للمشاركة في درس خصوصي",
        //        //    password = "",
        //        //    username = "",
        //        //    to = user.Email,
        //        //    cdate = string.Format("{0:MMM/dd/yyyy}", DateTime.Now)
        //        //};
        //        //new MailerController().InviteS(mail).Deliver();
            
        //    return getMessage(Enums.MStatus.check, "", "AddStdToCourse", "Requests");
        //}
    }
}