using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniProject7.Application.Dtos
{
    public class ApprovalRequest
    {
        public int? ProcessId { get; set; } // Foreign Key to RequestBook
        public string? ActorId { get; set; } // Foreign Key to AspNetUsers
        public string Role { get; set; }
        public bool IsApproved { get; set; }
        public string comment { get; set; }
    }
}
