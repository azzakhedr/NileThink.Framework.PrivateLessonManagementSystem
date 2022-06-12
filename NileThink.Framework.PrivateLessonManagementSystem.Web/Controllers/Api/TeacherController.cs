using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.ModelBinding;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.OAuth;
using NileThink.Framework.PrivateLessonManagementSystem.Web.Models;
using NileThink.Framework.PrivateLessonManagementSystem.Web.Providers;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Web.Http.Description;
using NileThink.Framework.PrivateLessonManagementSystem.Web.Helper;
using NileThink.Framework.PrivateLessonManagementSystem.BLL.BussinessLayer;
using NileThink.Framework.PrivateLessonManagementSystem.BLL.ViewModels;
using System.IO;
using NileThink.Framework.PrivateLessonManagementSystem.DAL.Models;
using Newtonsoft.Json;
using PrivateLessonMS.Resources;

namespace NileThink.Framework.PrivateLessonManagementSystem.Web.Controllers.Api
{

    [RoutePrefix("api/v1/Teacher")]
    public class TeacherController : BaseController
    {
        private const string LocalLoginProvider = "Local";
        string URL = ConfigurationManager.AppSettings["URL"];
        private ApplicationUserManager _userManager;
        UsersBLL _UsersBll = new UsersBLL();
        TeacherBLL _TeacherBll = new TeacherBLL();

        StudentBLL studentBLL = new StudentBLL();
        RequestCourceBLL _RequestCource = new RequestCourceBLL();
        RatingBLL _ratingBll = new RatingBLL();
        SpecializationBLL _specializatioBLL = new SpecializationBLL();
        ScheduleLessonsBLL _Lessons = new ScheduleLessonsBLL();
        NotificationBLL _notificationBLL = new NotificationBLL();
        CommonController _comm = new CommonController();
        PackageBLL _package = new PackageBLL();
        TeacherPackageBLL _teachPckage = new TeacherPackageBLL();
        SettingBLL _settingBLL = new SettingBLL();
        MembershipPackageBLL _membershipBLL = new MembershipPackageBLL();

        public TeacherController()
        {

        }

        public TeacherController(ApplicationUserManager userManager,
                  ISecureDataFormat<AuthenticationTicket> accessTokenFormat)
        {
            UserManager = userManager;
            AccessTokenFormat = accessTokenFormat;

        }

        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? Request.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }
        public ISecureDataFormat<AuthenticationTicket> AccessTokenFormat { get; private set; }

        [AllowAnonymous]
        [Route("Login")]
        public async Task<IHttpActionResult> Login(LoginBindingModel model)
        {
            string Lang = lang;
            if (!ModelState.IsValid)
            {
                return this.ResponseOK(new ResponseViewModel(HttpStatusCode.OK, Resource.UserNamePasswordError, false, null));
            }
            var checkAccount = await UserManager.FindByNameAsync(model.userName);
            if (checkAccount == null)
            {
                var studentData = _TeacherBll.GetTeacherEmail(model.userName);
                if (studentData == null)
                {
                    return this.ResponseOK(new ResponseViewModel(HttpStatusCode.OK, Resource.NotFoundTeacher, false, null));
                }
                else
                {
                    return this.ResponseOK(new ResponseViewModel(HttpStatusCode.OK, null, true, new RegisterResponseModel()
                    {
                        // expired = 14,
                        fullName = studentData.FirstName + " " + studentData.LastName,
                        //AbsherUserId = user.Id,
                        // userId = user.Id,
                        email = studentData.Email,
                        firstName = studentData.FirstName,
                        lastName = studentData.LastName,
                        //allowNotify = user.allow_notify,
                        id = studentData.TeacherId,
                        mobile = studentData.Mobile,
                        role = "Teacher",
                        isComplete = false,
                        IsNeedActivate = true,
                        //absherNo = user.absher.HasValue ? user.absher : 0,
                        //online = true,
                        // absher_id = user.absher_no,
                        // gender = Teacher.gender,
                        // genderId = Teacher.genderId.Value ? Teacher.genderId.Value : false,
                        //photo = URL + "/resources/users/" + user.photo,
                        country = !String.IsNullOrEmpty(studentData.RegCountry) ? studentData.RegCountry : "0",
                        //requireUpdateProfileAbsher = requireUpdateProfileAbsher,
                        //EntityPerc = decimal.Parse(EntityPerc)
                    }));
                }

            }
            var user = await UserManager.FindAsync(checkAccount.UserName, model.password);
            if (user != null)
            {
                var Teacher = _TeacherBll.Teacher_GetByUserId(user.Id);
                Teacher.gender = Teacher.genderId.HasValue ? Teacher.genderId.Value == true ? Resource.Female : Resource.Male : "";
                var requireUpdateProfileAbsher = user.absher != 1 && String.IsNullOrEmpty(user.country) ? "1" : user.absher != 1 && user.country == "1" ? "2" : "0";

                var EntityPerc = _settingBLL.GetSettings().Where(c => c.key == "EntityPerc").FirstOrDefault().value;


                if (requireUpdateProfileAbsher == "2")
                {
                    return this.ResponseOK(new ResponseViewModel(HttpStatusCode.OK, null, true, new RegisterResponseModel()
                    {
                        expired = 14,
                        fullName = user.first_name + " " + Teacher.lastName,
                        AbsherUserId = user.Id,
                        userId = user.Id,
                        email = user.Email,
                        firstName = Teacher.firstName,
                        lastName = Teacher.lastName,
                        allowNotify = user.allow_notify,
                        id = Teacher.teacherId,
                        mobile = user.PhoneNumber,
                        role = "Teacher",
                        isComplete = user.is_complete == 1 ? true : false,
                        absherNo = user.absher.HasValue ? user.absher : 0,
                        online = true,
                        absher_id = user.absher_no,
                        gender = Teacher.gender,
                        genderId = Teacher.genderId.Value ? Teacher.genderId.Value : false,
                        photo = URL + "/resources/users/" + user.photo,
                        country = !String.IsNullOrEmpty(user.country) ? user.country : "0",
                        requireUpdateProfileAbsher = requireUpdateProfileAbsher,
                        EntityPerc = decimal.Parse(EntityPerc)
                    })); ;
                }
                if (user.status == -2 || user.status == -1)
                    return this.ResponseOK(new ResponseViewModel(HttpStatusCode.OK, Resource.LockedUser, false, null));

                _TeacherBll.TeacherUpdateOnlineStatus(Teacher.teacherId, true);
                if (Teacher.isActive == true)
                {

                    var tokenid = await GetToken(user);
                    return this.ResponseOK(new ResponseViewModel(HttpStatusCode.OK, null, true,
                               new RegisterResponseModel()
                               {
                                   expired = 14,
                                   fullName = user.fullname,
                                   token = tokenid,
                                   userId = user.Id,
                                   email = user.Email,
                                   firstName = Teacher.firstName,
                                   lastName = Teacher.lastName,
                                   allowNotify = user.allow_notify,
                                   id = Teacher.teacherId,
                                   mobile = user.PhoneNumber,
                                   role = "Teacher",
                                   isComplete = user.is_complete == 1 ? true : false,
                                   absherNo = user.absher.HasValue ? user.absher : 0,
                                   online = true,
                                   photo = URL + "/resources/users/" + user.photo,
                                   country = !String.IsNullOrEmpty(user.country) ? user.country : "0",
                                   absher_id = user.absher_no,
                                   requireUpdateProfileAbsher = user.absher != 1 && String.IsNullOrEmpty(user.country) ? "1" : user.absher != 1 && user.country == "1" ? "2" : "0",
                                   EntityPerc = decimal.Parse(EntityPerc)
                               }));
                }
                else
                {
                    return this.ResponseOK(new ResponseViewModel(HttpStatusCode.OK, Resource.NotActivatedTeacher, false, null));

                }

            }
            else
            {

                // config.SetActualResponseType(typeof(Core.Models.Policy),
                // "Policy", "Get");
                return this.ResponseOK(new ResponseViewModel(HttpStatusCode.OK, Resource.UserNamePasswordError, false, null));
            }


        }

