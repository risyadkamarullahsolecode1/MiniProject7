using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniProject7.Domain.Entities
{
    public class Process
    {
        [Key]
        public int ProcessId { get; set; } // Primary Key
        public int WorkflowId { get; set; } // Foreign Key to Workflow
        public virtual Workflow? Workflow { get; set; }
        public string? RequesterId { get; set; } // Foreign Key to AspNetUsers
        public virtual AppUser? Requester { get; set; }
        public string? RequestType { get; set; }
        public string? Status { get; set; }
        public int? CurrentStepId { get; set; } 
        public virtual WorkflowSequence? CurrentStep { get; set; }
        public DateTime RequestDate { get; set; }
        public ICollection<LeaveRequest>? LeaveRequests { get; set; }
        public ICollection<WorkflowAction>? WorkflowActions { get; set; }
    }
}
