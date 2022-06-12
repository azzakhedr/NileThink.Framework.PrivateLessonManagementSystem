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
using NileThink.Framework.PrivateLessonManagementSystem.Web.Results;
using System.IO;
using System.Web.Hosting;
using System.Web.Http.Description;
using NileThink.Framework.PrivateLessonManagementSystem.Web.Helper;
using System.Net;
using System.Linq;
using System.Data.Entity;
using System.Configuration;
using NileThink.Framework.PrivateLessonManagementSystem.Web;
using NileThink.Framework.PrivateLessonManagementSystem.BLL.BussinessLayer;
using NileThink.Framework.PrivateLessonManagementSystem.BLL.ViewModels;
using PrivateLessonMS.Resources;
using ChangeUserModel = NileThink.Framework.PrivateLessonManagementSystem.Web.Models.ChangeUserModel;

namespace NileThink.Framework.PrivateLessonManagementSystem.Web.Controllers.Api
{

    [Authorize]
    [RoutePrefix("api/v1/Account")]
    public class AccountController : BaseController
    {
        UsersBLL _UsersBll = new UsersBLL();
        StudentBLL _StudentBll = new StudentBLL();
        TeacherBLL _TeacherBll = new TeacherBLL();
        private const string LocalLoginProvider = "Local";
        string URL = ConfigurationManager.AppSettings["URL"];

        NotificationBLL _notificationBLL = new NotificationBLL();
        string WebUrl = ConfigurationManager.AppSettings["WebLink"];
        private ApplicationUserManager _userManager;
        CommonController _comm = new CommonController();
        public AccountController()
        {




        }

