//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Leave.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class leave_type
    {
        public leave_type()
        {
            this.leave_entitlement = new HashSet<leave_entitlement>();
            this.leaves = new HashSet<leave>();
        }
    
        public int id { get; set; }
        public string name { get; set; }
        public double days { get; set; }
        public string accrual_type { get; set; }
    
        public virtual ICollection<leave_entitlement> leave_entitlement { get; set; }
        public virtual ICollection<leave> leaves { get; set; }
    }
}
