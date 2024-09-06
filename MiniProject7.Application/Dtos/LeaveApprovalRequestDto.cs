using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniProject7.Application.Dtos
{
    public class LeaveApprovalRequestDto
    {
        public int WorkflowActionId { get; set; }
        public int ProcessId { get; set; }
        public string ActorId { get; set; }
        public string Role { get; set; }
        public bool IsApproved { get; set; }
        public string Comment { get; set; }
    }
}
