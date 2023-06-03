using Dapper;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Vocation.Core.Models;
using Vocation.Repository.Infrastucture;

namespace Vocation.Repository.CQRS.Commands
{
    public interface IVacationDayCommand
    {
        Task<Guid> Add(VacationDay model);
        Task Update(VacationDay model);
    }

    public class VacationDayCommand : IVacationDayCommand
    {
        private readonly IUnitOFWork _unitOfWork;

        public VacationDayCommand(IUnitOFWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        private string _add = $@" INSERT Into VacationDays(NumberOfDay,PositionId,Notes,CreatedDate,DeleteStatus)
                                        OUTPUT Inserted.Id
                                        VALUES(@{nameof(VacationDay.NumberOfDay)},
                                               @{nameof(VacationDay.PositionId)},
                                               @{nameof(VacationDay.Notes)},
                                               @{nameof(VacationDay.CreatedDate)},
                                               0,
                                               NULL)";

        private string _update = $@"UPDATE VacationDays SET 
                                    NumberOfDay = @{nameof(VacationDay.NumberOfDay)},
                                    PositionId = @{nameof(VacationDay.PositionId)},
                                    Notes = @{nameof(VacationDay.Notes)},
                                    WHERE Id = @{nameof(VacationDay.Id)}";

        public async Task<Guid> Add(VacationDay model)
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

        public async Task Update(VacationDay model)
        {
            try
            {
                var result = await _unitOfWork.GetConnection().QueryAsync(_update, model, _unitOfWork.GetTransaction());
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}
