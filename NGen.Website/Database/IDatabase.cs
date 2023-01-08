using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System.Linq.Expressions;

namespace NGen
{
	public interface IDatabase<TEntity> where TEntity : BaseEntity
	{
		Task<List<TEntity>> GetListAsync();

		Task<List<TEntity>> GetListAsync(Expression<Func<TEntity, bool>> predicate);

		Task InsertAsync(TEntity predicate);

		Task<TEntity> FirstOrDefaultAsync();

		Task<TEntity> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> predicate);

		TEntity FirstOrDefault(Expression<Func<TEntity, bool>> predicate);

		Task DeleteAsync(Expression<Func<TEntity, bool>> predicate);

		Task UpdateAsync(TEntity row);

		IQueryable<TEntity> Table { get;  }
	}

	public class Database
	{
		private readonly HttpContext HttpContext;
		public Database(IHttpContextAccessor httpContext)
		{
			HttpContext = httpContext.HttpContext;
		}
		public IDatabase<TEntity> Of<TEntity>() where TEntity : BaseEntity
		{
			return HttpContext.RequestServices.GetRequiredService<IDatabase<TEntity>>();
		}
		public Task DeleteAsync<TEntity>(Expression<Func<TEntity, bool>> predicate) where TEntity : BaseEntity
		{
			return HttpContext.RequestServices.GetRequiredService<IDatabase<TEntity>>()
					.DeleteAsync(predicate);
		}

		public Task InsertAsync<TEntity>(TEntity row) where TEntity : BaseEntity
		{
			return HttpContext.RequestServices.GetRequiredService<IDatabase<TEntity>>()
					.InsertAsync(row);
		}

		public Task UpdateAsync<TEntity>(TEntity row) where TEntity : BaseEntity
		{
			return HttpContext.RequestServices.GetRequiredService<IDatabase<TEntity>>()
					.UpdateAsync(row);
		}
		
		public Task<List<TEntity>> GetListAsync<TEntity>() where TEntity : BaseEntity
		{
			return HttpContext.RequestServices.GetRequiredService<IDatabase<TEntity>>()
					.GetListAsync();
		}

		public Task<List<TEntity>> GetListAsync<TEntity>(Expression<Func<TEntity, bool>> predicate) where TEntity : BaseEntity
		{
			return HttpContext.RequestServices.GetRequiredService<IDatabase<TEntity>>()
					.GetListAsync(predicate);
		}
	}
}
