using PocketMapperORM.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PocketMapperORM.DatabaseObjects
{
    public class ColumnInfo : IColumnInfo
    {
        public string Name { get; set; }
        public string DataType { get; set; }
        public bool IsNullable { get; set; }
        public bool isAutoIncremented { get; set; }
    }
    public class ColumnInfo<TDataType> : IColumnInfo<TDataType>
    {
        public string Name { get; set; }
        public bool IsNullable { get; set; }
        public bool isAutoIncremented { get; set; }

        public IColumnInfo ToColumnInfo()
        {
            throw new NotImplementedException();
        }
    }
}
