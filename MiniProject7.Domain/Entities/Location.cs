using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniProject7.Domain.Entities
{
    public class Location
    {
        [Key]
        public int LocationId { get; set; }

        [Column("location")]
        [StringLength(100)]
        public string Locations { get; set; } = null!;

        [InverseProperty("LocationNavigation")]
        public virtual ICollection<Department> Departments { get; set; } = new List<Department>();
        public virtual ICollection<Project> Projects {  get; set; } = new List<Project>(); 
    }
}
