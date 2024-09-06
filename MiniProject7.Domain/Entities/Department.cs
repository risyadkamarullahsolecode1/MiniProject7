using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniProject7.Domain.Entities
{
    public class Department
    {
        [Key]
        [Column("deptno")]
        public int Deptno { get; set; }

        [Column("deptname")]
        [StringLength(100)]
        public string Deptname { get; set; } = null!;

        [Column("mgrempno")]
        public int? Mgrempno { get; set; }

        [Column("location")]
        [StringLength(100)]
        public string? Location { get; set; }

        [Column("spvempno")]
        public int? Spvempno { get; set; }

        [InverseProperty("DeptnoNavigation")]
        public virtual ICollection<Employee> Employees { get; set; } = new List<Employee>();

        [ForeignKey("Location")]
        [InverseProperty("Departments")]
        public virtual Location? LocationNavigation { get; set; }

        [ForeignKey("Mgrempno")]
        [InverseProperty("DepartmentMgrempnoNavigations")]
        public virtual Employee? MgrempnoNavigation { get; set; }

        [InverseProperty("DeptnoNavigation")]
        public virtual ICollection<Project> Projects { get; set; } = new List<Project>();

        [ForeignKey("Spvempno")]
        [InverseProperty("DepartmentSpvempnoNavigations")]
        public virtual Employee? SpvempnoNavigation { get; set; }
    }
}
