using Leave.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Leave.Controllers
{
    public class UserController : Controller
    {
        private LeaveEntities db = new LeaveEntities();
        
        [HttpGet]
        [CustomAuthorize(Roles = "Admin")]
        public ActionResult Index()
        {
            var u = db.users.Select(x => new UserVM { id = x.id, Employee = x.Employee.SurName + " " + x.Employee.OtherNames, username = x.username, Role = x.user_role.role, status = x.status }).ToList();
            return View(u);
        }

        
        [HttpGet]
        [CustomAuthorize(Roles = "Admin")]
        public ActionResult Edit(int id)
        {
            user usr = db.users.Find(id);
            if (usr == null)
            {
                return HttpNotFound();
            }
            EditUserVM u = new EditUserVM();
            u.id = usr.id;
            u.role_id = usr.role_id;
            u.username = usr.username;
            u.Employee = usr.Employee.SurName + " " + usr.Employee.OtherNames;
            u.status = usr.status;
            u.user_roles = new SelectList(db.user_role.ToList(), "id", "role", usr.role_id);
            return View(u);
        }             
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        [CustomAuthorize(Roles = "Admin")]
        public ActionResult Edit(EditUserVM u)
        {
            if (ModelState.IsValid)
            {
                user usr = db.users.Find(u.id);
                usr.username = u.username;
                usr.role_id = u.role_id;
                usr.status = u.status;
                if (u.reset_password)
                {
                    var crypto = new SimpleCrypto.PBKDF2();
                    usr.password = crypto.Compute("123456");
                    usr.passwordsalt = crypto.Salt;
                }
                db.Entry(usr).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            else
            {
                u.user_roles = new SelectList(db.user_role.ToList(), "id", "role", u.role_id);
                return View(u);
            }

        }

        [CustomAuthorize(Roles = "Admin")]
        public ActionResult Delete(int id)
        {
            user usr = db.users.Find(id);
            if (usr == null)
            {
                return HttpNotFound();
            }
            db.users.Remove(usr);
            db.SaveChanges();
            return RedirectToAction("Index");

        }
    }
}
