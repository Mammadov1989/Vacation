using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Vocation.Core.Models;
using Vocation.Repository.Infrastucture;
using Vocation.Repository.Repositories;

namespace Vocation.Service.Services
{
    public interface IVacationRequestService
    {
        Task<Guid> Add(VacationRequest model);
        Task<IEnumerable<VacationRequest>> GetAll();
    }

    public class VacationRequestService : IVacationRequestService
    {

        private readonly IVacationRequestRepository _vacationRepository;
        private readonly IUnitOFWork _unitOfWork;


        public VacationRequestService(IVacationRequestRepository vacationRepository, IUnitOFWork unitOfWork)
        {
            _vacationRepository = vacationRepository;
            _unitOfWork = unitOfWork;
        }
        public async Task<Guid> Add(VacationRequest model)
        {
            await using var transaction = _unitOfWork.BeginTransaction();
            try
            {
                var result = await _vacationRepository.Add(model);
                _unitOfWork.SaveChanges();
                return result;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public async Task<IEnumerable<VacationRequest>> GetAll()
        {
            await using var transaction = _unitOfWork.BeginTransaction();
            try
            {
                var result = await _vacationRepository.GetAll();
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
