using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Data.SqlClient;
using PocketMapperORM.Annotations;
using PocketMapperORM.Interfaces;
using static PocketMapperORM.Extensions.SqlExtensions;
using System.Data;
using System.Collections;
using PocketMapperORM.Builders;
using PocketMapperORM.DatatypeToSqlMapper;
using PocketMapperORM.Adapters;
using PocketMapperORM.DatabaseObjects;
using PocketMapperORM.Abstracts;
using PocketMapperORM.DatabaseObjects.Tables;
using PocketMapperORM.PocketMapperGroups;

namespace PocketMapperORM
{
    public class SqlServerPocketMapperOrm : PocketMapperOrm<SqlServerTable, SqlServerTableBuilder>
    {
        private bool disposedValue;
        public string ConnectionString { get; set; }
        public SqlServerPocketMapperOrm()
        {
            OnMapperCreated();
        }
        public override IPocketMapperGroup<TOutput> MapTo<TOutput>(string query)
            where TOutput : class
        {
            if(Tables.FirstOrDefault(t => t.TypeOfRepresentedEntity != typeof(TOutput)) == null)
            {
                throw new InvalidOperationException("Failed to map data to provided entity! Entity does not exist in PocketMapperOrm instance!");
            }

            PropertyInfo[] properties = typeof(TOutput).
                GetProperties().
                Where(p => p.GetCustomAttribute<IgnoreFieldAttribute>() is null).
                ToArray();

            List<TOutput> outputs = new List<TOutput>();

            using (var connection = new SqlConnection(ConnectionString).OpenConnection())
                using (var command = new SqlCommand(query, connection))
                    using (var reader = command.ExecuteReader())
            {
                if(reader.HasRows)
                {
                    while (reader.Read())
                    {
                        TOutput item = new TOutput();
                        for(int i = 0;i < reader.FieldCount; ++i)
                        {
                            try
                            {
                                var property = properties.Single(p => p.Name == reader.GetName(i));
                                property.SetValue(item, reader.GetValue(i));
                            }
                            catch (Exception ex)
                            {
                                throw new ArgumentException("Failed mapping values to provided entity! ", innerException: ex);
                            }
                        }
                        outputs.Add(item);
                    }
                }
            }

            return new SqlServerPocketMapperGroup<TOutput>(outputs, this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {

                }  

                disposedValue = true;
            }
        }  
        ~SqlServerPocketMapperOrm()
        {
            Dispose(disposing: false);
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
        public override void UpdateRowByEntity<TEntity>(TEntity entity) where TEntity : class
        {
            
            Type entityType = entity.GetType();
            if (!Tables.Any(t => t.TypeOfRepresentedEntity == entityType))
            {
                throw new InvalidOperationException($"Provided entity ({entityType.Name}) is not present in pocket mapper!");
            }
            var tableNameAttribute = entityType.GetCustomAttribute<TableNameAttribute>();
            string tableName = "dbo." + (tableNameAttribute?.Name ?? entityType.Name);

            var propertiesToUpdate = entityType.
                GetProperties().
                Where(p => p.GetCustomAttribute<IgnoreFieldAttribute>() is null).
                Where(p => p.GetCustomAttribute<PrimaryKeyAttribute>() is null).
                Where(p => (!p.PropertyType.IsClass && !p.PropertyType.IsInterface) || p.PropertyType == typeof(string)).
                ToArray();
            
            var primaryKeyProperty = entityType.
                GetProperties().
                FirstOrDefault(p => p.GetCustomAttribute<PrimaryKeyAttribute>() is not null);

            _ = primaryKeyProperty ?? throw new ArgumentException("Provided entity does not contain a primary key property!", nameof(entity));

            string[] columns = propertiesToUpdate.
                Select(p => p.Name).
                ToArray();

            string[] values = propertiesToUpdate.
                Select(p => '@' + p.Name).
                ToArray();

            string query = $"""
                update {tableName}
                set {string.Join(", ", columns.Select(c => $"{c} = @{c}"))}
                where {primaryKeyProperty.Name} = {'@' + primaryKeyProperty.Name};
                """;

            using (var connection = new SqlConnection(ConnectionString).OpenConnection())
            using (var command = new SqlCommand(query, connection))
            {
                for (int i = 0; i < propertiesToUpdate.Length; ++i)
                    command.Parameters.AddWithValue(values[i], propertiesToUpdate[i].GetValue(entity));

                command.Parameters.AddWithValue('@' + primaryKeyProperty.Name, primaryKeyProperty.GetValue(entity));

                try
                {
                    command.ExecuteNonQuery();
                }
                catch (SqlException ex)
                {
                    throw new ArgumentException("Failed to execute insert command for provided entity! Error on the server: ", innerException: ex);
                }
            }
        }
        public override void AddNewRowByEntity<TEntity>(TEntity entity) where TEntity : class
        {
            Type entityType = entity.GetType();
            var tableNameAttribute = entityType.GetCustomAttribute<TableNameAttribute>();
            string tableName = "dbo." + (tableNameAttribute?.Name ?? entityType.Name);

            var properties = entityType.
                GetProperties().
                Where(p => p.GetCustomAttribute<IgnoreFieldAttribute>() is null).
                Where(p => p.GetCustomAttribute<AutoIncrementedAttribute>() is null).
                ToArray();

            if(!properties.Any())
            {
                throw new ArgumentException("No properties found in provided entity!", nameof(entity));
            }

            string[] columns = properties.
            Select(p => p.Name).
            ToArray();

            string[] values = properties.
            Select(p => '@' + p.Name).
            ToArray();

            string query = $"""
                insert into {tableName} ({string.Join(", ", columns)})
                values ({string.Join(", ", values)});
                """;
            using (var connection = new SqlConnection(ConnectionString).OpenConnection())
                using (var command = new SqlCommand(query, connection))
            {
                for(int i = 0;i < properties.Length; ++i)
                    command.Parameters.AddWithValue(values[i], properties[i].GetValue(entity));
                
                try
                {
                    command.ExecuteNonQuery();
                }
                catch (SqlException ex)
                {
                    throw new ArgumentException("Failed to execute insert command for provided entity! Error on the server: ", innerException: ex);
                }
            }
        }
        public override void UpdateTableSchemaByEntity<TEntity>()
            where TEntity : class
        {

        }
        public void OnMapperCreated()
        {
        
        }

        public override void CreateNewTableSchemaByEntity<TEntity>()
            where TEntity : class
        {
           throw new NotImplementedException();
        }

        public override TOutput[] MapToExternalClass<TOutput>(string query)
        {
            PropertyInfo[] properties = typeof(TOutput).
                GetProperties().
                Where(p => p.GetCustomAttribute<IgnoreFieldAttribute>() is null).
                ToArray();

            List<TOutput> outputs = new List<TOutput>();

            using (var connection = new SqlConnection(ConnectionString).OpenConnection())
            using (var command = new SqlCommand(query, connection))
            using (var reader = command.ExecuteReader())
            {
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        TOutput item = new TOutput();
                        for (int i = 0; i < reader.FieldCount; ++i)
                        {
                            try
                            {
                                var property = properties.Single(p => p.Name == reader.GetName(i));
                                property.SetValue(item, reader.GetValue(i));
                            }
                            catch (Exception ex)
                            {
                                throw new ArgumentException("Failed mapping values to provided entity! ", innerException: ex);
                            }
                        }
                        outputs.Add(item);
                    }
                }
            }