        /// عرض بيانات معلم    .
        ///  /// </summary>
        [Authorize]
        [Route("GetTeacherInfo")]
        public IHttpActionResult GetTeacherInfo(int TeacherId, bool? IsTeacher)
        {
            string Lang = lang;
            //teacher
            var Teacher = _TeacherBll.Teacher_GetById(TeacherId);
            Teacher.gender = Teacher.genderId.HasValue ? Teacher.genderId == true ? Resource.Female : Resource.Male : "";
            if (Teacher != null)
            {
                ////Asp.netusers
                var user = UserManager.FindById(Teacher.userId);
                if (user != null)
                {
                    //if (user.Id!=User.Identity.GetUserId()  && !User.IsInRole("Student"))
                    //    return this.ResponseUnauthorized(new ResponseViewModel(HttpStatusCode.Unauthorized, "", false, ""));
                    if (user.status == -2 || user.status == -1)
                        return this.ResponseOK(new ResponseViewModel(HttpStatusCode.OK, Resource.LockedUser, false, null));
                    var EntityPerc = _settingBLL.GetSettings().Where(c => c.key == "EntityPerc").FirstOrDefault().value;
                    var rating = 0;
                    var rates = _ratingBll.GetTeacherRating(TeacherId);
                    if (rates.Count > 0)
                    {
                        try
                        {
                            rating = rates.Sum(x => x.rate).Value / rates.Count;
                        }
                        catch { }
                    }

                    var Education = Teacher.educationLevelTree;
                    var Availability = Teacher.teacherAvailability.Select(vm => new teacherAvailability()
                    {
                        dayOfWeek = vm.dayOfWeek,
                        fromTime = vm.fromTime,
                        // FromMinutes = vm.FromMinutes,
                        teacherId = vm.teacherId,
                        toTime = vm.toTime,
                        Type = vm.Type
                        // ToMinutes = vm.ToMinutes,


                    }).ToList();

                    var AvailabilityList = _TeacherBll.GetTeacherAvailabiltyTimes(TeacherId).ToList();
                    var material = Teacher.teacherMaterials.Select(vm => new Materials()
                    {
                        id = vm.id,
                        name = vm.name

                    }).ToList();
                    //var Specialization = new GeneralList();
                    //try
                    //{
                    //    if (Teacher.specializationId > 0)
                    //    {
                    //        Specialization = new GeneralList()
                    //        {
                    //            id = Teacher.specializationId,
                    //            name = Teacher.specializationId > 0 ? _specializatioBLL.GetSpecializationById(Teacher.specializationId).name : ""
                    //        };
                    //    }
                    //}
                    //catch
                    //{

                    //}
                    //var BranchSpecialization = new GeneralList();
                    //try
                    //{
                    //    BranchSpecialization = new GeneralList()
                    //    {
                    //        id = Teacher.branchSpecializationId,
                    //        name = _specializatioBLL.GetBranchSpecializationById(Teacher.branchSpecializationId).name


                    //    };
                    //}
                    //catch { }

                    List<TeacherPackagesVM> pack = IsTeacher == false ? null : _package.GetTeacherPackageData(Teacher.teacherId, Lang);
                    //List<TeacherPackagesVM> MembershipPack = IsTeacher == false ? null : _membershipBLL.GetTeacherMembershipPackageData(Teacher.teacherId, Lang);
                    var IsCurrentPackageData = _package.IsCurrentPackageData(TeacherId);
                    //var currentMembership = MembershipPack.Where(c => c.StartDate.Date >= DateTime.Now.Date && c.EndDate.Date <= DateTime.Now.Date && c.IsPaid == true).FirstOrDefault();
                    var teacherRes = new TeacherDetailsResponse()
                    {
                        AbsherUserId = Teacher.userId,
                        absher_no = user.absher_no,
                        city = Teacher.city,
                        streetNo = Teacher.streetNo,
                        postalCode = Teacher.postalCode,
                        absher = !string.IsNullOrEmpty(user.absher.ToString()) ? user.absher : 0,
                        birthDate = Teacher.birthDate.HasValue ? Teacher.birthDate.Value.ToString("yyyy-MM-dd") : "",
                        firstName = Teacher.firstName,
                        fullName = Teacher.fullName,
                        branchSpecialization = Teacher.branchSpecialization,
                        certificate = Teacher.certificate,
                        bio = Teacher.bio,
                        district = Teacher.district,
                        online = Teacher.online,
                        onlineCost = Teacher.onlineCost,
                        siteCost = Teacher.siteCost,
                        specialization = Teacher.specialization,
                        //  هنا بنتاكد ان لو مش مدرس و الية التدريس كانت تحتوي علي موقف و المدرس ملوش باقة فعالة للوقت الحالي متظهرش للطالب الموقع 
                        teachingMechanism = !string.IsNullOrEmpty(Teacher.teachingMechanism) ?
                        (int.Parse(Teacher.teachingMechanism) > 0 && IsTeacher == false && IsCurrentPackageData == false ? Teacher.teachingMechanism == "1" ? -1 : 0 : int.Parse(Teacher.teachingMechanism))
                        : -1,
                        email = user.Email,
                        mobile = Teacher.mobile,
                        nationalId = Teacher.nationalId,
                        gender = Teacher.gender,
                        genderId = Teacher.genderId.HasValue ? Teacher.genderId.Value : false,
                        id = Teacher.teacherId,
                        collage = Teacher.collage,
                        isComplete = user.is_complete == 1 ? true : false,
                        lastName = Teacher.lastName,
                        photo = URL + "/resources/users/" + user.photo,
                        status = user.status.HasValue ? user.status.Value : 0,
                        totalCourse = 1,
                        rating = rating,
                        //branchSpecializations = Teacher.branchSpecializationId > 0 ? BranchSpecialization : null,
                        //specializations = Teacher.specializationId > 0 ? Specialization : null,
                        educationLevelTree = Education,
                        teacherAvailability = Availability,
                        AvailableTimeLst = AvailabilityList,
                        teacherMaterials = material,
                        IsPackageAvailable = IsCurrentPackageData,
                        TeacherPacks = pack,
                        PackageLst = (IsTeacher == false ? null : _package.GetPackages(Lang)),
                        country = !String.IsNullOrEmpty(user.country) ? user.country : "0",
                        EntityPerc = decimal.Parse(EntityPerc),
                        IsOnline = Teacher.IsOnline,
                        GroupOnlineCost = Teacher.GroupOnlineCost,
                        GroupSiteCost = Teacher.GroupSiteCost,
                        Certificatephoto = URL + "/resources/users/" + Teacher.certificatePhoto,
                        //MembershipPack = MembershipPack,
                        //AllMembershipPackage=_membershipBLL.GetAllMembershipPackage(),
                        //IsCurrentMembershipPackage = MembershipPack != null && MembershipPack.Where(c => c.isCurrent == true).FirstOrDefault() != null ? true : false
                    };


                    return this.ResponseOK(new ResponseViewModel(HttpStatusCode.OK, null, true, teacherRes));

                }
                else
                {

                    // config.SetActualResponseType(typeof(Core.Models.Policy),
                    // "Policy", "Get");
                    return this.ResponseOK(new ResponseViewModel(HttpStatusCode.OK, Resource.NotFoundTeacher, false, null));
                }
            }
            return this.ResponseOK(new ResponseViewModel(HttpStatusCode.OK, Resource.NotFoundTeacher, false, null));

        }