        public AccountController(ApplicationUserManager userManager,
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

        [Authorize]
        [HttpPost]
        [Route("AddToken")]
        public async Task<IHttpActionResult> AddToken(NotificationTokenVM data)
        {
            string Lang = lang;
            try
            {
                string user_id = User.Identity.GetUserId();
                if (data.user_type == 1)
                {
                    data.user_id = _StudentBll.Student_GetByUserId(user_id).studentId;
                }
                else if (data.user_type == 2)
                {
                    data.user_id = _TeacherBll.Teacher_GetByUserId(user_id).teacherId;
                }
                _notificationBLL.insertUserToken(data);
                return this.ResponseOK(new ResponseViewModel(HttpStatusCode.OK, Resource.SavedSuccessfully, true, null));
            }
            catch (Exception ex)
            {
                return this.ResponseOK(new ResponseViewModel(HttpStatusCode.OK, ex.ToString(), false, null));
            }

        }
        [Authorize]
        [HttpPost]
        [Route("RemoveToken")]
        public async Task<IHttpActionResult> RemoveToken(NotificationTokenVM data)
        {
            try
            {
                string Lang = lang;
                string user_id = User.Identity.GetUserId();
                if (data.user_type == 1)
                {
                    data.user_id = _StudentBll.Student_GetByUserId(user_id).studentId;
                }
                else if (data.user_type == 2)
                {
                    data.user_id = _TeacherBll.Teacher_GetByUserId(user_id).teacherId;
                }
                _notificationBLL.RemoveUserToken(data);
                return this.ResponseOK(new ResponseViewModel(HttpStatusCode.OK, Resource.SavedSuccessfully, true, null));
            }
            catch (Exception ex)
            {
                return this.ResponseOK(new ResponseViewModel(HttpStatusCode.OK, ex.ToString(), false, null));
            }

        }


        [Authorize]
        /// <summary>
        /// تسحيل الخروج   .
        /// </summary>
        [Route("Logout")]
        public IHttpActionResult Logout()
        {
            if (User.IsInRole("2"))
            {
                var teacherId = _TeacherBll.Teacher_GetByUserId(User.Identity.GetUserId()).teacherId;
                _TeacherBll.TeacherUpdateOnlineStatus(teacherId, false);
            }
            Authentication.SignOut(CookieAuthenticationDefaults.AuthenticationType);
            return Ok();
        }
        /// <summary>
        [ResponseType(typeof(string))]
        [ResponseCodes(HttpStatusCode.OK)]
        // POST api/Account/ChangePassword
        [Route("ChangePassword")]
        public async Task<IHttpActionResult> ChangePassword(ChangePasswordBindingModel model)
        {
            string Lang = lang;
            if (!ModelState.IsValid)
            {
                return this.ResponseOK(new ResponseViewModel(HttpStatusCode.OK, Resource.PasswordError, false, null));
            }

            IdentityResult result = await UserManager.ChangePasswordAsync(User.Identity.GetUserId(), model.oldPassword, model.newPassword);



            if (!result.Succeeded)
            {

                return this.ResponseOK(new ResponseViewModel(HttpStatusCode.OK, Resource.PasswordError, false, null));
            }

            return this.ResponseOK(new ResponseViewModel(HttpStatusCode.OK, Resource.PasswordChangeSuccessfully, true, null));
        }
        /// <summary>
        /// نسيت كلمة المرور   .
        /// </summary>   
        [HttpPost]
        [AllowAnonymous]
        [Route("ForgotPassword")]
        public async Task<IHttpActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            try
            {
                string Lang = lang;
                if (ModelState.IsValid)
                {
                    var user = await UserManager.FindByEmailAsync(model.email);

                    if (user == null)
                    {
                        return this.ResponseOK(new ResponseViewModel(HttpStatusCode.OK, Resource.NotRegisterUser, false, null));
                    }
                    // For more information on how to enable account confirmation and password reset please visit http://go.microsoft.com/fwlink/?LinkID=320771
                    // Send an email with this link
                    string code = await UserManager.GeneratePasswordResetTokenAsync(user.Id);
                    var callbackUrl = WebUrl + "/Home/ResetPassword?userid=" + user.Id + "&&code=" + code;
                    var name = string.IsNullOrEmpty(user.fullname) ? user.first_name + " " + user.last_name : user.fullname;
                    ChangeUserModel mail = new ChangeUserModel()
                    {
                        name = name,
                        url = callbackUrl,
                        to = model.email
                    };

                    #region ---- send Mail -----------
                    var body = " عزيزي المستخدم:" + name + ", <br/> تم طلب عدم تذكر كلمة المرور برجاء الدخول علي اللينك التالي لاكمال الطلب <br/> " + callbackUrl;
                    List<string> emails = new List<string>();
                    emails.Add(model.email);
                    new MailerController().SendMail(emails, "تغيير كلمة المرور", body);
                    //SendMail(entity.EmployeeEmail, "New Teame User", body);
                    #endregion

                    // new MailerController().foregetPassword(mail).Deliver();
                    return this.ResponseOK(new ResponseViewModel(HttpStatusCode.OK, Resource.SendEmail, true, null));
                }
                return this.ResponseOK(new ResponseViewModel(HttpStatusCode.OK, Resource.ErrorOccure, false, null));
            }
            catch (Exception ex)
            {
                return this.ResponseOK(new ResponseViewModel(HttpStatusCode.OK, ex.ToString(), false, null));
            }

        }
        /// <summary>
        /// تسجيل طالب جديد   .
        /// </summary>
        [AllowAnonymous]
        [Route("StudentRegister")]
        [ResponseType(typeof(RegisterUserModel))]
        public async Task<IHttpActionResult> StudentRegister(RegisterUserModel model)
        {
            try
            {
                string Lang = lang;
                if (string.IsNullOrEmpty(model.email) || string.IsNullOrEmpty(model.password))
                    return this.ResponseOK(new ResponseViewModel(HttpStatusCode.OK, Resource.IncorrectData, false, null));
                if (string.IsNullOrEmpty(model.password) || model.password.Length < 6)
                    return this.ResponseOK(new ResponseViewModel(HttpStatusCode.OK, Resource.PasswordFormat, false, null));

                var check_username = await UserManager.FindByNameAsync(model.email);
                if (check_username != null)
                    return this.ResponseOK(new ResponseViewModel(HttpStatusCode.OK, Resource.UserUsedBefore, false, null));
                //var check_email = await UserManager.FindByEmailAsync(model.email);
                //if (check_email != null)
                var check_email = _UsersBll.CheckUserEmail(model.email, "Student");
                if (check_email == true)
                    return this.ResponseOK(new ResponseViewModel(HttpStatusCode.OK, Resource.EmailUsedBefore, false, null));
                var check_mobile = _UsersBll.CheckUserMobile(model.mobile, "Student", null);
                if (check_mobile == true)
                    return this.ResponseOK(new ResponseViewModel(HttpStatusCode.OK, Resource.PhoneUsedBefore, false, null));
                //var user = new ApplicationUser()
                //{
                //    UserName = model.email,
                //    Email = model.email,
                //    PhoneNumber = model.mobile,
                //    status = 1,
                //    last_login = DateTime.Now,
                //    cdate = DateTime.Now,
                //    allow_notify = false,
                //    is_complete = 0,
                //    absher = 0,
                //    fullname = model.fullName,
                //    photo = "defaultm.jpg",
                //    last_name = model.lastName,
                //    first_name = model.firstName,
                //    country = model.country

                //};
                //IdentityResult result = await UserManager.CreateAsync(user, model.password);

                //if (!result.Succeeded)
                //    return this.ResponseOK(new ResponseViewModel(HttpStatusCode.OK, Resource.ErrorOccure, false, result.Succeeded));

                //await UserManager.AddToRoleAsync(user.Id, "Student");
                // var UserId=   _UsersBll.AddUser(user.Id, user.Email, user.PhoneNumber, user.PasswordHash, 1);


                int checkstudentEmail = _StudentBll.checkStudentEmail(model.email);
                int checkteacherEmail = _TeacherBll.checkTeacherEmail(model.email);

                if (checkstudentEmail > 0 || checkteacherEmail > 0)
                {
                    return this.ResponseOK(new ResponseViewModel(HttpStatusCode.OK, Resource.EmailUsedBefore, false, null));
                }
                string ActivateCode = RandomToken(4);
                StudentVM student = new StudentVM()
                {
                    //userId = user.Id,
                    email = model.email,
                    mobile = model.mobile,
                    firstName = model.firstName,
                    lastName = model.lastName,
                    genderID = false,
                    ActivateCode = ActivateCode,
                    isActive = false,
                    PasswordHash = Encrypt(model.password)
                };
                var StudentId = _StudentBll.Student_Add(student);

                //if (!string.IsNullOrEmpty(user.PhoneNumber))
                //    phoneToken = UserManager.GenerateChangePhoneNumberToken(user.Id, user.PhoneNumber);
                // SMS _sms = new SMS();
                string MESSAGE = "أهلاً بك في تطبيق درس خصوصى .كود التفعيل هو  " + ActivateCode + " يرجي تفعيل الحساب لكي تتمكن من دخول التطبيق ";
                // string RECIEVER = "966" + user.PhoneNumber.Substring(1);
                // _sms.sendmessage(RECIEVER, MESSAGE);               
                //string msg = "";
                // if (model.role == "Student")
                //msg = "شكرأ لتسجيلك معنا في تطبيق درس خصوصي الآن يمكنك الاستفادة من التطبيق وتعزيز قدراتك العلمية من خلال مشاركتنا والاستفادة من خدماتنا ";

                //msg = "شكرأ لتسجيلك معنا في تطبيق درس خصوصي  يمكنك الاستفادة من التطبيق وتعزيز قدراتك العلمية من خلال مشاركتنا والاستفادة من خدماتنا ";
                //else
                //    msg = "شكرأ لتسجيلك معنا في تطبيق درس خصوصي  يرجى الدخول الى التطبيق لاكمال ملفك الشخصي حتى تتمكن الادارة من مراجعة ملفك الشخصي وتفعيل حسابك كمعلم  ";
                //RegEmail mail = new RegEmail()
                //{
                //    firstname = model.email,
                //    cdate = string.Format("{0:MM/dd/yyyy}", DateTime.Now),
                //    message = MESSAGE,
                //    type = "طالب",
                //    to = model.email
                //};
                // var body = " عزيزي الطالب:" + model.email + ", <br/> " + msg + "<br/> ";
                List<string> emails = new List<string>();
                emails.Add(model.email);
                new MailerController().SendMail(emails, "تفعيل حساب طالب جديد ", MESSAGE);

                // var msg2 = "هناك طلب تسجيل جديد لطالب:";
                //var body2 = " عزيزي مدير النظام:" + ", <br/> " + msg2 + "<br/> " + model.email;
                //List<string> emails2 = _UsersBll.GetAdminEmails("3");
                //new MailerController().SendMail(emails2, "تسجيل طالب جديد ", body2);
                //new Controllers.MailerController().RegEmail(mail).Deliver();
                if (StudentId.HasValue)
                {
                    //var tokenUser = await GetToken(user);
                    return this.ResponseOK(new ResponseViewModel(HttpStatusCode.OK, "تم التسجيل بنجاح برجاء مراجعة البريد الالكترونى لتفعيل الاشتراك", true,
                                new RegisterResponseModel()
                                {
                                    // expired = 14,
                                    fullName = model.firstName + " " + model.lastName,
                                    // token = tokenUser,
                                    email = model.email,
                                    //allowNotify = user.allow_notify,
                                    id = StudentId.HasValue ? StudentId.Value : 0,
                                    firstName = model.firstName,
                                    lastName = model.lastName,
                                    mobile = model.mobile,
                                    role = "Student",
                                    isComplete = false,
                                    IsNeedActivate = true,
                                    //NeedActivation=true
                                    //absherNo = user.absher.HasValue ? user.absher : 0,
                                    // online = true,
                                    //photo = URL + "/resources/users/" + model.
                                }));
                }
                return this.ResponseBadRequest(new ResponseViewModel(HttpStatusCode.OK, "حدث خطأ   من فضلك ادخل بيانات صحيحة", false, null));
            }
            catch (Exception ex)
            {
                return this.ResponseBadRequest(new ResponseViewModel(HttpStatusCode.OK, "حدث خطأ   من فضلك ادخل بيانات صحيحة", false, null));

            }
        }



