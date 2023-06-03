using System;
using System.Collections.Generic;
using System.Text;

namespace Vocation.Repository.Infrastucture.Models
{
    public class UserVacationModel
    {
        public Guid Id { get; set; }
        public string EmployeeName { get; set; }
        public string DisplayName { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Status { get; set; }
    }
}
