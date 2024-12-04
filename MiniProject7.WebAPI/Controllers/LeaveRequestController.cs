using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiniProject7.Application.Dtos;
using MiniProject7.Application.Interfaces;

namespace MiniProject7.WebAPI.Controllers
{
    [Route("api/[Controller]")]
    [ApiController]
    public class LeaveRequestController : ControllerBase
    {
        private readonly ILeaveRequestService _leaveRequestService;

        public LeaveRequestController(ILeaveRequestService leaveRequestService)
        {
            _leaveRequestService = leaveRequestService;
        }

        [Authorize(Roles = "Employee")]
        [HttpPost("submit")]
        public async Task<IActionResult> SubmitRequest(LeaveRequestDto leaveRequest)
        {
            var res = await _leaveRequestService.SubmitLeaveRequest(leaveRequest);
            if (res == null)
            {
                return NotFound();
            }
            return Ok(res);
        }

        [Authorize(Roles = "HR Manager,Employee Supervisor")]
        [HttpPost("review")]
        public async Task<IActionResult> ReviewRequest(ReviewRequestDto reviewRequest)
        {
            var res = await _leaveRequestService.ReviewLeaveRequest(reviewRequest);
            return Ok(res);
        }

        [Authorize]
        [HttpGet("all")]
        public async Task<IActionResult> GetAll()
        {
            var res = await _leaveRequestService.GetAllLeaveRequestStatuses();
            return Ok(res);
        }

        [Authorize]
        [HttpGet("{processId}")]
        public async Task<IActionResult> GetDetail(int processId)
        {
            var res = await _leaveRequestService.GetProcessAsync(processId);
            return Ok(res);
        }
    }
}