            return outputs.ToArray();
        }

        public override async Task<IPocketMapperGroup<TOutput>> MapToAsync<TOutput>(string query)
        {
            if (Tables.FirstOrDefault(t => t.TypeOfRepresentedEntity != typeof(TOutput)) == null)
            {
                throw new InvalidOperationException("Failed to map data to provided entity! Entity does not exist in PocketMapperOrm instance!");
            }

            PropertyInfo[] properties = typeof(TOutput).
                GetProperties().
                Where(p => p.GetCustomAttribute<IgnoreFieldAttribute>() is null).
                ToArray();

            List<TOutput> outputs = new List<TOutput>();

            using (var connection = await new SqlConnection(ConnectionString).OpenConnectionAsync())
            using (var command = new SqlCommand(query, connection))
            using (var reader = await command.ExecuteReaderAsync())
            {
                if (reader.HasRows)
                {
                    while (await reader.ReadAsync())
                    {
                        TOutput item = new TOutput();
                        for (int i = 0; i < reader.FieldCount; ++i)
                        {
                            try
                            {
                                var property = properties.Single(p => p.Name == reader.GetName(i));
                                property.SetValue(item, reader.GetValue(i));
                            }
                            catch (Exception ex)
                            {
                                throw new ArgumentException("Failed mapping values to provided entity! ", innerException: ex);
                            }
                        }
                        outputs.Add(item);
                    }
                }
            }

            return new SqlServerPocketMapperGroup<TOutput>(outputs, this);
        }