        [AllowAnonymous]
        [Route("StudentActivateCode")]

        public async Task<IHttpActionResult> StudentActivateCode(ActivateCodeModel model)
        {
            try
            {
                string Lang = lang;
                if (string.IsNullOrEmpty(model.Email) || string.IsNullOrEmpty(model.Code))
                    return this.ResponseOK(new ResponseViewModel(HttpStatusCode.OK, Resource.IncorrectData, false, null));



                var CheckUserCode = _StudentBll.CheckUserCode(model.Email, model.Code);
                if (CheckUserCode == null)
                    return this.ResponseOK(new ResponseViewModel(HttpStatusCode.OK, Resource.IncorrectData, false, null));
                if (!String.IsNullOrEmpty(CheckUserCode.UserId))
                {
                    return this.ResponseOK(new ResponseViewModel(HttpStatusCode.OK, Resource.AlreadyActivated, false, null));
                }
                var user = new ApplicationUser()
                {
                    UserName = CheckUserCode.Email,
                    Email = CheckUserCode.Email,
                    PhoneNumber = CheckUserCode.Mobile,
                    status = 1,
                    last_login = DateTime.Now,
                    cdate = DateTime.Now,
                    allow_notify = false,
                    is_complete = 0,
                    absher = 0,
                    fullname = CheckUserCode.FullName,
                    photo = "defaultm.jpg",
                    last_name = CheckUserCode.LastName,
                    first_name = CheckUserCode.FirstName,

                };
                var Password = Decrypt(CheckUserCode.PasswordHash);
                IdentityResult result = await UserManager.CreateAsync(user, Password);

                if (!result.Succeeded)
                    return this.ResponseOK(new ResponseViewModel(HttpStatusCode.OK, Resource.ErrorOccure, false, result.Succeeded));

                await UserManager.AddToRoleAsync(user.Id, "Student");
               // var UserId = _UsersBll.AddUser(user.Id, user.Email, user.PhoneNumber, user.PasswordHash, 1);
                _StudentBll.Student_UpdateUser(CheckUserCode.StudentId, user.Id);

                string MESSAGE = "أهلاً بك في تطبيق درس خصوصى  .  "
                    + " يرجي دخول التطبيق لكى نتمكن من الاستفاده من التطبيق   ";

                List<string> emails = new List<string>();
                emails.Add(CheckUserCode.Email);
                new MailerController().SendMail(emails, "تم تفعيل الحساب  ", MESSAGE);

                var tokenUser = await GetToken(user);
                return this.ResponseOK(new ResponseViewModel(HttpStatusCode.OK, "تم تفعيل الحساب بنجاح يرجي دخول التطبيق لكى نتمكن من الاستفاده من التطبيق", true,
                            new RegisterResponseModel()
                            {
                                expired = 14,
                                fullName = CheckUserCode.FirstName + " " + CheckUserCode.LastName,
                                token = tokenUser,
                                email = CheckUserCode.Email,
                                allowNotify = user.allow_notify,
                                id = CheckUserCode.StudentId,
                                firstName = CheckUserCode.FirstName,
                                lastName = CheckUserCode.LastName,
                                mobile = CheckUserCode.Mobile,
                                role = "Student",
                                isComplete = false,
                                //NeedActivation=true
                                absherNo = user.absher.HasValue ? user.absher : 0,
                                // online = true,
                                //photo = URL + "/resources/users/" + model.
                            }));

            }
            catch (Exception ex)
            {
                return this.ResponseBadRequest(new ResponseViewModel(HttpStatusCode.OK, "حدث خطأ   من فضلك ادخل بيانات صحيحة", false, null));

            }
        }



        [AllowAnonymous]
        [Route("TeacherResendActivateCode")]

        public async Task<IHttpActionResult> TeacherResendActivateCode(ResendCodeModel model)
        {
            try
            {
                string Lang = lang;
                if (string.IsNullOrEmpty(model.Email))
                    return this.ResponseOK(new ResponseViewModel(HttpStatusCode.OK, Resource.IncorrectData, false, null));



                var CheckUserCode = _TeacherBll.GetTeacherEmail(model.Email);
                if (CheckUserCode == null)
                    return this.ResponseOK(new ResponseViewModel(HttpStatusCode.OK, Resource.IncorrectData, false, null));
                if (!String.IsNullOrEmpty(CheckUserCode.UserId))
                {
                    return this.ResponseOK(new ResponseViewModel(HttpStatusCode.OK, Resource.AlreadyActivated, false, null));
                }

                CheckUserCode.ActivatedCode = RandomToken(4);
                _TeacherBll.UpdateActivatedCode(CheckUserCode);

                //string MESSAGE = "أهلاً بك في تطبيق المهنا.كود التفعيل هو  " + CheckUserCode.ActivatedCode;

                string msg = "تم إعادة ارسال كود التفعيل بنجاح.. يرجى مراجعة البريد الالكتروني و تفعيل الحساب  ";
                RegEmail mail = new RegEmail()
                {
                    firstname = CheckUserCode.Email,
                    cdate = string.Format("{0:MM/dd/yyyy}", DateTime.Now),
                    message = msg,
                    type = "معلم",
                    to = CheckUserCode.Email
                };
                var body = " عزيزي المعلم:" + model.Email + ", <br/> " + "كود التفعيل:" + CheckUserCode.ActivatedCode + "<br/> ";
                List<string> emails = new List<string>();
                emails.Add(CheckUserCode.Email);
                new MailerController().SendMail(emails, "اعادة ارسال كود التفعيل ", body);

                return this.ResponseOK(new ResponseViewModel(HttpStatusCode.OK, msg, true,
                              new RegisterResponseModel()
                              {
                                  // expired = 14,
                                  fullName = CheckUserCode.FirstName + " " + CheckUserCode.LastName,
                                  // token = tokenUser,
                                  //userId = user.Id,
                                  email = CheckUserCode.Email,
                                  //allowNotify = user.allow_notify,
                                  id = CheckUserCode.TeacherId,
                                  mobile = CheckUserCode.Mobile,
                                  role = "Teacher",
                                  firstName = CheckUserCode.FirstName,
                                  lastName = CheckUserCode.LastName,
                                  isComplete = false,
                                  IsNeedActivate = true,
                                  //absherNo = user.absher.HasValue ? user.absher : 0,
                                  //absher_id = user.absher_no,
                                  online = true,
                                  // photo = URL + "/resources/users/" + user.photo,
                                  country = CheckUserCode.RegCountry,
                                  //requireUpdateProfileAbsher = requireUpdateProfileAbsher,
                                  //AbsherUserId = user.Id,
                              }));

            }
            catch (Exception ex)
            {
                return this.ResponseBadRequest(new ResponseViewModel(HttpStatusCode.OK, "حدث خطأ   من فضلك ادخل بيانات صحيحة", false, null));

            }
        }

        [AllowAnonymous]
        [Route("StudentResendActivateCode")]

