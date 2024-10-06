using System.Linq.Expressions;

namespace Assignment3_Backend.Models
{
    public interface IRepository
    {
        Task<bool> SaveChangesAsync();
        
        void Add<T>(T entity) where T : class;

        Task<List<T>> GetAllAsync<T>() where T : class;
        IQueryable<T> Query<T>() where T : class;
        IQueryable<T> Include<T, TProperty>(IQueryable<T> query, Expression<Func<T, TProperty>> property) where T : class;
    }
}

