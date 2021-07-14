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

namespace NileThink.Framework.PrivateLessonManagementSystem.Web.Controllers.Api
{
   
    [RoutePrefix("api/v1/Teacher")]
    public class TeacherController : ApiController
    {
        private const string LocalLoginProvider = "Local";
        string URL = ConfigurationManager.AppSettings["URL"];
        private ApplicationUserManager _userManager;
        UsersBLL _UsersBll = new UsersBLL();
        TeacherBLL _TeacherBll = new TeacherBLL();
        RequestCourceBLL _RequestCource = new RequestCourceBLL();
        RatingBLL _ratingBll = new RatingBLL();
        SpecializationBLL _specializatioBLL = new SpecializationBLL();
        ScheduleLessonsBLL _Lessons = new ScheduleLessonsBLL();
       
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
        // [ResponseType(typeof(registerResponseRecipient))]
        // [ResponseType(typeof(registerResponseAdvertiser))]
        public async Task<IHttpActionResult> Login(LoginBindingModel model)
        {
            if (!ModelState.IsValid)
            {
                return this.ResponseOK(new ResponseViewModel(HttpStatusCode.OK, "اسم المستخدم أو كلمة المرور غير صحيحة", false, null));
            }
            var checkAccount = await UserManager.FindByNameAsync(model.userName);
            if (checkAccount == null)
                return this.ResponseOK(new ResponseViewModel(HttpStatusCode.OK, "هذا الحساب غير مسجل لدينا ", false, null));

            var user = await UserManager.FindAsync(checkAccount.UserName, model.password);
            if (user != null)
            {
                var Teacher = _TeacherBll.Teacher_GetByUserId(user.Id);
                if (user.status == -2 || user.status == -1)
                    return this.ResponseOK(new ResponseViewModel(HttpStatusCode.OK, "تم ايقاف حسابك لمزيد من التفاصيل يرجى التواصل مع الادارة ", false, null));

                var tokenid = await GetToken(user);
                return this.ResponseOK(new ResponseViewModel(HttpStatusCode.OK, null, true,
                           new RegisterResponseModel()
                           {
                               expired = 14,
                               fullName = user.fullname,
                                token = tokenid,
                               email = user.Email,
                               firstName = Teacher.firstName,
                               lastName=Teacher.lastName,
                                allowNotify = user.allow_notify,
                               id = Teacher.teacherId,
                               mobile = user.PhoneNumber,
                               role = "Teacher",
                               isComplete = user.is_complete == 1 ? true : false,
                               absherNo = user.absher.HasValue ? user.absher : 0,
                               online = true,
                               photo = URL + "/resources/users/" + user.photo
                           }));
            }
            else
            {

                // config.SetActualResponseType(typeof(Core.Models.Policy),
                // "Policy", "Get");
                return this.ResponseOK(new ResponseViewModel(HttpStatusCode.OK, "اسم المستخدم أو كلمة المرور غير صحيحة", false, null));
            }


        }