        public async Task<IHttpActionResult> StudentResendActivateCode(ResendCodeModel model)
        {
            try
            {
                string Lang = lang;
                if (string.IsNullOrEmpty(model.Email))
                    return this.ResponseOK(new ResponseViewModel(HttpStatusCode.OK, Resource.IncorrectData, false, null));



                var CheckUserCode = _StudentBll.GetStudentEmail(model.Email);
                if (CheckUserCode == null)
                    return this.ResponseOK(new ResponseViewModel(HttpStatusCode.OK, Resource.IncorrectData, false, null));
                if (!String.IsNullOrEmpty(CheckUserCode.UserId))
                {
                    return this.ResponseOK(new ResponseViewModel(HttpStatusCode.OK, Resource.AlreadyActivated, false, null));
                }

                CheckUserCode.ActivatedCode = RandomToken(4);
                _StudentBll.UpdateActivatedCode(CheckUserCode);

                //string MESSAGE = "أهلاً بك في تطبيق المهنا.كود التفعيل هو  " + CheckUserCode.ActivatedCode;

                string msg = "تم إعادة ارسال كود التفعيل بنجاح.. يرجى مراجعة البريد الالكتروني و تفعيل الحساب  ";
                RegEmail mail = new RegEmail()
                {
                    firstname = CheckUserCode.Email,
                    cdate = string.Format("{0:MM/dd/yyyy}", DateTime.Now),
                    message = msg,
                    type = "طالب",
                    to = CheckUserCode.Email
                };
                var body = " عزيزي الطالب:" + model.Email + ", <br/> " + ", <br/> " + "كود التفعيل:" + CheckUserCode.ActivatedCode + "<br/> ";
                List<string> emails = new List<string>();
                emails.Add(CheckUserCode.Email);
                new MailerController().SendMail(emails, "اعادة ارسال كود التفعيل ", body);

                return this.ResponseOK(new ResponseViewModel(HttpStatusCode.OK, msg, true,
                              new RegisterResponseModel()
                              {
                                  // expired = 14,
                                  fullName = CheckUserCode.FirstName + " " + CheckUserCode.LastName,
                                  // token = tokenUser,
                                  email = CheckUserCode.Email,
                                  //allowNotify = user.allow_notify,
                                  id = CheckUserCode.StudentId,
                                  firstName = CheckUserCode.FirstName,
                                  lastName = CheckUserCode.LastName,
                                  mobile = CheckUserCode.Mobile,
                                  role = "Student",
                                  isComplete = false,
                                  IsNeedActivate = true,
                                  //NeedActivation=true
                                  //absherNo = user.absher.HasValue ? user.absher : 0,
                                  // online = true,
                                  //photo = URL + "/resources/users/" + model.
                              }));

            }
            catch (Exception ex)
            {
                return this.ResponseBadRequest(new ResponseViewModel(HttpStatusCode.OK, "حدث خطأ   من فضلك ادخل بيانات صحيحة", false, null));

            }
        }


        /// تسجيل معلم جديد   .
        /// </summary>
        [AllowAnonymous]
        [Route("TeacherRegister")]
        [ResponseType(typeof(RegisterUserModel))]
        public async Task<IHttpActionResult> TeacherRegister(RegisterUserModel model)
        {
            try
            {
                string Lang = lang;
                if (string.IsNullOrEmpty(model.email) || string.IsNullOrEmpty(model.password))
                    return this.ResponseOK(new ResponseViewModel(HttpStatusCode.OK, Resource.IncorrectData, false, null));
                if (string.IsNullOrEmpty(model.password) || model.password.Length < 6)
                    return this.ResponseOK(new ResponseViewModel(HttpStatusCode.OK, Resource.PasswordFormat, false, null));

                var check_username = await UserManager.FindByNameAsync(model.email);
                if (check_username != null)
                    return this.ResponseOK(new ResponseViewModel(HttpStatusCode.OK, Resource.UserUsedBefore, false, null));
                //var check_email = await UserManager.FindByEmailAsync(model.email);
                //if (check_email != null)
                //    return this.ResponseOK(new ResponseViewModel(HttpStatusCode.OK, "البريد الالكتروني المستخدم موجود مسبقاً", false, null));

                var check_email = _UsersBll.CheckUserEmail(model.email);
                if (check_email == true)
                    return this.ResponseOK(new ResponseViewModel(HttpStatusCode.OK, Resource.EmailUsedBefore, false, null));
                var check_mobile = _UsersBll.CheckUserMobile(model.mobile, "Teacher");
                if (check_mobile == true)
                    return this.ResponseOK(new ResponseViewModel(HttpStatusCode.OK, Resource.PhoneUsedBefore, false, null));

                int checkstudentEmail = _StudentBll.checkStudentEmail(model.email);
                int checkteacherEmail = _TeacherBll.checkTeacherEmail(model.email);

                if (checkstudentEmail > 0 || checkteacherEmail > 0)
                {
                    return this.ResponseOK(new ResponseViewModel(HttpStatusCode.OK, Resource.EmailUsedBefore, false, null));
                }
                string ActivateCode = RandomToken(4);
                //var user = new ApplicationUser()
                //{
                //    UserName = model.email,
                //    Email = model.email,
                //    PhoneNumber = model.mobile,
                //    status = 0,
                //    last_login = DateTime.Now,
                //    cdate = DateTime.Now,
                //    allow_notify = false,
                //    is_complete = 0,
                //    absher = 0,
                //    fullname = model.fullName,
                //    first_name = model.firstName,
                //    last_name = model.lastName,
                //    photo = "defaultm.jpg",
                //    country = model.country


                //};
                //IdentityResult result = await UserManager.CreateAsync(user, model.password);

                //if (!result.Succeeded)
                //    return this.ResponseOK(new ResponseViewModel(HttpStatusCode.OK, Resource.ErrorOccure, false, result.Succeeded));

                //await UserManager.AddToRoleAsync(user.Id, "Teacher");
                // var UserId = _UsersBll.AddUser(user.Id, user.Email, user.PhoneNumber, user.PasswordHash, 2);

                TeacherVM teacher = new TeacherVM()
                {
                    // userId = user.Id,
                    email = model.email,
                    mobile = model.mobile,
                    firstName = model.firstName,
                    lastName = model.lastName,
                    fullName = model.fullName,
                    passwordHash = Encrypt(model.password),
                    activatedCode = ActivateCode,
                    country = model.country

                };


                var TeacherId = _TeacherBll.Teacher_Add(teacher);
                //string phoneToken = "";
                //if (!string.IsNullOrEmpty(user.PhoneNumber))
                //    phoneToken = UserManager.GenerateChangePhoneNumberToken(user.Id, user.PhoneNumber);
                // SMS _sms = new SMS();
                string MESSAGE = "أهلاً بك في تطبيق المهنا.كود التفعيل هو  " + ActivateCode;
                // string RECIEVER = "966" + user.PhoneNumber.Substring(1);
                // _sms.sendmessage(RECIEVER, MESSAGE);               
                string msg = "";
                // if (model.role == "Student")
                // msg = "شكرأ لتسجيلك معنا في تطبيق درس خصوصي الآن يمكنك الاستفادة من التطبيق وتعزيز قدراتك العلمية من خلال مشاركتنا والاستفادة من خدماتنا ";
                //else
                msg = "شكرأ لتسجيلك معنا في تطبيق درس خصوصي  يرجى مراجعة البريد الالكتروني و تفعيل الحساب  ";
                RegEmail mail = new RegEmail()
                {
                    firstname = model.email,
                    cdate = string.Format("{0:MM/dd/yyyy}", DateTime.Now),
                    message = msg,
                    type = "معلم",
                    to = model.email
                };
                var body = " عزيزي المعلم:" + model.email + ", <br/> " + msg + "<br/> ";
                List<string> emails = new List<string>();
                emails.Add(model.email);
                new MailerController().SendMail(emails, "تسجيل معلم جديد ", body);

                // new Controllers.MailerController().RegEmail(mail).Deliver();

                return this.ResponseOK(new ResponseViewModel(HttpStatusCode.OK, null, true,
                              new RegisterResponseModel()
                              {
                                  // expired = 14,
                                  fullName = model.firstName + " " + model.lastName,
                                  // token = tokenUser,
                                  //userId = user.Id,
                                  email = model.email,
                                  //allowNotify = user.allow_notify,
                                  id = TeacherId.HasValue ? TeacherId.Value : 0,
                                  mobile = model.mobile,
                                  role = "Teacher",
                                  firstName = model.firstName,
                                  lastName = model.lastName,
                                  isComplete = false,
                                  IsNeedActivate = true,
                                  //absherNo = user.absher.HasValue ? user.absher : 0,
                                  //absher_id = user.absher_no,
                                  online = true,
                                  // photo = URL + "/resources/users/" + user.photo,
                                  country = model.country,
                                  //requireUpdateProfileAbsher = requireUpdateProfileAbsher,
                                  //AbsherUserId = user.Id,
                              }));

            }
            catch (Exception ex)
            {
                return this.ResponseBadRequest(new ResponseViewModel(HttpStatusCode.OK, Resource.ErrorOccure, false, null));

            }
        }


