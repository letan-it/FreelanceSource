using Core;
using Data.Repository;
using Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace Services
{
    public class UserProvider : IUserProvider
    {
        private readonly UserRepository repository;

        public UserProvider(IDbContext dbContext)
        {
            repository = new UserRepository(dbContext);
        }

        public DataTable CheckLogin(string UserName)
        {
            return repository.CheckLogin(UserName);
        }

        public DataTable GetQueryString(string UserName)
        {
            return repository.GetQueryString(UserName);
        }

    }
}
