using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiniProject7.Domain.Entities;
using MiniProject7.Domain.Interfaces;
using MiniProject7.Infrastructure.Data.Repository;

namespace MiniProject7.WebAPI.Controllers
{
    [Route("api/[Controller]")]
    [ApiController]
    public class EmployeeController : ControllerBase
    {
        private readonly IEmployeeRepository _employeeRepository;

        public EmployeeController(IEmployeeRepository employeeRepository)
        {
            _employeeRepository = employeeRepository;
        }
        // get employee by id only for employee
        [HttpGet("employee/{empNo}")]
        public async Task<ActionResult<Employee>> GetEmployeeByIdEmployee(int empNo)
        {
            var employee = await _employeeRepository.GetEmployeeById(empNo);
            if (employee == null)
            {
                return NotFound();
            }
            return Ok(employee);
        }
        // get employee  only for employee
        [HttpGet]
        public ActionResult<IQueryable<Employee>> GetAllEmployee()
        {
            var employee = _employeeRepository.GetAllEmployee();
            return Ok(employee);
        }
        // get employee for role except for employee
        [HttpGet("view-employee")]
        public ActionResult<IQueryable<Employee>> GetAllEmployeeByAdmin()
        {
            var employee = _employeeRepository.GetAllEmployee();
            return Ok(employee);
        }
        // get employee by id for role except for employee
        [HttpGet("{empNo}")]
        public async Task<ActionResult<Employee>> GetEmployeeById(int empNo)
        {
            var employee = await _employeeRepository.GetEmployeeById(empNo);
            if (employee == null)
            {
                return NotFound();
            }
            return Ok(employee);
        }
        
        // add employee for role except for employee
        [HttpPost]
        public async Task<ActionResult<Employee>> AddEmployee(Employee employee)
        {
            var createdEmployee = await _employeeRepository.AddEmployee(employee);
            return Ok(createdEmployee);
        }

        // edit employee for role except for employee
        [HttpPut("{empNo}")]
        public async Task<IActionResult> UpdateEmployee(int empNo, Employee employee)
        {
            if (empNo != employee.Empno) return BadRequest();

            var updatedEmployee = await _employeeRepository.UpdateEmployee(employee);
            var employeeDto = updatedEmployee;
            return Ok(employeeDto);
        }

        // get employee for role except for employee
        [HttpPut("update-by-employee/{empNo}")]
        public async Task<IActionResult> UpdateEmployeeAsync(int empNo, Employee employee)
        {
            if (empNo != employee.Empno) return BadRequest();
            var updatedEmployee = await _employeeRepository.UpdateEmployeeAsync(employee);

            return Ok(updatedEmployee);
        }

        // delete employee for role except for employee
        [HttpDelete("{empNo}")]
        public async Task<ActionResult<bool>> DeleteEmployee(int empNo)
        {
            var deleted = await _employeeRepository.DeleteEmployee(empNo);
            if (!deleted) return NotFound();
            return Ok("Employee has been deleted !");
        }

    }
}
