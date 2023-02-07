using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;

namespace Core.Data
{
    public class FunctionInfo
    {
        public string Query { get; set; }
        public DbParameter[] Parameters { get; set; }

    }
}
