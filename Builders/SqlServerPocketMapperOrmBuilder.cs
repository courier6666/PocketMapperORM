using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PocketMapperORM.Interfaces;

namespace PocketMapperORM.Builders
{
    public class SqlServerPocketMapperOrmBuilder : IPocketMapperOrmBuilder<SqlServerPocketMapperOrm>
    {
        private SqlServerPocketMapperOrm SqlServerPocketMapperOrm = new SqlServerPocketMapperOrm();
        public IPocketMapperOrmBuilder<SqlServerPocketMapperOrm> SetConnectionString(string connectionString)
        {
            this.SqlServerPocketMapperOrm.ConnectionString = connectionString;
            return this;
        }

        public IPocketMapperOrmBuilder<SqlServerPocketMapperOrm> AddTableEntity<TEntity>()
            where TEntity : class
        {
            SqlServerPocketMapperOrm.AddTableByEntity<TEntity>();
            return this;
        }

        public IPocketMapperOrmBuilder<SqlServerPocketMapperOrm> FormPocketMapperOrmTables()
        {
            SqlServerPocketMapperOrm.FormTables();
            return this;
        }

        public IPocketMapperOrmBuilder<SqlServerPocketMapperOrm> Reset()
        {
            SqlServerPocketMapperOrm = new SqlServerPocketMapperOrm();
            return this;
        }

        public SqlServerPocketMapperOrm Build()
        {
            var res = SqlServerPocketMapperOrm;
            Reset();
            return res;
        }
        
    }
}
