using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PocketMapperORM.Annotations
{
    public class ColumnDataAttribute : Attribute
    {
        public string Name { get; set; }
        public string DataType { get; set; }
    }
}
