using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MiniProject7.Application.Interfaces;
using MiniProject7.Domain.Entities;
using MiniProject7.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace MiniProject7.Infrastructure.Data.Repository
{
    public class WorkflowRepository:IWorkflowRepository
    {
        private readonly CompaniesContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly UserManager<AppUser> _userManager;
        private readonly IEmailService _emailService;

        public WorkflowRepository(CompaniesContext context, IHttpContextAccessor httpContextAccessor, UserManager<AppUser> userManager, IEmailService emailService)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _userManager = userManager;
            _emailService = emailService;
        }

        // Add workflow
        public async Task<Workflow> AddWorkflow(Workflow workflow)
        {
            await _context.Workflows.AddAsync(workflow);
            await _context.SaveChangesAsync();
            return workflow;
        }

        // add workflow sequence
        public async Task<WorkflowSequence> AddWorkflowSequence(WorkflowSequence workflowSequence)
        {
            await _context.WorkflowSequences.AddAsync(workflowSequence);
            await _context.SaveChangesAsync();
            return workflowSequence;
        }

        // add leave request
        public async Task<LeaveRequest> SubmitLeaveRequestAsync(LeaveRequest request)
        {
            await _context.LeaveRequests.AddAsync(request);
            await _context.SaveChangesAsync();
            return request;
        }

        public async Task<Process> AddProcessLeaveRequest(Process process)
        {
            await _context.Processes.AddAsync(process);
            await _context.SaveChangesAsync();
            return process;
        }
        public async Task<bool> ApproveLeaveRequestAsync(int requestId, string approverRole)
        {
            var request = await _context.LeaveRequests.FindAsync(requestId);
            if (request == null) return false;

            if (approverRole == "Supervisor")
            {
                request.EmployeeId = "HR Manager";
            }
            else if (approverRole == "HR Manager")
            {
                request.Description = "Approved";
            }

            _context.LeaveRequests.Update(request);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RejectLeaveRequestAsync(int requestId, string approverRole)
        {
            var request = await _context.LeaveRequests.FindAsync(requestId);
            if (request == null) return false;

            request.Description = "Rejected";
            _context.LeaveRequests.Update(request);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<WorkflowAction> AddAction(WorkflowAction workflowAction)
        {
            await _context.WorkflowActions.AddAsync(workflowAction);
            await _context.SaveChangesAsync();
            return workflowAction;
        }

        // Approve Leave request
        public async Task<bool> ApproveLeaveRequestAsync(int workflowActionId, int processId, string actorId, string role, bool isApproved, string comment)
        {
            var userRoles = _httpContextAccessor.HttpContext.User.Claims
                  .Where(c => c.Type == ClaimTypes.Role)
                  .Select(c => c.Value)
                  .ToList();

            var leaveRequest = await _context.LeaveRequests.FirstOrDefaultAsync(lr => lr.ProcessId == processId);
            if (leaveRequest == null)
            {
                throw new Exception("Leave request not found.");
            }

            var currentAction = await _context.WorkflowActions
                                              .AsNoTracking()
                                              .FirstOrDefaultAsync(a => a.ProcessId == processId && a.ActorId == actorId && a.StepId == 1); // Ensure only for the right step

            if (currentAction == null) throw new Exception("Current action not found.");

            // Initialize variables for StepId and Status
            int nextStepId = 0;
            string nextProcessStatus = string.Empty;
            string emailSubject = string.Empty;
            string emailBody = string.Empty;
            string employeeEmail = await GetEmployeeEmailById(leaveRequest.EmployeeId);
            // Supervisor approval flow
            if (userRoles.Contains("Supervisor"))
            {
                if (isApproved)
                {
                    // Move to HR approval if Supervisor approves
                    nextStepId = 3; // StepId for HR Manager approval
                    nextProcessStatus = "Pending HR Approval";
                    leaveRequest.Description = "Pending HR Approval";
                }
                else
                {
                    // If Supervisor rejects, end the process
                    nextStepId = 5; // Rejected
                    nextProcessStatus = "Rejected";
                    leaveRequest.Description = "Rejected by Supervisor";
                }

                // Create new action for HR or rejection
                var newAction = new WorkflowAction
                {
                    ProcessId = processId,
                    StepId = isApproved ? nextStepId : 4, // Next step for HR if approved
                    ActorId = isApproved ? "ba2ed92d-d3f0-4eb9-afec-b7706ab4f87a" : actorId, // Assign to HR or keep the same actor for rejection
                    Action = isApproved ? "Pending HR Approval" : "Rejected by Supervisor",
                    ActionDate = DateTime.UtcNow,
                    Comment = comment
                };

                _context.WorkflowActions.Update(newAction);
            }
            // HR Manager approval flow
            else if (userRoles.Contains("HR Manager"))
            {
                if (isApproved)
                {
                    // Approve the leave request
                    nextStepId = 4; // StepId for final approval
                    nextProcessStatus = "Approved";
                    leaveRequest.Description = "Approved by HR";
                }
                else
                {
                    // Reject the leave request
                    nextStepId = 5; // Rejected
                    nextProcessStatus = "Rejected";
                    leaveRequest.Description = "Rejected by HR";
                }

                // Update the HR action in WorkflowActions
                var hrAction = new WorkflowAction
                {
                    ProcessId = processId,
                    StepId = nextStepId,
                    ActorId = actorId,
                    Action = isApproved ? "Approved by HR" : "Rejected by HR",
                    ActionDate = DateTime.UtcNow,
                    Comment = comment
                };

                 _context.WorkflowActions.Update(hrAction);
            }
            else
            {
                throw new UnauthorizedAccessException("You are not authorized to approve this request.");
            }

            // Update the Process
            var process = await _context.Processes.FindAsync(processId);
            if (process != null)
            {
                process.Status = nextProcessStatus;
                process.CurrentStepId = nextStepId;
            }

            

            await _context.SaveChangesAsync();

            return true;
        }

        public async Task SubmitLeaveRequestAsync(LeaveRequest request, string userId)
        {
            var process = new Process
            {
                WorkflowId = 1, // ID for Leave Request Workflow
                RequesterId = userId,
                RequestType = "Leave Request",
                Status = "Pending Supervisor Approval",
                CurrentStepId = 1, // Step ID for Librarian Approval
                RequestDate = DateTime.UtcNow
            };

             _context.Processes.Add(process);
            await _context.SaveChangesAsync();

            var existingRequest = await _context.LeaveRequests.FindAsync(request.RequestId);
            if (existingRequest != null)
            {
                throw new Exception("A leave request with this ID already exists.");
            }

            var leaveRequest = new LeaveRequest
            {
                RequestName = request.RequestName,
                ProcessId = process.ProcessId,
                EmployeeId = userId,
                Description = request.Description,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                LeaveType = request.LeaveType,
                Reason = request.Reason
            };
            _context.LeaveRequests.Add(leaveRequest);
            await _context.SaveChangesAsync();

            var action = new WorkflowAction
            {
                ProcessId = process.ProcessId,
                StepId = 1,
                ActorId = userId,
                Action = process.Status,
                ActionDate = DateTime.UtcNow,
                Comment = "Submitted a leave request"
            };
            _context.WorkflowActions.Add(action);
            await _context.SaveChangesAsync();
        }

        public async Task<string> GetEmployeeEmailById(string appUserId)
        {
            var employee = await _context.Employees
                .Where(e => e.UserId == appUserId) // Assuming 'AppUserId' is the property in 'Employee' that relates to the 'AspNetUsers'
                .FirstOrDefaultAsync();

            return employee?.Email;
        }
    }
}
