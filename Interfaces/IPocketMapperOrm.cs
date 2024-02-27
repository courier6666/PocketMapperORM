using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace PocketMapperORM.Interfaces
{
    public interface IPocketMapperOrm : IDisposable
    {
        public IPocketMapperGroup<TOutput> MapTo<TOutput>(string query)
            where TOutput : class, new();
        public TOutput[] MapToExternalClass<TOutput>(string query)
            where TOutput : class, new();
        public void UpdateRowByEntity<TEntity>(TEntity entity)
            where TEntity : class;
        public void AddNewRowByEntity<TEntity>(TEntity entity)
            where TEntity : class;
        public void CreateNewTableSchemaByEntity<TEntity>()
            where TEntity : class;
        public void UpdateTableSchemaByEntity<TEntity>()
            where TEntity : class;
        public void AddTableByEntity<TEntity>()
            where TEntity : class;
    }
}
