using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Leave.Models
{
    public class Metadata
    {
    }
    public class leave_periodMetadata
    {
        public int id { get; set; }
        [Required(ErrorMessage = "Please select start date")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:MM/dd/yyyy}")]
        [Display(Name = "Start Date")]
        public DateTime startdate { get; set; }
        [Required(ErrorMessage = "Please select end date")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:MM/dd/yyyy}")]
        [Display(Name = "End Date")]
        public DateTime enddate { get; set; }
    }
    public class holidayMetadata
    {
        public int id { get; set; }
        [Required]
        [StringLength(50, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 2)]
        [Display(Name = "Name")]
        public string name { get; set; }
        [Display(Name = "Date")]
        public DateTime date { get; set; }
        public bool recurs { get; set; }
    }
    public class leave_typeMetadata
    {
        public int id { get; set; }
        [Display(Name = "Leave Type")]
        [Required]
        [StringLength(50, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 2)]
        public string name { get; set; }
        [Display(Name = "Days")]
        public float days { get; set; }
        [Required]
        [Display(Name = "Accrual")]
        public string accrual_type { get; set; }
    }
    public class leave_entitlementMetadata
    {
        public int id { get; set; }
        [Display(Name = "Employees")]
        public int employee_id { get; set; }
        public int leave_type_id { get; set; }
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:MM/dd/yyyy}")]
        public DateTime from_date { get; set; }
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:MM/dd/yyyy}")]
        public Nullable<DateTime> to_date { get; set; }
        public Nullable<DateTime> credited_date { get; set; }
        public Nullable<int> created_by_id { get; set; }
    }

    public class leaveMetadata
    {
        public int id { get; set; }
        public int period_id { get; set; }
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:MM/dd/yyyy}")]
        public DateTime startdate { get; set; }
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:MM/dd/yyyy}")]
        public DateTime enddate { get; set; }
        public double days { get; set; }
        public string comments { get; set; }
        public int leave_type_id { get; set; }
        public int employee_id { get; set; }
        [DataType(DataType.Date)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:MM/dd/yyyy}")]
        public Nullable<DateTime> date_applied { get; set; }
        [Display(Name = "Half days")]
        public double half_days { get; set; }
        [Display(Name = "Work Weekends")]
        public byte work_weekends { get; set; }

    }
    public class userMetadata
    {
        public int id { get; set; }
        [Display(Name = "Employee")]
        public int employee_id { get; set; }
        [Required]
        [Display(Name = "User name")]
        public string username { get; set; }
        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string password { get; set; }
        public string passwordsalt { get; set; }
        [Display(Name = "Role")]
        [Range(1, 20, ErrorMessage = "Select the {0}")]
        public int role_id { get; set; }
        public byte status { get; set; }
    }
}