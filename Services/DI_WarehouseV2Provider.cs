using Core;
using Data.Repository;
using Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace Services
{
    public  class DI_WarehouseV2Provider : IDI_WarehouseV2Provider
    {
        private readonly DI_WarehouseV2Repository repository;

        public DI_WarehouseV2Provider(IDbContext dbContext)
        {
            repository = new DI_WarehouseV2Repository(dbContext);
        }

        public DataTable GetQueryString(string Query)
        {
            return repository.GetQueryString(Query);
        }

    }
}
