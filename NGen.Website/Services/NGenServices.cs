using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;


namespace NGen
{
	public static class NGenServices
	{
		public static IConfiguration Configuration
		{
			get
			{
				if (_configuration is null)
				{
					var builder = new ConfigurationBuilder()
								  .SetBasePath(Directory.GetCurrentDirectory())
								  .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
								  .AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: true);

					_configuration = builder.Build();
				}
				return _configuration;
			}
		}
		private static IConfiguration _configuration;

		public static void AddNGenServices(this IServiceCollection services)
		{
			services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
			services.AddScoped<Database, Database>();

			services.AddDbContext<EfCoreDbContext>(options =>
				options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));

			services.AddScoped(typeof(IDatabase<>), typeof(EFCoreDatabase<>));
			CreateDbIfNotExists(services);
		}

		private static void CreateDbIfNotExists(IServiceCollection services)
		{
			using var serviceProvider = services.BuildServiceProvider();
			using var context = serviceProvider.GetService<EfCoreDbContext>();
			context.Database.EnsureCreated();
		}
	}
}
