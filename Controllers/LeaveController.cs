using CrystalDecisions.CrystalReports.Engine;
using CrystalDecisions.Shared;
using Leave.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Core.EntityClient;
using System.Data.Entity.Core.Objects;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Leave.Controllers
{
    public class LeaveController : Controller
    {
        private LeaveEntities db = new LeaveEntities();

        [CustomAuthorize(Roles = "Admin,User")]
        public ActionResult Index()
        {

            return View();
        }

        [CustomAuthorize(Roles = "Admin")]
        [HttpGet]
        public ActionResult Period()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Period(leave_period l)
        {
            if (ModelState.IsValid)
            {
                //check for active period 1st
                var activeperiod = db.leave_period.Where(x => x.active == 1);
                if (activeperiod.Any() && l.active == 1) //if new period is set active set existing period as inactive
                {
                    var row = activeperiod.Single();
                    row.active = 0;
                    db.Entry(row).State = EntityState.Modified;
                }
                db.leave_period.Add(l);
                db.SaveChanges();
                EntityConnection ec = new EntityConnection(System.Configuration.ConfigurationManager.ConnectionStrings["LeaveEntities"].ConnectionString);
                SqlConnection sqlConn = ec.StoreConnection as SqlConnection;
                using (SqlConnection con = new SqlConnection(sqlConn.ConnectionString))
                {
                    using (SqlCommand command = con.CreateCommand())
                    {
                        //update all recurring holidays to current year
                        command.CommandText = "UPDATE dbo.holiday SET date = DATEADD(YEAR,(YEAR(GetDate())- year(date)),date) where recurs =1";

                        con.Open();

                        command.ExecuteNonQuery();

                        con.Close();
                    }
                }
                TempData["message"] = "New Period added successfully";
                return RedirectToAction("Home", "Home");
            }
            else
            {
                ModelState.AddModelError("", "Please correct the below errors to proceed");
                return View(l);
            }
        }

        [CustomAuthorize(Roles = "Admin")]
        [HttpGet]
        public ActionResult PeriodList()
        {
            var lst = db.leave_period.ToList();
            return PartialView(lst);
        }

        [CustomAuthorize(Roles = "Admin")]
        [HttpGet]
        public ActionResult EditPeriod(int id)
        {
            leave_period l = db.leave_period.Find(id);
            if (l == null)
            {
                return HttpNotFound();
            }
            return View(l);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditPeriod(leave_period l)
        {
            if (ModelState.IsValid)
            {
                db.Entry(l).State = EntityState.Modified;
                if (l.active == 1)
                {
                    int lp = db.leave_period.Where(x => x.active == 1).Select(x => x.id).SingleOrDefault();
                    if (lp != 0)
                    {
                        leave_period lp1 = db.leave_period.Find(lp);
                        lp1.active = 0;
                        db.Entry(lp1).State = EntityState.Modified;
                    }
                }
                db.SaveChanges();
                return RedirectToAction("Period");
            }
            return View(l);
        }

        [CustomAuthorize(Roles = "Admin")]
        public ActionResult DeletePeriod(int id = 0)
        {
            leave_period l = db.leave_period.Find(id);
            if (l == null)
            {
                TempData["message"] = "Period does not exist";
                return RedirectToAction("Home", "Home");

            }
            db.leave_period.Remove(l);
            db.SaveChanges();
            return RedirectToAction("Period");
        }

        [CustomAuthorize(Roles = "Admin")]
        [HttpGet]
        public ActionResult AddEntitlement()
        {
            AddEntitlementVM e = new AddEntitlementVM();
            e.employees = new SelectList(db.Employees.Select(c => new { c.EmployeeID, names = c.SurName + " " + c.OtherNames })
                .OrderBy(c => c.names).ToList(), "EmployeeID", "names");
            e.leave_types = new SelectList(db.leave_type, "id", "name");
            return View(e);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddEntitlement(AddEntitlementVM e)
        {
            int exist = (db.leave_entitlement
                .Where(x => x.employee_id == e.employee_id && (x.leave_type_id == e.leave_type_id))
                .Select(x => x.id)).SingleOrDefault();
            if (exist != 0)
            {
                ModelState.AddModelError("", "Employee entitlement to leave already assigned");
            }
            var dated = db.leave_period.Where(x => x.active == 1)
                       .Select(x => new TempDateClass { sdate = x.startdate, edate = x.enddate }).SingleOrDefault();
            if (dated == null)
            {
                ModelState.AddModelError("", "You must set an active period to assign employee leave entitlement");
            }
            if (ModelState.IsValid)
            {
                try
                {
                    leave_entitlement le = new leave_entitlement();
                    le.leave_type_id = e.leave_type_id;
                    le.employee_id = e.employee_id;
                    le.from_date = dated.sdate;
                    le.to_date = dated.edate;
                    le.credited_date = DateTime.Now;
                    le.created_by_id = db.users.Where(x => x.username == User.Identity.Name).Select(x => x.id).SingleOrDefault();

                    db.leave_entitlement.Add(le);

                    DateTime mydate = DateTime.Parse("01/01/1900");
                    leave l = new leave();
                    l.employee_id = e.employee_id;
                    l.startdate = mydate;
                    l.enddate = mydate;
                    l.days = 0;
                    l.comments = "default";
                    l.leave_type_id = e.leave_type_id;
                    l.employee_id = e.employee_id;
                    l.date_applied = DateTime.Now;
                    l.period_id = 1; //assign to our default period 

                    db.leaves.Add(l);

                    db.SaveChanges();


                }
                catch (Exception ex)
                {
                    TempData["message"] = ex.Message;
                }

                return RedirectToAction("Entitlement");
            }
            else
            {
                e.employees = new SelectList(db.Employees.Select(c => new { c.EmployeeID, names = c.SurName + " " + c.OtherNames }).OrderBy(c => c.names).ToList(), "EmployeeID", "names");
                e.leave_types = new SelectList(db.leave_type, "id", "name");
                ModelState.AddModelError("", "Correct the below errors to proceed");
                return View(e);
            }

        }

        [CustomAuthorize(Roles = "Admin,User")]
        public ActionResult Entitlement()
        {
            var ent = (from e in db.Employees
                       join le in db.leave_entitlement on e.EmployeeID equals le.employee_id
                       join lt in db.leave_type on le.leave_type_id equals lt.id
                       where e.IsDisengaged != true
                       select new EntitlementVM
                       {
                           employee = e.SurName + " " + e.OtherNames, // or pc.ProdId
                           leave_type = lt.name,
                           valid_period = le.from_date + "-" + le.to_date,
                           employee_id = e.EmployeeID,
                           leave_entitlement_id = le.id,
                       }).ToList();
            return View(ent);
        }

        [CustomAuthorize(Roles = "Admin")]
        public ActionResult EditEntitlement(int id)
        {
            var ent = (from e in db.Employees
                       join le in db.leave_entitlement on e.EmployeeID equals le.employee_id
                       join lt in db.leave_type on le.leave_type_id equals lt.id
                       where le.id == id && e.IsDisengaged != true
                       select new EntitlementVM
                       {
                           employee = e.SurName + " " + e.OtherNames,
                           leave_type = lt.name,
                           valid_period = le.from_date + "-" + le.to_date,
                           employee_id = e.EmployeeID,
                           leave_entitlement_id = le.id,
                           leave_type_id = le.leave_type_id,
                       }).SingleOrDefault();
            @ViewBag.leave_types = new SelectList(db.leave_type, "id", "name");
            return View(ent);
        }

        [CustomAuthorize(Roles = "Admin")]
        public ActionResult DeleteEntitlement(int id = 0)
        {
            leave_entitlement l = db.leave_entitlement.Find(id);
            if (l == null)
            {
                TempData["message"] = "No Leave Entitlemet of id:" + id + " exists";
            }
            db.leave_entitlement.Remove(l);
            db.SaveChanges();
            return RedirectToAction("Entitlement");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditEntitlement(EntitlementVM e)
        {
            var exist = (db.leave_entitlement
               .Where(x => x.employee_id == e.employee_id)
               .Where(x => x.leave_type_id == e.leave_type_id)
               .Select(x => x.id)).SingleOrDefault();

            if (exist != 0)
            {
                ModelState.AddModelError("", "Employee entitlement already assigned.");
                ModelState.AddModelError("", "No change was made.");
            }
            if (ModelState.IsValid)
            {
                leave_entitlement en = db.leave_entitlement.Find(e.leave_entitlement_id);
                en.employee_id = e.employee_id;
                en.credited_date = DateTime.Now;
                en.leave_type_id = e.leave_type_id;
                db.Entry(en).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Entitlement");
            }
            else
            {
                System.Text.StringBuilder sb = new System.Text.StringBuilder();
                sb.Append("You have some errors:");
                foreach (ModelState ms in ModelState.Values)
                {
                    foreach (ModelError error in ms.Errors)
                    {
                        sb.Append(error.ErrorMessage + "\n");
                    }
                }
                TempData["message"] = sb.ToString();
                @ViewBag.leave_types = new SelectList(db.leave_type, "id", "name");
                return View(e);
            }

        }

        [CustomAuthorize(Roles = "Admin,User")]
        [HttpGet]
        public ActionResult Types()
        {
            @ViewBag.types = db.leave_type.ToList();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Types(leave_type t)
        {
            if (ModelState.IsValid)
            {
                var n = db.leave_type.Where(x => x.name == t.name).SingleOrDefault();
                if (n != null)
                {
                    ModelState.AddModelError("", "Leave Type already exists");
                }
                else
                {
                    db.leave_type.Add(t);
                    db.SaveChanges();
                }
            }
            else
            {
                ModelState.AddModelError("", "Error occured trying to create leave type");
            }
            @ViewBag.types = db.leave_type.ToList();
            return View(t);
        }


        [HttpGet]
        [CustomAuthorize(Roles = "Admin")]
        public ActionResult EditTypes(int id)
        {
            leave_type l = db.leave_type.Find(id);
            if (l == null)
            {
                return HttpNotFound();
            }
            return View(l);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditTypes(leave_type l)
        {
            if (ModelState.IsValid)
            {
                db.Entry(l).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Types");
            }
            return View(l);
        }

        [CustomAuthorize(Roles = "Admin")]
        public ActionResult DeleteTypes(int id = 0)
        {
            leave_type l = db.leave_type.Find(id);
            if (l == null)
            {
                TempData["message"] = "No Leave Type of id:" + id + " exists";
                return RedirectToAction("Types");
            }
            db.leave_type.Remove(l);
            db.SaveChanges();
            return RedirectToAction("Types");
        }


        [HttpGet]
        [CustomAuthorize(Roles = "Admin,User")]
        public ActionResult EmployeeLeave()
        {
            if (issetup())
            {
                ViewBag.employees = db.Employees.Where(c => c.IsDisengaged != true).Select(c => new { c.EmployeeID, names = c.SurName + " " + c.OtherNames })
                    .OrderBy(c => c.names)
                    .ToList();
                return View();
            }
            else
            {
                TempData["message"] = "Create Leave Period First to proceed";
                return RedirectToAction("Period");
            }

        }

       

        IEnumerable<DateTime> GetDaysBetween(DateTime start, DateTime end)
        {
            for (DateTime i = start; i < end; i = i.AddDays(1))
            {
                yield return i;
            }
        }

        [CustomAuthorize(Roles = "Admin")]
        [HttpGet]
        public ActionResult AssignLeave(int id)
        {
            AssignLeaveVM e = new AssignLeaveVM();
            var data = (db.leave_type.Join(db.leave_entitlement, x => x.id, l => l.leave_type_id, (x, l) => new { x, l })
               .Where(xl => xl.l.employee_id == id).Select(xl => new { xl.x.id, xl.x.name })).ToList();

            var dated = db.leave_period.Where(x => x.active == 1).Select(x => new TempDateClass { sdate = x.startdate, edate = x.enddate }).SingleOrDefault();

            DateTime start = dated.sdate;
            DateTime end = dated.edate;
            ViewBag.names = db.Employees.Where(c => c.EmployeeID == id && (c.IsDisengaged != true)).Select(c => c.SurName + " " + c.OtherNames).SingleOrDefault();

            List<int> lt_id = db.leave_entitlement.Where(x => x.employee_id == id).Select(x => x.leave_type_id).ToList();

            e.startdate = DateTime.Now.Date;
            e.enddate = DateTime.Now.Date;
            e.days_used = db.leaves.Where(c => c.employee_id == id && c.startdate >= start && c.enddate <= end && lt_id.Contains(c.leave_type_id)).Sum(c => (double?)(c.days)) ?? 0;
            e.employee_id = id;
            e.leave_types = new SelectList(data, "id", "name");
            return View(e);
        }
 

        [CustomAuthorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AssignLeave(AssignLeaveVM e)
        {
            var exists = db.leaves.Where(l => l.startdate >= e.startdate && l.enddate <= e.enddate && l.employee_id == e.employee_id);
            int periodid = db.leave_period.Where(x => x.active == 1).Select(x => x.id).SingleOrDefault();
            if (exists.Any())
            {
                ModelState.AddModelError("", "Employee already has leave assigned on those days");
            }
            if (ModelState.IsValid)
            {
                try
                {

                    leave l = new leave();
                    l.employee_id = e.employee_id;
                    l.startdate = e.startdate;
                    l.enddate = e.enddate;
                    l.half_days = e.half_days;
                    l.work_weekends = e.work_weekend;
                    var holidays = db.holidays.Where(x => x.date >= e.startdate && x.date <= e.enddate).Select(x => x.date).ToList();
                    
                    int pushedholiday = holidays.Where(d => d.DayOfWeek == DayOfWeek.Sunday).Count();

                    //if weekends are working days
                    if (e.work_weekend == true)
                    {
                        l.days = (e.enddate - e.startdate).TotalDays + 1;//
                        l.days =  l.days - (e.holidays + (e.half_days / 2) + pushedholiday);

                    }
                    else
                    {
                        int weekends = GetDaysBetween(e.startdate, e.enddate).Where(d => d.DayOfWeek == DayOfWeek.Saturday || d.DayOfWeek == DayOfWeek.Sunday).Count(); 
                        // if startdate sunday/saturday add 1 to correct
                        if (e.startdate.DayOfWeek == DayOfWeek.Sunday || e.enddate.DayOfWeek == DayOfWeek.Sunday)
                        {
                            weekends = weekends + 1;
                        }
                        int weekendholidays = holidays.Where(d => d.DayOfWeek == DayOfWeek.Saturday || d.DayOfWeek == DayOfWeek.Sunday).Count();
                        double dbtw = (e.enddate - e.startdate).TotalDays + 1;
                        l.days = (e.enddate - e.startdate).TotalDays + 1 - e.holidays - weekends + weekendholidays - (e.half_days / 2) - pushedholiday;
                    }
                    if (l.days <= 0)
                    {
                        TempData["message"] = "Employee already has leave assigned on those days";
                        return RedirectToAction("Home", "Home");
                    }
                    l.comments = e.comments;
                    l.leave_type_id = e.leave_type_id;
                    l.employee_id = e.employee_id;
                    l.date_applied = DateTime.Now;
                    l.period_id = periodid;
         
                    db.leaves.Add(l);
                    db.SaveChanges();
                }
                catch (Exception ex)
                {
                    TempData["message"] = ex.ToString();
                }
                TempData["message"] = "Leave applied successfully";
                return RedirectToAction("Home", "Home");
            }
            else
            {
                e.leave_types = new SelectList(db.leave_type, "id", "name");
                ModelState.AddModelError("", "Correct the below errors to proceed");
                return View(e);
            }

        }


        public JsonResult GetDays(int id, int empid)
        {
            double diffdate, days_available, accrual_type;
            TempDateClass dated = db.leave_period.Where(x => x.active == 1)
                       .Select(x => new TempDateClass { sdate = x.startdate, edate = x.enddate }).SingleOrDefault();
            leave_type ltp = db.leave_type.Find(id);
            if (ltp.accrual_type == "EmploymentDate")
            {
                accrual_type = 1;//1 for accruing 0 non accruing
                DateTime? employedate = db.Employees.Where(x => x.EmployeeID == empid).Select(x => x.DateOfEmployment).SingleOrDefault();
                if (employedate == null)
                {
                    employedate = DateTime.Parse("01/01/2015");
                }
                // diffdate = ((DateTime.Now.Year - employedate.Value.Year) * 12 + DateTime.Now.Month - employedate.Value.Month + 1); //
                diffdate = DateTime.Now.Subtract(employedate.Value).Days / (365 / 12);
                //total mths leave have been accruing
                days_available = ((db.leave_type.Where(x => x.id == id).Select(x => x.days).SingleOrDefault()) * diffdate) / 12;
                days_available = Math.Round(days_available, 2);
            }
            else if (ltp.accrual_type == "PeriodDate")
            {
                accrual_type = 1;//1 for accruing 0 non accruing
                diffdate = ((DateTime.Now.Year - dated.sdate.Year) * 12 + DateTime.Now.Month - dated.sdate.Month + 1); //mths leave dys have been accruing
                days_available = ((db.leave_type.Where(x => x.id == id).Select(x => x.days).SingleOrDefault()) * diffdate) / 12;
                days_available = Math.Round(days_available, 2);
            }
            else
            {
                accrual_type = 0;//1 for accruing 0 non accruing
                days_available = (db.leave_type.Where(x => x.id == id).Select(x => x.days).SingleOrDefault());
            }

            double totalLeaveDays = db.leaves.Where(c => c.employee_id == empid && c.leave_type_id == id).Sum(c => (double?)(c.days)) ?? 0;
            //leave dys approved but not yet taken future days
            double daysApproved = db.leaves.Where(c => c.employee_id == empid && c.startdate > DateTime.Now && c.leave_type_id == id).Sum(c => (double?)(c.days)) ?? 0;
            List<double> data = new List<double>(new Double[] { days_available, totalLeaveDays, accrual_type, daysApproved });

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public ActionResult LeaveListReport()
        {
            ReportViewerViewModel model = new ReportViewerViewModel();
            string content = Url.Content("~/Reports/CrystalViewer/LeaveList.aspx");
            model.ReportPath = content;
            return View("ReportViewer", model);
        }


        public ActionResult LeaveReport()
        {
            ReportViewerViewModel model = new ReportViewerViewModel();
            string content = Url.Content("~/Reports/CrystalViewer/LeaveList2.aspx");
            model.ReportPath = content;
            return View("ReportViewer", model);
        }

        public ActionResult LeaveByEmployee()
        {
            ReportViewerViewModel model = new ReportViewerViewModel();
            string content = Url.Content("~/Reports/CrystalViewer/LeaveList3.aspx");
            model.ReportPath = content;
            return View("ReportViewer", model);
        }

        public ActionResult LeaveByMonth()
        {
            ReportViewerViewModel model = new ReportViewerViewModel();
            string content = Url.Content("~/Reports/CrystalViewer/LeaveList4.aspx");
            model.ReportPath = content;
            return View("ReportViewer", model);
        }
        [CustomAuthorize(Roles = "Admin,User")]
        [HttpGet]
        public ActionResult Leavelist()
        {
            DateTime mydate = DateTime.Parse("01/01/1900"); //to avoid default leaves
            List<LeaveVM> leavelst = db.leaves.Where(l => l.leave_type.accrual_type == "None"
                && l.startdate > mydate && l.Employee.IsDisengaged != true).Select(l => new LeaveVM
                {
                    id = l.id,
                    Employee = l.Employee.SurName + " " + l.Employee.OtherNames,
                    leave_type = l.leave_type.name,
                    startdate = l.startdate,
                    enddate = l.enddate,
                    days_used = l.days,
                    //days_left = Math.Round(l.leave_type.days - l.days, 2),
                    Date_Applied = l.date_applied,
                    Comment = l.comments,
                }).OrderBy(l => l.Employee).ThenByDescending(l => l.startdate).ToList();

            return View(leavelst);
        }
        [CustomAuthorize(Roles = "Admin,User")]
        [HttpGet]
        public ActionResult Leaves()
        {
            DateTime mydate = DateTime.Parse("01/01/1900"); //to avoid default leaves
            List<LeaveVM> leavelst = db.leaves.Where(l => l.startdate > mydate && l.Employee.IsDisengaged != true).Select(l => new LeaveVM
            {
                id = l.id,
                Employee = l.Employee.SurName + " " + l.Employee.OtherNames,
                leave_type = l.leave_type.name,
                startdate = l.startdate,
                enddate = l.enddate,
                days_used = l.days,
                //days_left = Math.Round(l.leave_type.days - l.days, 2),
                Date_Applied = l.date_applied,
                Comment = l.comments,
            }).OrderBy(l => l.Employee).ThenByDescending(l => l.startdate).ToList();

            return View(leavelst);
        }
        [CustomAuthorize(Roles = "Admin,User")]
        [HttpGet]
        public ActionResult ByPeriod()
        {
            DateTime dys_mth = db.leave_period.Where(x => x.active == 1).Select(x => x.startdate).SingleOrDefault();
            double diffdate = ((DateTime.Now.Month - dys_mth.Month) + 1); //mths leave dys have been accruing
            DateTime mydate = DateTime.Parse("01/01/1900"); //to avoid default leaves
            List<LeaveVM> leavelst = db.leaves
                .Where(l => l.leave_type.accrual_type == "PeriodDate"
                 && l.startdate > mydate && l.Employee.IsDisengaged != true)
                .Select(l => new LeaveVM
                {
                    id = l.id,
                    Employee = l.Employee.SurName + " " + l.Employee.OtherNames,
                    leave_type = l.leave_type.name,
                    startdate = l.startdate,
                    enddate = l.enddate,
                    days_used = l.days,
                    //days_left = Math.Round((l.leave_type.days * diffdate / 12) - l.days, 2),
                    Date_Applied = l.date_applied,
                    Comment = l.comments,
                }).OrderBy(l => l.Employee).ToList();
            return View(leavelst);
        }
        [CustomAuthorize(Roles = "Admin,User")]
        [HttpGet]
        public ActionResult ByEmploymentDate()
        {
            DateTime mydate = DateTime.Parse("01/01/1900"); //to avoid default leaves
            List<LeaveVM> leavelst = db.leaves
                .Where(l => l.leave_type.accrual_type == "EmploymentDate" && l.Employee.DateOfEmployment.HasValue
                && l.startdate > mydate && l.Employee.IsDisengaged != true)
               .Select(l => new LeaveVM
               {
                   id = l.id,
                   Employee = l.Employee.SurName + " " + l.Employee.OtherNames,
                   leave_type = l.leave_type.name,
                   startdate = l.startdate,
                   enddate = l.enddate,
                   days_used = l.days,
                   //days_left = Math.Round((lt.days * ((DateTime.Now.Month - e.DateOfEmployment.Value.Month) + 1) / 12) - l.days, 2),
                   //days_left = Math.Round(l.leave_type.days * ((DateTime.Now.Year - l.Employee.DateOfEmployment.Value.Year) * 12 + DateTime.Now.Month - l.Employee.DateOfEmployment.Value.Month + 1) / 12, 2) - l.days,//Math.Round((lt.days * ((DateTime.Now.Year - e.DateOfEmployment.Value.Year) * 12) + DateTime.Now.Month - e.DateOfEmployment.Value.Month + 1) / 12 - l.days, 2),
                   Date_Applied = l.date_applied,
                   Comment = l.comments,
               }).OrderBy(l => l.Employee).ToList();

            return View(leavelst);
        }

        [CustomAuthorize(Roles = "Admin")]
        [HttpGet]
        public ActionResult EditLeave(int id)
        {
            double diffdate;
            leave myleave = db.leaves.Find(id);
            if (myleave == null)
            {
                return HttpNotFound();
            }
            if (myleave.leave_type.accrual_type == "EmploymentDate")
            {
                diffdate = Math.Round(myleave.leave_type.days * ((DateTime.Now.Year - myleave.Employee.DateOfEmployment.Value.Year) * 12 + DateTime.Now.Month - myleave.Employee.DateOfEmployment.Value.Month + 1) / 12, 2) - myleave.days;
            }
            else if (myleave.leave_type.accrual_type == "PeriodDate")
            {
                diffdate = Math.Round(myleave.leave_type.days * ((DateTime.Now.Year - myleave.leave_period.startdate.Year) * 12 + DateTime.Now.Month - myleave.leave_period.startdate.Month + 1) / 12, 2) - myleave.days;
            }
            else
            {
                diffdate = myleave.leave_type.days - myleave.days;
            }

            DateTime dys_mth = db.leave_period.Where(x => x.active == 1).Select(x => x.startdate).SingleOrDefault();

           
            //ToDO
            //int pushedholiday = holidays.Where(d => d.DayOfWeek == DayOfWeek.Sunday).Count();
            diffdate = ((DateTime.Now.Month - dys_mth.Month) + 1); //mths leave dys have been accruing
            EditLeaveVM leavelst = db.leaves.Where(l => l.id == id).Select(l => new EditLeaveVM
            {
                id = l.id,
                Employee = l.Employee.SurName + " " + l.Employee.OtherNames,
                leave_type_id = l.leave_type_id,
                startdate = l.startdate,
                enddate = l.enddate,
                days_used = l.days,
                days_left = diffdate,
                Comment = l.comments,
                half_days = l.half_days??0.0,
                work_weekends = l.work_weekends??false,
            }).SingleOrDefault();
            //ToDO
            //var holidays = db.holidays.Where(x => x.date >= leavelst.startdate && x.date <= leavelst.enddate).Select(x => x.date).ToList();
            //leavelst.half_days/2 = leavelst.days_used - (leavelst.enddate - leavelst.startdate).TotalDays - 1 + holidays.count() + pushedholiday;
            int empid = myleave.employee_id;
            var data = (db.leave_type.Join(db.leave_entitlement, x => x.id, l => l.leave_type_id, (x, l) => new { x, l })
                .Where(xl => xl.l.employee_id == empid).Select(xl => new { xl.x.id, xl.x.name })).ToList();
            leavelst.leave_types = new SelectList(data, "id", "name");

            int holiday = db.holidays.Where(x => x.date >= leavelst.startdate && x.date <= leavelst.enddate).Count();
            leavelst.holidays = holiday;
            Session["page"] = Request.UrlReferrer.ToString();
            return View(leavelst);
        }

        [CustomAuthorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditLeave(EditLeaveVM lv)
        {
            if (ModelState.IsValid)
            {
                leave l = db.leaves.Find(lv.id);
                l.startdate = lv.startdate;
                l.enddate = lv.enddate;
                l.leave_type_id = lv.leave_type_id;
                l.comments = lv.Comment;
                l.half_days = lv.half_days;
                l.work_weekends = lv.work_weekends;

                var holidays = db.holidays.Where(x => x.date >= lv.startdate && x.date <= lv.enddate).Select(x => x.date).ToList();

                int pushedholiday = holidays.Where(d => d.DayOfWeek == DayOfWeek.Sunday).Count();

                //if weekends are working days
                if (lv.work_weekends == true)
                {
                    l.days = (lv.enddate - lv.startdate).TotalDays + 1 - (lv.holidays + (lv.half_days/ 2) + pushedholiday); //halfdays omitted- (e.holidays + e.half_days / 2 + pushedholiday);

                }
                else
                {
                    int weekends = GetDaysBetween(lv.startdate, lv.enddate).Where(d => d.DayOfWeek == DayOfWeek.Saturday || d.DayOfWeek == DayOfWeek.Sunday).Count();
                    // if startdate sunday/saturday add 1 to correct
                    if (lv.startdate.DayOfWeek == DayOfWeek.Sunday || lv.enddate.DayOfWeek == DayOfWeek.Sunday)
                    {
                        weekends = weekends + 1;
                    }
                    int weekendholidays = holidays.Where(d => d.DayOfWeek == DayOfWeek.Saturday || d.DayOfWeek == DayOfWeek.Sunday).Count();
                    l.days = (lv.enddate - lv.startdate).TotalDays + 1 - lv.holidays - weekends + weekendholidays - pushedholiday - lv.half_days / 2; //wont have half days
                }

                
                db.Entry(l).State = EntityState.Modified;
                db.SaveChanges();
                if (Session["page"] != null)
                {
                    return Redirect(Session["page"].ToString());
                }
                else
                {
                    return RedirectToAction("Leavelist");
                }
            }
            else
            {
                leave myleave = db.leaves.Find(lv.id);
                double diffdate;
                if (myleave.leave_type.accrual_type == "EmploymentDate")
                {
                    diffdate = Math.Round(myleave.leave_type.days * ((DateTime.Now.Year - myleave.Employee.DateOfEmployment.Value.Year) * 12 + DateTime.Now.Month - myleave.Employee.DateOfEmployment.Value.Month + 1) / 12, 2) - myleave.days;
                }
                else if (myleave.leave_type.accrual_type == "PeriodDate")
                {
                    diffdate = Math.Round(myleave.leave_type.days * ((DateTime.Now.Year - myleave.leave_period.startdate.Year) * 12 + DateTime.Now.Month - myleave.leave_period.startdate.Month + 1) / 12, 2) - myleave.days;
                }
                else
                {
                    diffdate = myleave.leave_type.days - myleave.days;
                }
                DateTime dys_mth = db.leave_period.Where(x => x.active == 1).Select(x => x.startdate).SingleOrDefault();

                lv = db.leaves.Where(l => l.id == lv.id).Select(l => new EditLeaveVM
                {
                    id = l.id,
                    Employee = l.Employee.SurName + " " + l.Employee.OtherNames,
                    leave_type_id = l.leave_type_id,
                    startdate = l.startdate,
                    enddate = l.enddate,
                    days_used = l.days,
                    days_left = diffdate,
                    Comment = l.comments,
                    half_days = l.half_days.Value,
                }).SingleOrDefault();

                int empid = db.leaves.Where(x => x.id == lv.id).Select(x => x.employee_id).SingleOrDefault();
                var data = (db.leave_type.Join(db.leave_entitlement, x => x.id, le => le.leave_type_id, (x, le) => new { x, le })
                   .Where(xl => xl.le.employee_id == empid).Select(xl => new { xl.x.id, xl.x.name })).ToList();
                lv.leave_types = new SelectList(data, "id", "name");
                return View(lv);
            }
        }

        [CustomAuthorize(Roles = "Admin")]
        [HttpPost]
        public ActionResult DeleteLeave(int id = 0)
        {
            leave l = db.leaves.Find(id);
            if (l == null)
            {
                return HttpNotFound();
            }
            db.leaves.Remove(l);
            db.SaveChanges();
            return Redirect(Request.UrlReferrer.ToString());
        }

        public bool issetup()
        {
            bool setup = false;
            var LinqResult = db.leave_period.Where(x => x.active == 1);
            if (LinqResult.Any())
            {
                setup = true;
            }
            return setup;
        }


    }

}
