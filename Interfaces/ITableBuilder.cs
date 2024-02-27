using PocketMapperORM.Abstracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PocketMapperORM.Interfaces
{
    public interface ITableBuilder<TTable> where
        TTable : Table<TTable>
    {
        ITableBuilder<TTable> Reset();
        ITableBuilder<TTable> SetTableName(string tableName);
        ITableBuilder<TTable> SetPrimaryKey<TDataType>(IColumnInfo<TDataType> column);
        ITableBuilder<TTable> SetPrimaryKey(IColumnInfo column);
        ITableBuilder<TTable> AddColumn<TDataType>(IColumnInfo<TDataType> column);
        ITableBuilder<TTable> AddColumn(IColumnInfo column);
        ITableBuilder<TTable> AddForeignKeyConstraint<TDataType>(IColumnInfo<TDataType> column,
            IColumnInfo<TDataType> columnReferenced,
            string referencedTable);
        ITableBuilder<TTable> AddForeignKeyConstraint(IColumnInfo foreignKeyColumn,
            IColumnInfo referencedColumn,
            TTable tableWithForeignKey,
            TTable referencedTable);
        ITableBuilder<TTable> AddRepresentedType<TEntity>();
        TTable Build();
        TTable Peek();
    }
}
