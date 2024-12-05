using Microsoft.EntityFrameworkCore;
using MiniProject7.Domain.Entities;
using MiniProject7.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace MiniProject7.Infrastructure.Data.Repository
{
    public class LeaveRequestRepository:ILeaveRequestRepository
    {
        private readonly CompaniesContext _context;
        public LeaveRequestRepository(CompaniesContext context)
        {
            _context = context;
        }

        public async Task<LeaveRequest> AddAsync(LeaveRequest leaveRequest)
        {
            await _context.LeaveRequests.AddAsync(leaveRequest);
            await _context.SaveChangesAsync();
            return leaveRequest;
        }

        public async Task DeleteAsync(int id)
        {
            var req = await _context.LeaveRequests.FindAsync(id);
            _context.LeaveRequests.Remove(req);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<LeaveRequest>> GetAllAsync()
        {
            return await _context.LeaveRequests.ToListAsync();
        }

        public async Task<LeaveRequest> GetAsync(int id)
        {
            var req = await _context.LeaveRequests.FindAsync(id);
            return req;
        }

        public async Task<LeaveRequest> UpdateAsync(LeaveRequest leaveRequest)
        {
            _context.LeaveRequests.Update(leaveRequest);
            await _context.SaveChangesAsync();
            return leaveRequest;
        }

        public async Task<IEnumerable<LeaveRequest>> GetAllByUserAsync(Expression<Func<LeaveRequest, bool>> expression)
        {
            return await _context.LeaveRequests
                .Include(r => r.Process)
                .ThenInclude(p => p.CurrentStep)
                .ThenInclude(wfs => wfs.RequiredRole)
                .Include(r => r.Process)
                .ThenInclude(p => p.Requester)
                .Include(r => r.Process)
                .ThenInclude(p => p.WorkflowActions)
                .Where(expression) // Filter
                .ToListAsync();
        }

        public async Task<IEnumerable<LeaveRequest>> GetAllToStatusAsync(string userRole)
        {
            return await _context.LeaveRequests
                .Include(r => r.Process)
                    .ThenInclude(p => p.CurrentStep)
                    .ThenInclude(wfs => wfs.RequiredRole)
                .Include(r => r.Process)
                    .ThenInclude(p => p.Requester)
                .Include(r => r.Process)
                    .ThenInclude(p => p.WorkflowActions)
                .Where(r => r.Process.CurrentStep.RequiredRole.Name == userRole)
                .ToListAsync();
        }

        public async Task<LeaveRequest> GetByProcessIdAsync(int processId)
        {
            return await _context.LeaveRequests
                                 .FirstOrDefaultAsync(lr => lr.ProcessId == processId);
        }
    }
}
