using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vocation.Core.Models;
using Vocation.Core.Models.Helpers;
using Vocation.Repository.Infrastucture;
using Vocation.Repository.Repositories;

namespace Vocation.Service.Services
{
    public interface IPositionService
    {
        Task<IEnumerable<Position>> GetAll();
        Task<Position> GetById(string id);
        Task<ListResult<Position>> GetPaginationAsync(string searchtext, int offset, int limit);
        Task<Position> Add(Position currency);
        Task Update(Position employeePosition);
        Task<bool> Delete(string id);
    }

    public class PositionService : IPositionService
    {

        private readonly IPositionRepository _employeePositionRepository;
        private readonly IUnitOFWork _unitOfWork;

        public PositionService(IPositionRepository employeePositionRepository, IUnitOFWork unitOfWork)
        {
            _employeePositionRepository = employeePositionRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<Position>> GetAll()
        {
            await using var transaction = _unitOfWork.BeginTransaction();
            try
            {
                var result = await _employeePositionRepository.GetAllAsync();
                return result;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public async Task<Position> GetById(string id)
        {
            await using var transaction = _unitOfWork.BeginTransaction();
            try
            {
                var result = await _employeePositionRepository.GetByIdAsync(id);
                return result;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public async Task<Position> Add(Position employeePosition)
        {
            await using var transaction = _unitOfWork.BeginTransaction();
            try
            {
                var data = await _employeePositionRepository.GetAllAsync();
                var arr = data.ToArray();
                var result = await _employeePositionRepository.AddAsync(employeePosition);
                //if (result != null)
                //{
                //    for (int i = 0; i < data.Count(); i++)
                //    {
                //        if (arr[i].Status >= employeePosition.Status)
                //        {

                //            arr[i].Status++;
                //            await _employeePositionRepository.UpdateAsync(arr[i]);
                //        }
                //    }
                //}

                _unitOfWork.SaveChanges();
                return result;
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                throw;
            }
        }

        public async Task Update(Position employeePosition)
        {
            await using var transaction = _unitOfWork.BeginTransaction();
            try
            {
                var item = await _employeePositionRepository.GetByIdAsync(employeePosition.Id.ToString());
                //if (item.Status != employeePosition.Status)
                //{

                //    var data = await _employeePositionRepository.GetAllAsync();
                //    var result = await _employeePositionRepository.UpdateAsync(employeePosition);
                //    if (result)
                //    {
                //        var arr = data.ToArray();
                //        for (int i = 0; i < data.Count(); i++)
                //        {
                //            if (item.Status > employeePosition.Status && arr[i].Status >= employeePosition.Status && item.Status > arr[i].Status)
                //            {

                //                arr[i].Status++;
                //                await _employeePositionRepository.UpdateAsync(arr[i]);
                //            }
                //            if (item.Status < employeePosition.Status && arr[i].Status <= employeePosition.Status && item.Status < arr[i].Status)
                //            {

                //                arr[i].Status--;
                //                await _employeePositionRepository.UpdateAsync(arr[i]);
                //            }
                //        }
                //    }

                //}
                //else
                //{
                //    await _employeePositionRepository.UpdateAsync(employeePosition);

                //}
                await _employeePositionRepository.UpdateAsync(employeePosition);
                _unitOfWork.SaveChanges();
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                throw;
            }
        }

        public async Task<bool> Delete(string id)
        {
            await using var transaction = _unitOfWork.BeginTransaction();
            try
            {
                var item = await _employeePositionRepository.GetByIdAsync(id.ToString());
                var res = await _employeePositionRepository.DeleteAsync(id);
                //if (res)
                //{
                //    var data = await _employeePositionRepository.GetAllAsync();
                //    if (item.Status <= data.Count())
                //    {

                //        var arr = data.ToArray();
                //        for (int i = 0; i < data.Count(); i++)
                //        {
                //            if (arr[i].Status > item.Status)
                //            {

                //                arr[i].Status--;
                //                await _employeePositionRepository.UpdateAsync(arr[i]);
                //            }
                //        }

                //    }
                //}
                _unitOfWork.SaveChanges();
                return res;
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                throw;
            }
        }

        public async Task<ListResult<Position>> GetPaginationAsync(string searchtext, int offset, int limit)
        {
            await using var transaction = _unitOfWork.BeginTransaction();
            try
            {
                var result = await _employeePositionRepository.GetPaginationAsync(searchtext, offset, limit);
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
