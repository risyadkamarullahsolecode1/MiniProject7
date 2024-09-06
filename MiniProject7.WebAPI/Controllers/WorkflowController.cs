using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiniProject7.Domain.Entities;
using MiniProject7.Domain.Interfaces;
using MiniProject7.Application.Dtos;

namespace MiniProject7.WebAPI.Controllers
{
    [Route("api/[Controller]")]
    [ApiController]
    public class WorkflowController : ControllerBase
    {
        private readonly IWorkflowRepository _workflowRepository;

        public WorkflowController(IWorkflowRepository workflowRepository)
        {
            _workflowRepository = workflowRepository;
        }

        [HttpPost("add-workflow")]
        public async Task<ActionResult<Workflow>> AddWorkflow(Workflow workflow)
        {
            var result = await _workflowRepository.AddWorkflow(workflow);
            return Ok(result);
        }

        [HttpPost("add-workflow-sequences")]
        public async Task<ActionResult<WorkflowSequence>> AddWorkflow(WorkflowSequence workflow)
        {
            var result = await _workflowRepository.AddWorkflowSequence(workflow);
            return Ok(result);
        }

        [HttpPost]
        public async Task<ActionResult<LeaveRequest>> AddLeaveRequest(LeaveRequest leaveRequest)
        {
            var result = await _workflowRepository.SubmitLeaveRequestAsync(leaveRequest);
            return Ok(result);
        }

        [HttpPost("Add-process")]
        public async Task<ActionResult<Process>> AddProcessLeaveRequest(Process process)
        {
            var result = await _workflowRepository.AddProcessLeaveRequest(process);
            return Ok(result);
        }
        [HttpPost("add-action")]
        public async Task<ActionResult<WorkflowAction>> AddAction(WorkflowAction workflowAction)
        {
            var result = await _workflowRepository.AddAction(workflowAction);
            return Ok(result);
        }

        [Authorize(Roles = "HR Manager, Supervisor")]
        [HttpPut("approve")]
        public async Task<IActionResult> ApproveLeaveRequest(int actionId,[FromBody] LeaveApprovalRequestDto request)
        {
            try
            {
                var result = await _workflowRepository.ApproveLeaveRequestAsync(
                    request.WorkflowActionId,
                    request.ProcessId,
                    request.ActorId,
                    request.Role,
                    request.IsApproved,
                    request.Comment
                );

                return Ok(new { success = result });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [Authorize(Roles = "Employee, HR Manager, Supervisor")]
        [HttpPost("submit-leave")]
        public async Task<ActionResult> SubmitLeaveRequestAsync(LeaveRequest request, string userId)
        {
            // Await the async call to ensure it completes correctly
            await _workflowRepository.SubmitLeaveRequestAsync(request, userId);
            return Ok("Leave request submitted successfully.");
        }

        [HttpGet]
        public async Task<ActionResult<string>> GetEmployeeEmailById(string employeeId)
        {
            var result = await _workflowRepository.GetEmployeeEmailById(employeeId);
            return Ok(result);
        }
    }
}
