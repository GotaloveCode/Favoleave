using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Globalization;
using System.Web.Mvc;
using System.Web.Security;

namespace Leave.Models
{
    public class LoginVM
    {
        [Required]
        [StringLength(200)]
        [Display(Name = "User name")]
        public string username { get; set; }
        [Required]
        [StringLength(200, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string password { get; set; }

     }

    public class HolidayVM
    {
        [Required]
        [StringLength(200)]
        [Display(Name = "Holiday")]
        public string name { get; set; }
        [Display(Name = "Date")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:d MMM yyyy}")]
        public DateTime date { get; set; }
        public bool recurs { get; set; }
    }


       public class RegisterVM
    {
        [Required]
        [Display(Name = "User name")]
        public string username { get; set; }
        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string password { get; set; }
        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("password", ErrorMessage = "password and confirmation password mismatch.")]
        public string ConfirmPassword { get; set; }
        [Display(Name = "Employee")]
        public int employee_id { get; set; }
        [Display(Name = "Role")]
        [Range(1,20, ErrorMessage = "Select the role")]
        public int role_id { get; set; }
        public byte status { get; set; }    
    }
       public class UserVM
       {
           [Required]
           public int id { get; set; }
           [Display(Name = "Username")]
           public string username { get; set; }
           public string Employee { get; set; }
           public string Role { get; set; }
           [Display(Name = "Status")]
           public byte status { get; set; }
       }
       public class EditUserVM
       {
           [Required]
           public int id { get; set; }
           [Required]
           [Display(Name = "User name")]
           public string username { get; set; }
           public string Employee { get; set; }
           [Display(Name = "Role")]
           [Range(1, 20, ErrorMessage = "Select the {0}")]
           public int role_id { get; set; }
           [Display(Name = "Reset Password")]
           public bool reset_password { get; set; }
           public byte status { get; set; }
           public SelectList user_roles { get; set; }
           
       }

    public class HomeVM
    {
        public int id { get; set; }
        public string Employee { get; set; }        
        public string Duration { get; set; }
        public DateTime startdate { get; set; }
   }

    public class user_requestVM
    {
        public int id { get; set; }
        public string employeename { get; set; }
    }

}