        public override async Task<TOutput[]> MapToExternalClassAsync<TOutput>(string query)
        {
            PropertyInfo[] properties = typeof(TOutput).
                GetProperties().
                Where(p => p.GetCustomAttribute<IgnoreFieldAttribute>() is null).
                ToArray();

            List<TOutput> outputs = new List<TOutput>();

            using (var connection = await new SqlConnection(ConnectionString).OpenConnectionAsync())
            using (var command = new SqlCommand(query, connection))
            using (var reader = await command.ExecuteReaderAsync())
            {
                if (reader.HasRows)
                {
                    while (await reader.ReadAsync())
                    {
                        TOutput item = new TOutput();
                        for (int i = 0; i < reader.FieldCount; ++i)
                        {
                            try
                            {
                                var property = properties.Single(p => p.Name == reader.GetName(i));
                                property.SetValue(item, reader.GetValue(i));
                            }
                            catch (Exception ex)
                            {
                                throw new ArgumentException("Failed mapping values to provided entity! ", innerException: ex);
                            }
                        }
                        outputs.Add(item);
                    }
                }
            }

            return outputs.ToArray();
        }

        public override async Task<IPocketMapperGroup<TOutput>> MapToAsync<TOutput>(string query, CancellationToken token)
        {
            if (Tables.FirstOrDefault(t => t.TypeOfRepresentedEntity != typeof(TOutput)) == null)
            {
                throw new InvalidOperationException("Failed to map data to provided entity! Entity does not exist in PocketMapperOrm instance!");
            }

            PropertyInfo[] properties = typeof(TOutput).
                GetProperties().
                Where(p => p.GetCustomAttribute<IgnoreFieldAttribute>() is null).
                ToArray();

            List<TOutput> outputs = new List<TOutput>();

            using (var connection = await new SqlConnection(ConnectionString).OpenConnectionAsync(token))
            using (var command = new SqlCommand(query, connection))
            using (var reader = await command.ExecuteReaderAsync(cancellationToken: token))
            {
                if (reader.HasRows)
                {
                    while (await reader.ReadAsync(cancellationToken: token))
                    {
                        TOutput item = new TOutput();
                        for (int i = 0; i < reader.FieldCount; ++i)
                        {
                            try
                            {
                                var property = properties.Single(p => p.Name == reader.GetName(i));
                                property.SetValue(item, reader.GetValue(i));
                            }
                            catch (Exception ex)
                            {
                                throw new ArgumentException("Failed mapping values to provided entity! ", innerException: ex);
                            }
                        }
                        outputs.Add(item);

                        token.ThrowIfCancellationRequested();
                    }
                }
            }

            return new SqlServerPocketMapperGroup<TOutput>(outputs, this);
        }

        public override async Task<TOutput[]> MapToExternalClassAsync<TOutput>(string query, CancellationToken token)
        {
            PropertyInfo[] properties = typeof(TOutput).
                GetProperties().
                Where(p => p.GetCustomAttribute<IgnoreFieldAttribute>() is null).
                ToArray();

            List<TOutput> outputs = new List<TOutput>();

            using (var connection = await new SqlConnection(ConnectionString).OpenConnectionAsync(token))
            using (var command = new SqlCommand(query, connection))
            using (var reader = await command.ExecuteReaderAsync(cancellationToken: token))
            {
                if (reader.HasRows)
                {
                    while (await reader.ReadAsync(cancellationToken: token))
                    {
                        TOutput item = new TOutput();
                        for (int i = 0; i < reader.FieldCount; ++i)
                        {
                            try
                            {
                                var property = properties.Single(p => p.Name == reader.GetName(i));
                                property.SetValue(item, reader.GetValue(i));
                            }
                            catch (Exception ex)
                            {
                                throw new ArgumentException("Failed mapping values to provided entity! ", innerException: ex);
                            }
                        }

                        outputs.Add(item);
                        token.ThrowIfCancellationRequested();
                    }
                }
            }

            return outputs.ToArray();
        }
    }
}
