using PocketMapperORM.Abstracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PocketMapperORM.Interfaces
{
    public interface ITableBuilder<TTable> : IDisposable
        where TTable : Table<TTable>
    {
        ITableBuilder<TTable> Reset();
        ITableBuilder<TTable> SetTableName(string tableName);
        ITableBuilder<TTable> SetPrimaryKey(IColumnInfo column);
        ITableBuilder<TTable> AddColumn(IColumnInfo column);
        ITableBuilder<TTable> AddForeignKeyConstraint(IColumnInfo foreignKeyColumn,
            IColumnInfo referencedColumn,
            TTable tableWithForeignKey,
            TTable referencedTable);
        ITableBuilder<TTable> AddRepresentedType<TEntity>();
        ITableBuilder<TTable> AddRepresentedType(Type type);
        TTable Build();
        TTable Peek();
    }
}
