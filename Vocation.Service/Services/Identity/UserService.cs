using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vocation.Core.Models.Helpers;
using Vocation.Core.Models.Identity;
using Vocation.Repository.Infrastucture;
using Vocation.Repository.Infrastucture.Identity;
using Vocation.Repository.Infrastucture.Models;
using Vocation.Repository.Repositories.Identity;

namespace Vocation.Service.Services.Identity
{
    public interface IUserService
    {
        Task<IdentityResult> CreateAsync(ApplicationUser user, string password);
        Task<IdentityResult> DeleteAsync(ApplicationUser user);
        Task<ApplicationUser> FindByEmailAsync(string email);
        Task<ApplicationUser> FindByIdAsync(string userId);
        Task<ApplicationUser> FindByNameAsync(string normalizedUserName);
        Task SeApplicationUserNameAsync(ApplicationUser user, string userName);
        Task<IdentityResult> UpdateAsync(ApplicationUser user);
        Task<SignInResult> PasswordSignInAsync(string email, string password, bool rememberMe, bool lockoutOnFailure = false);
        Task<IList<string>> GetRolesAsync(ApplicationUser user);
        //Task<IEnumerable<ApplicationRoleUI>> GetRolesByIdAsync(string userId);
        Task<bool> CheckPasswordAsync(ApplicationUser user, string password);
        Task<string> ValidatePassword(string password);
        Task<ApplicationUser> FindUniqueByNameAsync(string normalizedUserName, string userId);
        Task<ApplicationUser> FindByFullNameAsync(string fullName);
        Task<ApplicationUser> FindUniqueByEmailAsync(string email, string userId);
        Task<IdentityResult> AddToRoleAsync(ApplicationUser user, string roleName);
        //Task<ApplicationUser> Block(ApplicationUser user);
        Task<IdentityResult> ResetPassword(ApplicationUser user, string oldPassword, string newPassword);
        Task<ApplicationUser> FindByPhoneNumberAsync(string fullName);
        Task<IEnumerable<UserEmployeeModel>> GetActiveUsers();
        Task<ListResult<UserEmployeeModel>> GetActiveUsers(int offset, int limit);
        Task<ListResult<UserEmployeeModel>> GetBlockedUsers(int offset, int limit);
        Task<ListResult<UserEmployeeModel>> GetActiveUsers(string searchText, int offset, int limit);
        Task<IEnumerable<UserEmployeeModel>> GetAllUsers();
        Task<ListResult<UserEmployeeModel>> GetAllUsers(int offset, int limit);
        Task<ListResult<UserEmployeeModel>> GetAllUsers(string searchText, int offset, int limit);
        Task<IdentityResult> ResetPassword(string email, string newPassword);
    }


    public class UserService : IUserService
    {
        private readonly VacUserManager _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        private readonly IUserRepository _userRepository;
        private readonly IUnitOFWork _unitOfWork;

        public UserService(
            VacUserManager userManager,
            SignInManager<ApplicationUser> signInManager,
            IUserRepository userRepository, IUnitOFWork unitOfWork)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _userRepository = userRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> CheckPasswordAsync(ApplicationUser user, string password)
        {
            return await _userManager.CheckPasswordAsync(user, password);
        }

        public async Task<IdentityResult> CreateAsync(ApplicationUser user, string password)
        {
            var result = await _userManager.CreateAsync(user, password);
            return result;
        }

        public async Task<IdentityResult> DeleteAsync(ApplicationUser user)
        {
            var result = await _userManager.DeleteAsync(user);
            return result;
        }

        public async Task<ApplicationUser> FindByEmailAsync(string email)
        {
            var result = await _userManager.FindByEmailAsync(email);
            return result;
        }

        public async Task<IdentityResult> AddToRoleAsync(ApplicationUser user, string roleName)
        {
            var result = await _userManager.AddToRoleAsync(user, roleName);
            return result;
        }

        public async Task<ApplicationUser> FindByIdAsync(string userId)
        {
            var result = await _userManager.FindByIdAsync(userId);
            return result;
        }

        public async Task<ApplicationUser> FindByNameAsync(string normalizedUserName)
        {
            var result = await _userManager.FindByNameAsync(normalizedUserName);
            return result;
        }

        public async Task<string> GeApplicationUserNameAsync(ApplicationUser user)
        {
            var result = await _userManager.GetUserNameAsync(user);
            return result;
        }

        public async Task<IList<string>> GetRolesAsync(ApplicationUser user)
        {
            var result = await _userManager.GetRolesAsync(user);
            return result;
        }

