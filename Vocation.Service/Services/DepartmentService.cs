using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Vocation.Core.Models;
using Vocation.Repository.Infrastucture;
using Vocation.Repository.Repositories;

namespace Vocation.Service.Services
{
    public interface IDepartmentService
    {
        Task<Guid> Add(Department model);
        Task<IEnumerable<Department>> Update(Department model);
        Task<IEnumerable<Department>> GetAll();
    }

    public class DepartmentService : IDepartmentService
    {
        private readonly IDepartmentRepository _departmentRepository;
        private readonly IUnitOFWork _unitOfWork;


        public DepartmentService(IDepartmentRepository departmentRepository, IUnitOFWork unitOfWork)
        {
            _departmentRepository = departmentRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Guid> Add(Department model)
        {
            await using var transaction = _unitOfWork.BeginTransaction();
            try
            {
                var result = await _departmentRepository.Add(model);
                _unitOfWork.SaveChanges();
                return result;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public async Task<IEnumerable<Department>> GetAll()
        {
            await using var transaction = _unitOfWork.BeginTransaction();
            try
            {
                var result = await _departmentRepository.GetAll();
                return result;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public async Task<IEnumerable<Department>> Update(Department model)
        {
            await using var transaction = _unitOfWork.BeginTransaction();
            try
            {
                await _departmentRepository.Update(model);
                var result = await _departmentRepository.GetAll();
                _unitOfWork.SaveChanges();
                return result;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }
    }
}
