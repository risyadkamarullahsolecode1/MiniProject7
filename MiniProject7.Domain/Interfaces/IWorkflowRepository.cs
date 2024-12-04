using MiniProject7.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace MiniProject7.Domain.Interfaces
{
    public interface IWorkflowRepository
    {
        Task<Workflow> AddWorkflow(Workflow workflow);
        Task<Workflow?> GetFirstOrDefaultAsync(Expression<Func<Workflow, bool>> expression);
        Task<WorkflowSequence> AddWorkflowSequence(WorkflowSequence workflowSequence);
        Task<LeaveRequest> SubmitLeaveRequestAsync(LeaveRequest request);
        Task<Process> AddProcessLeaveRequest(Process process);
        Task<WorkflowAction> AddAction(WorkflowAction workflowAction);
        Task<bool> ApproveLeaveRequestAsync(int workflowActionId, int processId, string actorId, string role, bool isApproved, string comment);
        Task SubmitLeaveRequestAsync(LeaveRequest request, string userId);
        Task<string> GetEmployeeEmailById(string employeeId);
    }
}
