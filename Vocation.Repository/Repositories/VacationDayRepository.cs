using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Vocation.Core.Models;
using Vocation.Repository.CQRS.Commands;
using Vocation.Repository.CQRS.Queries;

namespace Vocation.Repository.Repositories
{
    public interface IVacationDayRepository
    {
        Task<IEnumerable<VacationDay>> GetAll();
        Task<Guid> Add(VacationDay model);
        Task Update(VacationDay model);
    }

    public class VacationDayRepository : IVacationDayRepository
    {
        private readonly IVacationDayCommand _vacationDayCommand;
        private readonly IVacationDayQuery _vacationDayQuery;

        public VacationDayRepository(IVacationDayCommand vacationDayCommand, IVacationDayQuery vacationDayQuery)
        {
            _vacationDayCommand = vacationDayCommand;
            _vacationDayQuery = vacationDayQuery;
        }

        public async Task<Guid> Add(VacationDay model)
        {
            var result = await _vacationDayCommand.Add(model);
            return result;
        }

        public async Task<IEnumerable<VacationDay>> GetAll()
        {
            var result = await _vacationDayQuery.GetAll();
            return result;
        }

        public async Task Update(VacationDay model)
        {
            await _vacationDayCommand.Update(model);
        }
    }
}
