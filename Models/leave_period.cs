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
    
    public partial class leave_period
    {
        public leave_period()
        {
            this.leaves = new HashSet<leave>();
        }
    
        public int id { get; set; }
        public System.DateTime startdate { get; set; }
        public System.DateTime enddate { get; set; }
        public byte active { get; set; }
    
        public virtual ICollection<leave> leaves { get; set; }
    }
}
