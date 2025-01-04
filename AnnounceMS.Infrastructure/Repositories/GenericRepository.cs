using AnnounceMS.Infrastructure.Context;
using AnnounceMS.Infrastructure.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace AnnounceMS.Infrastructure.Repositories
{
    public class GenericRepository<T> : IGenericRepostory<T> where T : class
    {
        protected readonly DataContext _context;
        private readonly DbSet<T> _dbSet;
        public GenericRepository(DataContext context)
        {
            _context = context;
            _dbSet = context.Set<T>();
        }
        public async Task<T> GenericCreate(T entity)
        {
            var result = await _dbSet.AddAsync(entity);
            return result.Entity;
        }

        public void GenericDelete(T entity) => _dbSet.Remove(entity);
        public async Task<IEnumerable<T>> GenericRead(bool trackChanges) => await _dbSet.AsNoTracking().ToListAsync();

        public IQueryable<T> GenericReadExpression(Expression<Func<T, bool>> expression, bool trackChanges) =>
       !trackChanges ? _context.Set<T>().Where(expression).AsNoTracking()
            : _dbSet.Where(expression);
        public void GenericUpdate(T entity) => _dbSet.Update(entity);
        public async Task<int> GetCountAsync() => await _dbSet.CountAsync();

    }
}
