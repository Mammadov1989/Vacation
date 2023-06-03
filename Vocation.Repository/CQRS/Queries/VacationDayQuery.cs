using Dapper;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Vocation.Core.Models;
using Vocation.Repository.Infrastucture;

namespace Vocation.Repository.CQRS.Queries
{
    public interface IVacationDayQuery
    {
        Task<IEnumerable<VacationDay>> GetAll();
    }

    public class VacationDayQuery : IVacationDayQuery
    {
        private readonly IUnitOFWork _unitOfWork;

        public VacationDayQuery(IUnitOFWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        private string _getAll = $@"SELECT * FROM VacationDays WHERE DeleteStatus = 0 ";

        public async Task<IEnumerable<VacationDay>> GetAll()
        {
            try
            {
                var result = await _unitOfWork.GetConnection().QueryAsync<VacationDay>(_getAll, null, _unitOfWork.GetTransaction());
                return result;
            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}
