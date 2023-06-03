using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Vocation.Core.Models;
using Vocation.Repository.CQRS.Commands;
using Vocation.Repository.CQRS.Queries;

namespace Vocation.Repository.Repositories
{
    public interface IVacationRequestRepository
    {
        Task<Guid> Add(VacationRequest model);
        Task<IEnumerable<VacationRequest>> GetAll();
    }

    public class VacationRequestRepository : IVacationRequestRepository
    {

        private readonly IVacationRequestCommand _vacationCommand;
        private readonly IVacationRequestQuery _vacationQuery;

        public VacationRequestRepository(IVacationRequestCommand vacationCommand, IVacationRequestQuery vacationQuery)
        {
            _vacationCommand = vacationCommand;
            _vacationQuery = vacationQuery;
        }

        public async Task<Guid> Add(VacationRequest model)
        {
            var result = await _vacationCommand.Add(model);
            return result;
        }

        public async Task<IEnumerable<VacationRequest>> GetAll()
        {
            var result = await _vacationQuery.GetAll();
            return result;
        }
    }
}
