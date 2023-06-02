using System;
using System.Collections.Generic;
using System.Text;

namespace Vocation.Core.Models
{
    public class Department
    {
        public Guid Id { get; set; }
        public string ShortName { get; set; }
        public string FullName { get; set; }
        public string Notes { get; set; }
        public DateTime CreatedDate { get; set; }
        public bool DeleteStatus { get; set; }
    }
}
