using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using MiniProject7.Application.Dtos;
using MiniProject7.Application.Dtos.Account;
using MiniProject7.Application.Interfaces;
using MiniProject7.Domain.Entities;
using MiniProject7.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace MiniProject7.Application.Services
{
    public class LeaveRequestService:ILeaveRequestService
    {
        private readonly IWorkflowRepository _workflowRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IEmailService _emailService;
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly INextStepRuleRepository _nextStepRuleRepository;
        private readonly IWorkflowSequenceRepository _workflowSequenceRepository;
        private readonly IWorkflowActionRepository _workflowActionRepository;
        private readonly IProcessRepository _processRepository;
        private readonly ILeaveRequestRepository _leaveRequestRepository;
        private readonly ILogger<LeaveRequestService> _logger;

        public LeaveRequestService(IWorkflowRepository workflowRepository, IHttpContextAccessor httpContextAccessor, IEmailService emailService, UserManager<AppUser> userManager, RoleManager<IdentityRole> roleManager, ILeaveRequestRepository leaveRequestRepository, INextStepRuleRepository nextStepRuleRepository, IWorkflowSequenceRepository workflowSequenceRepository, IWorkflowActionRepository workflowActionRepository, IProcessRepository processRepository, ILogger<LeaveRequestService> logger)
        {
            _workflowRepository = workflowRepository;
            _httpContextAccessor = httpContextAccessor;
            _emailService = emailService;
            _userManager = userManager;
            _roleManager = roleManager;
            _leaveRequestRepository = leaveRequestRepository;
            _workflowSequenceRepository = workflowSequenceRepository;
            _workflowActionRepository = workflowActionRepository;
            _processRepository = processRepository;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _nextStepRuleRepository = nextStepRuleRepository;
        }

        public async Task<IEnumerable<object>> GetAllLeaveRequestStatuses()
        {
            // Get user and roles from HttpContextAccessor
            var userName = _httpContextAccessor.HttpContext?.User.Identity?.Name;
            if (string.IsNullOrEmpty(userName))
            {
                throw new UnauthorizedAccessException("User not authenticated.");
            }

            var user = await _userManager.FindByNameAsync(userName);
            if (user == null)
            {
                throw new InvalidOperationException("User not found.");
            }

            var userRoles = await _userManager.GetRolesAsync(user);
            var applications = new List<LeaveRequest>();

            foreach (var role in userRoles)
            {
                if (role == "Employee")
                {
                    // Fetch book requests created by the user (RequesterId)
                    var userApplications = await _leaveRequestRepository.GetAllByUserAsync(r => r.RequestName == user.Id);
                    applications.AddRange(userApplications);
                }
                else if (role == "Employee Supervisor" || role == "HR Manager")
                {
                    // Fetch book requests assigned to the role (based on the workflow step)
                    var roleApplications = await _leaveRequestRepository.GetAllToStatusAsync(role);
                    applications.AddRange(roleApplications);
                }
            }

            // Include requests that are in progress for Library Users or completed but involve the current user
            var allApplications = applications.Select(app => new
            {
                RequestId = app.RequestId,
                RequestName = app.RequestName,
                ProcessId = app.ProcessId,
                StartDate = app.StartDate,
                EndDate = app.EndDate,
                LeaveType = app.LeaveType,
                ApplicantName = $"{app.Process?.Requester?.UserName}",
                Status = app.Process?.Status,
                CurrentStep = app.Process?.CurrentStep.StepName,  // Shows current step in the workflow
            }).ToList();

            return allApplications;
        }

        public async Task<ProcessDetailDto> GetProcessAsync(int processId)
        {
            var process = await _processRepository.GetByIdAsync(processId);
            if (process == null)
            {
                return null; // Handle the case where process is not found
            }

            // Fetch related BookRequest and WorkflowAction
            var bookRequest = await _leaveRequestRepository.GetAsync(processId);
            var workflowActions = await _workflowActionRepository.GetByProcessIdAsync(process.ProcessId);

            // Construct the response DTO to include BookRequest and WorkflowActions
            var processDetailDto = new ProcessDetailDto
            {
                ProcessId = process.ProcessId,
                RequestName = bookRequest.RequestName,
                Reason = bookRequest.Reason,
                LeaveType = bookRequest.LeaveType,
                Status = process.Status,
                StartDate = bookRequest.StartDate,
                EndDate = bookRequest.EndDate,
                Description = bookRequest.Description,
                WorkflowActions = workflowActions.Select(action => new WorkflowActionDto
                {
                    ActionDate = action.ActionDate,
                    ActionBy = action.ActorId,
                    Action = action.Action,
                    Comments = action.Comment
                }).ToList()
            };

            return processDetailDto; // Return the DTO instead of the entity
        }

        public async Task<BaseResponseDto> ReviewLeaveRequest(ReviewRequestDto reviewRequest)
        {
            try
            {
                // get user and role from httpcontextaccessor
                var userName = _httpContextAccessor.HttpContext!.User.Identity!.Name;
                var user = await _userManager.FindByNameAsync(userName!);
                var userRoles = await _userManager.GetRolesAsync(user!);
                var userRole = userRoles.Single();

                var process = await _processRepository.GetByIdAsync(reviewRequest.ProcessId);

                if (process == null)
                {
                    return new BaseResponseDto
                    {
                        Status = "Error",
                        Message = "Process does not exist"
                    };
                }

                // check if process has a requiredRole, if null then return error
                if (process.CurrentStep.RequiredRole == null)
                {
                    return new BaseResponseDto
                    {
                        Status = "Error",
                        Message = "Process already finished"
                    };
                }

                if (process.CurrentStep.RequiredRole.Name != userRole)
                {
                    return new BaseResponseDto
                    {
                        Status = "Error",
                        Message = "Unauthorize to review request"
                    };
                }

                var newWorkflowAction = new WorkflowAction
                {
                    ProcessId = process.ProcessId,
                    StepId = process.CurrentStepId,
                    ActorId = user.Id,
                    Action = reviewRequest.Action,
                    ActionDate = DateTime.UtcNow,
                    Comment = reviewRequest.Comment
                };

                await _workflowActionRepository.CreateAsync(newWorkflowAction);

                // get nextStepId
                var nextStepRule = await _nextStepRuleRepository.GetFirstOrDefaultAsync(nsr => nsr.CurrentStepId == process.CurrentStepId && nsr.ConditionValue == reviewRequest.Action);
                var nextStepId = nextStepRule!.NextStepId;

                // update process
                process.Status = $"{reviewRequest.Action} by {userRole}";
                process.CurrentStepId = nextStepId;
                await _processRepository.UpdateAsync(process);

                var requests = await _leaveRequestRepository.GetAllAsync();
                var reqData = requests.Where(r => r.ProcessId == process.ProcessId).Single();

                /**if (reviewRequest.Action == "Approved")
                {
                    var newBookRequest = new BookRequest
                    {
                        BookTitle = reqData.BookTitle,
                        Description = reqData.Description,
                        Author = reqData.Author,
                        Publisher = reqData.Publisher,
                        RequestName = user.Id,
                        ProcessId = process.ProcessId
                    };

                    await _bookRequestRepository.AddAsync(newBookRequest);
                }**/

                // send email to other actors
                // get other actors email
                //var workflowActions = await _workflowActionRepository.GetAllAsync();
                //var actorEmails = workflowActions.Where(a => a.ProcessId == process.ProcessId).Select(x => x.Actor.Email).Distinct().ToList();
                // get requester email
                //var requesterEmail = process.Requester.Email;
                // remove requesterEmail from actorEmails so requester only receive the email once
                //actorEmails.Remove(requesterEmail);

                /**if (user != null)
                {
                    var emailSubject = "Book Request Submitted";
                    var emailBody = $"Dear {user.UserName},<br>Your book request for has been reviewed and the answer is {reviewRequest.Comment}.";

                    // Sending email using the email from AspNetUsers
                    await _emailService.SendEmailAsync(requesterEmail, emailSubject, emailBody);
                }**/

                return new BaseResponseDto
                {
                    Status = "Success",
                    Message = "Request Reviewed Sucessfuly"
                };
            }
            catch (Exception ex)
            {
                return new BaseResponseDto
                {
                    Status = "Error",
                    Message = $"An error occurred: {ex.Message}"
                };
            }
        }

        public async Task<BaseResponseDto> SubmitLeaveRequest(LeaveRequestDto request)
        {
            try
            {
                // Get the current logged-in user
                var userName = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.Name);
                if (string.IsNullOrEmpty(userName))
                {
                    return new BaseResponseDto
                    {
                        Status = "Error",
                        Message = "User context is invalid or not authenticated!"
                    };
                }

                var user = await _userManager.FindByNameAsync(userName);
                if (user == null)
                {
                    return new BaseResponseDto
                    {
                        Status = "Error",
                        Message = "User not found!"
                    };
                }

                var workflow = await _workflowRepository.GetFirstOrDefaultAsync(w => w.WorkflowName == "Leave Request");
                if (workflow == null)
                {
                    return new BaseResponseDto
                    {
                        Status = "Error",
                        Message = "Workflow for request not found!"
                    };
                }

                var currentStepId = await _workflowSequenceRepository.GetFirstOrDefaultAsync(wfs => wfs.WorkflowId == workflow.WorkflowId);
                if (currentStepId == null)
                {
                    return new BaseResponseDto
                    {
                        Status = "Error",
                        Message = "Current workflow step not found!"
                    };
                }

                // Log current step ID
                _logger.LogInformation("Fetching next step for CurrentStepId: {@CurrentStepId}, StepId: {StepId}", currentStepId, currentStepId?.StepId);

                // Validate currentStepId and StepId
                if (currentStepId == null || currentStepId.StepId == null)
                {
                    _logger.LogError("CurrentStepId or StepId is null.");
                    return new BaseResponseDto
                    {
                        Status = "Error",
                        Message = "Current workflow step is invalid."
                    };
                }

                // Check if repository is initialized
                if (_nextStepRuleRepository == null)
                {
                    _logger.LogError("_nextStepRuleRepository is not initialized.");
                    return new BaseResponseDto
                    {
                        Status = "Error",
                        Message = "Internal error: Repository not initialized."
                    };
                }

                // Fetch the next step
                var nextStepId = await _nextStepRuleRepository.GetFirstOrDefaultAsync(n => n.CurrentStepId == currentStepId.StepId);
                if (nextStepId == null)
                {
                    _logger.LogWarning("No next step found for CurrentStepId: {CurrentStepId}", currentStepId.StepId);
                    return new BaseResponseDto
                    {
                        Status = "Error",
                        Message = "Next step in the workflow not found!"
                    };
                }

                _logger.LogInformation("NextStepId found: {NextStepId}", nextStepId.NextStepId);

                var newProcess = new Process
                {
                    RequesterId = user.Id,
                    WorkflowId = workflow.WorkflowId,
                    RequestType = "Leave Request",
                    Status = "Pending Approval",
                    RequestDate = DateTime.UtcNow,
                    CurrentStepId = nextStepId.NextStepId,
                };
                await _processRepository.CreateAsync(newProcess);

                var newLeaveRequest = new LeaveRequest
                {
                    RequestName = request.RequestName,
                    StartDate = request.StartDate,
                    EndDate = request.EndDate,
                    LeaveType = request.LeaveType,
                    Description = request.Description,
                    Reason = request.Reason,
                    ProcessId = newProcess.ProcessId,
                    EmployeeId = user.Id,
                };
                await _leaveRequestRepository.AddAsync(newLeaveRequest);

                var newWorkflowAction = new WorkflowAction
                {
                    ProcessId = newProcess.ProcessId,
                    StepId = nextStepId.CurrentStepId,
                    ActorId = user.Id,
                    Action = "Submit",
                    ActionDate = DateTime.UtcNow,
                    Comment = "New Leave Request"
                };
                await _workflowActionRepository.CreateAsync(newWorkflowAction);

                if (!string.IsNullOrEmpty(user.Email))
                {
                    var emailSubject = "Leave Request Submitted";
                    var emailBody = $"Dear {user.UserName},<br>Your leave request for {newLeaveRequest.LeaveType} with reason {newLeaveRequest.Reason} has been submitted and is awaiting approval.";
                    await _emailService.SendEmailAsync(user.Email, emailSubject, emailBody);
                }

                return new BaseResponseDto
                {
                    Status = "Success",
                    Message = "Leave request submitted successfully"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while submitting a leave request.");
                return new BaseResponseDto
                {
                    Status = "Error",
                    Message = $"An error occurred: {ex.Message}"
                };
            }
        }


    }
}
