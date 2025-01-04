using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace AnnounceMS.Infrastructure.Interfaces
{
    public interface IGenericRepostory<T> where T : class
    {
        Task<T> GenericCreate(T entity);
        Task<IEnumerable<T>> GenericRead(bool trackChanges);
        void GenericDelete(T entity);
        IQueryable<T> GenericReadExpression(Expression<Func<T, bool>> expression, bool trackChanges);
        void GenericUpdate(T entity);
        Task<int> GetCountAsync();
    }
}
