using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Vocation.Core.Models;
using Vocation.Core.Models.Helpers;
using Vocation.Repository.Infrastucture;
using Vocation.Repository.Repositories;

namespace Vocation.Service.Services
{
    public interface IEmployeeService
    {
        Task<IEnumerable<Employee>> GetAll();
        Task<Employee> GetById(string id);
        Task<Employee> Add(Employee employee);
        Task<ListResult<Employee>> GetAllPaginationAsync(string searchtext, int offset, int limit);
        Task<Employee> Update(Employee employee);
        Task<bool> Delete(string id);
        Task<ListResult<Employee>> GetByHeading(string searchtext, int offset, int limit);
        Task<ListResult<Employee>> GetByTeamMembersAsync(string searchtext, int offset, int limit);
        Task<Employee> GetByUserId(string userId);
    }

    public class EmployeeService : IEmployeeService
    {

        private readonly IEmployeeRepository _employeeRepository;
        private readonly IUnitOFWork _unitOfWork;

        public EmployeeService(IEmployeeRepository employeeRepository, IUnitOFWork unitOfWork)
        {
            _employeeRepository = employeeRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<Employee>> GetAll()
        {
            using var transaction = _unitOfWork.BeginTransaction();
            try
            {
                var result = await _employeeRepository.GetAllAsync();
                return result;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public async Task<Employee> GetById(string id)
        {
            using var transaction = _unitOfWork.BeginTransaction();
            try
            {
                var result = await _employeeRepository.GetByIdAsync(id);
                _unitOfWork.SaveChanges();
                return result;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public async Task<Employee> Add(Employee employee)
        {
            using var transaction = _unitOfWork.BeginTransaction();
            try
            {
                var data = await _employeeRepository.AddAsync(employee);
                var result = await _employeeRepository.GetByIdAsync(data.ToString());
                _unitOfWork.SaveChanges();

                return result;
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                throw;
            }
        }

        public async Task<Employee> Update(Employee employee)
        {
            using var transaction = _unitOfWork.BeginTransaction();
            try
            {

                await _employeeRepository.UpdateAsync(employee);
                _unitOfWork.SaveChanges();

                var result = await _employeeRepository.GetByIdAsync(employee.Id.ToString());
                return result;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public async Task<bool> Delete(string id)
        {
            using var transaction = _unitOfWork.BeginTransaction();
            try
            {
                var item = await _employeeRepository.GetByIdAsync(id);
                var res = await _employeeRepository.DeleteAsync(item.Id.ToString());
                _unitOfWork.SaveChanges();
                return res;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public async Task<ListResult<Employee>> GetAllPaginationAsync(string searchtext, int offset, int limit)
        {
            using var transaction = _unitOfWork.BeginTransaction();
            try
            {
                var result = await _employeeRepository.GetAllPaginationAsync(searchtext, offset, limit);
                return result;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public async Task<ListResult<Employee>> GetByHeading(string searchtext, int offset, int limit)
        {
            try
            {
                using (_unitOfWork.BeginTransaction())
                {
                    var result = await _employeeRepository.GetByHeading(searchtext, offset, limit);
                    return result;
                }
            }
            catch (Exception e)
            {

                throw e;
            }
        }

        public async Task<ListResult<Employee>> GetByTeamMembersAsync(string searchtext, int offset, int limit)
        {
            using var transaction = _unitOfWork.BeginTransaction();
            try
            {
                var result = await _employeeRepository.GetByTeamMembersAsync(searchtext, offset, limit);
                return result;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public async Task<Employee> GetByUserId(string userId)
        {
            await using var transaction = _unitOfWork.BeginTransaction();
            try
            {
                var result = await _employeeRepository.GetByUserIdAsync(userId);
                return result;
            }
            catch (Exception e)
            {
                throw e;
            }
        }
    }
}