        [Authorize]
        [Route("GetTeacherPackageInfo")]
        [HttpGet]
        public IHttpActionResult GetTeacherPackageInfo(int TeacherId)
        {
            string Lang = lang;
            //teacher
            var Teacher = _TeacherBll.Teacher_GetById(TeacherId);
            if (Teacher != null)
            {
                ////Asp.netusers
                var user = UserManager.FindById(Teacher.userId);
                if (user != null)
                {
                    //if (user.Id!=User.Identity.GetUserId()  && !User.IsInRole("Student"))
                    //    return this.ResponseUnauthorized(new ResponseViewModel(HttpStatusCode.Unauthorized, "", false, ""));
                    if (user.status == -2 || user.status == -1)
                        return this.ResponseOK(new ResponseViewModel(HttpStatusCode.OK, Resource.LockedUser, false, null));



                    List<TeacherPackagesVM> pack = _package.GetTeacherPackageData(Teacher.teacherId, Lang);
                    var teacherRes = new TeacherPackageDetailsResponse()
                    {

                        IsPackageAvailable = _package.IsCurrentPackageData(TeacherId),
                        TeacherPacks = pack,
                        PackageLst = _package.GetPackages(Lang),
                    };


                    return this.ResponseOK(new ResponseViewModel(HttpStatusCode.OK, null, true, teacherRes));

                }
                else
                {

                    // config.SetActualResponseType(typeof(Core.Models.Policy),
                    // "Policy", "Get");
                    return this.ResponseOK(new ResponseViewModel(HttpStatusCode.OK, Resource.NotFoundTeacher, false, null));
                }
            }
            return this.ResponseOK(new ResponseViewModel(HttpStatusCode.OK, Resource.NotFoundTeacher, false, null));

        }

