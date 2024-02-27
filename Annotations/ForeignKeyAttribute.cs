using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PocketMapperORM.Annotations
{
    public class ForeignKeyAttribute : Attribute
    {
        public string ReferencedTable { get; set; }
        public string ReferencedColumn { get; set; }
        
    }
    public class ForeignKeyAttribute<TReferencedEntity> : Attribute
    where TReferencedEntity : class
    {
        public string NameOfProperty { get; set; }
    }
}
