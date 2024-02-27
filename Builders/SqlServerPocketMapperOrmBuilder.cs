using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PocketMapperORM.Interfaces;

namespace PocketMapperORM.Builders
{
    public class SqlServerPocketMapperOrmBuilder : IPocketMapperOrmBuilder
    {
        private SqlServerPocketMapperOrm SqlServerPocketMapperOrm = new SqlServerPocketMapperOrm();
        public IPocketMapperOrmBuilder SetConnectionString(string connectionString)
        {
            this.SqlServerPocketMapperOrm.ConnectionString = connectionString;
            return this;
        }

        public IPocketMapperOrmBuilder Reset()
        {
            SqlServerPocketMapperOrm.Dispose();
            SqlServerPocketMapperOrm = new SqlServerPocketMapperOrm();
            return this;
        }

        public SqlServerPocketMapperOrm Build()
        {
            return SqlServerPocketMapperOrm;
        }

    }
}
