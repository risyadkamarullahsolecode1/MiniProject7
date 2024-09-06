using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniProject7.Application.Dtos
{
    public class LeaveApprovalRequestDto
    {
        /// <summary>
        /// The ID of the Workflow Action (step in the workflow process).
        /// </summary>
        public int WorkflowActionId { get; set; }

        /// <summary>
        /// The ID of the Process associated with the leave request.
        /// </summary>
        public int ProcessId { get; set; }

        /// <summary>
        /// The ID of the Actor (approver) performing the action.
        /// </summary>
        public string ActorId { get; set; }

        /// <summary>
        /// The role of the Actor (e.g., Supervisor, HR Manager).
        /// </summary>
        public string Role { get; set; }

        /// <summary>
        /// Indicates whether the leave request is approved (true) or rejected (false).
        /// </summary>
        public bool IsApproved { get; set; }

        /// <summary>
        /// Any additional comments regarding the approval or rejection of the leave request.
        /// </summary>
        public string Comment { get; set; }
    }
}
