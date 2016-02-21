using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Leave.Models
{
    public class AssignLeaveVM : IValidatableObject
    {
        [Display(Name = "Employee")]
        public int employee_id { get; set; }
        [Required]
        [Display(Name = "Leave Type")]
        public int leave_type_id { get; set; }
        public SelectList leave_types { get; set; }
        [Display(Name = "Holidays")]
        public float holidays { get; set; }
        [StringLength(250)]
        [DataType(DataType.MultilineText)]
        public string comments { get; set; }

        [Range(0.0, 160)]
        [Display(Name = "Half days")]
        public double half_days { get; set; }
        [RegularExpression(@"\d+(\.\d{1,2})?", ErrorMessage = "Used days can't have more than 1 decimal place")]
        [Range(0.0, 160)]
        [Display(Name = "Days Taken")]
        public double days_used { get; set; }
        [Display(Name = "Work Weekends")]
        public bool work_weekend { get; set; }
        [Required]
        [Display(Name = "Start Date")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:d MMM yyyy}")]
        public DateTime startdate { get; set; }
        [Required]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:d MMM yyyy}")]
        [Display(Name = "End Date")]
        public DateTime enddate { get; set; }
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (enddate < startdate)
            {
                yield return new ValidationResult("EndDate must be greater than StartDate");
            }
        }
    }

    public class LeaveVM
    {
        public int id { get; set; }
        public string Employee { get; set; }
        [Display(Name = "Leave Type")]
        public string leave_type { get; set; }
        public string Comment { get; set; }
        [Display(Name = "Used days")]
        public double days_used { get; set; }
        [Display(Name = "Days left")]
        public double days_left { get; set; }
        [Display(Name = "Start Date")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:d MMM yyyy}  ")]
        public DateTime startdate { get; set; }
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:d MMM yyyy}  ")]
        [Display(Name = "End Date")]
        public DateTime enddate { get; set; }
        //public string Duration { get; set; }
        [Display(Name = "Application Date")]
        [DataType(DataType.Date,ErrorMessage="Invalid Date")]
        public Nullable<DateTime> Date_Applied { get; set; }
        public int half_days { get; set; }
        [Display(Name = "Work Weekends")]
        public bool work_weekends { get; set; }
       
    }
    public class EditLeaveVM : IValidatableObject
    {
        [Required]
        public int id { get; set; }
        public string Employee { get; set; }
        [Display(Name = "Leave Type")]
        public int leave_type_id { get; set; }        
        public SelectList leave_types { get; set; }
        [StringLength(250)]
        [DataType(DataType.MultilineText)]
        public string Comment { get; set;}
        [Required(ErrorMessage="Please provide start date")]
        [Display(Name = "Start Date")]
        public DateTime startdate { get; set; }
        [Required(ErrorMessage = "Please provide end date")]
        [Display(Name = "End Date")]
        public DateTime enddate { get; set; }
        [Required]
        [Display(Name = "Used days")]
        public double days_used { get; set; }
        [Display(Name = "Holidays")]
        public int holidays { get; set; }
        //ToDO
        [Display(Name = "Half days")]
        public double half_days { get; set; }
        [Display(Name = "Work Weekends")]
        public bool work_weekends { get; set; }
        public double days_left { get; set; }
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (enddate < startdate)
            {
                yield return new ValidationResult("EndDate must be greater than StartDate");
            }
        }
    }
    public class LeavePeriodVM
    {
        [Required]
        [Display(Name = "Start Date")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:d MMM yyyy}  ")]
        public DateTime startdate { get; set; }
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:d MMM yyyy}")]
        [Display(Name = "End Date")]
        public DateTime enddate { get; set; }
        public int id { get; set; }
        
    }
    public class AddEntitlementVM
    {
        public DateTime from_date { get; set; }
        [Required]
        [Display(Name = "Employee")]
        public int employee_id { get; set; }
        public SelectList employees { get; set; }
        [Required]
        [Display(Name = "Leave Type")]
        public int leave_type_id { get; set; }
        public SelectList leave_types { get; set; }        

    }
    public class TempDateClass
    {
        public DateTime sdate { get; set; }
        public DateTime edate { get; set; }
    }
    public class EntitlementVM
    {
        [Display(Name = "Valid Between")]
        public string valid_period { get; set; }
        [Display(Name = "Employee")]
        public string employee { get; set; }
        public int employee_id { get; set; }
        [Display(Name = "Leave Type")]
        public string leave_type { get; set; }
        [Display(Name = "Leave Type")]
        public int leave_type_id { get; set; }
        public int leave_entitlement_id { get; set; }

    }
    public class LeaveTypeVM
    {
        public List<LeaveTypeVM> leavetypes { get; set; }
        [Required]
        public int id { get; set; }
        [Display(Name = "Leave Type")]
        [Required, StringLength(50, MinimumLength = 3)]
        public string name { get; set; }
        [Display(Name = "Days")]
        public float days { get; set; }
        [Required]
        [Display(Name = "Accrual")]
        public string accrual_type { get; set; }
    }


}