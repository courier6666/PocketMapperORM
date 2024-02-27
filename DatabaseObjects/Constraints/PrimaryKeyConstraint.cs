using PocketMapperORM.Abstracts;
using PocketMapperORM.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PocketMapperORM.DatabaseObjects.Constraints
{
    public class PrimaryKeyConstraint<TTable>
        where TTable : Table<TTable>
    {
        public IColumnInfo ColumnInfo { get; private set; }
        public TTable Table { get; private set; }
        public static PrimaryKeyConstraint<TTable> CreateConstraint(IColumnInfo column, TTable table)
        {
            if(column is null) throw new ArgumentNullException(nameof(column));
            
            if(table is null) throw new ArgumentNullException(nameof(table));

            if(table.PrimaryKey is null)
            {
                throw new ArgumentException("Provided column does not exist in the table", nameof(column));
            }

            return new PrimaryKeyConstraint<TTable>(column, table);
        }
        private PrimaryKeyConstraint(IColumnInfo column, TTable table)
        {
            ColumnInfo = column;
            Table = table;
        }
    }
}
