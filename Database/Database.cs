using Domain;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using RepoDb;
using System.Linq.Expressions;



namespace NSharp
{
    public class Database
    {
        private string ConnectionString;

        public Database(IConfiguration configuration)
        {
            ConnectionString = configuration["ConnectionString"];
        }

        public async Task<IEnumerable<T>> GetList<T>() where T : class
        {
            using (var connection = new SqlConnection(ConnectionString))
            {
                return await connection.QueryAllAsync<T>();
            }
        }

        public async Task<Guid> InsertAsync<T>(T Entity) where T : BaseEntity
        {
            using (var connection = new SqlConnection(ConnectionString))
            {
                Entity.Id =  Guid.NewGuid();
                Guid id = Guid.Empty;
                Guid.TryParse((await connection.InsertAsync(Entity)).ToString(), out id);
                return id;
            }
        }

        public async Task<Guid> UpdateAsync<T>(T Entity) where T : BaseEntity
        {
            using (var connection = new SqlConnection(ConnectionString))
            {
                Guid id = Guid.Empty;
                Guid.TryParse((await connection.UpdateAsync(Entity , c=>c.Id == Entity.Id)).ToString(), out id);
                return id;
            }
        }

        public async Task<IEnumerable<T>> QueryAsync<T>(Expression<Func<T, bool>> where) where T : BaseEntity
        {
            using (var connection = new SqlConnection(ConnectionString))
            {
                var rows = await connection.QueryAsync<T>(where);
                return rows;
            }
        }

        public async Task<IEnumerable<T>> QueryAsync<T,M>(M Model) where T : BaseEntity
        {
            using (var connection = new SqlConnection(ConnectionString))
            {
                var rows = await connection.QueryAsync<T>(Model);
                return rows;
            }
        }

        public async Task<IEnumerable<T>> QueryByViewModelAsync<T,U>(U ViewModel) where T : BaseEntity where U : class
        {
            using (var connection = new SqlConnection(ConnectionString))
            {

                var coulmn = new List<QueryField>();

                foreach (var item in ViewModel.GetType().GetProperties())
                {
                    var name = item.Name;
                    var value = item.GetValue(ViewModel);
                    coulmn.Add(new QueryField(field: new Field(name), value));
                }
                var rows = await connection.QueryAsync<T>(coulmn);
                return rows;
            }
        }

        public async Task<T?> FirstOrDefaultAsync<T>(Expression<Func<T, bool>> where) where T : BaseEntity
        {
            using (var connection = new SqlConnection(ConnectionString))
            {
                var rows = await connection.QueryAsync<T>(where);
                return rows.FirstOrDefault();
            }
        }

      

    }
}
