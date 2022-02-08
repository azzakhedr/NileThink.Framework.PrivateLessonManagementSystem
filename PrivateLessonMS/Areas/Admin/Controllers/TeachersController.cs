
using Microsoft.Ajax.Utilities;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Newtonsoft.Json;
using NileThink.Framework.PrivateLessonManagementSystem.BLL.BussinessLayer;
using NileThink.Framework.PrivateLessonManagementSystem.BLL.ViewModels;
using NileThink.Framework.PrivateLessonManagementSystem.DAL.Models;
using PrivateLessonMS.Controllers;
using PrivateLessonMS.Helper;
using PrivateLessonMS.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace PrivateLessonMS.Areas.Admin.Controllers
{
    //[Authorize(Roles = "admin")]
    public class TeachersController : BaseController
    {

        Membership _mem = new Membership();
        TeacherBLL _teach = new TeacherBLL();

        TeacherRequestRefunedBll _teachRef = new TeacherRequestRefunedBll();

        SpecializationBLL _spec = new SpecializationBLL();
        EducationLevelBLL _eduLevel = new EducationLevelBLL();
        BankAccountBLL _bankAccount = new BankAccountBLL();
        StudentNewRequestBLL _newReq = new StudentNewRequestBLL();
        RatingBLL _rate = new RatingBLL();
        InvitesBLL _invite = new InvitesBLL();
        CourseBLL _course = new CourseBLL();
        AbsharDataBLL _absharData = new AbsharDataBLL();
        PackageBLL _pack = new PackageBLL();
        TeacherPackageBLL _teachPack = new TeacherPackageBLL();

        PaymentBLL _payment = new PaymentBLL();
        NotificationBLL _notificationBLL = new NotificationBLL();
        public ActionResult Index()
        {

            var item = _teach.GetTeachersLst(null).Select(s => new TeacherListAdmin()
            {
                absher = s.absherInfoId,
                // cdate = s.,
                email = s.email,
                fullname = s.firstName + " " + s.lastName,
                id = s.userId,
                mobile = s.mobile,
                photo = s.photo,
                status = s.status,
                teacherId = s.teacherId,
                isActive = s.isActive

            }).DistinctBy(c => c.teacherId).OrderByDescending(o => o.cdate).ToList();
            return View(item);

        }
        public PartialViewResult TeacherLst(bool? status)
        {
            var item = _teach.GetTeachersLst(status).Select(s => new TeacherListAdmin()
            {
                absher = s.absherInfoId,
                // cdate = s.,
                email = s.email,
                fullname = s.firstName + "" + s.lastName,
                id = s.userId,
                mobile = s.mobile,
                photo = s.photo,
                status = s.status,
                teacherId = s.teacherId,
                isActive = s.isActive

            }).DistinctBy(c => c.teacherId).OrderByDescending(o => o.cdate).ToList();
            return PartialView(item);
        }
        public ActionResult Create()
        {
            return View();
        }

        public ActionResult Edit(string id)
        {
            var teacerData = _teach.Teacher_GetByUserId(id);
            //TeacherRegisterResponseModel
            var TT = _teach.GetTeacherAvailabilty(teacerData.teacherId);

            //TT = db.Teacher_Times.Where(w => w.user_id == user_id).Select(s => new TeacherTimes() { day_number = s.day_number, from = s.from_time, to = s.to_time }).ToList();
            ViewBag.specializations = _spec.GetSpecializations().Select(s => new keyValue() { id = s.id, value = s.name }).ToList();
            ViewBag.branch_specialization = _spec.GetBranchSpecializations(null).Select(s => new keyValue() { id = s.id, value = s.name }).ToList();
            ViewBag.education_level = _eduLevel.GetEducationSubLevels(null, null).Select(s => new keyValue() { id = s.id, value = s.name }).ToList();
            ViewBag.banks = _bankAccount.GetTeacherBankAccount(teacerData.teacherId);
            ViewBag.absherData = _absharData.GetAbsharData(id);
            ViewBag.Courses = _course.getcourses(id, null, null, null, null).OrderByDescending(o => o.cdate).ToList();
            ViewBag.tTimes = TT;
            var user = _teach.Teacher_GetByUserId(id);
            if (user != null)
            {



                return View(user);
            }

            return RedirectToAction("Index", "Dashboard", new { area = "Admin" });

        }

        public ActionResult Requests()
        {

            var items = _course.getcourses(null, null, null, null, 0).AsEnumerable().Select(s => new NewRequestList()
            {
                cdate = s.cdate + "",
                id = s.id,
                //  period = s.period,
                start_date = s.start_date,
                start_time = s.start_date.HasValue ? s.start_date.Value.TimeOfDay : DateTime.Now.TimeOfDay,
                status = s.status,
                student_name = s.student_name,
                // details = s.details,
                // teaching_mechanism = s.teaching_mechanism,
                teacher_name = s.teacher_name,
                //  material = s.material
            }).OrderByDescending(o => o.id).ToList();
            return View(items);

        }

        public ActionResult EditRequest(int? id)
        {

            var c = _course.getcourses(null, null, id, null, null).FirstOrDefault();
            // ViewBag.banks = _bankAccount.GetBankAccount(c.teacher_id).OrderByDescending(o => o.Id).ToList();
            return View(c);

        }
        [HttpPost]
        public ActionResult EditRequest(EditCourse model)
        {

            _newReq.UpdateRequest(model);

            return getMessage(Enums.MStatus.check, "", "Requests", "Teachers");


        }

        public ActionResult Rates()
        {


            var items = _rate.GetRateType(2).Select(s => new RateModel()
            {
                id = s.Id,
                cdate = DateTime.Now,
                comment = s.Comment,
                course_title = s.title,
                rate = s.Rate,
                course_id = s.CourseID,
                std_id = s.student_id,
                student_name = s.stu_fullname,
                teacher_name = s.teach_fullname,
                photo = "/resources/users/" + s.photo

            }).OrderByDescending(o => o.id).ToList();
            return View(items);

        }
        //public ActionResult Invites_Teacher(int? id)
        //{

        //    ViewBag.Courses = _course.getcourses(null, null, null, null, 1).Select(s => new keyValue() { id = s.id, value = s.title }).ToList();
        //    return View();



        //}
        //[HttpPost]
        //public async Task<ActionResult> Invites_Teacher(Invite model, int? type)
        //{
        //    var user_id = Functions.GetUser().id;

        //    model.cdate = DateTime.Now;
        //    model.send_by = user_id;
        //    model.status = 0;
        //    model.invite_type = 1; //0 for student without reg 1 for student with reg
        //    //_invite.InsertInvite(model);
        //    if (type != 1)
        //    {
        //        var mail = new emailInviteT()
        //        {
        //            cdate = DateTime.Now + "",
        //            from = User.Identity.Name,
        //            to = model.email,
        //            title = "دعوة الانضمام الى تطبيق درس خصوصي",
        //            msg = "لقد تمت دعوتك للانضمام الى تطبيق درس خصوصي من قبل المعلم " + model.email + "يمكنك الآن الانضمام الى التطبيق وتسجيل بياناتك من خلال الروابط المرفقة الخاصة بتطبيق الأيفون والأندرويد"
        //        };
        //        new MailerController().InviteT(mail).Deliver();
        //    }

        //    if (type == 1)
        //    {
        //        using (var context = new ApplicationDbContext())
        //        {
        //            var UserManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(context));
        //            var checkUser = UserManager.FindByName(model.email);
        //            if (checkUser != null)
        //                return getMessage(Enums.MStatus.check, "البريد الالكتروني مستخدم مسبقاً", "Create", "Users");
        //            var user = new ApplicationUser
        //            {
        //                cdate = DateTime.Now,
        //                Email = model.email,
        //                fullname = model.std_name,
        //                status = 0,
        //                UserName = model.email,
        //                is_complete = 0
        //            };

        //            string pass = System.Web.Security.Membership.GeneratePassword(8, 3);
        //            var x = await UserManager.CreateAsync(user, pass);
        //            if (model.invite_type == 1)
        //                await UserManager.AddToRoleAsync(user.Id, "Teacher");
        //            var mail = new emailInviteT()
        //            {
        //                cdate = DateTime.Now + "",
        //                from = User.Identity.Name,
        //                to = model.email,
        //                title = "دعوة الانضمام الى تطبيق درس خصوصي",
        //                msg = "لقد تمت دعوتك للانضمام الى تطبيق درس خصوصي من قبل المعلم " + model.email + "يمكنك الآن الانضمام الى التطبيق وتسجيل بياناتك من خلال الروابط المرفقة الخاصة بتطبيق الأيفون والأندرويد",
        //                username = model.email,
        //                password = pass
        //            };
        //            new MailerController().InviteT(mail).Deliver();

        //        }
        //    }

        //    return getMessage(Enums.MStatus.check, "", "Index", "Invites");
        //}

        public ActionResult DeactiveTeacher(string id)
        {

            _teach.DeactiveTeacher(id);

            return getMessage(Enums.MStatus.check, "", "Index", "Teachers");

        }
        public ActionResult ActiveTeacher(string id)
        {
            //var checkUser = db.AspNetUsers.Where(w => w.Id == id).FirstOrDefault();
            //if (checkUser != null)
            //{
            //    checkUser.status = 1;
            //    checkUser.absher = 1;
            //    db.Entry(checkUser).State = EntityState.Modified;
            //    db.SaveChanges();
            //TeacherWiziQ tw = new TeacherWiziQ()
            //{
            //    email = checkUser.Email,
            //    name = checkUser.fullname,
            //    about_the_teacher = checkUser.details,
            //    password = "123456"
            //};
            //var x = WiziQ.WizIQClass.AddTeacher(tw);
            //if (!string.IsNullOrEmpty(x) && x.Substring(0, 20).Contains("ok"))
            //{
            //    rsp resultObject = Functions.DeserializeXML<rsp>(x);
            //    checkUser.wiziq_id = resultObject.add_teacher.teacher_id + "";
            //    db.Entry(checkUser).State = EntityState.Modified;
            //    db.SaveChanges();
            //}
            //using (System.IO.StreamWriter file = new System.IO.StreamWriter(Server.MapPath("~/debug.txt"), true))
            //    file.WriteLine("/n" + x);

            // }
            var teacerData = _teach.Teacher_GetByUserId(id);
            _teach.ActiveTeacher(id);

            //var lst = _notificationBLL.GetNotificationToken(2, teacerData.teacherId.ToString());
            var msg = "تم تفعيل حسابك كمعلم في درس خصوصى  ";
            var body = " عزيزي المعلم:" + teacerData.email + ", <br/> " + msg + "<br/> ";
            List<string> mails = new List<string>();
            mails.Add(teacerData.email);
            new MailerController().SendMail(mails, "تفعيل معلم", body);
            // var title = "تفعيل من الإدارة";

            //foreach (var item in lst)
            //{
            //    if (item.IsActive == false)
            //    {
            //        dynamic returndata = new
            //        {
            //            user_id = item.user_id,
            //            subject = "تم تفعيل طلبك من الإدارة",
            //            type = 11// رسالةتفعيل  من لوحة التحكم
            //        };
            //        string NotificationMessage = JsonConvert.SerializeObject(returndata);
            //        SendNotification(returndata, item.Token, 11, title);

            //    }

            //}
            return getMessage(Enums.MStatus.check, "", "Index", "Teachers");
        }



        public ActionResult Delete(string id)
        {
            _teach.DeleteTeacher(id);
            return getMessage(Enums.MStatus.check, "", "Index", "Teachers");

        }

        public ActionResult TeacherRefunedRequests()
        {

            var items = _teachRef.GetTeacherRequests(null, 0).ToList();
            return View(items);

        }

        public PartialViewResult TeacReqLst()
        {
            return PartialView();
        }
        public ActionResult ChangeTeacherReqStatus(long ReqId, int Status)
        {
            _teachRef.updateTeacherRequestStatus(Status, ReqId);


            return getMessage(Enums.MStatus.check, "", "TeacherRefunedRequests", "Teachers");

        }

        public ActionResult WalletTransaction()
        {

            var items = _teachRef.GetWalletTransaction(0, 0, 0).ToList();
            return View(items);

        }

        public async Task<ActionResult> EditTeacherPackage(int id)
        {
            ViewBag.TeacherLst = await _teach.TeacherLst();
            ViewBag.teacherId = id;
            var lst = _pack.GetAllPackages(id, "ar");
            return View(lst);
        }
        #region MultiAction
        public async Task<ActionResult> EditTeacherPackages(string Ids)
        {
            ViewBag.TeacherLst = await _teach.TeacherLst();
            ViewBag.teacherIds = String.IsNullOrEmpty(Ids) ? new List<string>() : Ids.Split(',').ToList();
            var lstIds = Ids.Split(',');
            if (lstIds.ToList().Count() == 1)
            {
                var lst = _pack.GetAllPackages(int.Parse(lstIds[0]), "ar");
                return View("EditTeacherPackage", lst);
            }

            var lst1 = _pack.GetAllPackages(null, "ar");
            return View("EditTeacherPackage", lst1);
        }
        public async Task<ActionResult> ActiveTeachers(string Ids)
        {
            var lst = await _teach.GetTeacherByIds(Ids.Split(',').ToList());
            _teach.ActiveTeachers(Ids);



            var title = "تفعيل من الإدارة";
            var msg = "تم تفعيل حسابك كمعلم في درس خصوصى  ";
            List<string> mails = new List<string>();
            foreach (var item in lst)
            {
                if (item.IsActive == false)
                {
                    //dynamic returndata = new
                    //{
                    //    user_id = item.UserId,
                    //    subject = "تم تفعيل طلبك من الإدارة",
                    //    type = 11// رسالةتفعيل  من لوحة التحكم
                    //};
                    //string NotificationMessage = JsonConvert.SerializeObject(returndata);
                    //SendNotification(returndata, item.Token, 11, title);
                    var body = " عزيزي المعلم:" + item.Email + ", <br/> " + msg + "<br/> ";
                    mails = new List<string>();
                    mails.Add(item.Email);
                    new MailerController().SendMail(mails, "تفعيل معلم", body);
                }

            }

            return getMessage(Enums.MStatus.check, "", "Index", "Teachers");
        }
        public ActionResult DeactiveTeachers(string Ids)
        {
            _teach.DeactiveTeachers(Ids);
            return getMessage(Enums.MStatus.check, "", "Index", "Teachers");
        }
        public ActionResult DeleteTeachers(string Ids)
        {
            _teach.DeleteTeachers(Ids);
            return getMessage(Enums.MStatus.check, "", "Index", "Teachers");
        }
        #endregion

        [HttpPost]
        public async Task<ActionResult> EditTeacherPackage(TeacherPackVM data)
        {
            _teach.UpdateTeacherFeesPackage(data);

            return getMessage(Enums.MStatus.check, "", "Index", "Teachers");
        }

        public async Task<ActionResult> AssignTeacherPackage(int id)
        {
            ViewBag.TeacherPack = _pack.GetTeacherPackageData(id, "ar");
            ViewBag.teacherId = id;
            var lst = _pack.GetAllPackages(id, "ar");
            return View(lst);
        }
        [HttpPost]
        public async Task<ActionResult> AssignTeacherPackage(int PackId, int TeacherId)
        {
            _teachPack.InsertUpdateTeacherPackage(PackId, TeacherId, true);


            return getMessage(Enums.MStatus.check, "", "AssignTeacherPackage", "Teachers", TeacherId);
        }
        [HttpPost]
        public async Task<ActionResult> DeleteTeacherPackage(long TeacherPackId, int TeacherId)
        {
            var outObj = _teachPack.deleteTeacherPackage(TeacherPackId, TeacherId);
            if (outObj == 1)
            {
                return getMessage(Enums.MStatus.check, "", "AssignTeacherPackage", "Teachers", TeacherId);
            }
            else if (outObj == -1)
            {
                return getMessage(Enums.MStatus.remove, "برجاء التأكد من الباقة المراد حذفها", "AssignTeacherPackage", "Teachers", TeacherId);
            }
            else if (outObj == -2)
            {

                return getMessage(Enums.MStatus.remove, "لا يمكن حذف الباقة لأنها مدفوعة", "AssignTeacherPackage", "Teachers", TeacherId);
            }
            else
            {
                return getMessage(Enums.MStatus.check, "حدث خطأ في البيانات", "AssignTeacherPackage", "Teachers", TeacherId);
            }

        }


        [HttpPost]
        public async Task<ActionResult> MarkPaidTeacherPackage(long TeacherPackId, int TeacherId)
        {
            var outObj = _teachPack.MarkPaidTeacherPackage(TeacherPackId, TeacherId);
            if (outObj == 1)
            {
                // _payment.InsertPaymentPackageWalletTransaction(TeacherPackId,TeacherId,)
                return getMessage(Enums.MStatus.check, "", "AssignTeacherPackage", "Teachers", TeacherId);
            }
            else if (outObj == -1)
            {
                return getMessage(Enums.MStatus.remove, "برجاء التأكد من الباقة المراد تغيير حالتها لمدفوع", "AssignTeacherPackage", "Teachers", TeacherId);
            }
            else if (outObj == -2)
            {

                return getMessage(Enums.MStatus.remove, "لا يمكن تغيير حالة الباقة لأنها مدفوعة بالفعل", "AssignTeacherPackage", "Teachers", TeacherId);
            }
            else
            {
                return getMessage(Enums.MStatus.check, "حدث خطأ في البيانات", "AssignTeacherPackage", "Teachers", TeacherId);
            }

        }

        [HttpPost]

        public void AbsherExcel(string Id)
        {
            AbsherData result = _absharData.GetAbsharData(Id);
            DataTable dt = ToDataTable(result);
            using (ClosedXML.Excel.XLWorkbook wb = new ClosedXML.Excel.XLWorkbook())
            {
                wb.Worksheets.Add(dt, "DataAbsher");

                Response.Clear();
                Response.Buffer = true;
                Response.Charset = "";
                Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                Response.AddHeader("content-disposition", "attachment;filename=Customers.xlsx");
                using (System.IO.MemoryStream MyMemoryStream = new System.IO.MemoryStream())
                {
                    wb.SaveAs(MyMemoryStream);
                    MyMemoryStream.WriteTo(Response.OutputStream);
                    Response.Flush();
                    Response.End();
                }
            }
        }

        public DataTable ToDataTable<T>(T item)
        {
            DataTable dataTable = new DataTable(typeof(T).Name);
            PropertyInfo[] Props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (PropertyInfo prop in Props)
            {
                dataTable.Columns.Add(prop.Name);
            }

            var values = new object[Props.Length];
            for (int i = 0; i < Props.Length; i++)
            {
                values[i] = Props[i].GetValue(item, null);
            }
            dataTable.Rows.Add(values);

            return dataTable;
        }
    }
}