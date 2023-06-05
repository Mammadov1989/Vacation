using Dapper;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Vocation.Core.Models;
using Vocation.Repository.Infrastucture;

namespace Vocation.Repository.CQRS.Commands
{
    public interface IVacationRequestCommand
    {
        Task<Guid> Add(VacationRequest model);
    }

    public class VacationRequestCommand : IVacationRequestCommand
    {

        private readonly IUnitOFWork _unitOfWork;

        public VacationRequestCommand(IUnitOFWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        private string _add = $@" INSERT Into VacationRequests(EmployeeId,Status,CreatedDate,VacationPeriod,StartDate,DeleteStatus)
                                        OUTPUT Inserted.Id
                                        VALUES(@{nameof(VacationRequest.EmployeeId)},
                                               @{nameof(VacationRequest.Status)},
                                               @{nameof(VacationRequest.CreatedDate)},
                                               @{nameof(VacationRequest.VacationPeriod)},
                                               @{nameof(VacationRequest.StartDate)},
                                               0)";

        public async Task<Guid> Add(VacationRequest model)
        {
            try
            {
                var result = await _unitOfWork.GetConnection().QuerySingleAsync<Guid>(_add, model, _unitOfWork.GetTransaction());
                return result;

            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
    }
}
