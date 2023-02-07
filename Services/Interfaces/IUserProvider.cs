using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace Services.Interfaces
{
    public interface IUserProvider
    {
        DataTable CheckLogin(string UserName);

        DataTable GetQueryString(string Query);
    }
}
