using NileThink.Framework.PrivateLessonManagementSystem.BLL.BussinessLayer;
using NileThink.Framework.PrivateLessonManagementSystem.BLL.ViewModels;
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
            ViewBag.item = id.HasValue ? items.Where(w => w.id == id).FirstOrDefault() : null; ;
            return View(items);

        }

        public ActionResult RemoveSpecialization(int id)
        {


            _spec.RemoveSpecialization(id);
            return getMessage(Enums.MStatus.check, "", "Specialization", "Settings");

        }

        [HttpPost]
        public ActionResult Specialization(string name, int? edit_id = 0)
        {

            _spec.InsertUpdateSpecialization(edit_id.Value, name);
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

            _spec.InsertUpdateSpecialization(edit_id.Value, name);
            return getMessage(Enums.MStatus.check, "", "Specialization", "Settings");

        }


        public ActionResult branch(int? id)
        {

            var items = _spec.GetBranchSpecializations(null);
            ViewBag.item = id.HasValue ? _spec.GetBranchSpecializationById(id.Value) : null;
            ViewBag.list = _spec.GetSpecializations();
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


        public ActionResult edu_sub_level(int? id)
        {

            var items = _eduLevel.GetEducationSubLevels(null,null).OrderByDescending(o => o.id).ToList();
            ViewBag.item = id.HasValue ? items.Where(w => w.id == id).FirstOrDefault() : null;
            ViewBag.list = _spec.GetSpecializations();
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
    }
}