using System.Collections.ObjectModel;
using PocketMapperORM.Adapters;
using PocketMapperORM.Annotations;
using PocketMapperORM.Interfaces;
using System.Reflection;

namespace PocketMapperORM.Abstracts
{
    public abstract class PocketMapperOrm<TTable, TTableBuilder> : IPocketMapperOrm
        where TTable : Table<TTable>
        where TTableBuilder : ITableBuilder<TTable>, new()
    {
        protected ICollection<TTable> _tables = new List<TTable>();
        public bool ContainsEntityAsTable<TEntity>()
        {
            return _tables.
                FirstOrDefault(t => t.TypeOfRepresentedEntity == typeof(TEntity))
                != null;
        }

        public bool IsPrimitiveType(Type type)
        {
            return (!type.IsClass && !type.IsInterface) || type == typeof(string);
        }
        public bool ContainsEntityAsTable(Type type)
        {
            return _tables.
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
                Where(p => p.GetCustomAttribute<IgnoreFieldAttribute>() is null && IsPrimitiveType(p.PropertyType)).
                ToArray();

            if (properties.
                Where(p => p.GetCustomAttribute<PrimaryKeyAttribute>() is not null).
                Count() > 1)
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

            PropertyInfo? propertyInfoPrimaryKey;
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

            IColumnInfo tablePrimaryKey = new PropertyToColumnInfoAdapter(propertyInfoPrimaryKey);
            tableBuilder.SetPrimaryKey(tablePrimaryKey);
            IList<PropertyToColumnInfoAdapter> columnInfoAdapters = new Collection<PropertyToColumnInfoAdapter>();
            foreach (var propertyInfo in propertiesWithoutPrimaryKey)
            {
                var columnInfo = new PropertyToColumnInfoAdapter(propertyInfo);
                columnInfoAdapters.Add(columnInfo);
                tableBuilder.AddColumn(columnInfo);
            }

            foreach (var propertyInfo in propertiesWithoutPrimaryKey.
                Where(p => p.GetCustomAttribute(typeof(ForeignKeyAttribute)) is not null))
            {
                var columnInfo = columnInfoAdapters.First(c => c.PropertyInfo == propertyInfo);
                
                var fkAttributeType = typeof(ForeignKeyAttribute);
                var fkAttribute = propertyInfo.GetCustomAttribute(fkAttributeType) as ForeignKeyAttribute;
                
                var nameOfProperty = fkAttribute.NameOfProperty;

                var propertyToLoad = typeof(TEntity).GetProperties().FirstOrDefault(p => p.Name == nameOfProperty);
                var propertyToLoadType = propertyToLoad?.PropertyType;

                var typeAddedInFkAttribute = fkAttribute.TypeOfReferencedEntity;
                if (propertyToLoadType != null && typeAddedInFkAttribute != null && propertyToLoadType != typeAddedInFkAttribute)
                {
                    throw new ArgumentException($"Type added in foreign attribute does not match the type of property to load! {typeAddedInFkAttribute.Name} is not equal to {propertyToLoadType.Name}",
                        nameof(typeAddedInFkAttribute));
                }
                
                
                
                var referencedTableType = propertyToLoadType ?? typeAddedInFkAttribute;
                string referencedTableName =
                    (referencedTableType.GetCustomAttribute<TableNameAttribute>()?.Name
                    ?? referencedTableType.Name);

                TTable? referencedTable = _tables.FirstOrDefault(t => t.TableName == referencedTableName);
            
                if (referencedTableName == tableBuilder.Peek().TableName)
                {
                    referencedTable = tableBuilder.Peek();
                }
                
                if(referencedTable is null)
                {
                    throw new InvalidOperationException($"Cannot add foreign key constraint! Such table does not exist in PocketMapperOrm instance!: {referencedTableName}. Make sure you add tables in correct order!");
                }

                IColumnInfo? referencedColumnInTheTable = referencedTable?.PrimaryKey;
                if (fkAttribute.NameOfReferencedColumn != null)
                {
                    var referencedColumn = referencedTable?.Columns.FirstOrDefault(c => c.Name == fkAttribute.NameOfReferencedColumn);
                    if (referencedColumn == null)
                    {
                        throw new ArgumentException($"There is no such column '{fkAttribute.NameOfReferencedColumn}' in the table '{referencedTable.TableName}'!",
                            nameof(referencedColumn));
                    }
                    
                    if (!referencedColumn.isUnique)
                    {
                        throw new ArgumentException($"The column '{fkAttribute.NameOfReferencedColumn}' does not have 'unique' attribute in the table '{referencedTable.TableName}'!",
                            nameof(referencedColumn));
                    }

                    referencedColumnInTheTable = referencedColumn;
                }
                
                tableBuilder.AddForeignKeyConstraint(columnInfo, referencedColumnInTheTable, tableBuilder.Peek(), referencedTable);
            }

            _tables.Add(tableBuilder.Build());
        }

        public abstract void MigrateToDatabase();
        public abstract TOutput[] MapToExternalClass<TOutput>(string query)
            where TOutput : class, new();

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public abstract Task<IPocketMapperGroup<TOutput>> MapToAsync<TOutput>(string query)
            where TOutput : class, new();
        public abstract Task<IPocketMapperGroup<TOutput>> MapToAsync<TOutput>(string query, CancellationToken token)
            where TOutput : class, new();
        public abstract Task<TOutput[]> MapToExternalClassAsync<TOutput>(string query)
            where TOutput : class, new();
        public abstract Task<TOutput[]> MapToExternalClassAsync<TOutput>(string query, CancellationToken token)
            where TOutput : class, new();
    }
}
