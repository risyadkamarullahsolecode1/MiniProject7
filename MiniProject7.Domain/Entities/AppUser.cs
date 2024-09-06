using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniProject7.Domain.Entities
{
    public class AppUser:IdentityUser
    {
        public string? RefreshToken { get; set; }
        public DateTime? RefreshTokenExpiryTime { get; set; }
        public virtual ICollection<Employee>? Employees { get; set; }
        public virtual ICollection<Process>? Processes { get; set; }
        public virtual ICollection<LeaveRequest>? LeaveRequests { get; set; }
        public virtual ICollection<WorkflowAction>? WorkflowActions { get; set; }
    }
}
