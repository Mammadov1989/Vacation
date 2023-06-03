using Dapper;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Vocation.Core.Models;
using Vocation.Repository.Infrastucture;

namespace Vocation.Repository.CQRS.Queries
{
    public interface IVacationRequestQuery
    {
        Task<IEnumerable<VacationRequest>> GetAll();
    }

    public class VacationRequestQuery : IVacationRequestQuery
    {
        private readonly IUnitOFWork _unitOfWork;

        public VacationRequestQuery(IUnitOFWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        private string _getAll = $@"SELECT * FROM VacationRequests WHERE DeleteStatus = 0 ";

        public async Task<IEnumerable<VacationRequest>> GetAll()
        {
            try
            {
                var result = await _unitOfWork.GetConnection().QueryAsync<VacationRequest>(_getAll, null, _unitOfWork.GetTransaction());
                return result;
            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}
