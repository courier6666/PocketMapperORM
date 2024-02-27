using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PocketMapperORM.Interfaces
{
    public interface IPocketMapperOrmBuilder
    {
        IPocketMapperOrmBuilder SetConnectionString(string connectionString);
        IPocketMapperOrmBuilder Reset();
    }
}