        [AllowAnonymous]
        [Route("TeacherActivateCode")]
        [ResponseType(typeof(RegisterUserModel))]
        public async Task<IHttpActionResult> TeacherActivateCode(ActivateCodeModel model)
        {
            try
            {
                string Lang = lang;
                if (string.IsNullOrEmpty(model.Email) || string.IsNullOrEmpty(model.Code))
                    return this.ResponseOK(new ResponseViewModel(HttpStatusCode.OK, Resource.IncorrectData, false, null));



                var CheckUserCode = _TeacherBll.CheckUserCode(model.Email, model.Code);
                if (CheckUserCode == null)
                    return this.ResponseOK(new ResponseViewModel(HttpStatusCode.OK, Resource.IncorrectData, false, null));
                if (!String.IsNullOrEmpty(CheckUserCode.UserId))
                {
                    return this.ResponseOK(new ResponseViewModel(HttpStatusCode.OK, Resource.AlreadyActivated, false, null));
                }
                var user = new ApplicationUser()
                {
                    UserName = CheckUserCode.Email,
                    Email = CheckUserCode.Email,
                    PhoneNumber = CheckUserCode.Mobile,
                    status = 0,
                    last_login = DateTime.Now,
                    cdate = DateTime.Now,
                    allow_notify = false,
                    is_complete = 0,
                    absher = 0,
                    fullname = CheckUserCode.FirstName + " " + CheckUserCode.LastName,
                    first_name = CheckUserCode.FirstName,
                    last_name = CheckUserCode.LastName,
                    photo = "defaultm.jpg",
                    country = CheckUserCode.RegCountry


                };
                var Password = Decrypt(CheckUserCode.PasswordHash);
                IdentityResult result = await UserManager.CreateAsync(user, Password);

                if (!result.Succeeded)
                    return this.ResponseOK(new ResponseViewModel(HttpStatusCode.OK, Resource.ErrorOccure, false, result.Succeeded));



                await UserManager.AddToRoleAsync(user.Id, "Teacher");
                _TeacherBll.Teacher_UpdateUser(CheckUserCode.TeacherId, user.Id);

                // var UserId = _UsersBll.AddUser(user.Id, user.Email, user.PhoneNumber, user.PasswordHash, 2);

                //string MESSAGE = "أهلاً بك في تطبيق المهنا.كود التفعيل هو  " + phoneToken;
                // string RECIEVER = "966" + user.PhoneNumber.Substring(1);
                // _sms.sendmessage(RECIEVER, MESSAGE);               
                string msg = "";
                // if (model.role == "Student")
                // msg = "شكرأ لتسجيلك معنا في تطبيق درس خصوصي الآن يمكنك الاستفادة من التطبيق وتعزيز قدراتك العلمية من خلال مشاركتنا والاستفادة من خدماتنا ";
                //else
                msg = "شكرأ لتسجيلك معنا في تطبيق درس خصوصي  يرجى الدخول الى التطبيق لاكمال ملفك الشخصي حتى تتمكن الادارة من مراجعة ملفك الشخصي وتفعيل حسابك كمعلم  ";
                RegEmail mail = new RegEmail()
                {
                    firstname = CheckUserCode.Email,
                    cdate = string.Format("{0:MM/dd/yyyy}", DateTime.Now),
                    message = msg,
                    type = "معلم",
                    to = CheckUserCode.Email
                };
                var body = " عزيزي المعلم:" + CheckUserCode.Email + ", <br/> " + msg + "<br/> ";
                List<string> emails = new List<string>();
                emails.Add(CheckUserCode.Email);
                new MailerController().SendMail(emails, "تسجيل معلم جديد ", body);

                var msg2 = "هناك طلب تسجيل جديد لمعلم:";
                var body2 = " عزيزي مدير النظام:" + ", <br/> " + msg2 + "<br/> " + CheckUserCode.Email;
                List<string> emails2 = _UsersBll.GetAdminEmails("3");
                new MailerController().SendMail(emails2, "تسجيل معلم جديد ", body2);
                // new Controllers.MailerController().RegEmail(mail).Deliver();

                string requireUpdateProfileAbsher = "";
                if (user.absher != 1 && user.country == "1")
                {
                    requireUpdateProfileAbsher = "2";
                }
                var Teacher = _TeacherBll.Teacher_GetByUserId(user.Id);
                if (CheckUserCode.TeacherId > 0)
                {
                    var tokenUser = requireUpdateProfileAbsher == "2" || (user.status == -2 || user.status == -1) || (Teacher.isActive != true) ? null : await GetToken(user);
                    return this.ResponseOK(new ResponseViewModel(HttpStatusCode.OK, null, true,
                                new RegisterResponseModel()
                                {
                                    expired = 14,
                                    fullName = user.fullname,
                                    token = tokenUser,
                                    userId = user.Id,
                                    email = user.Email,
                                    allowNotify = user.allow_notify,
                                    id = CheckUserCode.TeacherId,
                                    mobile = user.PhoneNumber,
                                    role = "Teacher",
                                    firstName = CheckUserCode.FirstName,
                                    lastName = CheckUserCode.LastName,
                                    isComplete = false,
                                    absherNo = user.absher.HasValue ? user.absher : 0,
                                    absher_id = user.absher_no,
                                    online = true,
                                    photo = URL + "/resources/users/" + user.photo,
                                    country = !String.IsNullOrEmpty(user.country) ? user.country : "0",
                                    requireUpdateProfileAbsher = requireUpdateProfileAbsher,
                                    AbsherUserId = user.Id,
                                })); ;
                }
                return this.ResponseBadRequest(new ResponseViewModel(HttpStatusCode.OK, Resource.ErrorOccure, false, null));
            }
            catch (Exception ex)
            {
                return this.ResponseBadRequest(new ResponseViewModel(HttpStatusCode.OK, Resource.ErrorOccure, false, null));

            }
        }

