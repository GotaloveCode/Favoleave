using System;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;

namespace Leave.Models
{
    [MetadataType(typeof(leave_periodMetadata))]
    public partial class leave_period
    {
    }
    [MetadataType(typeof(leave_entitlementMetadata))]
    public partial class leave_entitlement
    {

    }
    [MetadataType(typeof(leave_typeMetadata))]
    public partial class leave_type
    {
    }
    [MetadataType(typeof(leaveMetadata))]
    public partial class leave
    {
    }
    [MetadataType(typeof(holidayMetadata))]
    public partial class holiday
    {
    }

}