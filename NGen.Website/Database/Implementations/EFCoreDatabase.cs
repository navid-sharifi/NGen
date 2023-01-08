using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace NGen
{
	public class EFCoreDatabase<TEntity> : IDatabase<TEntity> where TEntity : BaseEntity
	{
		protected readonly EfCoreDbContext DbContext;

		public DbSet<TEntity> Entities { get; set; }

		public virtual IQueryable<TEntity> Table => Entities;

		public virtual IQueryable<TEntity> TableNoTracking => Entities.AsNoTracking();

		public EFCoreDatabase(EfCoreDbContext dbContext)
		{
			DbContext = dbContext;
			Entities = DbContext.Set<TEntity>();
		}

		public Task<List<TEntity>> GetListAsync()
		{
			return TableNoTracking.ToListAsync();
		}

		public Task<List<TEntity>> GetListAsync(Expression<Func<TEntity, bool>> predicate)
		{
			return TableNoTracking.Where(predicate).ToListAsync();
		}

		public async Task InsertAsync(TEntity predicate)
		{
			await Entities.AddAsync(predicate);
			await DbContext.SaveChangesAsync();
		}

		public Task<TEntity> FirstOrDefaultAsync() => TableNoTracking.FirstOrDefaultAsync();

		public Task<TEntity> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> predicate) => TableNoTracking.Where(predicate).FirstOrDefaultAsync();

		public TEntity FirstOrDefault(Expression<Func<TEntity, bool>> predicate) => TableNoTracking.Where(predicate).FirstOrDefault();

		public async Task DeleteAsync(Expression<Func<TEntity, bool>> predicate)
		{
			var list = await GetListAsync(predicate);
			if (list.None())
				return;

			Entities.RemoveRange(list);
			await DbContext.SaveChangesAsync();
		}

		public Task UpdateAsync(TEntity row)
		{
			Entities.Update(row);
			return DbContext.SaveChangesAsync();
		}

		
	}
}