        /// <summary>
        /// التحقق من كود التفعيل 
        /// هتلاقي كود التفعيل راجعلك مع نتائج عملية التسجيل في متغير اسمو phone_code
        /// </summary>       
        /// <returns></returns>       
        [Route("ConfirmCode")]
        [ResponseType(typeof(string))]
        public IHttpActionResult ConfirmCode(string code, string mobile = null)
        {
            string Lang = lang;
            string id = User.Identity.GetUserId();
            var _user = UserManager.FindById(id);
            var res = UserManager.VerifyChangePhoneNumberToken(id, code, string.IsNullOrEmpty(mobile) ? _user.PhoneNumber : mobile);
            if (res)
            {
                _user.status = 1;
                _user.PhoneNumberConfirmed = true;
                _user.PhoneNumber = string.IsNullOrEmpty(mobile) ? _user.PhoneNumber : mobile;
                UserManager.Update(_user);
                return this.ResponseOK(new ResponseViewModel(HttpStatusCode.OK, Resource.PhoneConfirmed, true, null));
            }
            return this.ResponseOK(new ResponseViewModel(HttpStatusCode.OK, Resource.IncorrectActivatedCode, false, null));
        }
        /// <summary>
        /// ارسال كود التفعيل       
        /// </summary>       
        /// <returns></returns>       
        [Route("SendCode")]
        [ResponseType(typeof(string))]
        public IHttpActionResult SendCode(string mobile = null)
        {
            string Lang = lang;
            string id = User.Identity.GetUserId();
            var _user = UserManager.FindById(id);
            var phoneToken = UserManager.GenerateChangePhoneNumberToken(_user.Id, string.IsNullOrEmpty(mobile) ? _user.PhoneNumber : mobile);
            string myMobile = string.IsNullOrEmpty(mobile) ? _user.PhoneNumber : mobile;
            SMS _sms = new SMS();
            string MESSAGE = "أهلاً بك في تطبيق المهنا.كود التفعيل هو  " + phoneToken;
            string RECIEVER = "966" + myMobile.Substring(1);
            _sms.sendmessage(RECIEVER, MESSAGE);
            // "يرجى ادخال كود التفعيل المرسل على جوالك "
            return this.ResponseOK(new ResponseViewModel(HttpStatusCode.OK, Resource.EnterActivatedCode, true, null));

        }

        ///// <summary>
        /////   عرض الاشعارات.
        ///// </summary>
        //[Route("GetNotify")]
        //[ResponseType(typeof(List<NotifyListModel>))]
        //[HttpGet]
        //public IHttpActionResult GetNotify()
        //{
        //    string _user = User.Identity.GetUserId();
        //    using (var db = new AtnyDbEntities())
        //    {
        //        var items = db.User_Tr.Where(w => w.user_id == _user).Select(s => new NotifyListModel()
        //        {
        //            cdate = s.cdate,
        //            details = s.details,
        //            survey_id = s.survey_id,
        //            type = s.tr

        //        }).ToList();
        //        return this.ResponseOK(new ResponseViewModel(HttpStatusCode.OK, "تمت العملية بنجاح", true, items));
        //    }

        //}


        protected override void Dispose(bool disposing)
        {
            if (disposing && _userManager != null)
            {
                _userManager.Dispose();
                _userManager = null;
            }

            base.Dispose(disposing);
        }

        #region Helpers
        private bool IsBase64Encoded(String str)
        {
            try
            {
                // If no exception is caught, then it is possibly a base64 encoded string
                byte[] data = Convert.FromBase64String(str);
                // The part that checks if the string was properly padded to the
                // correct length was borrowed from d@anish's solution
                return (str.Replace(" ", "").Length % 4 == 0);
            }
            catch
            {
                // If exception is caught, then it is not a base64 encoded string
                return false;
            }
        }
        private string FixBase64ForImage(string Image)
        {
            System.Text.StringBuilder sbText = new System.Text.StringBuilder(Image, Image.Length);
            sbText.Replace("\r\n", String.Empty); sbText.Replace(" ", String.Empty);
            return sbText.ToString();
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
            Authentication.SignIn(cookiesIdentity);
            ////
            //هادا التوكن  Startup.OAuthOptions.AccessTokenFormat.Protect(ticket)
            ///
            var token = AuthenticationStartup.OAuthOptions.AccessTokenFormat.Protect(ticket);
            return token;



        }
        //private string FixBase64ForImage(string Image)
        //{
        //    System.Text.StringBuilder sbText = new System.Text.StringBuilder(Image, Image.Length);
        //    sbText.Replace("\r\n", String.Empty); sbText.Replace(" ", String.Empty);
        //    return sbText.ToString();
        //}
        private static void SaveFile(byte[] content, string path)
        {
            string filePath = GetFileFullPath(path);
            if (!Directory.Exists(Path.GetDirectoryName(filePath)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(filePath));
            }

            //Save file
            using (FileStream str = File.Create(filePath))
            {
                str.Write(content, 0, content.Length);
            }
        }
        private static string GetFileFullPath(string path)
        {
            string relName = path.StartsWith("~") ? path : path.StartsWith("/") ? string.Concat("~", path) : path;

            string filePath = relName.StartsWith("~") ? HostingEnvironment.MapPath(relName) : relName;

            return filePath;
        }
        //// POST api/Account/Logout
        //[Route("Logout")]
        //public IHttpActionResult Logout()
        //{
        //    Authentication.SignOut(CookieAuthenticationDefaults.AuthenticationType);
        //    return Ok();
        //}

        //// GET api/Account/ManageInfo?returnUrl=%2F&generateState=true
        //[Route("ManageInfo")]
        //public async Task<ManageInfoViewModel> GetManageInfo(string returnUrl, bool generateState = false)
        //{
        //    IdentityUser user = await UserManager.FindByIdAsync(User.Identity.GetUserId());

        //    if (user == null)
        //    {
        //        return null;
        //    }

        //    List<UserLoginInfoViewModel> logins = new List<UserLoginInfoViewModel>();

        //    foreach (IdentityUserLogin linkedAccount in user.Logins)
        //    {
        //        logins.Add(new UserLoginInfoViewModel
        //        {
        //            LoginProvider = linkedAccount.LoginProvider,
        //            ProviderKey = linkedAccount.ProviderKey
        //        });
        //    }

