using System;
using System.Collections.Generic;
using System.Text;

namespace Vocation.Core.Models
{
    public class Vacation
    {
        public Guid Id { get; set; }
        public int NumberOfDay { get; set; }

        public Guid PositionId { get; set; }
        public Guid EmployeeId { get; set; }
        public bool Status { get; set; }
        public string  Notes { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime StartDate { get; set; }
    }
}
