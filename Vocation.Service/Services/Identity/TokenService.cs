using System;
using System.Collections.Generic;
using System.Text;
using Vocation.Core.Models.Identity;
using Vocation.Repository.Infrastucture;
using Vocation.Repository.Repositories.Identity;

namespace Vocation.Service.Services.Identity
{
    public interface ITokenService
    {
        void Add(ApplicationUserToken appUserToken);
        ApplicationUserToken FindByKeys(string loginProvider, string refreshToken);
        void Remove(ApplicationUserToken appUserToken);
        void RemoveByRefreshToken(string refreshToken);
    }

    public class TokenService : ITokenService
    {
        private readonly ITokenRepository _tokenRepository;
        private readonly IUnitOFWork _unitOfWork;

        public TokenService(IUnitOFWork unitOfWork, ITokenRepository tokenRepository)
        {
            _unitOfWork = unitOfWork;
            _tokenRepository = tokenRepository;
        }

        public void Add(ApplicationUserToken appUserToken)
        {
            using var tran = _unitOfWork.BeginTransaction();
            _tokenRepository.Add(appUserToken);
            _unitOfWork.SaveChanges();
        }

        public ApplicationUserToken FindByKeys(string loginProvider, string refreshToken)
        {
            using (_unitOfWork.BeginTransaction())
            {
                var result = _tokenRepository.FindByKeys(loginProvider, refreshToken);
                return result;
            }
        }

        public void Remove(ApplicationUserToken appUserToken)
        {
            using (var tran = _unitOfWork.BeginTransaction())
            {
                try
                {
                    _tokenRepository.Remove(appUserToken);
                    _unitOfWork.SaveChanges();
                }
                catch (System.Exception)
                {
                    tran.Rollback();
                }

            }
        }

        public void RemoveByRefreshToken(string refreshToken)
        {
            using (var tran = _unitOfWork.BeginTransaction())
            {
                _tokenRepository.RemoveByRefreshToken(refreshToken);
                _unitOfWork.SaveChanges();
            }
        }
    }
}
