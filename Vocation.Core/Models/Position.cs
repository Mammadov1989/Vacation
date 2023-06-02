using System;
using System.Collections.Generic;
using System.Text;

namespace Vocation.Core.Models
{
    public class Position
    {
        public Guid Id { get; set; }
        public string  Name { get; set; }
        public DateTime CreatedDate { get; set; }
        public bool DeleteStatus { get; set; }
    }
}
