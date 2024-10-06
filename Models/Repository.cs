using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Assignment3_Backend.Models
{
    public class Repository:IRepository
    {
        private readonly AppDbContext _appDbContext;

        public Repository(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }

        public void Add<T>(T entity) where T : class
        {
            _appDbContext.Add(entity);
        }
        public async Task<List<T>> GetAllAsync<T>() where T : class
        {
            return await _appDbContext.Set<T>().ToListAsync();
        }

        public IQueryable<T> Query<T>() where T : class
        {
            return _appDbContext.Set<T>().AsQueryable();
        }

        public IQueryable<T> Include<T, TProperty>(IQueryable<T> query, Expression<Func<T, TProperty>> property) where T : class
        {
            return query.Include(property);
        }
        public async Task<bool> SaveChangesAsync()
        {
            return await _appDbContext.SaveChangesAsync() > 0;
        }
    }
}
