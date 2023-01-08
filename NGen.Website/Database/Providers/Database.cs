//using Microsoft.Data.SqlClient;
//using Microsoft.Extensions.Configuration;
//using RepoDb;
//using System.Linq.Expressions;

//namespace NGen
//{
//	public class Database
//	{
//		private string ConnectionString;
//		IConfiguration Configuration;

//		public Database(IConfiguration configuration)
//		{
//			Configuration = configuration;
//			ConnectionString = Configuration.GetConnectionString("DefaultConnection");
//		}

//		public Database<T> Of<T>() where T : BaseEntity
//		{
//			return new Database<T>(Configuration);

//		}

//		public async Task<IEnumerable<T>> GetList<T>() where T : class
//		{
//			using (var connection = new SqlConnection(ConnectionString))
//			{
//				return await connection.QueryAllAsync<T>();
//			}
//		}

//		public async Task<Guid> InsertAsync<T>(T Entity) where T : BaseEntity
//		{
//			using (var connection = new SqlConnection(ConnectionString))
//			{
//				Entity.Id = Guid.NewGuid();
//				Guid id = Guid.Empty;
//				Guid.TryParse((await connection.InsertAsync(Entity)).ToString(), out id);
//				return id;
//			}
//		}

//		public async Task<Guid> UpdateAsync<T>(T Entity) where T : BaseEntity
//		{
//			using (var connection = new SqlConnection(ConnectionString))
//			{
//				Guid id = Guid.Empty;
//				Guid.TryParse((await connection.UpdateAsync(Entity, c => c.Id == Entity.Id)).ToString(), out id);
//				return id;
//			}
//		}

//		public async Task<IEnumerable<T>> QueryAsync<T>(Expression<Func<T, bool>> where) where T : BaseEntity
//		{
//			using (var connection = new SqlConnection(ConnectionString))
//			{
//				var rows = await connection.QueryAsync<T>(where);
//				return rows;
//			}
//		}

//		public async Task<IEnumerable<T>> QueryAsync<T, M>(M Model) where T : BaseEntity
//		{
//			using (var connection = new SqlConnection(ConnectionString))
//			{
//				var rows = await connection.QueryAsync<T>(Model);
//				return rows;
//			}
//		}

//		public async Task<IEnumerable<T>> QueryByViewModelAsync<T, U>(U ViewModel) where T : BaseEntity where U : class
//		{
//			using (var connection = new SqlConnection(ConnectionString))
//			{

//				var coulmn = new List<QueryField>();

//				foreach (var item in ViewModel.GetType().GetProperties())
//				{
//					var name = item.Name;
//					var value = item.GetValue(ViewModel);
//					coulmn.Add(new QueryField(field: new Field(name), value));
//				}
//				var rows = await connection.QueryAsync<T>(coulmn);
//				return rows;
//			}
//		}

//		public async Task<T> FirstOrDefaultAsync<T>(Expression<Func<T, bool>> where) where T : BaseEntity
//		{
//			using (var connection = new SqlConnection(ConnectionString))
//			{
//				var rows = await connection.QueryAsync<T>(where);
//				return rows.FirstOrDefault();
//			}
//		}

//		public async Task DeleteAsync<T>(Expression<Func<T, bool>> where) where T : BaseEntity
//		{
//			using (var connection = new SqlConnection(ConnectionString))
//			{
//				await connection.DeleteAsync<T>(where);
//			}
//		}

//	}

//	public class Database<T> : Database where T : BaseEntity
//	{
//		private string ConnectionString;

//		public Database(IConfiguration configuration) : base(configuration)
//		{
//			ConnectionString = configuration["ConnectionString"];
//		}


//		public async Task<IEnumerable<T>> GetList<U>(Expression<Func<T, U>> incloud) where U : BaseEntity
//		{
//			var rows = await GetList<T>();

//			if (rows.Any())
//			{
//				var rowIds = rows.Select<Guid, T>(typeof(U).Name + "Id");
//				var incloudRows = await QueryAsync<U>(c => rowIds.Any(x => x == c.Id));

//				if (incloudRows.Any())
//				{
//					foreach (var row in rows)
//					{
//						var value = Guid.Parse(typeof(T).GetProperty(typeof(U).Name + "Id").GetValue(row).ToString());

//						var find = incloudRows.FirstOrDefault(c => c.Id == value);

//						if (find != null)
//							typeof(T).GetProperty(typeof(U).Name).SetValue(row, find);

//					}

//				}

//			}

//			return rows;



//		}

//	}


//	[AttributeUsage(AttributeTargets.Property, Inherited = false)]
//	public class NInclude : Attribute
//	{
//		public NInclude()
//		{
//			//Text = text;
//		}
//		//public string Text { get; set; }

//	}
//}