        /// تعديل بيانات معلم    .
        ///  /// </summary>
        [Authorize]
        [Route("TeacherUpdate")]
        public IHttpActionResult TeacherUpdate(TeacherVM Teacher)
        {
            string Lang = lang;
            Teacher.teacherId = Teacher.id;
            if (!string.IsNullOrEmpty(Teacher.photo)) Teacher.photo = Path.GetFileName(Teacher.photo);
            if (!string.IsNullOrEmpty(Teacher.certificatePhoto)) Teacher.certificatePhoto = Path.GetFileName(Teacher.certificatePhoto);
            var TeacherVM = _TeacherBll.Teacher_GetById(Teacher.id);
            if (TeacherVM.userId != User.Identity.GetUserId()) return this.ResponseUnauthorized(new ResponseViewModel(HttpStatusCode.Unauthorized, "", false, ""));

            Teacher.userId = TeacherVM.userId;
            if (Teacher.id < 1)
                return this.ResponseOK(new ResponseViewModel(HttpStatusCode.OK, Resource.EnterTeacherData, false, null));
            if (string.IsNullOrEmpty(Teacher.email))
                return this.ResponseOK(new ResponseViewModel(HttpStatusCode.OK, Resource.EnterTeacherData, false, null));
            var user = UserManager.FindById(Teacher.userId);
            var check_Email = _UsersBll.CheckUserEmail(Teacher.email, "Teacher");
            if (user == null)
            {
                return this.ResponseOK(new ResponseViewModel(HttpStatusCode.OK, Resource.EnterTeacherData, false, null));
            }
            var check_email = _UsersBll.CheckUserEmail(Teacher.email, Teacher.userId);
            if (check_email == true)
                return this.ResponseOK(new ResponseViewModel(HttpStatusCode.OK, Resource.EmailUsedBefore, false, null));

            var check_mobile = _UsersBll.CheckUserMobile(Teacher.mobile, "Teacher", Teacher.userId);
            if (check_mobile == true)
                return this.ResponseOK(new ResponseViewModel(HttpStatusCode.OK, Resource.PhoneUsedBefore, false, null));

            var TeacherVm = _TeacherBll.Teacher_UpdateInfo(Teacher);

            if (Teacher != null)
            {
                var userm = new UserVM()
                {

                    certificatePhoto = Teacher.certificatePhoto,
                    photo = Teacher.photo,
                    phoneNumber = Teacher.mobile,
                    email = Teacher.email,
                    id = Teacher.userId,
                    isCompelete = 1,
                    fullname = Teacher.fullName,
                    first_name = Teacher.firstName,
                    last_name = Teacher.lastName,
                    country = Teacher.country,
                };

                _UsersBll.UserUpdate(userm);
                var User = _TeacherBll.Teacher_GetById(Teacher.id);
                var rating = 0;
                var rates = _ratingBll.GetTeacherRating(Teacher.id);
                if (rates.Count > 0)
                {
                    try
                    {
                        rating = rates.Sum(x => x.rate).Value / rates.Count;
                    }
                    catch { }
                }

                var Education = User.educationLevelTree;
                var Availability = User.teacherAvailability.Select(vm => new teacherAvailability()
                {
                    dayOfWeek = vm.dayOfWeek,
                    fromTime = vm.fromTime,
                    // FromMinutes = vm.FromMinutes,
                    teacherId = vm.teacherId,
                    toTime = vm.toTime,
                    Type = vm.Type
                    // ToMinutes = vm.ToMinutes,


                }).ToList();
                var material = User.teacherMaterials.Select(vm => new Materials()
                {
                    id = vm.id,
                    name = vm.name

                }).ToList();
                //var Specialization = new GeneralList();
                //try
                //{
                //    if (User.specializationId > 0)
                //    {
                //        Specialization = new GeneralList()
                //        {
                //            id = Teacher.specializationId,
                //            name = Teacher.specializationId > 0 ? _specializatioBLL.GetSpecializationById(Teacher.specializationId).name : ""
                //        };
                //    }
                //}
                //catch
                //{

                //}
                //var BranchSpecialization = new GeneralList();
                //try
                //{
                //    BranchSpecialization = new GeneralList()
                //    {
                //        id = User.branchSpecializationId,
                //        name = _specializatioBLL.GetBranchSpecializationById(Teacher.branchSpecializationId).name


                //    };
                //}
                //catch { }

                var birth = "";
                try
                {
                    birth = (User.birthDate != null) ? User.birthDate.Value.ToShortDateString() : "";
                }
                catch
                {

                }
                string requireUpdateProfileAbsher = "0";
                if (user.absher != 1 && String.IsNullOrEmpty(user.country))
                {
                    requireUpdateProfileAbsher = "1";
                }
                else if (user.absher != 1 && user.country == "1")
                {
                    requireUpdateProfileAbsher = "2";
                }
                return this.ResponseOK(new ResponseViewModel(HttpStatusCode.OK, null, true,
                          new TeacherDetailsResponse()
                          {
                              city = User.city,
                              streetNo = User.streetNo,
                              postalCode = User.postalCode,
                              absher = !string.IsNullOrEmpty(User.absherNo) ? int.Parse(User.absherNo) : 0,
                              birthDate = birth,
                              firstName = User.firstName,
                              fullName = !string.IsNullOrEmpty(User.fullName) ? User.fullName : string.Format("{0} {1} ", User.firstName, User.lastName),
                              branchSpecialization = User.branchSpecialization,
                              certificate = User.certificate,
                              bio = User.bio,
                              district = User.district,
                              online = User.online,
                              onlineCost = User.onlineCost,
                              siteCost = User.siteCost,
                              specialization = User.specialization,
                              teachingMechanism = !string.IsNullOrEmpty(User.teachingMechanism) ? int.Parse(User.teachingMechanism) : -1,
                              email = User.email,
                              mobile = User.mobile,
                              nationalId = User.nationalId,
                              gender = User.gender,

                              id = User.teacherId,
                              collage = User.collage,
                              isComplete = user.is_complete == 1 ? true : false,
                              lastName = User.lastName,
                              photo = URL + "/resources/users/" + user.photo,
                              status = user.status.HasValue ? user.status.Value : 0,
                              totalCourse = 1,
                              rating = rating,
                              //branchSpecializations = Teacher.branchSpecializationId > 0 ? BranchSpecialization : null,
                              //specializations = Teacher.specializationId > 0 ? Specialization : null,
                              educationLevelTree = Education,
                              teacherAvailability = Availability,
                              teacherMaterials = material,
                              country = user.country,
                              requireUpdateProfileAbsher = requireUpdateProfileAbsher

                          }));
            }
            else
            {

                // config.SetActualResponseType(typeof(Core.Models.Policy),
                // "Policy", "Get");
                return this.ResponseOK(new ResponseViewModel(HttpStatusCode.OK, Resource.NotFoundTeacher, false, null));
            }
        }

        /// عرض الاوقات المحجوزه معلم    .
        ///  /// </summary>
        [Authorize]
        [Route("GetBookedTimes")]
        public IHttpActionResult GetBookedTimes(int TeacherId)
        {

            string Lang = lang;
            var Teachers = _TeacherBll.Teacher_BookedTimes(TeacherId);

            if (Teachers != null)
            {


                var TimesDates = Teachers.Select(x => new BookedDates
                {
                    BookDate = x.startDate.Value.Date,
                    startDate = x.startDate.Value.Date.ToString(),
                    bookedDayTimes = new List<BookedDayTimes>() { new BookedDayTimes() { fromTime = x.startDate.Value.TimeOfDay.ToString(), toTime = x.endDate.Value.TimeOfDay.ToString() } },
                    dayOfWeek = (int)x.startDate.Value.DayOfWeek

                }).ToList();
                var timesL = TimesDates.GroupBy(x => x.BookDate).Select(y => new BookedDates()
                {
                    startDate = y.Key.Date.ToShortDateString(),
                    dayOfWeek = (int)y.Key.DayOfWeek,
                    bookedDayTimes = y.SelectMany(z => z.bookedDayTimes).ToList()

                }).ToList();
                return this.ResponseOK(new ResponseViewModel(HttpStatusCode.OK, null, true, timesL));


            }
            else
            {

                // config.SetActualResponseType(typeof(Core.Models.Policy),
                // "Policy", "Get");
                return this.ResponseOK(new ResponseViewModel(HttpStatusCode.OK, Resource.ErrorOccure, false, null));
            }



        }

