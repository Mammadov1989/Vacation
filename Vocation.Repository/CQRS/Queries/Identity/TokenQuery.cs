using Dapper;
using System;
using System.Collections.Generic;
using System.Text;
using Vocation.Core.Models.Identity;
using Vocation.Repository.Infrastucture;

namespace Vocation.Repository.CQRS.Queries.Identity
{
    public interface ITokenQuery
    {
        ApplicationUserToken FindByKeys(string loginProvider, string refreshToken);
    }

    public class TokenQuery : ITokenQuery
    {
        private const string ByKeysSql = @"SELECT * FROM AppUserTokens WHERE LoginProvider = @loginProvider AND [Value] = @refreshToken";
        private readonly IUnitOFWork _unitOfWork;

        public TokenQuery(IUnitOFWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public ApplicationUserToken FindByKeys(string loginProvider, string refreshToken)
        {
            var parameters = new
            {
                loginProvider,
                refreshToken
            };

            var result = _unitOfWork.GetConnection().QuerySingle<ApplicationUserToken>(ByKeysSql, parameters, _unitOfWork.GetTransaction());
            return result;
        }
    }
}
