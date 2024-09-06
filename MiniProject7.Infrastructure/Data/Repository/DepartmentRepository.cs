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
    public class DepartmentRepository : IDepartmentRepository
    {
        private readonly CompaniesContext _context;
        public DepartmentRepository(CompaniesContext context)
        {
            _context = context;
        }

        public IQueryable<Department> GetAllDepartment()
        {
            var department = _context.Departments.AsQueryable();
            return department;
        }
        public async Task<Department> GetDepartmentById(int deptNo)
        {
            return await _context.Departments.FindAsync(deptNo);
        }
        public async Task<Department> AddDepartment(Department department)
        {
            _context.Departments.Add(department);
            await _context.SaveChangesAsync();
            return department;
        }
        public async Task<Department> UpdateDepartment(Department department)
        {
            _context.Departments.Update(department);
            await _context.SaveChangesAsync();
            return department;
        }
        public async Task<bool> DeleteDepartment(int deptNo)
        {
            var department = await _context.Departments.FindAsync(deptNo);
            if (department == null)
            {
                return false;
            }

            _context.Departments.Remove(department);
            await _context.SaveChangesAsync();
            return true;
        }
        public async Task<Employee> GetManagerByDeptNoAsync(int deptNo)
        {
            // Get the department by department number
            var department = await _context.Departments
                .Where(d => d.Deptno == deptNo)
                .Select(d => d.Mgrempno) // Assuming Mgrempno is the manager's employee ID
                .FirstOrDefaultAsync();

            if (department == null)
                return null;

            // Get the manager’s details using the employee ID
            return await _context.Employees
                .Where(e => e.Empno == department)
                .FirstOrDefaultAsync();
        }

        public async Task<Employee> GetSupervisorByDeptNoAsync(int deptNo)
        {
            var department = await _context.Departments
                .Where(d => d.Deptno == deptNo)
                .Select(d => d.Spvempno)
                .FirstOrDefaultAsync();

            if (department == null)
                return null;

            return await _context.Employees
                .Where(e => e.Empno == department)
                .FirstOrDefaultAsync();
        }
        public async Task<List<Employee>> GetEmployeesBySupervisorIdAsync(int spvEmpNo)
        {
            // Find the department number where the given supervisor is assigned
            var department = await _context.Departments
                .Where(d => d.Spvempno == spvEmpNo)
                .Select(d => new { d.Deptno, d.Spvempno })
                .FirstOrDefaultAsync();

            var manager = await _context.Departments
                .Where(d => d.Mgrempno == spvEmpNo)
                .Select(d => new { d.Deptno, d.Mgrempno })  // Include department number and manager number
                .FirstOrDefaultAsync();

            if (department == null)
            {
                throw new KeyNotFoundException($"Department with supervisor No {spvEmpNo} not found.");
            }

            // Get employees within the department, excluding the supervisor and manager
            var employees = await _context.Employees
                .Where(e => e.Deptno == department.Deptno && e.Empno != spvEmpNo)  // Exclude the supervisor themselves
                .ToListAsync();

            return employees;
        }
    }
}