        /// شاشة عرض طلباتي لدى المدرس
        /// </summary>
        /// <param name="TeacherId"></param>
        /// <param name="PageNo"></param>
        /// <param name="Records"></param>
        /// <returns></returns>
        [Authorize]
        [Route("TeacherRequests")]
        [HttpGet]
        public IHttpActionResult TeacherRequests(int TeacherId, int PageNo, int Records)
        {


            string Lang = lang;
            string user_id = User.Identity.GetUserId();
            var Teacher = _TeacherBll.Teacher_GetById(TeacherId);
            if (Teacher != null)
            {

                if (Teacher.userId != user_id) return this.ResponseUnauthorized(new ResponseViewModel(HttpStatusCode.Unauthorized, "", false, ""));
            }
            else
            {
                return this.ResponseNotFound(new ResponseViewModel(HttpStatusCode.NotFound, "", false, ""));
            }
            var items = _TeacherBll.GetRequestsByTeacherID(TeacherId, PageNo, Records, null).Select(s => new NewRequestListViewModel()
            {
                id = s.requestId,
                createdDate = s.createdAt.ToString(),
                pricePerHour = s.pricePerHour,
                totalHours = s.TotalHours.HasValue ? s.TotalHours.Value : 0,
                totalPrice = s.totalPrice,
                //details = s.details,
                //material = s.material,
                //period = s.period,
                //start_date = s.start_date.HasValue ? s.start_date.Value.Year + "-" + s.start_date.Value.Month + "-" + s.start_date.Value.Day : "",
                //start_time = s.start_time.HasValue ? s.start_time.Value.Hours + ":" + s.start_time.Value.Minutes : "---",
                status = s.courceStatus,
                studentId = s.studentId,
                teacherId = TeacherId,
                studentName = s.studentName,
                teacherName = "",
                comment = s.comment,
                subject = s.subject,
                // photo = URL + "/resources/users/" +  : "default.png"),
                teachingMechanism = s.teachingMechanism,
                statusId = s.requestStatusId,
                teachingMechanismStatus = s.teachingMechanismStatus,
                EducationLevelId = s.EducationLevelId,
                EducationLevelName = s.EducationLevelName,
                EducationSublevelName = s.EducationSublevelName,
                SubLevelId = s.SubLevelId,
                liveType = "video",
                Longitude = s.longitude.ToString(),
                Latitude = s.longitude.ToString(),
                courseDates = _RequestCource.CourseDatesByRequestId(s.requestId).Select(vm => new RequestCourceDates()
                {
                    endDate = vm.endDate,
                    startDate = vm.startDate,
                    requestId = s.requestId,
                    idFk = vm.idFk.HasValue ? vm.idFk.Value : 0,
                    type = vm.Type.HasValue ? vm.Type.Value : 0,
                    id = vm.id
                }).ToList()

                ,
                AddressName = s.AddressName,
                photo = !string.IsNullOrEmpty(s.photo) ? URL + "/resources/users/" + s.photo : null,
                subSpecializationName = s.subSpecializationName,
                specializationName = s.specializationName
            }).ToList();
            return this.ResponseOK(new ResponseViewModel(HttpStatusCode.OK, "", true, items));

        }
        [Authorize]
        [Route("RateStudent")]
        [HttpPost]
        public IHttpActionResult RateStudent(Models.RateModel model)
        {
            string Lang = lang;
            var Teacher = _TeacherBll.Teacher_GetById(model.teacherId);

            if (Teacher != null)
            {
                var user = UserManager.FindById(Teacher.userId);

                if (user != null)
                {
                    if (user.Id != User.Identity.GetUserId())
                        return this.ResponseOK(new ResponseViewModel(HttpStatusCode.OK, Resource.NotAuthorized, false, null));
                }
            }

            if (!model.rate.HasValue || model.teacherId == 0 || model.studentId == 0)
                return this.ResponseOK(new ResponseViewModel(HttpStatusCode.OK, Resource.ErrorOccure, false, null));
            //var requset = _ratingBll.Teacher_RateById(model.teacherId, model.studentId).FirstOrDefault();
            //if (requset != null)
            // return this.ResponseOK(new ResponseViewModel(HttpStatusCode.OK, "لقد قمت بتقييم هذا الطالب مسبقاً", false, null));

            RatingVM mr = new RatingVM()
            {
                createdDate = DateTime.Now,
                comment = model.comment,
                courseId = model.courseId,
                rate = model.rate,
                rateType = model.rateType,
                studentId = model.studentId,
                teacherId = model.teacherId,
                complainId = model.complainId

            };
            var id = _TeacherBll.RateStudent(mr);
            if (id == -2)
                return this.ResponseOK(new ResponseViewModel(HttpStatusCode.OK, Resource.CanNotEvaluate, false, null));

            return this.ResponseOK(new ResponseViewModel(HttpStatusCode.OK, Resource.EvaluationSendedSuccessfully, true, null));


        }
        [Authorize]
        [Route("TeacherRatings")]
        [HttpGet]
        public IHttpActionResult TeacherRatings(int TeacherId, int PageNo, int Records)
        {


            string Lang = lang;
            string user_id = User.Identity.GetUserId();
            var Teacher = _TeacherBll.Teacher_GetById(TeacherId);
            if (Teacher == null)
            {
                return this.ResponseNotFound(new ResponseViewModel(HttpStatusCode.NotFound, "", false, ""));
            }

            var items = _ratingBll.Teacher_RateById(TeacherId, PageNo, Records).Select(s => new Models.RateModel()
            {
                comment = s.comment,
                complainId = s.complainId,
                complain = s.complain,
                courseId = s.courseId,
                rate = s.rate,
                rateType = s.rateType,
                studentId = s.studentId.HasValue ? s.studentId.Value : 0,
                teacherId = TeacherId,
                studentName = s.studentName,
                teacherName = s.teacherName,
                createdDate = s.createdDate,
                photo = URL + "/resources/users/" + _ratingBll.GetProfilePhoto(s.studentId.Value, 1)

            }).ToList();
            return this.ResponseOK(new ResponseViewModel(HttpStatusCode.OK, "", true, items));

        }
        [Authorize]
        [Route("HomeCourses")]
        [HttpGet]
        public IHttpActionResult HomeCourses(int TeacherId, int PageNo, int Records, int? SpecId = 0, int? SubSpecId = 0, int? LevelId = 0, int? SubLevelId = 0)
        {


            string Lang = lang;
            string user_id = User.Identity.GetUserId();
            var Teacher = _TeacherBll.Teacher_GetById(TeacherId);
            if (Teacher != null)
            {

                if (Teacher.userId != user_id) return this.ResponseUnauthorized(new ResponseViewModel(HttpStatusCode.Unauthorized, "", false, ""));
            }
            else
            {
                return this.ResponseNotFound(new ResponseViewModel(HttpStatusCode.NotFound, "", false, ""));
            }
            var items = _Lessons.LessonsbyTeacherId(TeacherId, SpecId, SubSpecId, LevelId, SubLevelId, PageNo, Records).Select(s => new CourseModel()
            {
                conferanceZoom = s.conferanceZoom,
                endDate = Convert.ToDateTime(s.endDate),
                lessonId = s.lessonId,
                requestDateId = Convert.ToInt32(s.requestDateId),
                requestId = Convert.ToInt32(s.requestId),
                startDate = Convert.ToDateTime(s.startDate),
                // studentZoom = s.studentZoom,

                requestDetails = new NewRequestListViewModel()
                {

                    id = s.requestId,
                    createdDate = DateTime.Now + "",
                    status = s.RequestDetails.courceStatus,
                    studentId = s.RequestDetails.studentId,
                    teacherId = TeacherId,
                    studentName = s.RequestDetails.studentName,
                    teacherName = "",
                    comment = s.RequestDetails.comment,
                    subject = s.RequestDetails.subject,
                    // photo = URL + "/resources/users/" +  : "default.png"),
                    teachingMechanism = s.RequestDetails.teachingMechanism,
                    statusId = s.RequestDetails.requestStatusId,
                    teachingMechanismStatus = s.RequestDetails.teachingMechanismStatus,

                    liveType = "video",
                    Longitude = s.RequestDetails.longitude.ToString(),
                    Latitude = s.RequestDetails.longitude.ToString(),


                },
                SubLevelId = s.SubLevelId,
                EducationLevelId = s.EducationLevelId,
                EducationLevelName = s.EducationLevelName,
                EducationSublevelName = s.EducationSublevelName,
                AddressName = s.AddressName
            }).ToList();
            return this.ResponseOK(new ResponseViewModel(HttpStatusCode.OK, "", true, items));

        }
        [Authorize]
        [Route("TeacherCourses")]
        [HttpPost]
        public IHttpActionResult TeacherCourses(LessonModel Lesson)
        {

            string Lang = lang;
            string user_id = User.Identity.GetUserId();
            var Teacher = _TeacherBll.Teacher_GetById(Lesson.teacherId);
            if (Teacher != null)
            {

                if (Teacher.userId != user_id) return this.ResponseUnauthorized(new ResponseViewModel(HttpStatusCode.Unauthorized, "", false, ""));
            }
            else
            {
                return this.ResponseNotFound(new ResponseViewModel(HttpStatusCode.NotFound, "", false, ""));
            }
            var items = _Lessons.LessonsbyTeacherId(Lesson.teacherId, Lesson.SpecId, Lesson.SubSpecId, Lesson.LevelId, Lesson.SubLevelId, Lesson.pageno, Lesson.records, Lesson.requestDate, Lesson.courseTime, Lesson.material, Lesson.studentName, Lesson.TeachingMehod).Select(s => new CourseModel()
            {
                conferanceZoom = s.conferanceZoom,
                endDate = Convert.ToDateTime(s.endDate),
                lessonId = s.lessonId,
                requestDateId = Convert.ToInt32(s.requestDateId),
                requestId = Convert.ToInt32(s.requestId),
                startDate = Convert.ToDateTime(s.startDate),
                // studentZoom = s.studentZoom,

                requestDetails = new NewRequestListViewModel()
                {

                    id = s.requestId,
                    createdDate = s.RequestDetails.createdAt.ToString(),
                    status = s.RequestDetails.courceStatus,
                    studentId = s.RequestDetails.studentId,
                    teacherId = Lesson.teacherId,
                    studentName = s.RequestDetails.studentName,
                    teacherName = "",
                    comment = s.RequestDetails.comment,
                    subject = s.RequestDetails.subject,
                    // photo = URL + "/resources/users/" +  : "default.png"),
                    teachingMechanism = s.RequestDetails.teachingMechanism,
                    statusId = s.RequestDetails.requestStatusId,
                    teachingMechanismStatus = s.RequestDetails.teachingMechanismStatus,

                    liveType = "video",
                    Longitude = s.RequestDetails.longitude.ToString(),
                    Latitude = s.RequestDetails.longitude.ToString(),

                    EducationLevelId = s.RequestDetails.EducationLevelId,
                    EducationLevelName = s.RequestDetails.EducationLevelName,
                    EducationSublevelName = s.RequestDetails.EducationSublevelName,
                    SubLevelId = s.RequestDetails.SubLevelId,

                    specializationName = s.RequestDetails.specializationName,
                    subSpecializationName = s.RequestDetails.subSpecializationName,
                    spec_id = s.RequestDetails.spec_id,
                    sub_spec_id = s.RequestDetails.sub_spec_id,

                }


            }).ToList();
            return this.ResponseOK(new ResponseViewModel(HttpStatusCode.OK, "", true, items));

        }
        [Route("CourseDetails")]
        [ResponseCodes(HttpStatusCode.OK)]
        [HttpGet]
        public IHttpActionResult CourseDetails(int courseId, int teacherId)
        {
            string Lang = lang;
            var student = _TeacherBll.Teacher_GetById(teacherId);
            //if (student.userId != User.Identity.GetUserId()) return this.ResponseUnauthorized(new ResponseViewModel(HttpStatusCode.Unauthorized, "", false, null));

            var lesson = _Lessons.ScheduleLessonsById(courseId);
            var zoomWeb = GeneralExtention.RefreshZoomUrl(Convert.ToInt64(lesson.MeetingId));
            var items = new CourseModel()
            {
                // conferanceZoom=s.conferanceZoom,
                endDate = Convert.ToDateTime(lesson.endDate),
                lessonId = lesson.lessonId,
                requestDateId = Convert.ToInt32(lesson.requestDateId),
                requestId = Convert.ToInt32(lesson.requestId),
                startDate = Convert.ToDateTime(lesson.startDate),
                conferanceZoom = zoomWeb.start_url,

                requestDetails = new NewRequestListViewModel()
                {

                    id = lesson.RequestDetails.requestId,
                    createdDate = lesson.startDate.ToString(),
                    status = lesson.RequestDetails.courceStatus,
                    teacherId = teacherId,
                    studentId = lesson.RequestDetails.studentId,
                    comment = lesson.RequestDetails.comment,
                    subject = lesson.RequestDetails.subject,
                    // photo = URL + "/resources/users/" +  : "default.png"),
                    teachingMechanism = lesson.RequestDetails.teachingMechanism,
                    statusId = lesson.RequestDetails.requestStatusId,
                    teachingMechanismStatus = lesson.RequestDetails.teachingMechanismStatus,
                    liveType = "video",
                    Longitude = lesson.RequestDetails.longitude.ToString(),
                    Latitude = lesson.RequestDetails.latitude.ToString(),
                }


            };
            return this.ResponseOK(new ResponseViewModel(HttpStatusCode.OK, "", true, items));

        }
        [Authorize]
        [Route("TeacherPendingRequests")]
        [HttpGet]
        public IHttpActionResult TeacherPendingRequests(int TeacherId, int PageNo, int Records)
        {


            string Lang = lang;
            string user_id = User.Identity.GetUserId();
            var Teacher = _TeacherBll.Teacher_GetById(TeacherId);
            if (Teacher != null)
            {

                if (Teacher.userId != user_id) return this.ResponseUnauthorized(new ResponseViewModel(HttpStatusCode.Unauthorized, "", false, ""));
            }
            else
            {
                return this.ResponseNotFound(new ResponseViewModel(HttpStatusCode.NotFound, "", false, ""));
            }
            var items = _TeacherBll.GetRequestsByTeacherID(TeacherId, PageNo, Records, "Pending Confirmation").Select(s => new NewRequestListViewModel()
            {
                id = s.requestId,
                createdDate = DateTime.Now + "",
                //details = s.details,
                //material = s.material,
                //period = s.period,
                //start_date = s.start_date.HasValue ? s.start_date.Value.Year + "-" + s.start_date.Value.Month + "-" + s.start_date.Value.Day : "",
                //start_time = s.start_time.HasValue ? s.start_time.Value.Hours + ":" + s.start_time.Value.Minutes : "---",
                status = s.courceStatus,
                studentId = s.studentId,
                teacherId = TeacherId,
                studentName = s.studentName,
                teacherName = "",
                comment = s.comment,
                subject = s.subject,
                // photo = URL + "/resources/users/" +  : "default.png"),
                teachingMechanism = s.teachingMechanism,
                statusId = s.requestStatusId,
                teachingMechanismStatus = s.teachingMechanismStatus,

                liveType = "video",
                Longitude = s.longitude.ToString(),
                Latitude = s.longitude.ToString(),
                courseDates = s.requestCourceDates.Select(vm => new RequestCourceDates()
                {
                    endDate = vm.endDate,
                    startDate = vm.startDate,
                    requestId = s.requestId,
                    idFk = vm.idFk.HasValue ? vm.idFk.Value : 0,
                    type = vm.Type.HasValue ? vm.Type.Value : 0,
                    id = vm.id

                }).ToList()
                ,
                SubLevelId = s.SubLevelId,
                EducationLevelId = s.EducationLevelId,
                EducationLevelName = s.EducationLevelName,
                EducationSublevelName = s.EducationSublevelName,
                AddressName = s.AddressName,
                totalPrice = s.totalPrice,
                totalHours = s.TotalHours.HasValue ? s.TotalHours.Value : 0

            }).ToList();
            return this.ResponseOK(new ResponseViewModel(HttpStatusCode.OK, "", true, items));

        }

