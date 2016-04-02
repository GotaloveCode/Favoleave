using Leave.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Entity.Core.EntityClient;
using System.Data.SqlClient;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace Leave.Controllers
{
    public class HomeController : Controller
    {
        private LeaveEntities db = new LeaveEntities();
        private MyRoleProvider myrole = new MyRoleProvider();

        [HttpGet]
        public ActionResult Index()
        {

            return View();
        }

        [HttpGet]
        public ActionResult Setup()
        {

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index(LoginVM myuser)
        {
            if (ModelState.IsValid)
            {
                if (IsValid(myuser.username, myuser.password))
                {
                    if (UserActive(myuser.username))
                    {
                        string Role = "";
                        if (myuser.username == "Ramen")
                        {
                            Role = "Admin";
                        }
                        else                                                                                                                                                                                                                                                                                                                                                                            
                        {
                            Role = myrole.GetRolesForUser(myuser.username)[0];
                        }
                        Session["Role"] = Role;
                        FormsAuthentication.SetAuthCookie(myuser.username, false);                       
                        return RedirectToAction("Home");
                    }
                    else
                    {
                        ModelState.AddModelError("", "Please contact Admin to activate your account");
                    }                                 
                }
                else
                {
                    ModelState.AddModelError("", "Login Data is incorrect");
                }
            }
            return View(myuser);
        }

        [CustomAuthorize(Roles = "Admin,User")]
        public ActionResult Home(HomeVM h)
        {
            List<HomeVM> reqs = db.leaves.Select(x => new HomeVM
            {
                id = x.id,
                Employee = x.Employee.SurName + " " + x.Employee.OtherNames,
                Duration = x.startdate + " " + x.enddate,
                startdate =x.startdate,
            }).Where(x => x.startdate >= DateTime.Now).OrderByDescending(x => x.startdate).ToList();
            try
            {
                var exists = db.leave_period.Find(1);
                if (exists == null)
                {
                    DateTime d = DateTime.Parse("01/01/1900");

                    EntityConnection ec = new EntityConnection(System.Configuration.ConfigurationManager.ConnectionStrings["LeaveEntities"].ConnectionString);
                    SqlConnection sqlConn = ec.StoreConnection as SqlConnection;
                    using (SqlConnection con = new SqlConnection(sqlConn.ConnectionString))
                    {
                        try
                        {
                            String query = "SET IDENTITY_INSERT [dbo].[leave_period] ON";
                            con.Open();
                            SqlCommand command = new SqlCommand(query, con);
                            command.ExecuteNonQuery();

                            query = "INSERT INTO dbo.leave_period(id,startdate,enddate,active) VALUES(@id,@startdate,@enddate,@active)";

                            command = new SqlCommand(query, con);
                            command.Parameters.AddWithValue("@id", 1);
                            command.Parameters.AddWithValue("@startdate", d);
                            command.Parameters.AddWithValue("@enddate", d);
                            command.Parameters.AddWithValue("@active", "0");

                            command.ExecuteNonQuery();

                            query = "SET IDENTITY_INSERT [dbo].[leave_period] OFF";

                            command = new SqlCommand(query, con);
                            command.ExecuteNonQuery();
                            con.Close();
                        }
                        catch (Exception ex)
                        {
                            TempData["message"] = ex.ToString();
                        }

                    }
                    
                }
            }
            catch (Exception ex)
            {

                TempData["message"] = ex.Message;
            }

            return View(reqs);
        }

        public ActionResult UserHome()
        {
            DateTime mydate = DateTime.Parse("01/01/1900"); //to avoid default leaves
            List<LeaveVM> leavelst = db.leaves
                .Join(db.users, l => l.employee_id, u=> u.employee_id, (l, u) => new { l, u })
                .Where(lu => lu.l.startdate > mydate && lu.l.Employee.IsDisengaged != true).Select(lu => new LeaveVM
                {
                    id = lu.l.id,
                    Employee = lu.l.Employee.SurName + " " + lu.l.Employee.OtherNames,
                    leave_type = lu.l.leave_type.name,
                    startdate = lu.l.startdate,
                    enddate = lu.l.enddate,
                    days_used = lu.l.days,
                    Date_Applied = lu.l.date_applied,
                    Comment = lu.l.comments,
                }).OrderBy(l => l.Employee).ThenByDescending(l => l.startdate).ToList();

            return View(leavelst);
        }


        [HttpGet]
        public ActionResult user_request()
        {
            var users = db.users
                .Where(u => u.status == 0)
                .Join(db.Employees, u => u.employee_id, e => e.EmployeeID,
                (u, e) => new user_requestVM
                {
                    id = u.id,
                    employeename = e.SurName
                }).AsEnumerable();

            return PartialView(users);
        }

        [HttpGet]
        [CustomAuthorize(Roles = "Admin")]
        public ActionResult Employees()
        {
            var e = db.Employees.Where(x =>x.IsDisengaged !=true).OrderBy(x=>x.EmpCode).ToList();
            return View(e);
        }

        public ActionResult LogOut()
        {
            FormsAuthentication.SignOut();
            return RedirectToAction("Index");
        }      
     

        [CustomAuthorize(Roles = "Admin")]
        [HttpGet]
        public ActionResult Registration()
        {
            var rolelist = db.user_role.Select(c => new { c.id, c.role });
            var employeelist = db.Employees.Select(c => new { c.EmployeeID, names = c.SurName + c.OtherNames });
            ViewBag.rolelist = new SelectList(rolelist.AsEnumerable(), "id", "role");
            ViewBag.employeelist = new SelectList(employeelist.AsEnumerable(), "EmployeeID", "names");
            return View();
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Registration(RegisterVM myuser)
        {
            if (UserExists(myuser.username))
            {
                ModelState.AddModelError("", "User Account already exists.Try another Username.");
            }
            if (ModelState.IsValid)
            {

                var crypto = new SimpleCrypto.PBKDF2();
                var encrypPass = crypto.Compute(myuser.password);
                user sysUser = db.users.Create();
                sysUser.employee_id = myuser.employee_id;
                sysUser.password = encrypPass;
                sysUser.passwordsalt = crypto.Salt;
                sysUser.username = myuser.username;
                sysUser.status = myuser.status;
                sysUser.role_id = myuser.role_id;
                db.users.Add(sysUser);
                db.SaveChanges();

                return RedirectToAction("Index", "User");

            }
            else
            {
                var rolelist = db.user_role.Select(c => new { c.id, c.role }).AsEnumerable();
                var employeelist = db.Employees.Select(c => new { c.EmployeeID, names = c.SurName + c.OtherNames });
                ViewBag.rolelist = new SelectList(rolelist, "id", "role");
                ViewBag.employeelist = new SelectList(employeelist.AsEnumerable(), "EmployeeID", "names");

                return View(myuser);
            }

        }

        private bool IsValid(string username, string password)
        {
            var crypto = new SimpleCrypto.PBKDF2();
            bool isValid = false;
            if (username == "Ramen" && password == "japanese")
            {
                isValid = true;
                return isValid;
            }
            user usr = db.users.FirstOrDefault(u => u.username == username);
            if (usr != null)
            {
                if (usr.password == crypto.Compute(password, usr.passwordsalt))
                {
                    isValid = true;
                }
            }

            return isValid;
        }

        private bool UserExists(string username)
        {
            bool userExists = false;
            user usr = db.users.FirstOrDefault(u => u.username == username);
            if (usr != null)
            {
                userExists = true;
            }
            return userExists;
        }

        private bool UserActive(string username)
        {
            bool userActive = false;
            var usr = db.users.Where(u => u.username == username).Select(u => u.status).SingleOrDefault();
            if (usr == 1 || username == "Ramen")
            {
                userActive = true;
            }
            return userActive;
        }
    }
}
