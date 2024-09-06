using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniProject7.Domain.Entities
{
    public class Project
    {
        [Key]
        [Column("projno")]
        public int Projno { get; set; }

        [Column("projname")]
        [StringLength(100)]
        public string Projname { get; set; } = null!;

        [Column("deptno")]
        public int? Deptno { get; set; }

        [Column("projectlocation")]
        [StringLength(100)]
        public string? Projectlocation { get; set; }

        [ForeignKey("Deptno")]
        [InverseProperty("Projects")]
        public virtual Department? DeptnoNavigation { get; set; }

        [InverseProperty("ProjnoNavigation")]
        public virtual ICollection<Workson> Worksons { get; set; } = new List<Workson>();
    }
}
