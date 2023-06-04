﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Vocation.Core.Models;
using Vocation.Core.Models.Helpers;
using Vocation.Repository.CQRS.Commands;
using Vocation.Repository.CQRS.Queries;

namespace Vocation.Repository.Repositories
{
    public interface IPositionRepository
    {
        Task<Guid> AddAsync(Position item);
        Task UpdateAsync(Position item);
        Task<bool> DeleteAsync(string id);
        Task<IEnumerable<Position>> GetAllAsync();
        Task<ListResult<Position>> GetPaginationAsync(string searchtext, int offset, int limit);
        Task<Position> GetByIdAsync(string id);
    }
    public class PositionRepository : IPositionRepository
    {
        private readonly IPositionCommand employeePostionCommand;
        private readonly IPositionQuery employeePositionQuery;

        public PositionRepository(IPositionCommand employeePostionCommand, IPositionQuery employeePositionQuery)
        {
            this.employeePostionCommand = employeePostionCommand;
            this.employeePositionQuery = employeePositionQuery;
        }

        public async Task<Guid> AddAsync(Position item)
        {
            var data = await employeePostionCommand.Add(item);
            return data;
        }

        public async Task<bool> DeleteAsync(string id)
        {
            var result = await employeePostionCommand.DeleteAsync(id);
            return result;
        }

        public async Task<IEnumerable<Position>> GetAllAsync()
        {
            var data = await employeePositionQuery.GetAllAsync();
            return data;
        }

        public async Task<Position> GetByIdAsync(string id)
        {
            var data = await employeePositionQuery.GetByIdAsync(id);
            return data;
        }

        public async Task<ListResult<Position>> GetPaginationAsync(string searchtext, int offset, int limit)
        {
            var data = await employeePositionQuery.GetPaginationAsync(searchtext, offset, limit);
            return data;
        }

        public async Task UpdateAsync(Position item)
        {
             await employeePostionCommand.Update(item);
        }
    }
}
