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
    [Authorize]
    [RoutePrefix("api/v1/Student")]
    public class StudentController : ApiController
    {
        private const string LocalLoginProvider = "Local";
        string URL = ConfigurationManager.AppSettings["URL"];
        private ApplicationUserManager _userManager;
        UsersBLL _UsersBll = new UsersBLL();
        StudentBLL _StudentBll = new StudentBLL();
        TeacherBLL _TeacherBll = new TeacherBLL();
        RatingBLL _RatingBLL = new RatingBLL();
        RequestCourceBLL _RequestCource = new RequestCourceBLL();
        ScheduleLessonsBLL _Lessons = new ScheduleLessonsBLL();
        public StudentController()
        {

        }
        public StudentController(ApplicationUserManager userManager,
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
        /// تسجيل دخول طالب   .
        /// </summary>
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
                var student = _StudentBll.Student_GetByUserId(user.Id);
                if (user.status == -2 || user.status == -1)
                    return this.ResponseOK(new ResponseViewModel(HttpStatusCode.OK, "تم ايقاف حسابك لمزيد من التفاصيل يرجى التواصل مع الادارة ", false, null));
                return this.ResponseOK(new ResponseViewModel(HttpStatusCode.OK, null, true,
                           new RegisterResponseModel()
                           {
                               expired = 14,
                               fullName = user.fullname,
                               token = await GetToken(user),
                               email = user.Email,
                               allowNotify = user.allow_notify,
                               id = student.studentId,
                               firstName=student.firstName,
                               lastName=student.lastName,
                               mobile = user.PhoneNumber,
                               role =  "Student",
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
        /// عرض بيانات طالب   .
        ///  /// </summary>
        [Authorize]
        [Route("GetStudentInfo")]
        public  IHttpActionResult GetStudentInfo(int StudentId)
        {

           
            var Student = _StudentBll.Student_GetById(StudentId);

            if (Student != null)
            {
                var user = UserManager.FindById(Student.userId);
                if (user != null)
                {
                    //if (user.Id!=User.Identity.GetUserId())
                    //    return this.ResponseOK(new ResponseViewModel(HttpStatusCode.OK, "ليس لديك صلاحية التعديل ", false, null));
                    if (user.status == -2 || user.status == -1)
                        return this.ResponseOK(new ResponseViewModel(HttpStatusCode.OK, "تم ايقاف حسابك لمزيد من التفاصيل يرجى التواصل مع الادارة ", false, null));

                    var rating = 0;
                    var rates = _RatingBLL.GetStudentRating(StudentId);
                    if (rates.Count > 0)
                    {
                        try
                        {
                            rating = rates.Sum(x => x.rate).Value / rates.Count;
                        }
                        catch { }
                    }
                    var Education = Student.educationLevel.Select(vm => new EducationLevel()
                    {
                        id = vm.id,
                        name = vm.name,
                        subLevels = vm.subLevels != null ? vm.subLevels.Distinct().ToList().Select(VM => new EducationSuBLevel()
                        {
                            id = VM.id,
                            name = VM.name
                        }).ToList().Distinct().ToList() : new List<EducationSuBLevel>()


                    }).ToList();


                    return this.ResponseOK(new ResponseViewModel(HttpStatusCode.OK, null, true,
                               new GeneralUserResponseModel()
                               {
                                   rating = rating,
                                   city = Student.city,
                                   birthDate = Student.birthDate.ToString(),
                                   firstName = Student.firstName,
                                   fullName = user.fullname,
                                   email = user.Email,
                                   mobile = Student.mobile,
                                   district=Student.district,
                                   gender = Student.gender,
                                   id = StudentId,
                                   isComplete = user.is_complete == 1 ? true : false,
                                   lastName = Student.lastName,
                                   photo = URL + "/resources/users/" + user.photo,
                                   status = user.status,
                                   postalCode=!string.IsNullOrEmpty(Student.postalCode)?Student.postalCode:"",
                                   streetNo =!string.IsNullOrEmpty(Student.streetNo)? Student.streetNo:"",
                                   educationLevel= Education
                               }));
                }
                else
                {

                    // config.SetActualResponseType(typeof(Core.Models.Policy),
                    // "Policy", "Get");
                    return this.ResponseOK(new ResponseViewModel(HttpStatusCode.OK, "هذا الطالب غير موجود", false, null));
                }
            }
            return this.ResponseOK(new ResponseViewModel(HttpStatusCode.OK, "هذا الطالب غير موجود", false, null));

        }
        ///  تعديل بيانات طالب   .
        ///  /// </summary>
        [Authorize]
        [Route("StudentUpdate")]
        public IHttpActionResult StudentUpdate(StudentVM Student)
        {
            Student.userId=User.Identity.GetUserId();
            Student.studentId = Student.id;
            if (Student.id<1)
                return this.ResponseOK(new ResponseViewModel(HttpStatusCode.OK, "يجب ادخال بيانات الطالب", false, null));
            if (string.IsNullOrEmpty(Student.email))
                return this.ResponseOK(new ResponseViewModel(HttpStatusCode.OK, "يجب ادخال البريد الطالب", false, null));
            var user =  UserManager.FindById(Student.userId);

            if (user == null)
                return this.ResponseOK(new ResponseViewModel(HttpStatusCode.OK, "يجب ادخال بيانات الطالب", false, null));
            var check_email = _UsersBll.CheckUserEmail(Student.email, Student.userId);
            if (check_email == true)
                return this.ResponseOK(new ResponseViewModel(HttpStatusCode.OK, "هذا البريد الالكترونى موجود من قبل", false, null));
            var check_mobile = _UsersBll.CheckUserMobile(Student.mobile, "Student",Student.userId);
            if (check_mobile == true)
                return this.ResponseOK(new ResponseViewModel(HttpStatusCode.OK, "رقم الهاتف المستخدم موجود مسبقاً", false, null));

            Student.genderID = Student.gender == "انثى" ? true : false;
            if (!string.IsNullOrEmpty(Student.photo)) Student.photo = Path.GetFileName(Student.photo);
          
            var StudentVM = _StudentBll.Student_UpdateInfo(Student);

            if (Student != null)
            {
                var userm = new UserVM()
                {
                    certificatePhoto = Student.certificatePhoto,
                    photo = Student.photo,
                    phoneNumber = Student.mobile,
                    email = Student.email,
                    id = Student.userId,
                    isCompelete = 1
                };

                _UsersBll.UserUpdate(userm);
                var Education = Student.educationLevel!=null? Student.educationLevel.Select(vm => new EducationLevel()
                {
                    id = vm.id,
                    name = vm.name,
                    subLevels = vm.subLevels!=null? vm.subLevels.Select(VM => new EducationSuBLevel()
                    {
                        id = VM.id,
                        name = VM.name
                    }).ToList():null


                }).ToList():new List<EducationLevel>();
                var User = _StudentBll.Student_GetById(Student.id);
                    return this.ResponseOK(new ResponseViewModel(HttpStatusCode.OK, null, true,
                          new GeneralUserResponseModel()
                          {
                              city = Student.city,
                              birthDate = Student.birthDate.HasValue? Student.birthDate.Value.Date.ToString():"",
                              firstName = Student.firstName,
                              email = Student.email,
                              mobile = Student.mobile,
                              gender = Student.gender,
                              id = Student.studentId,
                              postalCode = Student.postalCode,
                              streetNo = Student.streetNo,
                              fullName = !string.IsNullOrEmpty(Student.fullName )? Student.fullName: string.Format("{0} {1}",Student.firstName,Student.lastName),
                              lastName = Student.lastName,
                              district=Student.district,
                              educationLevel = User.educationLevel != null ? Education : new List<EducationLevel>()
                              ,isComplete = user.is_complete == 1 ? true : false,
                              photo = URL + "/resources/users/" + user.photo,
                              status = user.status,

                          }));
                }
                else
                {

                    // config.SetActualResponseType(typeof(Core.Models.Policy),
                    // "Policy", "Get");
                    return this.ResponseOK(new ResponseViewModel(HttpStatusCode.OK, "حدث خطأ اثناء تعديل بيانات الطالب", false, null));
                }
            
            

        }
        ///بحث عن معلم    .
        ///  /// </summary>
        [Authorize]
        [Route("AvailableTeachers")]
        public IHttpActionResult AvailableTeachers(AvailableTeacherVM ATeacher)
        {
             if (ATeacher == null )
            {
                ATeacher = new AvailableTeacherVM();

                ATeacher.pageNo = 1;
                ATeacher.recordsPerPage = 10;
                
            }
            if (string.IsNullOrEmpty(ATeacher.FullName)) ATeacher.FullName = null;
            if (string.IsNullOrEmpty(ATeacher.firstName)) ATeacher.firstName = null;
            if (string.IsNullOrEmpty(ATeacher.lastName)) ATeacher.lastName = null;
            if (string.IsNullOrEmpty(ATeacher.material)) ATeacher.material = null;
            if (string.IsNullOrEmpty(ATeacher.specialization)) ATeacher.specialization = null;
            if (string.IsNullOrEmpty(ATeacher.branchSpecialization)) ATeacher.branchSpecialization  = null;
            if (string.IsNullOrEmpty(ATeacher.city)) ATeacher.city = null;
            if (ATeacher.rate<1) ATeacher.rate = null;
           

           
            if (!ATeacher.pageNo.HasValue) ATeacher.pageNo = 1;
            if (!ATeacher.recordsPerPage.HasValue) ATeacher.recordsPerPage = 10;

            var listTeachers = _StudentBll.Available_Teachers(ATeacher).Select( vm=> 

            new TeacherResponseModel()
            {
                teacherId = vm.teacherId,
                city = vm.city,
                birthDate = vm.birthDate.HasValue? vm.birthDate.Value.ToShortDateString():"",
                firstName = vm.firstName,
                fullName = !string.IsNullOrEmpty(vm.FullName) ? vm.FullName : string.Format("{0} {1} ", vm.firstName, vm.lastName),
                email = vm.email,
                mobile = vm.mobile,
                gender = vm.gender.HasValue ? vm.gender.Value == false ? "ذكر" : "أنثي" : "ذكر",
                id = vm.teacherId,
                isComplete = vm.isComplete,
                lastName = vm.lastName,
                photo = !string.IsNullOrEmpty(vm.photo) ? URL + "/resources/users/" + vm.photo : null,
                status = vm.status,
                specialization = vm.specialization,
                branchSpecialization = vm.branchSpecialization,
                certificate = vm.certificate,
                certificatePhoto = !string.IsNullOrEmpty(vm.certificatePhoto) ? URL + "/resources/users/" + vm.certificatePhoto : null,
                absher = vm.absher,
                rate = vm.rate,
                educationLevel = vm.educations.Select(VM => new EducationLevel()
                {
                    id = VM.id,
                    name = VM.name

                }).ToList(),
                teacherTimes = vm.teacherAvailability.Select(VM => new teacherAvailability()
                {
                    dayOfWeek = VM.dayOfWeek,
                    fromTime = VM.fromTime,
                    //FromMinutes = VM.FromMinutes,
                    teacherId = VM.teacherId,
                    toTime = VM.toTime,
                    // ToMinutes = VM.ToMinutes,
                    // FromTime=VM.FromTime,
                    // ToTime=VM.ToTime

                }).ToList(),
                teacherMaterials = vm.teacherMaterial.Select(VM => new Materials()
                {
                    id = VM.id,
                    name = VM.name

                }).ToList(),

            }).ToList();
            return this.ResponseOK(new ResponseViewModel(HttpStatusCode.OK, null, true, listTeachers));
        }
        ///اضافه درس   .
        ///  /// </summary>
        [Authorize]
        [Route("SendTeacherRequest")]
        public IHttpActionResult SendTeacherRequest(RequestCourceVM Request)
        {

            if (Request == null) return this.ResponseOK(new ResponseViewModel(HttpStatusCode.OK, "يجب ادخال بيانات طلب الخدمة", false, null));
            else
            {
                if (Request.requestCourceDates == null) return this.ResponseOK(new ResponseViewModel(HttpStatusCode.OK, "يجب ادخال بيانات طلب الخدمة", false, null));
            }
            var Id = _RequestCource.RequestCourceAdd(Request);

            if(Id >= 1)
            {
                return this.ResponseOK(new ResponseViewModel(HttpStatusCode.OK, "تم ارسال طلبك بنجاح يرجى انتظار الرد في أقرب فرصة", true, null));

            }
            else if (Id==-1)
            {
                return this.ResponseOK(new ResponseViewModel(HttpStatusCode.OK, "لم نتمكن من حجز الموعد لانه  محجوز من  قبل طالب اخر ", false, null));
            }

            else
            {
                return this.ResponseOK(new ResponseViewModel(HttpStatusCode.OK, " البيانات غير صحيحه", false, null));
            }
            
        }

        /// شاشة عرض طلباتي لدى الطالب
        /// </summary>
        /// <param name="rows"></param>
        /// <returns></returns>
        [Authorize]
        [Route("MyRequests")]
        
        [ResponseCodes(HttpStatusCode.OK)]
        [HttpGet]
        public IHttpActionResult MyRequests(int StudentId ,int PageNo ,int Records)
        {
           
              

                string user_id = User.Identity.GetUserId();
            var items = _StudentBll.GetRequestsByStudentID(StudentId, PageNo, Records).Select(s => new NewRequestListViewModel()
            {
                id = s.requestId,
                createdDate =s.createdAt.ToString() ,
                pricePerHour=s.pricePerHour,
                totalHours=s.totalPrice/s.pricePerHour,
                totalPrice=s.totalPrice,
                //details = s.details,
                //material = s.material,
                //period = s.period,
                //start_date = s.start_date.HasValue ? s.start_date.Value.Year + "-" + s.start_date.Value.Month + "-" + s.start_date.Value.Day : "",
                //start_time = s.start_time.HasValue ? s.start_time.Value.Hours + ":" + s.start_time.Value.Minutes : "---",
                status = s.courceStatus,
                studentId = StudentId,
                studentName = "",
                teacherId = s.teacherId,
                teacherName = s.teacherName,
                comment=s.comment,
                subject=s.subject,
                // photo = URL + "/resources/users/" +  : "default.png"),
                teachingMechanism = s.teachingMechanism,
                statusId=s.requestStatusId,
                teachingMechanismStatus=s.teachingMechanismStatus,
                liveType = "video",
                Longitude = s.longitude.ToString(),
                Latitude = s.latitude.ToString(),
                courseDates =_RequestCource.CourseDatesByRequestId(s.requestId).Select(vm => new RequestCourceDates()
                {
                    endDate = vm.endDate,
                    startDate = vm.startDate,
                    requestId = s.requestId


                }).ToList()


            }).ToList();
                return this.ResponseOK(new ResponseViewModel(HttpStatusCode.OK, "", true, items));
            
        }
        [Authorize]
        [Route("MyCourses")]

        [ResponseCodes(HttpStatusCode.OK)]
        [HttpGet]
        public IHttpActionResult MyCourses(int StudentId, int PageNo, int Records)
        {



          
            var student = _StudentBll.Student_GetById(StudentId);
            //if (student.userId != User.Identity.GetUserId()) return this.ResponseUnauthorized(new ResponseViewModel(HttpStatusCode.Unauthorized, "", false, null));
            var items = _Lessons.LessonsbyStudentId(StudentId, PageNo, Records).Select(s => new CourseModel()
            {
               // conferanceZoom=s.conferanceZoom,
                endDate=s.endDate,
                lessonId=s.lessonId,
                requestDateId=s.requestDateId,
                requestId=s.requestId,
                startDate=s.startDate,
                conferanceZoom=s.studentZoom,

                requestDetails =  new NewRequestListViewModel() { 
                
                    id = s.RequestDetails.requestId,
                    createdDate = DateTime.Now + "",

                    status = s.RequestDetails.courceStatus,
                    studentId = StudentId,
                    studentName = "",
                    teacherId = s.RequestDetails.teacherId,
                    teacherName = s.RequestDetails.teacherName,
                    comment = s.RequestDetails.comment,
                    subject = s.RequestDetails.subject,
                    // photo = URL + "/resources/users/" +  : "default.png"),
                    teachingMechanism = s.RequestDetails.teachingMechanism,
                    statusId = s.RequestDetails.requestStatusId,
                    teachingMechanismStatus = s.RequestDetails.teachingMechanismStatus,
                    liveType = "video",
                    Longitude = s.RequestDetails.longitude.ToString(),
                    Latitude = s.RequestDetails.latitude.ToString(),
                }


            }).ToList();
            return this.ResponseOK(new ResponseViewModel(HttpStatusCode.OK, "", true, items));

        }
        [Authorize]
        [Route("CourseDetails")]

        [ResponseCodes(HttpStatusCode.OK)]
        [HttpGet]
        public IHttpActionResult CourseDetails(int courseId,int studentId)
        {
            var student = _StudentBll.Student_GetById(studentId);
            //if (student.userId != User.Identity.GetUserId()) return this.ResponseUnauthorized(new ResponseViewModel(HttpStatusCode.Unauthorized, "", false, null));

            var lesson = _Lessons.ScheduleLessonsById(courseId);
            var zoomWeb= GeneralExtention.RefreshZoomUrl(lesson.MeetingId);
            var items =  new CourseModel()
            {
                // conferanceZoom=s.conferanceZoom,
                endDate = lesson.endDate,
                lessonId = lesson.lessonId,
                requestDateId = lesson.requestDateId,
                requestId = lesson.requestId,
                startDate = lesson.startDate,
                conferanceZoom = zoomWeb.join_url,
                
                requestDetails = new NewRequestListViewModel()
                {

                    id = lesson.RequestDetails.requestId,
                    createdDate = lesson.startDate.ToString(),

                    status = lesson.RequestDetails.courceStatus,
                    studentId=studentId,
                    teacherId=lesson.RequestDetails.teacherId,
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
        [Route("StudentCourses")]

        [ResponseCodes(HttpStatusCode.OK)]
        [HttpPost]
        public IHttpActionResult StudentCourses(LessonStudentModel Lesson)
        {




            var student = _StudentBll.Student_GetById(Lesson.studentId);
            //if (student.userId != User.Identity.GetUserId()) return this.ResponseUnauthorized(new ResponseViewModel(HttpStatusCode.Unauthorized, "", false, null));
            var items = _Lessons.LessonsbyStudentId(Lesson.studentId,Lesson. pageno,Lesson. records,Lesson.requestDate,Lesson.courseTime,Lesson.material,Lesson.teacherName,Lesson.TeachingMehod).Select(s => new CourseModel()
            {
                // conferanceZoom=s.conferanceZoom,
                endDate = s.endDate,
                lessonId = s.lessonId,
                requestDateId = s.requestDateId,
                requestId = s.requestId,
                startDate = s.startDate,
                conferanceZoom = s.studentZoom,

                requestDetails = new NewRequestListViewModel()
                {

                    id = s.RequestDetails.requestId,
                    createdDate = DateTime.Now + "",

                    status = s.RequestDetails.courceStatus,
                    studentId = s.RequestDetails.studentId,
                    studentName = "",
                    teacherId = s.RequestDetails.teacherId,
                    teacherName = s.RequestDetails.teacherName,
                    comment = s.RequestDetails.comment,
                    subject = s.RequestDetails.subject,
                    // photo = URL + "/resources/users/" +  : "default.png"),
                    teachingMechanism = s.RequestDetails.teachingMechanism,
                    statusId = s.RequestDetails.requestStatusId,
                    teachingMechanismStatus = s.RequestDetails.teachingMechanismStatus,
                    liveType = "video",
                    Longitude = s.RequestDetails.longitude.ToString(),
                    Latitude = s.RequestDetails.latitude.ToString(),
                }


            }).ToList();
            return this.ResponseOK(new ResponseViewModel(HttpStatusCode.OK, "", true, items));

        }
        /// <summary>
        /// عرض بيانات معلم   
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [Authorize]
        [Route("GetTeacherDetails")]
        [ResponseType(typeof(TeacherDetailsResponse))]
        [ResponseCodes(HttpStatusCode.OK)]
        [HttpGet]
        public IHttpActionResult GetTeacherDetails(int TeacherId)
        {

            var rating = 0;
            var Teacher = _TeacherBll.Teacher_GetById(TeacherId);
            if (Teacher != null)
            {
                var rates = _RatingBLL.GetTeacherRating(TeacherId);
                if (rates.Count > 0) {
                    try {
                        rating = rates.Sum(x => x.rate).Value / rates.Count; }
                    catch { }
                }
               
                var user = UserManager.FindById(Teacher.userId);
                if (user != null)
                {
                    if (user.status == -2 || user.status == -1)
                        return this.ResponseOK(new ResponseViewModel(HttpStatusCode.OK, "تم ايقاف حسابك لمزيد من التفاصيل يرجى التواصل مع الادارة ", false, null));
                   
                    if (user == null)
                        return this.ResponseOK(new ResponseViewModel(HttpStatusCode.OK, "هذا المستخدم غير فعال لدينا", false, null));

                    TeacherDetailsResponse t = new TeacherDetailsResponse()
                    {
                        id = Teacher.teacherId,
                        fullName = Teacher.fullName,
                        gender = Teacher.gender,
                        bio = Teacher.bio,
                        email = user.Email,
                        teachingMechanism = int.Parse(Teacher.teachingMechanism),
                        certificate = Teacher.certificate,
                        onlineCost = Convert.ToDouble(Teacher.onlineCost),
                        siteCost = Convert.ToDouble(Teacher.siteCost),
                        specialization = Teacher.specialization ,
                        branchSpecialization = Teacher.branchSpecialization,
                        totalCourse = 1,
                        rating = rating,
                        createdDate = string.Format("{0:MMM/dd/yyyy}", user.cdate),
                        photo = URL + "/resources/users/" + (string.IsNullOrEmpty(Teacher.photo) ? "default.png" : Teacher.photo),
                        absher = !string.IsNullOrEmpty(Teacher.absherNo) ?int.Parse(Teacher.absherNo):0,
                        online = Teacher.online.HasValue ? Teacher.online.Value: false,
                        educationLevel = Teacher.educationLevel.Select(vm => new EducationLevel()
                        {
                            id = vm.id,
                            name = vm.name

                        }).ToList(),
                        teacherAvailability = Teacher.teacherAvailability.Select(vm => new teacherAvailability()
                        {
                            dayOfWeek = vm.dayOfWeek,
                            fromTime = vm.fromTime,
                            //FromMinutes = vm.FromMinutes,
                            teacherId = vm.teacherId,
                            toTime = vm.toTime,
                            //ToMinutes = vm.ToMinutes,


                        }).ToList()
                    };

                    return this.ResponseOK(new ResponseViewModel(HttpStatusCode.OK, null, true, t));
                }
            }
            return this.ResponseOK(new ResponseViewModel(HttpStatusCode.OK, "هذا المعلم غير موجود", false, null));
        }

        /// <summary>
        /// تقييم طالب ل معلم   
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [Authorize]
        [Route("RateTeacher")]
        [HttpPost]
        public IHttpActionResult RateTeacher(RateModel model)
        {
            var Student = _StudentBll.Student_GetById(model.studentId);

            if (Student != null)
            {
                var user = UserManager.FindById(Student.userId);
                if (user != null)
                {
                    if (user.Id != User.Identity.GetUserId())
                        return this.ResponseOK(new ResponseViewModel(HttpStatusCode.OK, "ليس لديك صلاحية التعديل ", false, null));
                }
            }

                    if (!model.rate.HasValue || model.teacherId==0 || model.studentId==0)
                    return this.ResponseOK(new ResponseViewModel(HttpStatusCode.OK, "هنالك خطأ في البيانات", false, null));
                var requset =_RatingBLL.Teacher_RateById(model.teacherId,model.studentId).FirstOrDefault();
                if (requset != null)
                    return this.ResponseOK(new ResponseViewModel(HttpStatusCode.OK, "لقد قمت بتقييم هذا المعلم مسبقاً", false, null));
               
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
                 var id = _StudentBll.RateTeacher(mr);
               if(id==-2)
                return this.ResponseOK(new ResponseViewModel(HttpStatusCode.OK, " لايمكنك التقييم ", false, null));

            return this.ResponseOK(new ResponseViewModel(HttpStatusCode.OK, "شكراً لك تم ارسال تقييم بنجاح", true, null));

            
        }
        [Authorize]
        [Route("StudentRatings")]
        [HttpGet]
        public IHttpActionResult StudentRatings(int StudentId, int PageNo, int Records)
        {
            string user_id = User.Identity.GetUserId();
            var Teacher = _StudentBll.Student_GetById(StudentId);
            if (Teacher == null)

            {
                return this.ResponseNotFound(new ResponseViewModel(HttpStatusCode.NotFound, "", false, ""));
            }
            var items = _RatingBLL.student_RateById(StudentId, PageNo, Records).Select(s => new RateModel()
            {
                comment = s.comment,
                complain=s.complain,
                complainId = s.complainId,
                courseId = s.courseId,
                rate = s.rate,
                rateType = s.rateType,
                studentId = StudentId,
                teacherId = s.teacherId.HasValue ? s.teacherId.Value : 0,
                studentName = s.studentName,
                teacherName = s.teacherName,
                createdDate=s.createdDate,
                photo = URL + "/resources/users/" + _RatingBLL.GetProfilePhoto(s.teacherId.Value, 2)

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
            ////
            //هادا التوكن  Startup.OAuthOptions.AccessTokenFormat.Protect(ticket)

            ///
           
            
            var token = AuthenticationStartup.OAuthOptions.AccessTokenFormat.Protect(ticket);
            return token;



        }

    }
}