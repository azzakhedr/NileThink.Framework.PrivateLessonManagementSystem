
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace PrivateLessonMS.Areas.Teacher.Controllers
{
    //public class PaymentsController : Controller
    //{
    //    // GET: Teacher/Payments
    //    public ActionResult Index()
    //    {
    //        var user_id = Functions.GetUser().id;
    //        ViewBag.rate = Functions.getOption("site_rate");
    //        using (var db = new MhanaDbEntities())
    //        {
    //            var items = db.Courses.AsEnumerable().Where(w => w.teacher_id == user_id).Select(s => new CourseListAdmin()
    //            {
    //                cdate = s.cdate + "",
    //                id = s.Id,
    //                lectures_count = s.lectures_count,
    //                period = s.period,
    //                photo = s.photo,
    //                start_date = s.start_date,
    //                start_time = s.start_date.HasValue ? s.start_date.Value.TimeOfDay : DateTime.Now.TimeOfDay,
    //                status = s.status,
    //                title = s.title,
    //                teacher_name = s.AspNetUser.fullname,
    //                total_std = s.total_std,
    //                cost = s.cost
    //            }).OrderByDescending(o => o.id).ToList();
    //            return View(items);
    //        }
    //    }


    //    public ActionResult BankAccount(int? id)
    //    {
    //        var user_id = Functions.GetUser().id;

    //        using (var db = new MhanaDbEntities())
    //        {
    //            var items = db.Banks_Account.AsEnumerable().Where(w => w.user_id == user_id).OrderByDescending(o => o.Id).ToList();
    //            ViewBag.item = items.Where(w => w.Id == id).FirstOrDefault();
    //            return View(items);
    //        }
    //    }       
    //    [HttpPost]
    //    public ActionResult BankAccount(Banks_Account model, int? edit_id)
    //    {
    //        var user_id = Functions.GetUser().id;
    //        using (var db = new MhanaDbEntities())
    //        {
    //            if (edit_id.HasValue)
    //            {
    //                var item = db.Banks_Account.Where(w => w.Id == model.Id).FirstOrDefault();
    //                if (item != null)
    //                {
    //                    item.bank_account = model.bank_account;
    //                    item.bank_name = model.bank_name;
    //                    item.bank_account_name = model.bank_account_name;
    //                    db.Entry(item).State = EntityState.Modified;
    //                    db.SaveChanges();
    //                    return RedirectToAction("BankAccount", "Payments");
    //                }
    //            }
    //            var co = new Banks_Account()
    //            {
    //                bank_account = model.bank_account,
    //                bank_name = model.bank_name,
    //                bank_account_name = model.bank_account_name,
    //                user_id = user_id,
    //                cdate = DateTime.Now
    //            };
    //            db.Banks_Account.Add(co);
    //            db.SaveChanges();
    //            return RedirectToAction("BankAccount", "Payments");
    //        }
    //    }

    //}
}