using NileThink.Framework.PrivateLessonManagementSystem.BLL.BussinessLayer;
using NileThink.Framework.PrivateLessonManagementSystem.BLL.ViewModels;
using NileThink.Framework.PrivateLessonManagementSystem.DAL.Models;
using PrivateLessonMS.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace PrivateLessonMS.Areas.Admin.Controllers
{
    ////[Authorize(Roles = "3")]
    public class SettingsController : BaseController
    {
        SettingBLL _set = new SettingBLL();
        SpecializationBLL _spec = new SpecializationBLL();
        EducationLevelBLL _eduLevel = new EducationLevelBLL();

        CertificateTypesBLL _certificate = new CertificateTypesBLL();

        MembershipPackageBLL _membershipBll = new MembershipPackageBLL();
        // GET: Admin/Settings
        public ActionResult Index()
        {

            var items = _set.GetSettings();
            ViewBag.item = items;
            return View();

        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Index(FormCollection form)
        {
            var items = _set.GetSettings();
            foreach (var key in form.AllKeys)
            {
                _set.InsertUpdateSetting(new SettingVM() { Id = 0, key = key, value = Request.Form[key] });


            }

            return getMessage(Enums.Status.Visible, "", "Index", "Settings");

        }
        public ActionResult Specialization(int? id)
        {

            var items = _spec.GetSpecializations().OrderByDescending(o => o.id).ToList(); ;
            var item = id.HasValue ? items.Where(w => w.id == id).FirstOrDefault() : null;
            ViewBag.item = item;
            ViewBag.mainId = item != null && item.mainId > 0 ? item.mainId : 0;

            ViewBag.LevelId = item != null ? item.EducationLevelId : 0;
            ViewBag.EducationLevels = _eduLevel.GetEducationLevels().OrderByDescending(o => o.id).ToList();
            //if (id > 0)
            //{
            //    ViewBag.EducationSubLevels = _eduLevel.GetEducationSubLevels(null, 9).OrderByDescending(o => o.id).ToList();

            //}
            return View(items);

        }

        [HttpPost]
        public JsonResult LevelChange(int LevelId)
        {
            return Json(_eduLevel.GetEducationSubLevels(null, LevelId).OrderByDescending(o => o.id).ToList().Select(c => new { id = c.id, name = c.name }).ToList());

        }

        [HttpPost]
        public JsonResult SubLevelChange(int SubLevelId)
        {
            return Json(_spec.GetSpecializations().Where(c => c.mainId == SubLevelId).OrderByDescending(o => o.id).ToList().Select(c => new { id = c.id, name = c.name }).ToList());

        }

        public ActionResult RemoveSpecialization(int id)
        {




            try
            {
                _spec.RemoveSpecialization(id);
                return getMessage(Enums.MStatus.check, "", "Specialization", "Settings");
            }
            catch
            {
                //return getMessage(Enums.MStatus.remove, "", "branch", "Settings");
                TempData["Message"] = " تعذر حذف التخصص لارتباطه بالمعلمين والدروس";
                TempData["Status"] = "danger";
                var items = _spec.GetSpecializations().OrderByDescending(o => o.id).ToList();

                return View("Specialization", items);
            }

        }

        [HttpPost]
        public ActionResult Specialization(SpecializationVM data)
        {

            _spec.InsertUpdateSpecialization(data.id, data.name, data.mainId);
            return getMessage(Enums.MStatus.check, "", "Specialization", "Settings");

        }

        public ActionResult mail_edu_level(int? id)
        {
            var items = _eduLevel.GetEducationLevels().OrderByDescending(o => o.id).ToList();
            ViewBag.item = id.HasValue ? items.Where(w => w.id == id).FirstOrDefault() : null;
            return View(items);

        }
        [HttpPost]
        public ActionResult mail_edu_level(string name, int? edit_id = 0)
        {

            _spec.InsertUpdateEducationLevel(edit_id.Value, name);
            return getMessage(Enums.MStatus.check, "", "education_level", "Settings");

        }


        public ActionResult Material(int? id)
        {

            var items = _spec.GetSpecializations().OrderByDescending(o => o.id).ToList();
            ViewBag.item = id.HasValue ? items.Where(w => w.id == id).FirstOrDefault() : null; ;
            return View(items);

        }
        [HttpPost]
        public ActionResult Material(string name, int? edit_id = 0)
        {

            _spec.InsertUpdateSpecialization(edit_id.Value, name, 0);
            return getMessage(Enums.MStatus.check, "", "Specialization", "Settings");

        }


        public ActionResult branch(int? id)
        {

            var items = _spec.GetBranchSpecializations(null);
            var item = id.HasValue ? _spec.GetBranchSpecializationById(id.Value) : null;
            ViewBag.item = item;
            ViewBag.list = _spec.GetSpecializations();

            var mainId = id.HasValue ? item.specializationId : 0;
            ViewBag.mainId = mainId;

            var SubLevelId = id.HasValue ? item.education_sub_level_id : 0;
            ViewBag.SubLevelId = SubLevelId;
            ViewBag.LevelId = id.HasValue ? item.EducationLevelId : 0;
            ViewBag.EducationLevels = _eduLevel.GetEducationLevels().OrderByDescending(o => o.id).ToList();


            return View(items);

        }

        public ActionResult RemoveBranch(int id)
        {

            _spec.RemoveSpecializationBranch(null, id);
            return getMessage(Enums.MStatus.check, "", "branch", "Settings");


        }



        [HttpPost]
        public ActionResult branch(branch_specialization model)
        {

            _spec.InsertUpdateBranchSpecialization(model);
            return getMessage(Enums.MStatus.check, "", "branch", "Settings");

        }


        public ActionResult edu_sub_level(int? id, int? mainId)
        {

            var items = _eduLevel.GetEducationSubLevels(null, null).OrderByDescending(o => o.id).ToList();
            var item = id.HasValue ? items.Where(w => w.id == id).FirstOrDefault() : null;
            ViewBag.item = item;
            ViewBag.mainId = mainId.HasValue ? mainId : (item != null ? item.mainId : null);
            ViewBag.list = _eduLevel.GetEducationLevels().OrderByDescending(o => o.id).ToList();
            return View(items);

        }
        [HttpPost]
        public ActionResult edu_sub_level(EducationSublevelVM model)
        {


            _eduLevel.InsertUpdateEducationSubLevel(model);
            return getMessage(Enums.MStatus.check, "", "edu_sub_level", "Settings");

        }


        public ActionResult levels(int? id)
        {


            var items = _eduLevel.GetEducationLevels().OrderByDescending(o => o.id).ToList();
            ViewBag.item = id.HasValue ? items.Where(w => w.id == id).FirstOrDefault() : null;
            return View(items);

        }
        [HttpPost]
        public ActionResult levels(EducationLevelVM model)
        {

            _eduLevel.InsertUpdateEducationLevel(model);
            return getMessage(Enums.MStatus.check, "", "levels", "Settings");

        }

        public ActionResult Removelevel(int id)
        {

            _eduLevel.RemoveEduLevel(id);
            return getMessage(Enums.MStatus.check, "", "levels", "Settings");

        }
        public ActionResult RemoveSublevel(int id)
        {

            _eduLevel.RemoveSubEduLevel(id);
            return getMessage(Enums.MStatus.check, "", "edu_sub_level", "Settings");

        }




        public ActionResult CertificateTypes(int? id)
        {
            var items = _certificate.GetCertificateTypes().OrderByDescending(o => o.id).ToList();
            ViewBag.item = id.HasValue ? items.Where(w => w.id == id).FirstOrDefault() : null;
            return View(items);

        }
        [HttpPost]
        public ActionResult CertificateTypes(CertificateTypesVM model)
        {
            _certificate.InsertUpdateCertificateTypes(model);
            return getMessage(Enums.MStatus.check, "", "CertificateTypes", "Settings");

        }

        public ActionResult RemoveCertificateType(int id)
        {
            _certificate.deleteCertificateTypes(id);
            return getMessage(Enums.MStatus.check, "", "CertificateTypes", "Settings");

        }

        #region ------- membership -----
        public ActionResult MembershipPackages()
        {
            var lst = _membershipBll.GetMembershipPackages();
            return View(lst);
        }

        public ActionResult Edit(int Id)
        {
            var lst = _membershipBll.GetMembershipPackageById(Id);
            return View(lst);
        }
        [HttpPost]
        public ActionResult Edit(tbl_membership data)
        {

            _membershipBll.AddEditMembershipPackage(data);
            return getMessage(Enums.MStatus.check, "", "MembershipPackages", "Settings");
        }


      
        public ActionResult DeleteMemebership(long id)
        {

            _membershipBll.DeleteMembershipPackage(id);
            return getMessage(Enums.MStatus.check, "", "MembershipPackages", "Settings");
        }
        public ActionResult Add()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Add(tbl_membership data)
        {

            _membershipBll.AddEditMembershipPackage(data);
            return getMessage(Enums.MStatus.check, "", "MembershipPackages", "Settings");
        }
        #endregion

    }
}