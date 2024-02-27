using PocketMapperORM.DatabaseObjects.Constraints;
using PocketMapperORM.DatabaseObjects.Tables;
using PocketMapperORM.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PocketMapperORM.Builders
{
    public class SqlServerTableBuilder : ITableBuilder<SqlServerTable>
    {
        SqlServerTable _table = new SqlServerTable();
        public ITableBuilder<SqlServerTable> AddColumn<TDataType>(IColumnInfo<TDataType> column)
        {
            throw new NotImplementedException();
        }

        public ITableBuilder<SqlServerTable> AddColumn(IColumnInfo column)
        {
            _table.Columns.Add(column);
            return this;
        }

        public ITableBuilder<SqlServerTable> AddForeignKeyConstraint<TDataType>(IColumnInfo<TDataType> column, IColumnInfo<TDataType> columnReferenced, string referencedTable)
        {
            throw new NotImplementedException();
        }

        public ITableBuilder<SqlServerTable> AddForeignKeyConstraint
            (IColumnInfo column,
            IColumnInfo referencedColumn,
            SqlServerTable tableWithForeignKey,
            SqlServerTable referencedTable)
        {
            _table.ForeignKeyConstraints.Add(
                ForeignKeyConstraint<SqlServerTable>.
                    CreateForeignKeyConstraint(
                        column,
                        referencedColumn,
                        tableWithForeignKey,
                        referencedTable));

            return this;
        }

        public ITableBuilder<SqlServerTable> AddRepresentedType<TEntity>()
        {
            _table.TypeOfRepresentedEntity = typeof(TEntity);
            return this;
        }

        public SqlServerTable Build()
        {
            var result = _table;
            _table = new SqlServerTable();
            return result;
        }

        public SqlServerTable Peek()
        {
            return _table;
        }

        public ITableBuilder<SqlServerTable> Reset()
        {
            _table = new SqlServerTable();
            return this;
        }

        public ITableBuilder<SqlServerTable> SetPrimaryKey<TDataType>(IColumnInfo<TDataType> column)
        {
            throw new NotImplementedException();
        }

        public ITableBuilder<SqlServerTable> SetPrimaryKey(IColumnInfo column)
        {
            _table.PrimaryKey = column;
            return this;
        }

        public ITableBuilder<SqlServerTable> SetTableName(string tableName)
        {
            _table.TableName = tableName;
            return this;
        }
    }
}
