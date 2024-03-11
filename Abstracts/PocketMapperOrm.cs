using PocketMapperORM.Adapters;
using PocketMapperORM.Annotations;
using PocketMapperORM.DatabaseObjects;
using PocketMapperORM.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace PocketMapperORM.Abstracts
{
    public abstract class PocketMapperOrm<TTable, TTableBuilder> : IPocketMapperOrm
        where TTable : Table<TTable>
        where TTableBuilder : ITableBuilder<TTable>, new()
    {
        protected ICollection<TTable> Tables = new List<TTable>();
        public bool ContainsEntityAsTable<TEntity>()
        {
            return Tables.
                FirstOrDefault(t => t.TypeOfRepresentedEntity == typeof(TEntity))
                != null;
        }
        public bool ContainsEntityAsTable(Type type)
        {
            return Tables.
                FirstOrDefault(t => t.TypeOfRepresentedEntity == type)
                != null;
        }
        public abstract IPocketMapperGroup<TOutput> MapTo<TOutput>(string query)
            where TOutput : class, new();
        public abstract void UpdateRowByEntity<TEntity>(TEntity entity)
            where TEntity : class;
        public abstract void AddNewRowByEntity<TEntity>(TEntity entity)
            where TEntity : class;
        public abstract void CreateNewTableSchemaByEntity<TEntity>()
            where TEntity : class;
        public abstract void UpdateTableSchemaByEntity<TEntity>()
            where TEntity : class;
        public virtual void AddTableByEntity<TEntity>()
            where TEntity : class
        {
            PropertyInfo[] properties = typeof(TEntity).
                GetProperties().
                Where(p => p.GetCustomAttribute<IgnoreFieldAttribute>() is null).
                ToArray();

            if (properties.
                Where(p => p.GetCustomAttribute<PrimaryKeyAttribute>() is not null).Count() > 1)
            {
                throw new ArgumentException("Provided entity contains more than on primary keys!", typeof(TEntity).Name);
            }

            PropertyInfo[] propertiesWithoutPrimaryKey = properties.
                Where(p => p.GetCustomAttribute<PrimaryKeyAttribute>() is null).
                ToArray();

            ITableBuilder<TTable> tableBuilder = new TTableBuilder();

            tableBuilder.AddRepresentedType<TEntity>();

            string tableName = typeof(TEntity).GetCustomAttribute<TableNameAttribute>()?.Name ?? typeof(TEntity).Name;
            tableBuilder.SetTableName(tableName);

            PropertyInfo propertyInfoPrimaryKey;
            try
            {
                propertyInfoPrimaryKey = properties.
                    SingleOrDefault(p => p.GetCustomAttribute<PrimaryKeyAttribute>() is not null);

                if (propertyInfoPrimaryKey is null)
                {
                    throw new ArgumentException("No primary key found in provided entity!");
                }
            }
            catch (InvalidOperationException ex)
            {
                throw new ArgumentException("Provided entity has more than one primary key! Ensure it has onle one primary key!", innerException: ex);
            }
            catch (Exception ex)
            {
                throw new Exception("Error has occured while retrieving primary key from provided entity!", innerException: ex);
            }

            tableBuilder.SetPrimaryKey(new PropertyToColumnInfoAdapter(propertyInfoPrimaryKey));
            foreach (var propertyInfo in propertiesWithoutPrimaryKey)
            {
                var columnInfo = new PropertyToColumnInfoAdapter(propertyInfo);
                tableBuilder.AddColumn(columnInfo);
            }

            foreach (var propertyInfo in propertiesWithoutPrimaryKey.
                Where(p => p.GetCustomAttribute<ForeignKeyAttribute>() is not null))
            {
                var columnInfo = tableBuilder.Peek().PrimaryKey;

                var fkAttribute = propertyInfo.GetCustomAttribute<ForeignKeyAttribute>();

                string referencedTableName = fkAttribute.ReferencedTable;

                TTable referencedTable = Tables.FirstOrDefault(t => t.TableName == referencedTableName);
                IColumnInfo referencedColumn = Tables.
                    Where(t => t.TableName == referencedTableName).
                    FirstOrDefault().
                    Columns.FirstOrDefault(t => t.Name == fkAttribute.ReferencedColumn);


                if (referencedTable is null)
                {
                    throw new InvalidOperationException($"Cannot add foreign key constraint! Such table does not exist in PocketMapperOrm instance!: {referencedTableName}");
                }

                tableBuilder.AddForeignKeyConstraint(columnInfo, referencedColumn, tableBuilder.Peek(), referencedTable);

            }

            foreach (var propertyInfo in propertiesWithoutPrimaryKey.
                Where(p => p.GetCustomAttribute(typeof(ForeignKeyAttribute<>)) is not null))
            {
                var columnInfo = new PropertyToColumnInfoAdapter(propertyInfoPrimaryKey);

                var fkAttributeType = propertyInfo.GetCustomAttribute(typeof(ForeignKeyAttribute<>)).GetType();

                var referencedTableType = fkAttributeType.GetGenericArguments().First();
                string referencedTableName =
                    (referencedTableType.GetCustomAttribute<TableNameAttribute>()?.Name
                    ?? referencedTableType.Name);

                TTable referencedTable = Tables.FirstOrDefault(t => t.TableName == referencedTableName);

                if(referencedTable is null)
                {
                    throw new InvalidOperationException($"Cannot add foreign key constraint! Such table does not exist in PocketMapperOrm instance!: {referencedTableName}. Make sure you add tables in correct order!");
                }

                IColumnInfo referencedTablePrimaryKey = referencedTable.PrimaryKey;

                tableBuilder.AddForeignKeyConstraint(columnInfo, referencedTablePrimaryKey, tableBuilder.Peek(), referencedTable);
            }

            Tables.Add(tableBuilder.Build());

        }

        public abstract TOutput[] MapToExternalClass<TOutput>(string query)
            where TOutput : class, new();

        public void Dispose()
        {
            throw new NotImplementedException();
        }


    }
}