        [Authorize]
        [Route("RejectRequest")]
        [ResponseCodes(HttpStatusCode.OK)]
        [HttpGet]
        public IHttpActionResult RejectRequest(int RequestId)
        {

            string Lang = lang;

            string user_id = User.Identity.GetUserId();
            var canEdit = _TeacherBll.CheckRequest(RequestId, user_id);
            if (canEdit == false)
            {

                return this.ResponseUnauthorized(new ResponseViewModel(HttpStatusCode.Unauthorized, "", false, ""));
            }

            var id = _TeacherBll.Teacher_RejectRequest(RequestId);
            //var lesson = _Lessons.ScheduleLessonsById(id);
            var lesson = _RequestCource.GetRequestDetailsBytId(RequestId);
            var teacher = _TeacherBll.Teacher_GetByUserId(user_id);
            var student = studentBLL.Student_GetById(lesson.studentId);
            //if (student.userId != User.Identity.GetUserId()) return this.ResponseUnauthorized(new ResponseViewModel(HttpStatusCode.Unauthorized, "", false, null));


            var lst = _notificationBLL.GetNotificationToken(lesson.studentId, 1);
            dynamic returndata = new
            {
                CourseId = RequestId,
                teacherId = lesson.teacherId,
                teacherName = teacher.fullName,
                studentId = lesson.studentId,
                studentName = student.fullName,
                subject = lesson.subject,
                type = 3
            };
            string NotificationMessage = JsonConvert.SerializeObject(returndata);
            var title = "رفض طلبك من قبل المعلم";
            _notificationBLL.InsertUserNotification(new NotificationVM() { course_id = lesson.requestId, details = NotificationMessage, title = title, typeId = 1, userId = lesson.studentId, user_type = 1 });
            foreach (var item in lst)
            {
                _comm.SendNotification(returndata, item.Token, 3, title);

            }
            return this.ResponseOK(new ResponseViewModel(HttpStatusCode.OK, Resource.ServiceRefused, true, null));
            // return this.ResponseOK(new ResponseViewModel(HttpStatusCode.OK, "حدث خطأ ما من فضلك حاول مره اخرى", false, null));

        }
        [Authorize]
        [Route("AcceptRequest")]
        [ResponseCodes(HttpStatusCode.OK)]
        [HttpGet]
        public IHttpActionResult AcceptRequest(int RequestId)
        {


            string Lang = lang;
            string user_id = User.Identity.GetUserId();
            var id = _TeacherBll.Teacher_AcceptRequest(RequestId);

            var lesson = _RequestCource.GetRequestDetailsBytId(RequestId);
            var teacher = _TeacherBll.Teacher_GetByUserId(user_id);
            var student = studentBLL.Student_GetById(lesson.studentId);
            //   var student = _TeacherBll.Teacher_GetById(lesson.studentId);
            //if (student.userId != User.Identity.GetUserId()) return this.ResponseUnauthorized(new ResponseViewModel(HttpStatusCode.Unauthorized, "", false, null));


            var lst = _notificationBLL.GetNotificationToken(lesson.studentId, 1);
            dynamic returndata = new
            {
                CourseId = RequestId,
                teacherId = lesson.teacherId,
                teacherName = teacher.fullName,
                studentId = lesson.studentId,
                studentName = student.fullName,
                subject = lesson.subject,
                type = 2
            };
            string NotificationMessage = JsonConvert.SerializeObject(returndata);
            var title = "قبول طلبك من قبل المعلم";
            _notificationBLL.InsertUserNotification(new NotificationVM() { course_id = lesson.requestId, details = NotificationMessage, title = title, typeId = 1, userId = lesson.studentId, user_type = 1 });
            foreach (var item in lst)
            {
                _comm.SendNotification(returndata, item.Token, 2, title);

            }
            return this.ResponseOK(new ResponseViewModel(HttpStatusCode.OK, Resource.ServiceAccepted, true, null));
            //  return this.ResponseOK(new ResponseViewModel(HttpStatusCode.OK, "حدث خطأ ما من فضلك حاول مره اخرى", false, null));

        }

