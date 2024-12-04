using MiniProject7.Domain.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniProject7.Application.Dtos
{
    public class LeaveRequestDto
    {
        public string? RequestName { get; set; }
        public string? Description { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? LeaveType { get; set; }
        public string? Reason { get; set; }
    }
}