        public async Task<SignInResult> PasswordSignInAsync(string email, string password, bool rememberMe, bool lockoutOnFailure = false)
        {
            var result = await _signInManager.PasswordSignInAsync(email, password, rememberMe, lockoutOnFailure);
            return result;
        }

        public async Task SeApplicationUserNameAsync(ApplicationUser user, string userName)
        {
            await _userManager.SetUserNameAsync(user, userName);
        }

        public async Task<IdentityResult> UpdateAsync(ApplicationUser user)
        {
            var result = await _userManager.UpdateAsync(user);
            return result;
        }

        public async Task<string> ValidatePassword(string password)
        {
            List<string> passwordErrors = new List<string>();

            var validators = _userManager.PasswordValidators;

            foreach (var validator in validators)
            {
                var validation = await validator.ValidateAsync(_userManager, null, password);

                if (!validation.Succeeded)
                {
                    foreach (var error in validation.Errors)
                    {
                        passwordErrors.Add(error.Description);
                    }
                }
            }

            var result = passwordErrors.Count > 0 ? passwordErrors.Aggregate((i, j) => i + "\n" + j) : null;

            return result;
        }

        public async Task<ApplicationUser> FindUniqueByNameAsync(string normalizedUserName, string userId)
        {
            var result = await _userManager.FindUniqueByNameAsync(normalizedUserName, userId);
            return result;
        }
        public async Task<ApplicationUser> FindByFullNameAsync(string fullName)
        {
            var result = await _userManager.FindByFullNameAsync(fullName);
            return result;
        }
        public async Task<ApplicationUser> FindUniqueByEmailAsync(string email, string userId)
        {
            var result = await _userManager.FindUniqueByEmailAsync(email, userId);
            return result;
        }

        //public async Task<IEnumerable<ApplicationRoleUI>> GetRolesByIdAsync(string userId)
        //{
        //    var result = await _userManager.GetRoleByIdAsync(userId);
        //    return result;
        //}
        //public async Task<ApplicationUser> Block(ApplicationUser user)
        //{
        //    var result = await _userManager.Block(user);
        //    return result;
        //}

        public async Task<IdentityResult> ResetPassword(ApplicationUser user, string oldPassword, string newPassword)
        {
            try
            {
                var res = await _userManager.ChangePasswordAsync(user, oldPassword, newPassword);
                return res;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<ApplicationUser> FindByPhoneNumberAsync(string phonenumber)
        {
            var result = await _userManager.FindByPhoneNumberAsync(phonenumber);
            return result;

        }

        public async Task<IEnumerable<UserEmployeeModel>> GetActiveUsers()
        {
            await using (_unitOfWork.BeginTransaction())
            {
                var result = await _userRepository.GetActiveUsers();
                return result;
            }
        }

        public async Task<ListResult<UserEmployeeModel>> GetActiveUsers(int offset, int limit)
        {
            await using (_unitOfWork.BeginTransaction())
            {
                var result = await _userRepository.GetActiveUsers(offset, limit);
                return result;
            }
        }

        public async Task<ListResult<UserEmployeeModel>> GetActiveUsers(string searchText, int offset, int limit)
        {
            await using (_unitOfWork.BeginTransaction())
            {
                var result = await _userRepository.GetActiveUsers(searchText, offset, limit);
                return result;
            }
        }

        public async Task<IEnumerable<UserEmployeeModel>> GetAllUsers()
        {
            await using (_unitOfWork.BeginTransaction())
            {
                var result = await _userRepository.GetAllUsers();
                return result;
            }
        }

        public async Task<ListResult<UserEmployeeModel>> GetAllUsers(int offset, int limit)
        {
            await using (_unitOfWork.BeginTransaction())
            {
                var result = await _userRepository.GetAllUsers(offset, limit);
                return result;
            }
        }

        public async Task<ListResult<UserEmployeeModel>> GetAllUsers(string searchText, int offset, int limit)
        {
            await using (_unitOfWork.BeginTransaction())
            {
                var result = await _userRepository.GetAllUsers(searchText, offset, limit);
                return result;
            }
        }


        public async Task<ListResult<UserEmployeeModel>> GetBlockedUsers(int offset, int limit)
        {
            await using (_unitOfWork.BeginTransaction())
            {
                var result = await _userRepository.GetBlockedUsers(offset, limit);
                return result;
            }
        }

        public async Task<IdentityResult> ResetPassword(string email, string newPassword)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null) return null;
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var identityResult = await _userManager.ResetPasswordAsync(user, token, newPassword);
            return identityResult;

        }
    }
}
