using System;
using System.Collections.Generic;
using System.Text;

namespace Vocation.Repository.Infrastucture.Models
{
    public class UserEmployeeModel
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
        public string Address { get; set; }
        public string Name { get; set; }
        public string PostCode { get; set; }
        public string Phone { get; set; }
        public bool Blocked { get; set; }
        public string City { get; set; }
        public Guid CityId { get; set; }


        public string Position { get; set; }
        public Guid PositionId { get; set; }

        public decimal InitialBalance { get; set; }
        public decimal Salary { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string Info { get; set; }
        public bool Active { get; set; }

        public bool IsHead { get; set; }
        public decimal PermissionLimit { get; set; }
    }
}
