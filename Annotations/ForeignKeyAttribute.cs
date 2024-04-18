using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PocketMapperORM.Annotations
{
    public class ForeignKeyAttribute : Attribute
    {
        public string NameOfProperty { get; set; } = null;
        public Type TypeOfReferencedEntity { get; set; } = null;
        public string NameOfReferencedColumn { get; set; } = null;
    }
}
