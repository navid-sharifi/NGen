using Microsoft.EntityFrameworkCore;
using RepoDb.Enumerations;
using System.Reflection;

namespace NGen
{
	public class EfCoreDbContext : DbContext
	{
		public EfCoreDbContext(DbContextOptions<EfCoreDbContext> options) : base(options) { }

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			base.OnModelCreating(modelBuilder);

			var assembliesFiles = Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory);

			foreach (var file in assembliesFiles.Where(c => c.EndsWith(".dll")))
			{
				var assemblies = Assembly.LoadFrom(file);
				var types = assemblies.GetExportedTypes()
							 .Where(c => c.IsClass && !c.IsAbstract && c.IsPublic && typeof(BaseEntity).IsAssignableFrom(c));

				foreach (var type in types)
					modelBuilder.Entity(type);

			}
		}

		public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
		{
			return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
		}

		public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
		{
			foreach (var entityEntry in ChangeTracker.Entries()) // Iterate all made changes
			{
				if (entityEntry.State == EntityState.Added) // If you want to update TenantId when Order is added
				{
					await (entityEntry.Entity as BaseEntity).OnSaving();
				}
				else if (entityEntry.State == EntityState.Modified) // If you want to update TenantId when Order is modified
				{
					await (entityEntry.Entity as BaseEntity).OnSaving();
				}
			}
			return await base.SaveChangesAsync(cancellationToken);
		}
	}
}
