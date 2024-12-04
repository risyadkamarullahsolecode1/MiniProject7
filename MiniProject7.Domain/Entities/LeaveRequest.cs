using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniProject7.Domain.Entities
{
    public class LeaveRequest
    {
        [Key]
        public int RequestId { get; set; } // Primary Key
        public string? RequestName { get; set; }
        public string? Description { get; set; }
        public int ProcessId { get; set; } // Foreign Key to Process
        public virtual Process? Process { get; set; }
        public string? EmployeeId { get; set; } // Foreign Key to AspNetUsers
        public virtual AppUser? Employee { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        [RegularExpression("Annual Leave|Sick Leave|Personal Leave")]
        public string? LeaveType { get; set; } // e.g., Sick Leave, Personal Leave
        public string? Reason { get; set; }

    }
}
