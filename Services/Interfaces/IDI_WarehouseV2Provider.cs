using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace Services.Interfaces
{
    public interface IDI_WarehouseV2Provider
    {
        DataTable GetQueryString(string Query);
    }
}