        //    if (user.PasswordHash != null)
        //    {
        //        logins.Add(new UserLoginInfoViewModel
        //        {
        //            LoginProvider = LocalLoginProvider,
        //            ProviderKey = user.UserName,
        //        });
        //    }

        //    return new ManageInfoViewModel
        //    {
        //        LocalLoginProvider = LocalLoginProvider,
        //        Email = user.UserName,
        //        Logins = logins,
        //        ExternalLoginProviders = GetExternalLogins(returnUrl, generateState)
        //    };
        //}

        //// POST api/Account/ChangePassword
        //[Route("ChangePassword")]
        //public async Task<IHttpActionResult> ChangePassword(ChangePasswordBindingModel model)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        return BadRequest(ModelState);
        //    }

        //    IdentityResult result = await UserManager.ChangePasswordAsync(User.Identity.GetUserId(), model.OldPassword,
        //        model.NewPassword);

        //    if (!result.Succeeded)
        //    {
        //        return GetErrorResult(result);
        //    }

        //    return Ok();
        //}

        //// POST api/Account/SetPassword
        //[Route("SetPassword")]
        //public async Task<IHttpActionResult> SetPassword(SetPasswordBindingModel model)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        return BadRequest(ModelState);
        //    }

        //    IdentityResult result = await UserManager.AddPasswordAsync(User.Identity.GetUserId(), model.NewPassword);

        //    if (!result.Succeeded)
        //    {
        //        return GetErrorResult(result);
        //    }

        //    return Ok();
        //}

        //// POST api/Account/AddExternalLogin
        //[Route("AddExternalLogin")]
        //public async Task<IHttpActionResult> AddExternalLogin(AddExternalLoginBindingModel model)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        return BadRequest(ModelState);
        //    }

        //    Authentication.SignOut(DefaultAuthenticationTypes.ExternalCookie);

        //    AuthenticationTicket ticket = AccessTokenFormat.Unprotect(model.ExternalAccessToken);

        //    if (ticket == null || ticket.Identity == null || (ticket.Properties != null
        //        && ticket.Properties.ExpiresUtc.HasValue
        //        && ticket.Properties.ExpiresUtc.Value < DateTimeOffset.UtcNow))
        //    {
        //        return BadRequest("External login failure.");
        //    }

        //    ExternalLoginData externalData = ExternalLoginData.FromIdentity(ticket.Identity);

        //    if (externalData == null)
        //    {
        //        return BadRequest("The external login is already associated with an account.");
        //    }

        //    IdentityResult result = await UserManager.AddLoginAsync(User.Identity.GetUserId(),
        //        new UserLoginInfo(externalData.LoginProvider, externalData.ProviderKey));

        //    if (!result.Succeeded)
        //    {
        //        return GetErrorResult(result);
        //    }

        //    return Ok();
        //}

        //// POST api/Account/RemoveLogin
        //[Route("RemoveLogin")]
        //public async Task<IHttpActionResult> RemoveLogin(RemoveLoginBindingModel model)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        return BadRequest(ModelState);
        //    }

        //    IdentityResult result;

        //    if (model.LoginProvider == LocalLoginProvider)
        //    {
        //        result = await UserManager.RemovePasswordAsync(User.Identity.GetUserId());
        //    }
        //    else
        //    {
        //        result = await UserManager.RemoveLoginAsync(User.Identity.GetUserId(),
        //            new UserLoginInfo(model.LoginProvider, model.ProviderKey));
        //    }

        //    if (!result.Succeeded)
        //    {
        //        return GetErrorResult(result);
        //    }

        //    return Ok();
        //}

        //// GET api/Account/ExternalLogin
        //[OverrideAuthentication]
        //[HostAuthentication(DefaultAuthenticationTypes.ExternalCookie)]
        //[AllowAnonymous]
        //[Route("ExternalLogin", Name = "ExternalLogin")]
        //public async Task<IHttpActionResult> GetExternalLogin(string provider, string error = null)
        //{
        //    if (error != null)
        //    {
        //        return Redirect(Url.Content("~/") + "#error=" + Uri.EscapeDataString(error));
        //    }

        //    if (!User.Identity.IsAuthenticated)
        //    {
        //        return new ChallengeResult(provider, this);
        //    }

        //    ExternalLoginData externalLogin = ExternalLoginData.FromIdentity(User.Identity as ClaimsIdentity);

        //    if (externalLogin == null)
        //    {
        //        return InternalServerError();
        //    }

        //    if (externalLogin.LoginProvider != provider)
        //    {
        //        Authentication.SignOut(DefaultAuthenticationTypes.ExternalCookie);
        //        return new ChallengeResult(provider, this);
        //    }

        //    ApplicationUser user = await UserManager.FindAsync(new UserLoginInfo(externalLogin.LoginProvider,
        //        externalLogin.ProviderKey));

        //    bool hasRegistered = user != null;

        //    if (hasRegistered)
        //    {
        //        Authentication.SignOut(DefaultAuthenticationTypes.ExternalCookie);

        //         ClaimsIdentity oAuthIdentity = await user.GenerateUserIdentityAsync(UserManager,
        //            OAuthDefaults.AuthenticationType);
        //        ClaimsIdentity cookieIdentity = await user.GenerateUserIdentityAsync(UserManager,
        //            CookieAuthenticationDefaults.AuthenticationType);

        //        AuthenticationProperties properties = ApplicationOAuthProvider.CreateProperties(user.UserName);
        //        Authentication.SignIn(properties, oAuthIdentity, cookieIdentity);
        //    }
        //    else
        //    {
        //        IEnumerable<Claim> claims = externalLogin.GetClaims();
        //        ClaimsIdentity identity = new ClaimsIdentity(claims, OAuthDefaults.AuthenticationType);
        //        Authentication.SignIn(identity);
        //    }

        //    return Ok();
        //}

        //// GET api/Account/ExternalLogins?returnUrl=%2F&generateState=true
        //[AllowAnonymous]
        //[Route("ExternalLogins")]
        //public IEnumerable<ExternalLoginViewModel> GetExternalLogins(string returnUrl, bool generateState = false)
        //{
        //    IEnumerable<AuthenticationDescription> descriptions = Authentication.GetExternalAuthenticationTypes();
        //    List<ExternalLoginViewModel> logins = new List<ExternalLoginViewModel>();

        //    string state;

        //    if (generateState)
        //    {
        //        const int strengthInBits = 256;
        //        state = RandomOAuthStateGenerator.Generate(strengthInBits);
        //    }
        //    else
        //    {
        //        state = null;
        //    }

        //    foreach (AuthenticationDescription description in descriptions)
        //    {
        //        ExternalLoginViewModel login = new ExternalLoginViewModel
        //        {
        //            Name = description.Caption,
        //            Url = Url.Route("ExternalLogin", new
        //            {
        //                provider = description.AuthenticationType,
        //                response_type = "token",
        //                client_id = Startup.PublicClientId,
        //                redirect_uri = new Uri(Request.RequestUri, returnUrl).AbsoluteUri,
        //                state = state
        //            }),
        //            State = state
        //        };
        //        logins.Add(login);
        //    }

