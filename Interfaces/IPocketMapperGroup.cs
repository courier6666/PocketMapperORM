using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace PocketMapperORM.Interfaces
{
    public interface IPocketMapperGroup<TEntity> : IEnumerable<TEntity>
    {
        IPocketMapperGroup<TEntity> Load<TLoaded>(Expression<Func<TEntity, TLoaded>> selector)
            where TLoaded : class, new();
        public IPocketMapperGroup<TEntity> LoadCollection<TLoaded>(Expression<Func<TEntity, ICollection<TLoaded>>> selector)
            where TLoaded : class, new();
    }
}
