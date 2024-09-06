using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniProject7.Application.Dtos.Account
{
    public class RegisterEmployee
    {
        public int Empno { get; set; }
        public string Fname { get; set; } = null!;
        public string Lname { get; set; } = null!;
        public string Address { get; set; } = null!;
        public DateOnly Dob { get; set; }
        public string? Sex { get; set; }
        public int? Phonenumber { get; set; }
        public string? Email { get; set; }
        public string? Password { get; set; }
        public string? Position { get; set; }
        public int? Deptno { get; set; }
        public string? Employeetype { get; set; }
        public int? Level { get; set; }
        public DateTime? Lastupdateddate { get; set; }
        public int? Nik { get; set; }
        public string? Status { get; set; }
        public string? Statusreason { get; set; }
        public int? Salary { get; set; }
        public string? UserId { get; set; }
    }
}
