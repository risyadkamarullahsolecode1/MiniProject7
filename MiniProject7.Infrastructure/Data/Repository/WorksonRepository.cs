using Microsoft.EntityFrameworkCore;
using MiniProject7.Domain.Entities;
using MiniProject7.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniProject7.Infrastructure.Data.Repository
{
    public class WorksonRepository : IWorksonRepository
    {
        private readonly CompaniesContext _context;

        public WorksonRepository(CompaniesContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Workson>> GetAllWorkson()
        {
            return await _context.Worksons.ToListAsync();
        }
        public async Task<Workson> GetWorksonById(int empNo, int projNo)
        {
            return await _context.Worksons.FindAsync(empNo, projNo);
        }
        public async Task<Workson> AddWorkson(Workson workson)
        {
            _context.Worksons.Add(workson);
            await _context.SaveChangesAsync();
            return workson;
        }
        public async Task<Workson> UpdateWorkson(Workson workson)
        {
            _context.Worksons.Update(workson);
            await _context.SaveChangesAsync();
            return workson;

        }
        public async Task<bool> DeleteWorkson(int empNo, int projNo)
        {
            var deleted = await _context.Worksons.FindAsync(empNo, projNo);
            if (deleted == null)
            {
                return false;
            }
            _context.Worksons.Remove(deleted);
            await _context.SaveChangesAsync();
            return true;
        }
        public async Task<IEnumerable<Workson>> GetWorksonByEmployee(int empNo)
        {
            return await _context.Worksons.ToListAsync();
        }
    }
}
