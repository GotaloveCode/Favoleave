using Leave.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Leave.Controllers
{
    public class HolidayController : Controller
    {
        private LeaveEntities db = new LeaveEntities();

        public ActionResult Index()
        {
            List<holiday> h = db.holidays.ToList();
            return View(h);
        }

        [CustomAuthorize(Roles = "Admin")]
        [HttpGet]
        public ActionResult Create()
        {
            HolidayVM h = new HolidayVM();
            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(HolidayVM h)
        {
            if (ModelState.IsValid)
            {
                holiday le = new holiday();
                le.name = h.name;
                le.date = h.date;
                le.recurs = h.recurs;
                db.holidays.Add(le);
                db.SaveChanges();
                TempData["message"] = "Holiday created successfully";
                return RedirectToAction("Index");
            }
            else
            {
                return View(h);
            }
        }

        public ActionResult Edit(int id)
        {
            holiday l = db.holidays.Find(id);
            if (l == null)
            {
                return HttpNotFound();
            }
            return View(l);
        }


        [CustomAuthorize(Roles = "Admin")]
        [HttpPost]
        public ActionResult Edit(holiday l)
        {
            if (ModelState.IsValid)
            {
                db.Entry(l).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            else
            {
                return View(l);
            }
        }


        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {
            holiday l = db.holidays.Find(id);
            if (l == null)
            {
                return Json(new { Success = "Holiday does not exist" });
            }
            db.holidays.Remove(l);
            db.SaveChanges();
            
            return Json(new { Success = true });
        }

        public JsonResult getHolidays(DateTime start, DateTime end)
        {
            var holidays = db.holidays.Where(x => x.date >= start && x.date <= end).Count();
            return Json(holidays, JsonRequestBehavior.AllowGet);
        }
       

    }
}
