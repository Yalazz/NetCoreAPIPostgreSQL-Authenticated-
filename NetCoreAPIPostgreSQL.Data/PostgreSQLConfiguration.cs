using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetCoreAPIPostgreSQL
{
    public class PostgreSQLConfiguration
    {
        public string ConnectionString { get; }

        public PostgreSQLConfiguration(string connectionString)
        {
            ConnectionString = connectionString;
        }
    }
}

