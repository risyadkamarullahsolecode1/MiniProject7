using MiniProject7.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniProject7.Domain.Interfaces
{
    public interface IEmployeeRepository
    {
        IQueryable<Employee> GetAllEmployee();
        Task<Employee> GetEmployeeById(int empNo);
        Task<Employee> AddEmployee(Employee employee);
        Task<Employee> UpdateEmployee(Employee employee);
        Task<Employee> UpdateEmployeeAsync(Employee employee);
        Task<bool> DeleteEmployee(int empNo);
        Task SaveChangesAsync();
    }
}
