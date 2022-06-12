using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using NileThink.Framework.PrivateLessonManagementSystem.BLL.BussinessLayer;
using NileThink.Framework.PrivateLessonManagementSystem.BLL.ViewModels;
using NileThink.Framework.PrivateLessonManagementSystem.Web.Models;
using PrivateLessonMS.Controllers;
using PrivateLessonMS.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace PrivateLessonMS.Areas.Admin.Controllers
{
    public class AdvertiseBannersController : BaseController
    {
        AdvertiseBannersBLL _advertiseBanner = new AdvertiseBannersBLL();

        public ActionResult Index()
        {
            var lst = _advertiseBanner.GetAllAdvertiseBanners();
            return View(lst);
        }

        public ActionResult Create()
        {

            return View("AddEdit");
        }

        public ActionResult Update(int Id)
        {
            var lst = _advertiseBanner.GetAllAdvertiseBanners(Id).FirstOrDefault();
            return View("AddEdit", lst);
        }

        public ActionResult AddEdit(int? Id = 0)
        {
            if (Id > 0)
            {
                var lst = _advertiseBanner.GetAllAdvertiseBanners(Id).FirstOrDefault();
                return View(lst);
            }
            else
            {
                return View();
            }

        }

        public ActionResult RemoveBanner(int id)
        {

            _advertiseBanner.DeleteBanner(id);
            return getMessage(Enums.MStatus.check, "", "Index", "AdvertiseBanners");

        }
        [HttpPost]
        public ActionResult AddEdit(AdvertiseBannersVM model, HttpPostedFileBase photo_file)
        {

            var lst = _advertiseBanner.GetAllAdvertiseBanners(0).ToList();
            if (model.is_top_teacher_index1 == 1)
            {

                model.is_top_teacher_index = true;
            }
            else
            {
                model.is_top_teacher_index = false;
            }
            if (model.is_top_teacher_profile1 == 1)
            {
                model.is_top_teacher_profile = true;
            }
            else
            {
                model.is_top_teacher_profile = false;
            }
            if (model.is_top_request1 == 1)
            {
                model.is_top_request = true;
            }
            else
            {
                model.is_top_request = false;
            }

            if (model.is_active1 == 1)
            {
                model.IsActive = true;
            }
            else
            {
                model.IsActive = false;
            }


            if (photo_file != null)
            {
                Tuple<bool, string> imgValidation = Functions.ValidateBannerImage(photo_file);
                if (!imgValidation.Item1)
                {
                    TempData["Message"] = imgValidation.Item2;
                    TempData["Status"] = "danger";
                    return View(model);
                }

                string res = Functions.SaveTempFile(photo_file, "~/resources/users/");
                model.photo_img = "/resources/users/" + res;
            }
            if (model.to_time.Date < model.from_date.Date)
            {
                TempData["Message"] = "تاريخ نهاية الإعلان لابد أن يكون أكبر من أو يساوى تاريخ البداية";
                TempData["Status"] = "danger";
                return View(model);

            }
            var obj = lst.Where(C => model.IsActive == true && C.IsActive == model.IsActive && C.Id != model.Id &&

              (
             (model.is_top_teacher_index == true && C.is_top_teacher_index == true)
             ||
             (model.is_top_teacher_profile == true && C.is_top_teacher_profile == true)
             ||
             (model.is_top_request == true && C.is_top_request == true)
              )
              &&
              (
              (C.from_date.Date == model.from_date.Date)
              || (C.to_time.Date == model.to_time.Date)
              ||
              (
             (C.from_date.Date < model.from_date) &&
             ((C.to_time.Date > model.from_date.Date && C.to_time.Date < model.to_time.Date)
              || (C.to_time.Date > model.to_time.Date)
              )
              )
               ||
              (C.from_date.Date > model.from_date.Date &&
             (model.to_time.Date > C.from_date.Date && model.to_time.Date < C.to_time.Date)

              )
              )

            ).FirstOrDefault();
            if (obj != null
                 )
            {

                TempData["Message"] = "لا يمكن اتمام العملية نظرا لوجود فترة تقاطع لنفس نوع البانر";
                TempData["Status"] = "danger";
                return View(model);
                //return getMessage(Enums.MStatus.danger, "لا يمكن اتمام العملية نظرا لوجود فترة تقاطع لنفس نوع البانر ", "AddEdit", "AdvertiseBanners",model.Id);
            }

            _advertiseBanner.InsertUpdateAdvertiseBanners(model);
            return getMessage(Enums.MStatus.check, "", "Index", "AdvertiseBanners");
            // }
        }

    }

}
