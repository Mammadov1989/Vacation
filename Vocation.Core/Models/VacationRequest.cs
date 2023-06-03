using System;
using System.Collections.Generic;
using System.Text;

namespace Vocation.Core.Models
{
    public class VacationRequest
    {
        public Guid Id { get; set; }
        public string  EmployeeName { get; set; }
        public Guid EmployeeId { get; set; }
        public bool Status { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime StartDate { get; set; }
        public int VacationPeriod { get; set; }
        public bool DeleteStatus { get; set; }
    }
}