        /// عرض بيانات معلم    .
        ///  /// </summary>
        [Authorize]
        [Route("GetTeacherInfo")]
        public IHttpActionResult GetTeacherInfo(int TeacherId)
        {

            
            var Teacher = _TeacherBll.Teacher_GetById(TeacherId);

            if (Teacher != null)
            {
                var user = UserManager.FindById(Teacher.userId);
                if (user != null)
                {
                   
                    //if (user.Id!=User.Identity.GetUserId()  && !User.IsInRole("Student"))
                    //    return this.ResponseUnauthorized(new ResponseViewModel(HttpStatusCode.Unauthorized, "", false, ""));
                    if (user.status == -2 || user.status == -1)
                        return this.ResponseOK(new ResponseViewModel(HttpStatusCode.OK, "تم ايقاف حسابك لمزيد من التفاصيل يرجى التواصل مع الادارة ", false, null));
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

                    var Education = Teacher.educationLevel.Select(vm => new EducationLevel()
                    {
                        id = vm.id,
                        name = vm.name,
                        subLevels = vm.subLevels.Select(VM => new EducationSuBLevel()
                        {
                            id = VM.id,
                            name = VM.name
                        }).ToList()


                    }).ToList();
                    var Availability = Teacher.teacherAvailability.Select(vm => new teacherAvailability()
                    {
                        dayOfWeek = vm.dayOfWeek,
                        fromTime = vm.fromTime,
                        // FromMinutes = vm.FromMinutes,
                        teacherId = vm.teacherId,
                        toTime = vm.toTime,
                        // ToMinutes = vm.ToMinutes,


                    }).ToList();
                    var material = Teacher.teacherMaterials.Select(vm => new Materials()
                    {
                        id = vm.id,
                        name = vm.name

                    }).ToList();
                    var Specialization = new GeneralList();
                    try
                    {
                        if (Teacher.specializationId > 0)
                        {
                            Specialization = new GeneralList()
                            {
                                id = Teacher.specializationId,
                                name = Teacher.specializationId > 0 ? _specializatioBLL.GetSpecializationById(Teacher.specializationId).name : ""
                            };
                        }
                    }
                    catch
                    {
                         
                    }
                    var BranchSpecialization = new GeneralList();
                    try
                    {
                        BranchSpecialization=  new GeneralList()
                        {
                            id = Teacher.branchSpecializationId,
                            name = _specializatioBLL.GetBranchSpecializationById(Teacher.branchSpecializationId).name


                        };
                    }
                    catch { }

                    var teacherRes= new TeacherDetailsResponse()
                    {
                        city = Teacher.city,
                        streetNo = Teacher.streetNo,
                        postalCode = Teacher.postalCode,
                        absher = !string.IsNullOrEmpty(Teacher.absherNo) ? int.Parse(Teacher.absherNo) : 0,
                        birthDate = Teacher.birthDate.ToString(),
                        firstName = Teacher.firstName,
                        fullName = user.fullname,
                        branchSpecialization = Teacher.branchSpecialization,
                        certificate = Teacher.certificate,
                        bio = Teacher.bio,
                        district = Teacher.district,
                        online = Teacher.online,
                        onlineCost = Teacher.onlineCost,
                        siteCost = Teacher.siteCost,
                        specialization = Teacher.specialization,
                        teachingMechanism =!string.IsNullOrEmpty(Teacher.teachingMechanism)? int.Parse(Teacher.teachingMechanism):-1,
                        email = user.Email,
                        mobile = Teacher.mobile,
                        nationalId = Teacher.nationalId,
                        gender = Teacher.gender,
                        id = Teacher.teacherId,
                        collage = Teacher.collage,
                        isComplete = user.is_complete == 1 ? true : false,
                        lastName = Teacher.lastName,
                        photo = URL + "/resources/users/" + user.photo,
                        status = user.status.HasValue ? user.status.Value : 0,
                        totalCourse=1,
                        rating = rating,
                        branchSpecializations = Teacher.branchSpecializationId > 0? BranchSpecialization  :null,
                        specializations = Teacher.specializationId > 0 ? Specialization : null,
                        educationLevel = Education,
                        teacherAvailability = Availability,
                        teacherMaterials = material,
                    };


                    return this.ResponseOK(new ResponseViewModel(HttpStatusCode.OK, null, true, teacherRes));
                          
                }
                else
                {

                    // config.SetActualResponseType(typeof(Core.Models.Policy),
                    // "Policy", "Get");
                    return this.ResponseOK(new ResponseViewModel(HttpStatusCode.OK, "هذا المعلم غير موجود", false, null));
                }
            }
            return this.ResponseOK(new ResponseViewModel(HttpStatusCode.OK, "هذا المعلم غير موجود", false, null));

        }
        /// تعديل بيانات معلم    .
        ///  /// </summary>
        [Authorize]
        [Route("TeacherUpdate")]
        public IHttpActionResult TeacherUpdate(TeacherVM Teacher)
        {

            Teacher.teacherId=Teacher.id;
           if(!string.IsNullOrEmpty(Teacher.photo)) Teacher.photo = Path.GetFileName(Teacher.photo);
           if (!string.IsNullOrEmpty(Teacher.certificatePhoto)) Teacher.certificatePhoto = Path.GetFileName(Teacher.certificatePhoto);
            var TeacherVM = _TeacherBll.Teacher_GetById(Teacher.id);

           if (TeacherVM.userId != User.Identity.GetUserId()) return this.ResponseUnauthorized(new ResponseViewModel(HttpStatusCode.Unauthorized, "", false, ""));
           
            Teacher.userId = TeacherVM.userId;
            if (Teacher.id < 1)
                return this.ResponseOK(new ResponseViewModel(HttpStatusCode.OK, "يجب ادخال بيانات المعلم", false, null));
            if (string.IsNullOrEmpty(Teacher.email))
                return this.ResponseOK(new ResponseViewModel(HttpStatusCode.OK, "يجب ادخال البريد المعلم", false, null));
            var user = UserManager.FindById(Teacher.userId);
            var check_Email = _UsersBll.CheckUserEmail(Teacher.email, "Teacher");
            if (user == null)
            {
                return this.ResponseOK(new ResponseViewModel(HttpStatusCode.OK, "يجب ادخال بيانات المعلم", false, null));
            }
            var check_email = _UsersBll.CheckUserEmail(Teacher.email, Teacher.userId);
            if (check_email == true)
                return this.ResponseOK(new ResponseViewModel(HttpStatusCode.OK, "البريد الالكترونى المستخدم موجود مسبقاً", false, null));

            var check_mobile = _UsersBll.CheckUserMobile(Teacher.mobile, "Teacher",Teacher.userId);
            if (check_mobile == true)
                return this.ResponseOK(new ResponseViewModel(HttpStatusCode.OK, "رقم الهاتف المستخدم موجود مسبقاً", false, null));

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
                    isCompelete = 1
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

                var Education = User.educationLevel.Select(vm => new EducationLevel()
                {
                    id = vm.id,
                    name = vm.name,
                    subLevels = vm.subLevels.Select(VM => new EducationSuBLevel()
                    {
                        id = VM.id,
                        name = VM.name
                    }).ToList()


                }).ToList();
                var Availability = User.teacherAvailability.Select(vm => new teacherAvailability()
                {
                    dayOfWeek = vm.dayOfWeek,
                    fromTime = vm.fromTime,
                    // FromMinutes = vm.FromMinutes,
                    teacherId = vm.teacherId,
                    toTime = vm.toTime,
                    // ToMinutes = vm.ToMinutes,


                }).ToList();
                var material = User.teacherMaterials.Select(vm => new Materials()
                {
                    id = vm.id,
                    name = vm.name

                }).ToList();
                var Specialization = new GeneralList();
                try
                {
                    if (User.specializationId > 0)
                    {
                        Specialization = new GeneralList()
                        {
                            id = Teacher.specializationId,
                            name = Teacher.specializationId > 0 ? _specializatioBLL.GetSpecializationById(Teacher.specializationId).name : ""
                        };
                    }
                }
                catch
                {

                }
                var BranchSpecialization = new GeneralList();
                try
                {
                    BranchSpecialization = new GeneralList()
                    {
                        id = User.branchSpecializationId,
                        name = _specializatioBLL.GetBranchSpecializationById(Teacher.branchSpecializationId).name


                    };
                }
                catch { }

                var birth = "";
                try
                {
                    birth = (User.birthDate != null) ? User.birthDate.Value.ToShortDateString() : "";
                }
                catch
                {

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
                              fullName = !string.IsNullOrEmpty(User.fullName)? User.fullName:string.Format("{0} {1} ",User.firstName,User.lastName),
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
                              branchSpecializations = Teacher.branchSpecializationId > 0 ? BranchSpecialization : null,
                              specializations = Teacher.specializationId > 0 ? Specialization : null,
                              educationLevel = Education,
                              teacherAvailability = Availability,
                              teacherMaterials = material,
                          }));
                }
                else
                {

                    // config.SetActualResponseType(typeof(Core.Models.Policy),
                    // "Policy", "Get");
                    return this.ResponseOK(new ResponseViewModel(HttpStatusCode.OK, "هذا المعلم غير موجود", false, null));
                }
           

        }

        /// عرض الاوقات المحجوزه معلم    .
        ///  /// </summary>
        [Authorize]
        [Route("GetBookedTimes")]
       
        public IHttpActionResult GetBookedTimes(int TeacherId)
        {


            var Teachers = _TeacherBll.Teacher_BookedTimes(TeacherId);

            if (Teachers != null)
            {
               
                
                var TimesDates = Teachers.Select(x => new BookedDates
                {
                    BookDate= x.startDate.Value.Date,
                    startDate = x.startDate.Value.Date.ToString(),
                    bookedDayTimes = new List<BookedDayTimes>() { new BookedDayTimes() { fromTime= x.startDate.Value.TimeOfDay.ToString(), toTime=x.endDate.Value.TimeOfDay.ToString() } },
                    dayOfWeek=(int)x.startDate.Value.DayOfWeek
                    
                }).ToList();
                var timesL = TimesDates.GroupBy(x => x.BookDate).Select(y => new BookedDates()
                {
                    startDate=y.Key.Date.ToShortDateString(),
                    dayOfWeek=(int) y.Key.DayOfWeek,
                    bookedDayTimes=y.SelectMany (z=> z.bookedDayTimes).ToList()

                }).ToList();
                return this.ResponseOK(new ResponseViewModel(HttpStatusCode.OK, null, true, timesL));


            }
            else
            {

                // config.SetActualResponseType(typeof(Core.Models.Policy),
                // "Policy", "Get");
                return this.ResponseOK(new ResponseViewModel(HttpStatusCode.OK, "حدث خطأ أثناء عرض البيانات", false, null));
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



            string user_id = User.Identity.GetUserId();
            var Teacher = _TeacherBll.Teacher_GetById(TeacherId);
            if(Teacher!=null)
            {

              if(Teacher.userId!=user_id) return this.ResponseUnauthorized(new ResponseViewModel(HttpStatusCode.Unauthorized, "", false, ""));
            }
            else
            {
                return this.ResponseNotFound(new ResponseViewModel(HttpStatusCode.NotFound, "", false, ""));
            }
            var items = _TeacherBll.GetRequestsByTeacherID(TeacherId, PageNo, Records,null).Select(s => new NewRequestListViewModel()
            {
                id = s.requestId,
                createdDate = s.createdAt.ToString(),
                pricePerHour = s.pricePerHour,
                totalHours = s.totalPrice / s.pricePerHour,
                totalPrice = s.totalPrice,
                //details = s.details,
                //material = s.material,
                //period = s.period,
                //start_date = s.start_date.HasValue ? s.start_date.Value.Year + "-" + s.start_date.Value.Month + "-" + s.start_date.Value.Day : "",
                //start_time = s.start_time.HasValue ? s.start_time.Value.Hours + ":" + s.start_time.Value.Minutes : "---",
                status = s.courceStatus,
                studentId = s.studentId,
                teacherId=TeacherId,
                studentName = s.studentName,
                teacherName="",
                comment=s.comment,
                subject=s.subject,
                // photo = URL + "/resources/users/" +  : "default.png"),
                teachingMechanism = s.teachingMechanism,
                statusId=s.requestStatusId,
               teachingMechanismStatus=s.teachingMechanismStatus,
              
                liveType = "video",
                Longitude = s.longitude.ToString(),
                Latitude = s.longitude.ToString(),
                courseDates = _RequestCource.CourseDatesByRequestId(s.requestId).Select(vm => new RequestCourceDates()
                {
                    endDate = vm.endDate,
                    startDate = vm.startDate,
                    requestId = s.requestId


                }).ToList()


            }).ToList();
            return this.ResponseOK(new ResponseViewModel(HttpStatusCode.OK, "", true, items));

        }
        [Authorize]
        [Route("RateStudent")]
        [HttpPost]
        public IHttpActionResult RateStudent(RateModel model)
        {
            var Teacher = _TeacherBll.Teacher_GetById(model.teacherId);

            if (Teacher != null)
            {
                var user = UserManager.FindById(Teacher.userId);
              
                if (user != null)
                {
                    if (user.Id != User.Identity.GetUserId())
                        return this.ResponseOK(new ResponseViewModel(HttpStatusCode.OK, "ليس لديك صلاحية التعديل ", false, null));
                }
            }

            if (!model.rate.HasValue || model.teacherId == 0 || model.studentId == 0)
                return this.ResponseOK(new ResponseViewModel(HttpStatusCode.OK, "هنالك خطأ في البيانات", false, null));
            var requset = _ratingBll.Teacher_RateById(model.teacherId, model.studentId).FirstOrDefault();
            if (requset != null)
                return this.ResponseOK(new ResponseViewModel(HttpStatusCode.OK, "لقد قمت بتقييم هذا الطالب مسبقاً", false, null));

            RatingVM mr = new RatingVM()
            {
                createdDate = DateTime.Now,
                comment = model.comment,
                courseId = model.courseId,
                rate = model.rate,
                rateType = model.rateType,
                studentId = model.studentId,
                teacherId = model.teacherId,
                complainId=model.complainId

            };
            var id = _TeacherBll.RateStudent(mr);
            if (id == -2)
                return this.ResponseOK(new ResponseViewModel(HttpStatusCode.OK, " لايمكنك التقييم ", false, null));

            return this.ResponseOK(new ResponseViewModel(HttpStatusCode.OK, "شكراً لك تم ارسال تقييم بنجاح", true, null));


        }
        [Authorize]
        [Route("TeacherRatings")]
        [HttpGet]
        public IHttpActionResult TeacherRatings(int TeacherId, int PageNo, int Records)
        {



            string user_id = User.Identity.GetUserId();
            var Teacher = _TeacherBll.Teacher_GetById(TeacherId);
            if (Teacher == null)
            {
                return this.ResponseNotFound(new ResponseViewModel(HttpStatusCode.NotFound, "", false, ""));
            }

            var items = _ratingBll.Teacher_RateById(TeacherId, PageNo, Records).Select(s => new RateModel()
            {
                comment=s.comment,
                complainId=s.complainId,
                complain=s.complain,
                courseId=s.courseId,
                rate=s.rate,
                rateType=s.rateType,
                studentId=s.studentId.HasValue?s.studentId.Value:0,
                teacherId= TeacherId,
                studentName=s.studentName,
                teacherName=s.teacherName,
                createdDate=s.createdDate,
                photo= URL + "/resources/users/" + _ratingBll.GetProfilePhoto(s.studentId.Value,1)

            }).ToList();
            return this.ResponseOK(new ResponseViewModel(HttpStatusCode.OK, "", true, items));

        }
        [Authorize]
        [Route("HomeCourses")]
        [HttpGet]
        public IHttpActionResult HomeCourses(int TeacherId, int PageNo, int Records)
        {



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
            var items = _Lessons.LessonsbyTeacherId(TeacherId, PageNo, Records).Select(s => new CourseModel()
            {
                conferanceZoom = s.conferanceZoom,
                endDate = s.endDate,
                lessonId = s.lessonId,
                requestDateId = s.requestDateId,
                requestId = s.requestId,
                startDate = s.startDate,
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
                    
                }
           

            }).ToList();
            return this.ResponseOK(new ResponseViewModel(HttpStatusCode.OK, "", true, items));

        }
        [Authorize]
        [Route("TeacherCourses")]
        [HttpPost]
        public IHttpActionResult TeacherCourses(LessonModel Lesson)
        {

            
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
            var items = _Lessons.LessonsbyTeacherId(Lesson.teacherId, Lesson.pageno, Lesson.records, Lesson.requestDate, Lesson.courseTime, Lesson.material, Lesson.studentName,Lesson.TeachingMehod).Select(s => new CourseModel()
            {
                conferanceZoom = s.conferanceZoom,
                endDate = s.endDate,
                lessonId = s.lessonId,
                requestDateId = s.requestDateId,
                requestId = s.requestId,
                startDate = s.startDate,
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

                }


            }).ToList();
            return this.ResponseOK(new ResponseViewModel(HttpStatusCode.OK, "", true, items));

        }
        [Route("CourseDetails")]

        [ResponseCodes(HttpStatusCode.OK)]
        [HttpGet]
        public IHttpActionResult CourseDetails(int courseId, int teacherId)
        {
            var student = _TeacherBll.Teacher_GetById(teacherId);
            //if (student.userId != User.Identity.GetUserId()) return this.ResponseUnauthorized(new ResponseViewModel(HttpStatusCode.Unauthorized, "", false, null));

            var lesson = _Lessons.ScheduleLessonsById(courseId);
            var zoomWeb = GeneralExtention.RefreshZoomUrl(lesson.MeetingId);
            var items = new CourseModel()
            {
                // conferanceZoom=s.conferanceZoom,
                endDate = lesson.endDate,
                lessonId = lesson.lessonId,
                requestDateId = lesson.requestDateId,
                requestId = lesson.requestId,
                startDate = lesson.startDate,
                conferanceZoom = zoomWeb.start_url,

                requestDetails = new NewRequestListViewModel()
                {

                    id = lesson.RequestDetails.requestId,
                    createdDate = lesson.startDate.ToString(),
                    status = lesson.RequestDetails.courceStatus,
                    teacherId=teacherId,
                    studentId=lesson.RequestDetails.studentId,
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
                courseDates = _RequestCource.CourseDatesByRequestId(s.requestId).Select(vm => new RequestCourceDates()
                {
                    endDate = vm.endDate,
                    startDate = vm.startDate,
                    requestId = s.requestId


                }).ToList()


            }).ToList();
            return this.ResponseOK(new ResponseViewModel(HttpStatusCode.OK, "", true, items));

        }

        [Authorize]
        [Route("RejectRequest")]
        [ResponseCodes(HttpStatusCode.OK)]
        [HttpGet]
        public IHttpActionResult RejectRequest(int RequestId)
        {



            string user_id = User.Identity.GetUserId();
            var canEdit = _TeacherBll.CheckRequest(RequestId, user_id);
            if (canEdit==false)
            {

                return this.ResponseUnauthorized(new ResponseViewModel(HttpStatusCode.Unauthorized, "", false, ""));
            }
           
            var id = _TeacherBll.Teacher_RejectRequest(RequestId);
           
            return this.ResponseOK(new ResponseViewModel(HttpStatusCode.OK, "شكراً لك تم رفض طلب الخدمة", true, null));
           // return this.ResponseOK(new ResponseViewModel(HttpStatusCode.OK, "حدث خطأ ما من فضلك حاول مره اخرى", false, null));

        }
        [Authorize]
        [Route("AcceptRequest")]
        [ResponseCodes(HttpStatusCode.OK)]
        [HttpGet]
        public IHttpActionResult AcceptRequest(int RequestId)
        {



            string user_id = User.Identity.GetUserId();
            var id = _TeacherBll.Teacher_AcceptRequest(RequestId);
           
                return this.ResponseOK(new ResponseViewModel(HttpStatusCode.OK, "شكراً لك تم قبول طلب الخدمة", true, null));
          //  return this.ResponseOK(new ResponseViewModel(HttpStatusCode.OK, "حدث خطأ ما من فضلك حاول مره اخرى", false, null));

        }

        // <summary>
        /// عرض تقييمات الطلاب للمعلم       
        /// </summary>
        /// <param name="rows"></param>
        /// <returns></returns>
        [Authorize]
        [Route("TeacherRates")]
        [ResponseType(typeof(List<RateModel>))]
        [ResponseCodes(HttpStatusCode.OK)]
        [HttpGet]
        public IHttpActionResult TeacherRates(int TeacherId, int pageNo, int recordsNo)
        {
          
            

                var items = _ratingBll.TeacherRatingList(TeacherId,pageNo, recordsNo).Select(s => new RateModel()
                {
                    id = s.id,
                    createdDate = s.createdDate,
                    comment = s.comment,
                    teacherId= TeacherId,
                    rate = s. rate,
                    courseTitle="",
                    courseId = s.courseId,
                    studentId = s.studentId.HasValue? s.studentId.Value:0,
                    studentName = s.studentName,
                    teacherName = s.teacherName,
                    photo= !string.IsNullOrEmpty(s.photo)? URL + "/resources/users/" + s.photo:"",

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
    }
}