using System;
using System.Collections.Generic;
using System.Text;
using Vocation.Core.Models.Identity;
using Vocation.Repository.CQRS.Commands.Identity;
using Vocation.Repository.CQRS.Queries.Identity;

namespace Vocation.Repository.Repositories.Identity
{
    public interface ITokenRepository
    {
        void Add(ApplicationUserToken appUserToken);
        ApplicationUserToken FindByKeys(string loginProvider, string refreshToken);
        void Remove(ApplicationUserToken appUserToken);
        void RemoveByRefreshToken(string refreshToken);
    }

    public class TokenRepository : ITokenRepository
    {
        private readonly ITokenCommand _tokenCommand;
        private readonly ITokenQuery _tokenQuery;

        public TokenRepository(ITokenCommand tokenCommand, ITokenQuery tokenQuery)
        {
            _tokenCommand = tokenCommand;
            _tokenQuery = tokenQuery;
        }

        public void Add(ApplicationUserToken appUserToken)
        {
            _tokenCommand.Execute(appUserToken.UserId, appUserToken.LoginProvider, appUserToken.Name, appUserToken.Value, DateTime.Now, appUserToken.Type);
        }

        public ApplicationUserToken FindByKeys(string loginProvider, string refreshToken)
        {
            var result = _tokenQuery.FindByKeys(loginProvider, refreshToken);
            return result;
        }

        public void Remove(ApplicationUserToken appUserToken)
        {
            _tokenCommand.Remove(appUserToken.UserId, appUserToken.LoginProvider, appUserToken.Name);
        }

        public void RemoveByRefreshToken(string refreshToken)
        {
            _tokenCommand.RemoveByRefreshToken(refreshToken);
        }
    }
}
