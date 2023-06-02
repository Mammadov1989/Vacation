using System;
using System.Collections.Generic;
using System.Text;

namespace Vocation.Core.Models
{
    public class Employee
    {
        public Guid Id { get; set; }
        public string  Name { get; set; }
        public string  SurName { get; set; }
        public Guid PositionId { get; set; }
        public Guid  DepartmentId { get; set; }
        public DateTime CreatedDate { get; set; }
        public string  Email { get; set; }
        public string  Password { get; set; }
    }
}
