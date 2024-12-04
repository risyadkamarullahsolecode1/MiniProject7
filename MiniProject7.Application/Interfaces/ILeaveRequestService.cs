using MiniProject7.Application.Dtos.Account;
using MiniProject7.Application.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniProject7.Application.Interfaces
{
    public interface ILeaveRequestService
    {
        Task<BaseResponseDto> SubmitLeaveRequest(LeaveRequestDto request);
        Task<BaseResponseDto> ReviewLeaveRequest(ReviewRequestDto reviewRequest);
        Task<IEnumerable<object>> GetAllLeaveRequestStatuses();
        Task<ProcessDetailDto> GetProcessAsync(int processId);
    }
}
