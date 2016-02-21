using System.Data.Entity.Infrastructure.Interception;

namespace DBinterceptor
{
    using System;
    using System.Data.Entity;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;
    using Leave.Models;

    public partial class LeaveEntities : DbContext
    {
        public LeaveEntities()
            : base("name=LeaveEntities")
        {
            DbInterception.Add(new DatabaseLogger());
        }

        public virtual DbSet<error> errors { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
        }
    }
}
