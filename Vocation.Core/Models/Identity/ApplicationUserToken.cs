using System;
using System.Collections.Generic;
using System.Text;

namespace Vocation.Core.Models.Identity
{
    public class ApplicationUserToken
    {
        public int Id { get; set; }
        public DateTime AddedDate { get; set; }
        public int Type { get; set; }
        public string UserId { get; set; }
        public string LoginProvider { get; set; }
        public string Name { get; set; }
        public string Value { get; set; }
    }
}
