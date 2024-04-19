using PocketMapperORM.Annotations;
using PocketMapperORM.Interfaces;
using System.Collections;
using System.Linq.Expressions;
using System.Reflection;

namespace PocketMapperORM.PocketMapperGroups
{
    public class SqlServerPocketMapperGroup<TEntity> : IPocketMapperGroup<TEntity>
        where TEntity : class
    {
        public static object? GetDefault(Type type)
        {
            if (type.IsValueType)
            {
                return Activator.CreateInstance(type);
            }

            return null;
        }
        public static Type GetPropertyFromGenericCollection(Type type)
        {
            try
            {
                var genericDefinitionType = type.GetGenericTypeDefinition();
                var genericArguments = type.GetGenericArguments();

                if(genericArguments.Length != 1)
                {
                    throw new ArgumentException("Provided type generic arguments count is not equal to 1!", type.Name);
                }

                return genericArguments.First();
            }
            catch
            {
                return null;
            }
        }
        public SqlServerPocketMapperGroup(IEnumerable<TEntity> entities, SqlServerPocketMapperOrm sqlServerPocketMapperOrm)
        {
            _entities = entities.ToArray();
            SqlServerPocketMapperOrm = sqlServerPocketMapperOrm;
        }

        TEntity[] _entities;
        SqlServerPocketMapperOrm SqlServerPocketMapperOrm { get; set; }
        public IEnumerator<TEntity> GetEnumerator()
        {
            return _entities.ToList().GetEnumerator();
        }
        public IPocketMapperGroup<TEntity> Load<TLoaded>(Expression<Func<TEntity, ICollection<TLoaded>>> selector)
            where TLoaded : class, new()
        {
            if (!SqlServerPocketMapperOrm.ContainsEntityAsTable<TLoaded>())
            {
                throw new InvalidOperationException("Cannot load entity that is not present in PocketMapper!");
            }

            if (selector.Body is not MemberExpression)
            {
                throw new ArgumentException("Provided expression is not a member expression! Make sure you provide class member in selector!", nameof(selector));
            }

            string nameOfProperty = ((MemberExpression)selector.Body).Member.Name;

            PropertyInfo? primaryKeyOfEntity = typeof(TEntity).
                GetProperties().
                FirstOrDefault(p => p.GetCustomAttribute<PrimaryKeyAttribute>() != null);

            PropertyInfo? primaryKeyOfLoadedEntity = typeof(TLoaded).
                GetProperties().
                FirstOrDefault(p => p.GetCustomAttribute<PrimaryKeyAttribute>() != null);

            PropertyInfo? foreignKeyPropertyOfLoadedEntities = typeof(TLoaded).
                GetProperties().
                FirstOrDefault(p => p.GetCustomAttribute<ForeignKeyAttribute>()?.NameOfProperty == nameOfProperty);

            if (foreignKeyPropertyOfLoadedEntities == null)
                throw new InvalidOperationException("Cannot perform loading of data! Foreign key property that references collection of entities could not be found! Specify properly foreign key property, so it would reference a collection to be loaded!");

            PropertyInfo? collectionOfLoadedEntitiesProperty = typeof(TEntity).
                GetProperties().
                FirstOrDefault(p => p.Name == nameOfProperty);

            var unqiueIdsOfAllEntities = _entities.
                Select(e => primaryKeyOfEntity.GetValue(e)).
                Distinct().
                ToArray();

            var allLoadedEntities = SqlServerPocketMapperOrm.
                   MapTo<TLoaded>($"""
                        select * from dbo.{(typeof(TLoaded).Name)}
                        where {foreignKeyPropertyOfLoadedEntities.Name} in ({string.Join(", ", unqiueIdsOfAllEntities.Select(id => $"'{id}'").ToArray())});
                    """).ToArray();

            foreach (var entity in _entities)
            {
                var loadedEntitiesForEntity = allLoadedEntities.
                    Where(e => foreignKeyPropertyOfLoadedEntities.
                        GetValue(e).
                        Equals(primaryKeyOfEntity.GetValue(entity))).
                    ToArray();

                if (loadedEntitiesForEntity == null || loadedEntitiesForEntity.Count() <= 0)
                    continue;

                collectionOfLoadedEntitiesProperty.SetValue(entity, loadedEntitiesForEntity);
            }

            return this;
        }
        public IPocketMapperGroup<TEntity> Load<TLoaded>(Expression<Func<TEntity, TLoaded>> selector)
            where TLoaded : class, new()
        {
            if(!SqlServerPocketMapperOrm.ContainsEntityAsTable<TLoaded>())
            {
                throw new InvalidOperationException("Cannot load entity that is not present in PocketMapper!");
            }

            if(selector.Body is not MemberExpression)
            {
                throw new ArgumentException("Provided expression is not a member expression! Make sure you provide class member in selector!", nameof(selector));
            }   
            
            string nameOfProperty = ((MemberExpression)selector.Body).Member.Name;
            
            PropertyInfo primaryKeyOfLoadedEntity = typeof(TLoaded).
                GetProperties().
                FirstOrDefault(p => p.GetCustomAttribute<PrimaryKeyAttribute>() != null);
            
            PropertyInfo foreignKeyProperty = typeof(TEntity).
                GetProperties().
                FirstOrDefault(p => p.GetCustomAttribute<ForeignKeyAttribute>()?.NameOfProperty == nameOfProperty);

            PropertyInfo loadedProperty = typeof(TEntity).
                GetProperties().
                FirstOrDefault(p => p.Name == nameOfProperty);

            var unqiueIdsOfLoadedEntities = _entities.
                Where(e => foreignKeyProperty.GetValue(e) is not null).
                Select(e => foreignKeyProperty.GetValue(e)).
                Distinct().ToArray();

            var allLoadedEntities = SqlServerPocketMapperOrm.
                MapTo<TLoaded>($"""
                    select * from dbo.{loadedProperty.PropertyType.Name}
                    where {primaryKeyOfLoadedEntity.Name} in ({string.Join(", ", unqiueIdsOfLoadedEntities.Select(id=>$"'{id}'").ToArray())});
                """);

            foreach (var entity in _entities)
            {
                if (foreignKeyProperty.GetValue(entity) == GetDefault(foreignKeyProperty.PropertyType))
                    continue;

                var loadedEntity = allLoadedEntities.
                    First(e => foreignKeyProperty.GetValue(entity).Equals(primaryKeyOfLoadedEntity.GetValue(e)));

                loadedProperty.SetValue(entity, loadedEntity);
            }

            return this;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _entities.GetEnumerator();
        }

        public IPocketMapperGroup<TEntity> UpdateToDatabase()
        {
            var propertiesClassesToUpdate = typeof(TEntity).
                GetProperties().
                Where(p => SqlServerPocketMapperOrm.ContainsEntityAsTable(p.PropertyType));

            var propertiesClassesCollectionsToUpdate = typeof(TEntity).
                GetProperties().
                Where(p => SqlServerPocketMapperOrm.ContainsEntityAsTable(GetPropertyFromGenericCollection(p.PropertyType)));
            
            foreach(var property in propertiesClassesToUpdate)
            {
                var uniqueObjects = new HashSet<object>();
                var primaryKeyProperty = property.PropertyType.GetProperties().FirstOrDefault(p => p.GetCustomAttribute<PrimaryKeyAttribute>() is not null);
                
                if(primaryKeyProperty is null)
                {
                    throw new InvalidOperationException($"Failed to update entities, this entity ({property.PropertyType.Name}) does not have a primary key!");
                }

                foreach(var entity in _entities)
                {
                    var subEntity = property.GetValue(entity);
                    if(!uniqueObjects.Any(e => primaryKeyProperty.GetValue(e).Equals(primaryKeyProperty.GetValue(subEntity))))
                        uniqueObjects.Add(subEntity);
  
                }

                foreach(var entity in uniqueObjects)
                {
                    SqlServerPocketMapperOrm.UpdateRowByEntity(entity);
                }
            }

            foreach(var property in propertiesClassesCollectionsToUpdate)
            {
                var uniqueObjects = new HashSet<object>();
                var primaryKeyProperty = GetPropertyFromGenericCollection(property.PropertyType)?.GetProperties().FirstOrDefault(p => p.GetCustomAttribute<PrimaryKeyAttribute>() is not null);

                if (primaryKeyProperty is null)
                {
                    throw new InvalidOperationException($"Failed to update entities, this entity in the collection ({property.PropertyType.Name}) does not have a primary key!");
                }

                foreach (var entity in _entities)
                {
                    IEnumerable coll = (IEnumerable)property.GetValue(entity);

                    if (coll is null)
                        continue;

                    foreach (var subEntity in coll)
                    {
                        if (!uniqueObjects.Any(e => primaryKeyProperty.GetValue(e).Equals(primaryKeyProperty.GetValue(subEntity))))
                            uniqueObjects.Add(subEntity);
                    }

                }

                foreach (var entity in uniqueObjects)
                {
                    SqlServerPocketMapperOrm.UpdateRowByEntity(entity);
                }
            }

            return this;
        }

        public IPocketMapperGroup<TEntity> WhereGroup(Func<TEntity, bool> predicate)
        {
            return new SqlServerPocketMapperGroup<TEntity>(_entities.Where(predicate), SqlServerPocketMapperOrm);
        }

        public Task<IPocketMapperGroup<TEntity>> LoadAsync<TLoaded>(Expression<Func<TEntity, TLoaded>> selector, CancellationToken token)
            where TLoaded : class, new()
        {
            throw new NotImplementedException();
        }

        public async Task<IPocketMapperGroup<TEntity>> LoadAsync<TLoaded>(Expression<Func<TEntity, TLoaded>> selector)
            where TLoaded : class, new()
        {
            if (!SqlServerPocketMapperOrm.ContainsEntityAsTable<TLoaded>())
            {
                throw new InvalidOperationException("Cannot load entity that is not present in PocketMapper!");
            }

            if (selector.Body is not MemberExpression)
            {
                throw new ArgumentException("Provided expression is not a member expression! Make sure you provide class member in selector!", nameof(selector));
            }

            string nameOfProperty = ((MemberExpression)selector.Body).Member.Name;

            PropertyInfo? primaryKeyOfLoadedEntity = typeof(TLoaded).
                GetProperties().
                FirstOrDefault(p => p.GetCustomAttribute<PrimaryKeyAttribute>() != null);


            PropertyInfo? foreignKeyProperty = typeof(TEntity).
                GetProperties().
                FirstOrDefault(p => p.GetCustomAttribute<ForeignKeyAttribute>()?.NameOfProperty == nameOfProperty);

            PropertyInfo? loadedProperty = typeof(TEntity).
                GetProperties().
                FirstOrDefault(p => p.Name == nameOfProperty);

            var unqiueIdsOfLoadedEntities = _entities.
                Where(e => foreignKeyProperty.GetValue(e) is not null).
                Select(e => foreignKeyProperty.GetValue(e)).
                Distinct().ToArray();

            var allLoadedEntities = await SqlServerPocketMapperOrm.
                MapToAsync<TLoaded>($"""
                    select * from dbo.{loadedProperty.PropertyType.Name}
                    where {primaryKeyOfLoadedEntity.Name} in ({string.Join(", ", unqiueIdsOfLoadedEntities.Select(id => $"'{id}'").ToArray())});
                """);

            foreach (var entity in _entities)
            {
                if (foreignKeyProperty.GetValue(entity) == GetDefault(foreignKeyProperty.PropertyType))
                    continue;

                var loadedEntity = allLoadedEntities.
                    First(e => foreignKeyProperty.GetValue(entity).Equals(primaryKeyOfLoadedEntity.GetValue(e)));

                loadedProperty.SetValue(entity, loadedEntity);
            }

            return this;
        }

        public Task<IPocketMapperGroup<TEntity>> LoadAsync<TLoaded>(Expression<Func<TEntity, ICollection<TLoaded>>> selector, CancellationToken token)
            where TLoaded : class, new()
        {
            throw new NotImplementedException();
        }

        public async Task<IPocketMapperGroup<TEntity>> LoadAsync<TLoaded>(Expression<Func<TEntity, ICollection<TLoaded>>> selector)
            where TLoaded : class, new()
        {
             if (!SqlServerPocketMapperOrm.ContainsEntityAsTable<TLoaded>())
            {
                throw new InvalidOperationException("Cannot load entity that is not present in PocketMapper!");
            }

            if (selector.Body is not MemberExpression)
            {
                throw new ArgumentException("Provided expression is not a member expression! Make sure you provide class member in selector!", nameof(selector));
            }

            string nameOfProperty = ((MemberExpression)selector.Body).Member.Name;

            PropertyInfo? primaryKeyOfEntity = typeof(TEntity).
                GetProperties().
                FirstOrDefault(p => p.GetCustomAttribute<PrimaryKeyAttribute>() != null);

            PropertyInfo? primaryKeyOfLoadedEntity = typeof(TLoaded).
                GetProperties().
                FirstOrDefault(p => p.GetCustomAttribute<PrimaryKeyAttribute>() != null);

            PropertyInfo? foreignKeyPropertyOfLoadedEntities = typeof(TLoaded).
                GetProperties().
                FirstOrDefault(p => p.GetCustomAttribute<ForeignKeyAttribute>()?.NameOfProperty == nameOfProperty);

            if (foreignKeyPropertyOfLoadedEntities == null)
                throw new InvalidOperationException("Cannot perform loading of data! Foreign key property that references collection of entities could not be found! Specify properly foreign key property, so it would reference a collection to be loaded!");

            PropertyInfo? collectionOfLoadedEntitiesProperty = typeof(TEntity).
                GetProperties().
                FirstOrDefault(p => p.Name == nameOfProperty);

            var unqiueIdsOfAllEntities = _entities.
                Select(e => primaryKeyOfEntity.GetValue(e)).
                Distinct().
                ToArray();

            var allLoadedEntities = (await SqlServerPocketMapperOrm.
                   MapToAsync<TLoaded>($"""
                        select * from dbo.{(typeof(TLoaded).Name)}
                        where {foreignKeyPropertyOfLoadedEntities.Name} in ({string.Join(", ", unqiueIdsOfAllEntities.Select(id => $"'{id}'").ToArray())});
                    """)).ToArray();

            foreach (var entity in _entities)
            {
                var loadedEntitiesForEntity = allLoadedEntities.
                    Where(e => foreignKeyPropertyOfLoadedEntities.
                        GetValue(e).
                        Equals(primaryKeyOfEntity.GetValue(entity))).
                    ToArray();

                if (loadedEntitiesForEntity == null || loadedEntitiesForEntity.Count() <= 0)
                    continue;

                collectionOfLoadedEntitiesProperty.SetValue(entity, loadedEntitiesForEntity);
            }

            return this;
        }

        public void Dispose()
        {
            SqlServerPocketMapperOrm.Dispose();
        }
    }
}
