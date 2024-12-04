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
    public class NextStepRuleRepository:INextStepRuleRepository
    {
        private readonly CompaniesContext _context;

        public NextStepRuleRepository(CompaniesContext context)
        {
            _context = context;
        }

        public async Task CreateAsync(NextStepRule entity)
        {
            await _context.NextStepRules.AddAsync(entity);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(NextStepRule entity)
        {
            _context.NextStepRules.Remove(entity);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<NextStepRule>> GetAllAsync()
        {
            var nextStepRules = await _context.NextStepRules.ToListAsync();

            return nextStepRules;
        }

        public async Task<NextStepRule> GetByIdAsync(int id)
        {
            var nextStepRule = await _context.NextStepRules.FindAsync(id);

            return nextStepRule;
        }

        public async Task<NextStepRule?> GetFirstOrDefaultAsync(Expression<Func<NextStepRule, bool>> expression)
        {
            return await _context.NextStepRules.FirstOrDefaultAsync(expression);
        }

        public async Task UpdateAsync(NextStepRule entity)
        {
            _context.NextStepRules.Update(entity);
            await _context.SaveChangesAsync();
        }
    }
}
