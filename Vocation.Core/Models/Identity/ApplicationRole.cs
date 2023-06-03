using System;
using System.Collections.Generic;
using System.Text;

namespace Vocation.Core.Models.Identity
{
    public class ApplicationRole
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string NormalizedName { get; set; }
        public int GroupId { get; set; }
        public string GroupName { get; set; }
    }

    public class ApplicationRoleUI
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string NormalizedName { get; set; }
    }

    public class ApplicationRoleGroupby
    {
        public int GroupId { get; set; }
        public string GroupName { get; set; }
        public List<ApplicationRoleUI> Roles { get; set; }
    }
}