        // <summary>
        /// عرض تقييمات الطلاب للمعلم       
        /// </summary>
        /// <param name="rows"></param>
        /// <returns></returns>
        [Authorize]
        [Route("TeacherRates")]
        [ResponseType(typeof(List<Models.RateModel>))]
        [ResponseCodes(HttpStatusCode.OK)]
        [HttpGet]
        public IHttpActionResult TeacherRates(int TeacherId, int pageNo, int recordsNo)
        {


            string Lang = lang;
            var items = _ratingBll.TeacherRatingList(TeacherId, pageNo, recordsNo).Select(s => new Models.RateModel()
            {
                id = s.id,
                createdDate = s.createdDate,
                comment = s.comment,
                teacherId = TeacherId,
                rate = s.rate,
                courseTitle = "",
                courseId = s.courseId,
                studentId = s.studentId.HasValue ? s.studentId.Value : 0,
                studentName = s.studentName,
                teacherName = s.teacherName,
                photo = !string.IsNullOrEmpty(s.photo) ? URL + "/resources/users/" + s.photo : "",

            }).ToList();
            return this.ResponseOK(new ResponseViewModel(HttpStatusCode.OK, "", true, items));

        }
        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return Request.GetOwinContext().Authentication;
            }
        }
        private async Task<string> GetToken(ApplicationUser user)
        {
            ClaimsIdentity oAuthIdentity = await user.GenerateUserIdentityAsync(UserManager,
                   OAuthDefaults.AuthenticationType);

            ClaimsIdentity cookiesIdentity = await user.GenerateUserIdentityAsync(UserManager,
            CookieAuthenticationDefaults.AuthenticationType);

            AuthenticationProperties properties = ApplicationOAuthProvider.CreateProperties(user.UserName);

            AuthenticationTicket ticket = new AuthenticationTicket(oAuthIdentity, properties);

            ticket.Properties.ExpiresUtc = DateTime.Now.AddDays(1000);
            AuthenticationManager.SignIn(cookiesIdentity);
            //  System.Security.Authentication.SignIn(cookiesIdentity);
            ////
            //هادا التوكن  Startup.OAuthOptions.AccessTokenFormat.Protect(ticket)
            ///
            var token = AuthenticationStartup.OAuthOptions.AccessTokenFormat.Protect(ticket);
            return token;



        }

        [Authorize]
        [Route("GetUserNotification")]
        [ResponseCodes(HttpStatusCode.OK)]
        [HttpPost]
        public IHttpActionResult GetUserNotification(PaginationVM data)
        {
            string Lang = lang;
            string user_id = User.Identity.GetUserId();
            var Teacher = _TeacherBll.Teacher_GetByUserId(user_id);
            NotificationVM obj = new NotificationVM();
            obj.user_type = 2;
            obj.userId = Teacher.teacherId;
            obj.pageNo = data.pageNo;
            obj.pageRows = data.pageRows;
            var items = _notificationBLL.selectUserNotification(obj);
            return this.ResponseOK(new ResponseViewModel(HttpStatusCode.OK, "", true, items));

        }

        [Authorize]
        [Route("InsertRenewTeacherPackage")]
        [HttpPost]
        public IHttpActionResult InsertRenewTeacherPackage(int TeacherId, int PackId)
        {
            string Lang = lang;
            string user_id = User.Identity.GetUserId();
            var Teacher = _TeacherBll.Teacher_GetById(TeacherId);
            if (Teacher != null)
            {

                if (Teacher.userId != user_id) return this.ResponseUnauthorized(new ResponseViewModel(HttpStatusCode.Unauthorized, Resource.NotAuthorized, false, ""));
            }
            else
            {
                return this.ResponseNotFound(new ResponseViewModel(HttpStatusCode.NotFound, Resource.NotFoundTeacher, false, ""));
            }
            var teachPackId = _teachPckage.InsertUpdateTeacherPackage(PackId, TeacherId, null);
            return this.ResponseOK(new ResponseViewModel(HttpStatusCode.OK, "", true, teachPackId));

        }

        [Authorize]
        [Route("DeleteTeacherPackage")]
        [HttpPost]
        public IHttpActionResult DeleteTeacherPackage(int TeacherId, long TeachPackId)
        {
            string Lang = lang;
            string user_id = User.Identity.GetUserId();
            var Teacher = _TeacherBll.Teacher_GetById(TeacherId);
            if (Teacher != null)
            {

                if (Teacher.userId != user_id) return this.ResponseUnauthorized(new ResponseViewModel(HttpStatusCode.Unauthorized, "", false, ""));
            }
            else
            {
                return this.ResponseNotFound(new ResponseViewModel(HttpStatusCode.NotFound, "", false, ""));
            }
            var outObj = _teachPckage.deleteTeacherPackage(TeachPackId, TeacherId);
            return this.ResponseOK(new ResponseViewModel(HttpStatusCode.OK, "", true, outObj));

        }

        [Authorize]
        [Route("TeacherDelete")]
        [HttpPost]
        public IHttpActionResult TeacherDelete()
        {
            string Lang = lang;
            var userId = User.Identity.GetUserId();
            try
            {
                _TeacherBll.DeleteTeacher(userId);
                return this.ResponseOK(new ResponseViewModel(HttpStatusCode.OK, "", true, null));
            }
            catch (Exception e)
            {

                return this.ResponseOK(new ResponseViewModel(HttpStatusCode.OK, Resource.ErrorOccure, false, e.Message));
            }

        }

        [Authorize]
        [Route("UpdateOnlineStatus")]
        [HttpPost]
        public IHttpActionResult UpdateOnlineStatus(bool IsOnline)
        {
            string Lang = lang;
            string user_id = User.Identity.GetUserId();
            var Teacher = _TeacherBll.Teacher_GetByUserId(user_id);
            if (Teacher != null)
            {
                _TeacherBll.TeacherUpdateOnlineStatus(Teacher.teacherId, IsOnline);
            }
            else
            {
                return this.ResponseNotFound(new ResponseViewModel(HttpStatusCode.NotFound, "", false, ""));
            }

            return this.ResponseOK(new ResponseViewModel(HttpStatusCode.OK, Resource.SavedSuccessfully, true, null));

        }


        [Authorize]
        [Route("DeleteTeacherMembershipPackage")]
        [HttpPost]
        public IHttpActionResult DeleteTeacherMembershipPackage(long Id)
        {
            string Lang = lang;
            string user_id = User.Identity.GetUserId();
            var Teacher = _TeacherBll.Teacher_GetByUserId(user_id);

            var res = _membershipBLL.DeleteTeacherMembership(Id, Teacher.teacherId);
            if (res == -3)
            {
                return this.ResponseNotFound(new ResponseViewModel(HttpStatusCode.OK, Resource.NotAuthorized, false, ""));
            }
            else if (res == -4)
            {
                return this.ResponseNotFound(new ResponseViewModel(HttpStatusCode.OK, Resource.IncorrectData, false, ""));
            }
            else if (res == -2)
            {
                return this.ResponseNotFound(new ResponseViewModel(HttpStatusCode.OK, Resource.PaidBefore, false, ""));
            }
            return this.ResponseOK(new ResponseViewModel(HttpStatusCode.OK, Resource.SavedSuccessfully, true, null));

        }

        [Authorize]
        [Route("AddTeacherMembershipPackage")]
        [HttpPost]
        public IHttpActionResult AddTeacherMembershipPackage(int Id)
        {
            string Lang = lang;
            string user_id = User.Identity.GetUserId();
            var Teacher = _TeacherBll.Teacher_GetByUserId(user_id);
            TeacherMembershipVM obj = new TeacherMembershipVM() { membership_id = Id, teacher_id = Teacher.teacherId };
            var res = _membershipBLL.InsertTeacherMembership(obj);

            return this.ResponseOK(new ResponseViewModel(HttpStatusCode.OK, Resource.SavedSuccessfully, true, res));

        }



        [Authorize]
        [Route("GetTeacherMembershipPackage")]
        [HttpGet]
        public IHttpActionResult GetTeacherMembershipPackage()
        {
            string Lang = lang;
            string user_id = User.Identity.GetUserId();
            var Teacher = _TeacherBll.Teacher_GetByUserId(user_id);
            List<TeacherPackagesVM> MembershipPack = _membershipBLL.GetTeacherMembershipPackageData(Teacher.teacherId, Lang);

            dynamic teacherRes = new
            {
                MembershipPack = MembershipPack,
                AllMembershipPackage = _membershipBLL.GetAllMembershipPackage(),
                IsCurrentMembershipPackage = MembershipPack != null && MembershipPack.Where(c => c.isCurrent == true).FirstOrDefault() != null ? true : false
            };

            return this.ResponseOK(new ResponseViewModel(HttpStatusCode.OK, Resource.SavedSuccessfully, true, teacherRes));

        }
    }
}