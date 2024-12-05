using MiniProject7.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace MiniProject7.Domain.Interfaces
{
    public interface ILeaveRequestRepository
    {
        Task<IEnumerable<LeaveRequest>> GetAllAsync();
        Task<LeaveRequest> GetAsync(int id);
        Task<LeaveRequest> AddAsync(LeaveRequest leaveRequest);
        Task<LeaveRequest> UpdateAsync(LeaveRequest leaveRequest);
        Task DeleteAsync(int id);
        Task<IEnumerable<LeaveRequest>> GetAllByUserAsync(Expression<Func<LeaveRequest, bool>> expression);
        Task<IEnumerable<LeaveRequest>> GetAllToStatusAsync(string userRole);
        Task<LeaveRequest> GetByProcessIdAsync(int processId);
    }
}
