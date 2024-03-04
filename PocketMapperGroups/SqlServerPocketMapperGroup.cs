using PocketMapperORM.Annotations;
using PocketMapperORM.Interfaces;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace PocketMapperORM.PocketMapperGroups
{
    public class SqlServerPocketMapperGroup<TEntity> : IPocketMapperGroup<TEntity>
        where TEntity : class
    {
        public static object GetDefault(Type type)
        {
            if (type.IsValueType)
            {
                return Activator.CreateInstance(type);
            }
            return null;
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
        public IPocketMapperGroup<TEntity> LoadCollection<TLoaded>(Expression<Func<TEntity, ICollection<TLoaded>>> selector)
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

            PropertyInfo primaryKeyOfEntity = typeof(TEntity).
                GetProperties().
                FirstOrDefault(p => p.GetCustomAttribute<PrimaryKeyAttribute>() != null);

            PropertyInfo primaryKeyOfLoadedEntity = typeof(TLoaded).
                GetProperties().
                FirstOrDefault(p => p.GetCustomAttribute<PrimaryKeyAttribute>() != null);

            PropertyInfo foreignKeyPropertyOfLoadedEnitties = typeof(TLoaded).
                GetProperties().
                FirstOrDefault(p => p.GetCustomAttribute<ForeignKeyAttribute<TEntity>>() is not null);

            PropertyInfo collectionOfLoadedEntitiesProperty = typeof(TEntity).
                GetProperties().
                FirstOrDefault(p => p.Name == nameOfProperty);

            List<TLoaded> loadedEntitiesBuffer = new List<TLoaded>();

            foreach (var entity in _entities)
            {
                var loadedEntities = SqlServerPocketMapperOrm.
                    MapTo<TLoaded>($"""
                        select * from dbo.{(typeof(TLoaded).Name)}
                        where {foreignKeyPropertyOfLoadedEnitties.Name} = '{primaryKeyOfEntity.GetValue(entity)}';
                    """).ToArray();

                if (!loadedEntities.Any())
                    continue;

                List<TLoaded> collectionOfLoadedEntities = new List<TLoaded>();

                foreach(var loadedEntity in loadedEntities)
                {
                    var loadedEntityInBuffer = loadedEntitiesBuffer.FirstOrDefault(e =>
                        primaryKeyOfLoadedEntity.GetValue(e)
                        ==
                        primaryKeyOfLoadedEntity.GetValue(loadedEntity));

                    if (loadedEntitiesBuffer is not null)
                    {

                        loadedEntitiesBuffer.Add(loadedEntity);
                        collectionOfLoadedEntities.Add(loadedEntity);
                    }
                    else
                    {
                        collectionOfLoadedEntities.Add(loadedEntityInBuffer);
                    }
                }

                collectionOfLoadedEntitiesProperty.SetValue(entity, collectionOfLoadedEntities);
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
                FirstOrDefault(p => p.GetCustomAttribute<ForeignKeyAttribute<TLoaded>>()?.NameOfProperty == nameOfProperty);

            PropertyInfo loadedProperty = typeof(TEntity).
                GetProperties().
                FirstOrDefault(p => p.Name == nameOfProperty);


            List<TLoaded> loadedEntities = new List<TLoaded>();

            foreach(var entity in _entities)
            {
                if (foreignKeyProperty.GetValue(entity) == GetDefault(foreignKeyProperty.PropertyType))
                    continue;

                var loadedEntity = SqlServerPocketMapperOrm.
                    MapTo<TLoaded>($"""
                        select * from dbo.{loadedProperty.PropertyType.Name}
                        where {primaryKeyOfLoadedEntity.Name} = '{foreignKeyProperty.GetValue(entity)}';
                    """).FirstOrDefault();

                if (loadedEntity is null)
                    continue;

                if(!loadedEntities.
                    Any(e =>
                    primaryKeyOfLoadedEntity.GetValue(e) == primaryKeyOfLoadedEntity.GetValue(loadedEntity)))
                {
                    loadedProperty.SetValue(entity, loadedEntity);
                    loadedEntities.Add(loadedEntity);
                }
                else
                {
                    loadedProperty.SetValue(entity, loadedEntities.First(e =>
                        primaryKeyOfLoadedEntity.GetValue(e) == primaryKeyOfLoadedEntity.GetValue(loadedEntity)));
                }

            }

             return this;
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return _entities.GetEnumerator();
        }
    }
}
