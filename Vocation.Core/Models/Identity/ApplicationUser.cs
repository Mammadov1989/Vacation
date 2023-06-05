using System;
using System.Collections.Generic;
using System.Text;

namespace Vocation.Core.Models.Identity
{
    public class ApplicationUser
    {
        public Guid Id { get; set; }
        public string UserName { get; set; }
        public string NormalizedUserName { get; set; }
        public string Email { get; set; }
        public string NormalizedEmail { get; set; }
        public bool EmailConfirmed { get; set; }
        public string PasswordHash { get; set; }
        public string PhoneNumber { get; set; }
        public bool PhoneNumberConfirmed { get; set; }
        public bool TwoFactorEnabled { get; set; }
        public bool LockoutEnabled { get; set; }
        public string SecurityStamp { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime LastModifiedDate { get; set; }
        public string DisplayName { get; set; }
        public bool PasswordRecovery { get; set; }
        public bool Delete { get; set; }
        public bool Active { get; set; }
    }
}
