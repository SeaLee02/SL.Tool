using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Abstractions.Adapter
{
    public enum DbProvider
    {
        [Description("SqlServer")]
        SqlServer = 0,
        [Description("MySql")]
        MySql = 1,
        [Description("Oracle")]
        Oracle = 2
    }
}
