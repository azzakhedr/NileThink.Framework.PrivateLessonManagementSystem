
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
using System.Xml;


namespace PrivateLessonMS.Areas.Teacher.Controllers
{
    //[Authorize (Roles ="2")]
    public class AccountController : BaseController
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
        // GET: Teacher/Account
        public ActionResult Index()
        {
            string user_id = Functions.GetUser().id;
            var teacData = _teach.Teacher_GetByUserId(user_id);
            List<TeacherAvailabilityVM> TT = new List<TeacherAvailabilityVM>();
            List<string> el = new List<string>();

            TT = _teach.GetTeacherAvailabilty(teacData.teacherId);//.Select(s => new TeacherTimes() { day_number = s.day_number, from = s.from_time, to = s.to_time }).ToList();
            ViewBag.specializations = _spec.GetSpecializations().Select(s => new keyValue() { id = s.id, value = s.name }).ToList();
            ViewBag.branch_specialization = _spec.GetBranchSpecializations(null).Select(s => new keyValue() { id = s.id, value = s.name }).ToList();
            ViewBag.education_level = _eduLevel.GetEducationLevels().Select(s => new keyValue() { id = s.id, value = s.name }).ToList();


            var user = _mem.GetUsers(user_id, null, null, null).FirstOrDefault();

            if (user != null)
            {
                TeacherRegisterResponseModel response = new TeacherRegisterResponseModel()
                {
                    branch_specialization = user.branch_specialization,
                    certificate = user.certificate,
                    specialization = user.specialization,
                    mobile = user.PhoneNumber,
                    first_name = user.first_name,
                    last_name = user.last_name,
                    id = user.Id,
                    certificate_photo = user.certificate_photo,
                    city = user.city,
                    region = user.region,
                    country = user.country,
                    details = user.details,
                    dob = string.Format("{0:MM/dd/yyyy}", user.dob),
                    email = user.Email,
                    expired = 14,
                    fcm = user.fcm,
                    fullname = user.fullname,
                    gender = user.gender,
                    photo = user.photo,
                    online_cost = string.IsNullOrEmpty(user.online_cost) ? 0 : Convert.ToDouble(user.online_cost),
                    site_cost = string.IsNullOrEmpty(user.site_cost) ? 0 : Convert.ToDouble(user.site_cost),
                    status = user.status,
                    teaching_mechanism = user.teaching_mechanism,
                    university = user.university,
                    teacher_times = TT,
                    education_level = !string.IsNullOrEmpty(user.education_level_text) ? user.education_level_text.Split(',').ToList() : el,
                    is_complete = user.is_complete == 1 ? true : false,
                    nid = user.absher_no,
                };
                return View(response);
            }
            return RedirectToAction("Index", "Dashboard", new { area = "Teacher" });

        }

        [HttpPost]
        public ActionResult Index(AdminTeacher model, HttpPostedFileBase photo_file, HttpPostedFileBase certificate_photo)
        {
            string user_id = Functions.GetUser().id;
            //if (!string.IsNullOrEmpty(model.photo))
            //{
            //    var ph = Functions.UploadB64File(model.photo, "~/resources/users");
            //    model.photo = ph;
            //}
            //if (!string.IsNullOrEmpty(model.certificate_photo))
            //{

            //    var ph2 = Functions.UploadB64File(model.certificate_photo, "~/resources/files");
            //    model.certificate_photo = ph2;
            //}

            var user = _teach.Teacher_GetByUserId(user_id);

            if (user != null)
            {
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


                if (certificate_photo != null)
                {
                    Tuple<bool, string> imgValidation = Functions.ValidateImage(certificate_photo);
                    if (!imgValidation.Item1)
                    {
                        TempData["Message"] = imgValidation.Item2;
                        TempData["Status"] = "danger";
                        return View();
                    }
                    string res = Functions.SaveTempFile(certificate_photo, "~/resources/files");
                    model.certificate_photo = res;
                }
                user.fullName = user.firstName + " " + user.lastName;
                user.firstName = string.IsNullOrEmpty(model.first_name) ? user.firstName : model.first_name;
                user.lastName = string.IsNullOrEmpty(model.last_name) ? user.lastName : model.last_name;
                user.gender = string.IsNullOrEmpty(model.gender) ? user.gender : model.gender;
                user.mobile = string.IsNullOrEmpty(model.mobile) ? user.mobile : model.mobile;
                user.city = string.IsNullOrEmpty(model.city) ? user.city : model.city;
                //user.re = string.IsNullOrEmpty(model.region) ? user.region : model.region;
                user.certificate = string.IsNullOrEmpty(model.certificate) ? user.certificate : model.certificate;
                user.certificatePhoto = string.IsNullOrEmpty(model.certificate_photo) ? user.certificatePhoto : model.certificate_photo;
                user.photo = model.photo;
                user.absherNo = model.nid;
                user.birthDate = !model.dob.HasValue ? user.birthDate : model.dob;
                //user.educationLevel = (model.education_level != null && model.education_level.Count() > 0) ?  model.education_level : user.educationLevel;
                //user.details = string.IsNullOrEmpty(model.details) ? user.details : model.details;
                user.collage = string.IsNullOrEmpty(model.university) ? user.collage : model.university;
                user.teachingMechanism = string.IsNullOrEmpty(model.teaching_mechanism) ? user.teachingMechanism : model.teaching_mechanism;
                user.specializationId = !model.specialization.HasValue ? user.specializationId : model.specialization.Value;
                user.branchSpecializationId = !model.branch_specialization.HasValue ? user.branchSpecializationId : model.branch_specialization.Value;
                user.isComplete = true;
                user.onlineCost = model.online_cost.HasValue ? model.online_cost : 0;
                user.siteCost = model.site_cost.HasValue ? model.site_cost : 0;


                List<TeacherAvailability> TT = new List<TeacherAvailability>();
                List<string> el = new List<string>();
                var teach_id = _teach.Teacher_GetByUserId(user_id).teacherId;
                if (model.teacher_times.Count() > 0)
                {

                    //using (var db = new MhanaDbEntities())
                    //{
                    //var removed = db.Teacher_Times.Where(w => w.user_id == user_id).ToList();
                    //foreach (var item in removed)
                    //{
                    //    db.Teacher_Times.Remove(item);
                    //}
                    foreach (var item in model.teacher_times)
                    {
                        if (!String.IsNullOrEmpty(item.dayOfWeek))
                        {
                            var _ct = new TeacherAvailability()
                            {
                                DayOfWeek = item.dayOfWeek,

                                FromTime = item.fromTime,
                                ToTime = item.toTime,
                                TeacherId = teach_id
                            };
                            TT.Add(_ct);
                        }
                    }

                    //}
                }
                _teach.UpdateTeacherAvailability(user_id, user, TT);


            }

            return getMessage(Enums.MStatus.check, "", "Index", "Account");
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
                    return RedirectToAction("login", "Home", new { area = "" });
                }
            }
            return getMessage(Enums.MStatus.remove, "هنالك خطأ في كلمة المرور الحالية", "Change_password", "Account");
        }


    }

    public class AdminTeacher : GeneralUser
    {
        public string details { get; set; }
        public string certificate { get; set; }
        public string certificate_photo { get; set; }
        public int? specialization { get; set; }
        public int? branch_specialization { get; set; }
        /// <summary>
        /// online -- site -- all
        /// </summary>
        public string teaching_mechanism { get; set; }
        public string university { get; set; }
        public List<string> education_level { get; set; }
        public double? online_cost { get; set; }
        public double? site_cost { get; set; }
        public List<TeacherAvailabilityVM> teacher_times { get; set; }

    }

}