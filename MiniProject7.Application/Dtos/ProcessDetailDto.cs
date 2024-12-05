using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniProject7.Application.Dtos
{
    public class ProcessDetailDto
    {
        public int ProcessId { get; set; }
        public string? RequestName { get; set; }
        public string? Description { get; set; }
        public DateOnly? StartDate { get; set; }
        public DateOnly? EndDate { get; set; }
        public string? LeaveType { get; set; }
        public string? Reason { get; set; }
        public string Status { get; set; }
        public List<WorkflowActionDto> WorkflowActions { get; set; }
    }
}
