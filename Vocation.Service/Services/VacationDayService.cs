using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Vocation.Core.Models;
using Vocation.Repository.Infrastucture;
using Vocation.Repository.Repositories;

namespace Vocation.Service.Services
{
    public interface IVacationDayService
    {
        Task<IEnumerable<VacationDay>> GetAll();
        Task<Guid> Add(VacationDay model);
        Task<IEnumerable<VacationDay>> Update(VacationDay model);
    }

    public class VacationDayService : IVacationDayService
    {
        private readonly IVacationDayRepository _vacationDayRepository;
        private readonly IUnitOFWork _unitOfWork;

        public VacationDayService(IVacationDayRepository vacationDayRepository, IUnitOFWork unitOfWork)
        {
            _vacationDayRepository = vacationDayRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Guid> Add(VacationDay model)
        {
            await using var transaction = _unitOfWork.BeginTransaction();
            try
            {
                var result = await _vacationDayRepository.Add(model);
                _unitOfWork.SaveChanges();
                return result;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public async Task<IEnumerable<VacationDay>> GetAll()
        {
            await using var transaction = _unitOfWork.BeginTransaction();
            try
            {
                var result = await _vacationDayRepository.GetAll();
                return result;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public async Task<IEnumerable<VacationDay>> Update(VacationDay model)
        {
            await using var transaction = _unitOfWork.BeginTransaction();
            try
            {
                await _vacationDayRepository.Update(model);
                var result = await _vacationDayRepository.GetAll();
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
