using System.Collections.ObjectModel;
using PocketMapperORM.Adapters;
using PocketMapperORM.Annotations;
using PocketMapperORM.Interfaces;
using System.Reflection;
using PocketMapperORM.DatabaseObjects.Constraints;
using PocketMapperORM.Extensions;

namespace PocketMapperORM.Abstracts
{
    public abstract class PocketMapperOrm<TTable, TTableBuilder> : IPocketMapperOrm
        where TTable : Table<TTable>
        where TTableBuilder : ITableBuilder<TTable>, new()
    {
        protected ICollection<TTable> _tables = new List<TTable>();
        protected ICollection<Type> _entityTableTypes = new List<Type>();
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
            #region OldImplementationofAddTableByEntity
            /*
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
            */
            #endregion

            Type entityType = typeof(TEntity);
            if (_entityTableTypes.Any(t => t == entityType))
                throw new InvalidOperationException($"Cannot add provided entity into PocketMapperOrm. Such entity '{entityType}' already exists in PocketMapperOrm instance.");
            
            _entityTableTypes.Add(entityType);
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

        public void FormTables()
        {
            ICollection<TTable> newTables = new List<TTable>();
            var tableBuilder = new TTableBuilder();
            foreach (var typeOfTableEntity in _entityTableTypes)
            {
                PropertyInfo[] properties = typeOfTableEntity.
                GetProperties().
                Where(p => p.GetCustomAttribute<IgnoreFieldAttribute>() is null && IsPrimitiveType(p.PropertyType)).
                ToArray();

                if (properties.
                    Where(p => p.GetCustomAttribute<PrimaryKeyAttribute>() is not null).
                    Count() > 1)
                {
                    throw new ArgumentException("Provided entity contains more than on primary keys!", typeOfTableEntity.Name);
                }
    
                PropertyInfo[] propertiesWithoutPrimaryKey = properties.
                    Where(p => p.GetCustomAttribute<PrimaryKeyAttribute>() is null).
                    ToArray();

                tableBuilder.Reset();
                tableBuilder.AddRepresentedType(typeOfTableEntity);
    
                string tableName = typeOfTableEntity.GetCustomAttribute<TableNameAttribute>()?.Name ?? typeOfTableEntity.Name;
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
                
                newTables.Add(tableBuilder.Build());
            }
            
            foreach(var tb in newTables)
            {
                PropertyInfo[] properties = tb.TypeOfRepresentedEntity.
                    GetProperties().
                    Where(p => p.GetCustomAttribute<IgnoreFieldAttribute>() is null && IsPrimitiveType(p.PropertyType)).
                    ToArray();
                
                PropertyInfo[] propertiesForeignKeys = properties.
                        Where(p => p.GetCustomAttribute<ForeignKeyAttribute>() is not null).
                        ToArray();
                
                foreach (var fkPropertyInfo in propertiesForeignKeys)
                {
                    var columnInfo = tb.Columns.FirstOrDefault(c => c.Name == fkPropertyInfo.GetDbTableColumnName());
                    if (columnInfo is null)
                    {
                        
                    }
                    
                    var fkAttributeType = typeof(ForeignKeyAttribute);
                    var fkAttribute = fkPropertyInfo.GetCustomAttribute(fkAttributeType) as ForeignKeyAttribute;
                
                    var nameOfProperty = fkAttribute?.NameOfProperty;
                    
                    var propertyToLoad = tb.TypeOfRepresentedEntity.
                        GetProperties().
                        FirstOrDefault(p => p.Name == nameOfProperty);
                    
                    var propertyToLoadType = propertyToLoad?.PropertyType;
                    var typeAddedInFkAttribute = fkAttribute?.TypeOfReferencedEntity;
                    
                    if (propertyToLoadType != null && typeAddedInFkAttribute != null && propertyToLoadType != typeAddedInFkAttribute)
                    {
                        throw new InvalidOperationException($"Entity '{tb.TypeOfRepresentedEntity.Name}'. Type added in property '{fkPropertyInfo.Name}' in foreign key attribute does not match the type of property to load! '{typeAddedInFkAttribute.Name}' is not equal to '{propertyToLoadType.Name}'");
                    }

                    if (propertyToLoadType == null && typeAddedInFkAttribute == null)
                    {
                        throw new InvalidOperationException($"Entity '{tb.TypeOfRepresentedEntity.Name}'. No entity found, that is being referenced by foreign key.");
                    }
                    
                    var referencedTableType = propertyToLoadType ?? typeAddedInFkAttribute;
                    string referencedTableName =
                        (referencedTableType.GetCustomAttribute<TableNameAttribute>()?.Name
                         ?? referencedTableType.Name);
                    
                    TTable? referencedTable = newTables.FirstOrDefault(t => t.TableName == referencedTableName);
                
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
                    tb.ForeignKeyConstraints.Add(ForeignKeyConstraint<TTable>.CreateForeignKeyConstraint(columnInfo, referencedColumnInTheTable, tb, referencedTable));
                }
            }
            _tables = newTables;
        }
    }
}
