using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PocketMapperORM.Interfaces
{
    public interface IColumnInfo
    {
        string Name { get; }
        string DataType { get; }
        bool IsNullable { get; }
        bool isAutoIncremented { get; }
        bool isUnique { get; }
    }
}
