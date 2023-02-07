using Core;
using Core.Data.Attributes;
using Core.Entities;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Data.Repository
{
    public class UserRepository : EfRepository<UserEntity>
    {
        public UserRepository(IDbContext dbContext) : base(dbContext)
        {
        }

        [Function("[dbo].[StoreProC]")]
        public DataTable CheckLogin(string UserName)
        {
            return DbContext.ExecuteDatatable((MethodInfo)MethodBase.GetCurrentMethod(), UserName);
        }

        public DataTable GetQueryString(string Query)
        {
            return DbContext.ExecuteQuery(Query).Tables[0];
        }

        public bool CheckExistsUser(string Username)
        {
            var check = DbContext.Set<UserEntity>().Where(u => u.Username == Username).Any();
            return check;
        }

        public IQueryable<UserEntity> GetFormItems(int? UserId)
        {
            var query = from p in DbContext.Set<UserEntity>() select p;

            if (UserId != null)
            {
                query = query.Where(p => p.Id == UserId.Value);
            }
            return query;
        }



    }
}
