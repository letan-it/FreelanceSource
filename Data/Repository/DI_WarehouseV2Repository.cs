using Core;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace Data.Repository
{
    public class DI_WarehouseV2Repository : EfRepository<BaseEntity>
    {
        public DI_WarehouseV2Repository(IDbContext dbContext) : base(dbContext)
        {
        }

        public DataTable GetQueryString(string Query)
        {
            return DbContext.ExecuteQuery(Query).Tables[0];
        }

    }
}
