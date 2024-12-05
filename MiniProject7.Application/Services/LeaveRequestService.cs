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
using System.Reflection.Metadata;
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
            var request = _httpContextAccessor.HttpContext.Request;
            var baseUrl = $"{request.Scheme}://{request.Host}/Uploads/";

            foreach (var role in userRoles)
            {
                if (role == "Employee")
                {
                    // Fetch leave requests created by the user (RequesterId)
                    var userApplications = await _leaveRequestRepository.GetAllByUserAsync(r => r.EmployeeId == user.Id);
                    applications.AddRange(userApplications);
                }
                else if (role == "Employee Supervisor" || role == "HR Manager")
                {
                    // Fetch leave requests assigned to the role (based on the workflow step)
                    var roleApplications = await _leaveRequestRepository.GetAllToStatusAsync(role);
                    applications.AddRange(roleApplications);
                }
            }

            // Include all requests, add total days, and fix ApplicantName
            var allApplications = applications.Select(app => new
            {
                RequestId = app.RequestId,
                RequestName = app.RequestName,
                ProcessId = app.ProcessId,
                StartDate = app.StartDate,
                EndDate = app.EndDate,
                TotalDays = app.EndDate.HasValue && app.StartDate.HasValue
                    ? (app.EndDate.Value.DayNumber - app.StartDate.Value.DayNumber + 1)
                    : 0,
                LeaveType = app.LeaveType,
                Reason = app.Reason,
                SubmissionDate = app.Process?.RequestDate,
                ApplicantName = app.Process?.Requester?.UserName ?? "Unknown", 
                Status = app.Process?.Status,
                CurrentStep = app.Process?.CurrentStep?.StepName ?? "Unknown",
                FileName = app.FileName,
                FilePath = app.FileName != null ? $"{baseUrl}{Uri.EscapeDataString(app.FileName)}" : null
            }).ToList();

            return allApplications;
        }

        public async Task<ProcessDetailDto> GetProcessAsync(int processId)
        {
            var process = await _processRepository.GetByIdAsync(processId);
            if (process == null)
            {
                throw new NullReferenceException("Process not found.");
            }

            var leaveRequest = await _leaveRequestRepository.GetByProcessIdAsync(processId);
            if (leaveRequest == null)
            {
                throw new NullReferenceException("Leave request not found for the given process ID.");
            }

            var workflowActions = await _workflowActionRepository.GetByProcessIdAsync(process.ProcessId) ?? new List<WorkflowAction>();

            var processDetailDto = new ProcessDetailDto
            {
                ProcessId = process.ProcessId,
                RequestName = leaveRequest.RequestName ?? "No request name provided",
                Reason = leaveRequest.Reason ?? "No reason provided",
                LeaveType = leaveRequest.LeaveType ?? "Unknown",
                Status = process.Status ?? "No status available",
                StartDate = leaveRequest.StartDate,
                EndDate = leaveRequest.EndDate,
                Description = leaveRequest.Description ?? "No description provided",
                WorkflowActions = workflowActions.Select(action => new WorkflowActionDto
                {
                    ActionDate = action.ActionDate,
                    ActionBy = action.Actor.UserName ?? "Unknown",
                    Action = action.Action ?? "No action",
                    Comments = action.Comment ?? "No comments"
                }).ToList()
            };

            return processDetailDto;
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

        public async Task<BaseResponseDto> SubmitLeaveRequest(LeaveRequestDto request, IFormFile? file)
        {
            try
            {
                // Validate the file if provided
                if (file != null)
                {
                    var allowedExtensions = new[] { ".pdf", ".jpg", ".jpeg" };
                    var maxFileSize = 5 * 1024 * 1024; // 5 MB

                    var fileExtension = Path.GetExtension(file.FileName).ToLower();
                    if (!allowedExtensions.Contains(fileExtension))
                    {
                        return new BaseResponseDto
                        {
                            Status = "Error",
                            Message = "Invalid file type. Only PDF and JPG/JPEG are allowed."
                        };
                    }

                    if (file.Length > maxFileSize)
                    {
                        return new BaseResponseDto
                        {
                            Status = "Error",
                            Message = "File size exceeds the 5MB limit."
                        };
                    }
                }

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

                // Save the file if provided
                string? savedFileName = null;
                string? savedFilePath = null;
                if (file != null)
                {
                    var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "Uploads");
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    var fileExtension = Path.GetExtension(file.FileName); // Get file extension
                    var originalFileName = Path.GetFileNameWithoutExtension(file.FileName); // Get original file name without ext
                    savedFileName = $"{Guid.NewGuid()}_{originalFileName}{fileExtension}";
                    savedFilePath = Path.Combine(uploadsFolder, savedFileName);

                    using (var stream = new FileStream(savedFilePath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }
                }

                var newProcess = new Process
                {
                    RequesterId = user.Id,
                    WorkflowId = workflow.WorkflowId,
                    RequestType = "Leave Request",
                    Status = "Under Review",
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
                    FileName = savedFileName,
                    FilePath = savedFilePath
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
