﻿using System.Collections.Generic;
using System.Threading.Tasks;
using Surging.Cloud.CPlatform;
using Surging.Cloud.CPlatform.Exceptions;
using Surging.Cloud.Dapper.Repositories;
using Surging.Hero.Common;

namespace Surging.Hero.Auth.Domain.Users
{
    public class LoginManager : ILoginManager
    {
        private readonly IPasswordHelper _passwordHelper;
        private readonly IDapperRepository<UserInfo, long> _userRepository;

        public LoginManager(IPasswordHelper passwordHelper, IDapperRepository<UserInfo, long> userRepository)
        {
            _passwordHelper = passwordHelper;
            _userRepository = userRepository;
        }

        public async Task<IDictionary<string, object>> Login(string userName, string password)
        {
            var userInfo = await _userRepository.SingleOrDefaultAsync(p =>
                p.UserName == userName || p.Phone == userName || p.Email == userName);
            if (userInfo == null) throw new BusinessException($"不存在账号为{userName}的用户");
            if (userInfo.Status == Status.Invalid) throw new BusinessException("账号为被激活,请先激活该账号");
            if (!_passwordHelper.EncryptPassword(userInfo.UserName, password).Equals(userInfo.Password))
                throw new BusinessException("密码不正确");
            var payload = new Dictionary<string, object>
            {
                {ClaimTypes.UserId, userInfo.Id},
                {ClaimTypes.UserName, userInfo.UserName},
                {ClaimTypes.OrgId, userInfo.OrgId}
            };
            return payload;
        }
    }
}