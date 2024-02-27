using PocketMapperORM.Abstracts;
using PocketMapperORM.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PocketMapperORM.DatabaseObjects.Constraints
{
    public class ForeignKeyConstraint<TTable>
        where TTable : Table<TTable>
    {
        public IColumnInfo ForeignKeyColumn { get; private set; }
        public IColumnInfo ReferencedColumn { get; private set; }
        public string ReferencedTableName { get; private set; }
        public TTable TableWithForeignKey { get; private set; }
        public TTable ReferencedTable { get; private set; }
        public static ForeignKeyConstraint<TTable> CreateForeignKeyConstraint(
            IColumnInfo foreignKeyColumn,
            IColumnInfo referencedColumn,
            TTable tableWithForeignKey,
            TTable referencedTable)
        {
            if (foreignKeyColumn is null)
                throw new ArgumentNullException(nameof(foreignKeyColumn));

            if (referencedColumn is null)
                throw new ArgumentNullException(nameof(referencedColumn));

            if (referencedTable is null)
                throw new ArgumentNullException(nameof(referencedTable));

            if (tableWithForeignKey is null)
                throw new ArgumentNullException(nameof(tableWithForeignKey));



            return new ForeignKeyConstraint<TTable>(foreignKeyColumn, referencedColumn, referencedTable, tableWithForeignKey);
        }
        private ForeignKeyConstraint(IColumnInfo foreignKeyColumn,
            IColumnInfo referencedColumn,
            TTable tableWithForeignKey,
            TTable referencedTable)
        {
            ForeignKeyColumn = foreignKeyColumn;
            ReferencedColumn = referencedColumn;
            ReferencedTable = referencedTable;
            TableWithForeignKey = tableWithForeignKey;
        }
    }
}
