using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PocketMapperORM.Interfaces
{
    public interface IPocketMapperOrmBuilder<TPocketMapper>
    where TPocketMapper : class, IPocketMapperOrm
    {
        IPocketMapperOrmBuilder<TPocketMapper> SetConnectionString(string connectionString);

        IPocketMapperOrmBuilder<TPocketMapper> AddTableEntity<TEntity>()
            where TEntity : class;

        IPocketMapperOrmBuilder<TPocketMapper> FormPocketMapperOrmTables();
        IPocketMapperOrmBuilder<TPocketMapper> Reset();
        TPocketMapper Build();

    }
}