        //    return logins;
        //}

        //// POST api/Account/Register
        //[AllowAnonymous]
        //[Route("Register")]
        //public async Task<IHttpActionResult> Register(RegisterBindingModel model)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        return BadRequest(ModelState);
        //    }

        //    var user = new ApplicationUser() { UserName = model.Email, Email = model.Email };

        //    IdentityResult result = await UserManager.CreateAsync(user, model.Password);

        //    if (!result.Succeeded)
        //    {
        //        return GetErrorResult(result);
        //    }

        //    return Ok();
        //}

        //// POST api/Account/RegisterExternal
        //[OverrideAuthentication]
        //[HostAuthentication(DefaultAuthenticationTypes.ExternalBearer)]
        //[Route("RegisterExternal")]
        //public async Task<IHttpActionResult> RegisterExternal(RegisterExternalBindingModel model)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        return BadRequest(ModelState);
        //    }

        //    var info = await Authentication.GetExternalLoginInfoAsync();
        //    if (info == null)
        //    {
        //        return InternalServerError();
        //    }

        //    var user = new ApplicationUser() { UserName = model.Email, Email = model.Email };

        //    IdentityResult result = await UserManager.CreateAsync(user);
        //    if (!result.Succeeded)
        //    {
        //        return GetErrorResult(result);
        //    }

        //    result = await UserManager.AddLoginAsync(user.Id, info.Login);
        //    if (!result.Succeeded)
        //    {
        //        return GetErrorResult(result); 
        //    }
        //    return Ok();
        //}
        private IAuthenticationManager Authentication
        {
            get { return Request.GetOwinContext().Authentication; }
        }

        private IHttpActionResult GetErrorResult(IdentityResult result)
        {
            if (result == null)
            {
                return InternalServerError();
            }

            if (!result.Succeeded)
            {
                if (result.Errors != null)
                {
                    foreach (string error in result.Errors)
                    {
                        ModelState.AddModelError("", error);
                    }
                }

                if (ModelState.IsValid)
                {
                    // No ModelState errors are available to send, so just return an empty BadRequest.
                    return BadRequest();
                }

                return BadRequest(ModelState);
            }

            return null;
        }

        private class ExternalLoginData
        {
            public string LoginProvider { get; set; }
            public string ProviderKey { get; set; }
            public string UserName { get; set; }

            public IList<Claim> GetClaims()
            {
                IList<Claim> claims = new List<Claim>();
                claims.Add(new Claim(ClaimTypes.NameIdentifier, ProviderKey, null, LoginProvider));

                if (UserName != null)
                {
                    claims.Add(new Claim(ClaimTypes.Name, UserName, null, LoginProvider));
                }

                return claims;
            }

            public static ExternalLoginData FromIdentity(ClaimsIdentity identity)
            {
                if (identity == null)
                {
                    return null;
                }

                Claim providerKeyClaim = identity.FindFirst(ClaimTypes.NameIdentifier);

                if (providerKeyClaim == null || String.IsNullOrEmpty(providerKeyClaim.Issuer)
                    || String.IsNullOrEmpty(providerKeyClaim.Value))
                {
                    return null;
                }

                if (providerKeyClaim.Issuer == ClaimsIdentity.DefaultIssuer)
                {
                    return null;
                }

                return new ExternalLoginData
                {
                    LoginProvider = providerKeyClaim.Issuer,
                    ProviderKey = providerKeyClaim.Value,
                    UserName = identity.FindFirstValue(ClaimTypes.Name)
                };
            }
        }

        private static class RandomOAuthStateGenerator
        {
            private static RandomNumberGenerator _random = new RNGCryptoServiceProvider();

            public static string Generate(int strengthInBits)
            {
                const int bitsPerByte = 8;

                if (strengthInBits % bitsPerByte != 0)
                {
                    throw new ArgumentException("strengthInBits must be evenly divisible by 8.", "strengthInBits");
                }

                int strengthInBytes = strengthInBits / bitsPerByte;

                byte[] data = new byte[strengthInBytes];
                _random.GetBytes(data);
                return HttpServerUtility.UrlTokenEncode(data);
            }
        }

        #endregion

        [AllowAnonymous]
        [Route("testNotification")]
        [ResponseCodes(HttpStatusCode.OK)]
        [HttpGet]
        public IHttpActionResult testNotification(string Token)
        {
            string Lang = lang;
            var outData = _comm.SendNotification("", Token, 3, "test notif");
            // "يرجى ادخال كود التفعيل المرسل على جوالك "
            return this.ResponseOK(new ResponseViewModel(HttpStatusCode.OK, outData, true, null));

        }

        [Authorize]
        [Route("RequireUpdateProfileAbsher")]
        [ResponseCodes(HttpStatusCode.OK)]
        [HttpGet]
        public IHttpActionResult RequireUpdateProfileAbsher()
        {
            string Lang = lang;
            var requireUpdateProfileAbsher = "0";
            var msg = "";

            var user = UserManager.FindById(User.Identity.GetUserId());
            if (User.IsInRole("Teacher"))
            {
                if (user.absher != 1 && String.IsNullOrEmpty(user.country))
                {
                    requireUpdateProfileAbsher = "1";
                    msg = Resource.NeedUpdateProfile;
                }
                else if (user.absher != 1 && user.country == "1")
                {
                    requireUpdateProfileAbsher = "2";
                    msg = Resource.NeedAbsher;
                }
            }

            dynamic obj = new { absher_no = user.absher_no, requireUpdateProfileAbsher = requireUpdateProfileAbsher };
            // "يرجى ادخال كود التفعيل المرسل على جوالك "
            return this.ResponseOK(new ResponseViewModel(HttpStatusCode.OK, msg, true, obj));

        }

        [AllowAnonymous]
        [Route("GetRequiredAbsher")]
        [ResponseCodes(HttpStatusCode.OK)]
        [HttpGet]
        public IHttpActionResult GetRequiredAbsher(string UserId)
        {
            string Lang = lang;
            var requireUpdateProfileAbsher = "0";
            var msg = "";

            var user = UserManager.FindById(UserId);
            if (User.IsInRole("Teacher"))
            {
                if (user.absher != 1 && String.IsNullOrEmpty(user.country))
                {
                    requireUpdateProfileAbsher = "1";
                    msg = Resource.NeedUpdateProfile;
                }
                else if (user.absher != 1 && user.country == "1")
                {
                    requireUpdateProfileAbsher = "2";
                    msg = Resource.NeedAbsher;
                }
            }

            dynamic obj = new { absher_no = user.absher_no, requireUpdateProfileAbsher = requireUpdateProfileAbsher };

            return this.ResponseOK(new ResponseViewModel(HttpStatusCode.OK, msg, true, obj));

        }

        private Random random = new Random();

        protected string RandomToken(int length)
        {
            const string chars = "0123456789";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }

    }
}