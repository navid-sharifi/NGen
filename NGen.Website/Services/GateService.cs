using Microsoft.AspNetCore.Http;


namespace NGen
{
    public class GateService
    {
        public User User = null;
        private bool FindUser = false;

        private Database Database;
        private IHttpContextAccessor HttpContextAccessor;
        public GateService(Database database, IHttpContextAccessor httpContextAccessor)
        {
            Database = database;
            this.HttpContextAccessor = httpContextAccessor;
        }

        public async Task Login(User user)
        {
            var datatime = DateTimeOffset.Now;
            var token = datatime.ToUnixTimeMilliseconds() + Guid.NewGuid().ToString().Replace("-", "");

            await Database.Of<UserToken>().InsertAsync(
                 new UserToken
                 {
                     Date = datatime.LocalDateTime,
                     UserId = user.Id,
                     Token = token
                 });

            HttpContextAccessor.HttpContext.Response.Headers.Add("NGate", token);
        }
        
        public async Task Login(string userName)
        {
            var user = await GetUserByName(userName);
            if (user is null)
                return;

            var datatime = DateTimeOffset.Now;
            var token = datatime.ToUnixTimeMilliseconds() + Guid.NewGuid().ToString().Replace("-", "");

            await Database.Of<UserToken>().InsertAsync(
                 new UserToken
                 {
                     Date = datatime.LocalDateTime,
                     UserId = user.Id,
                     Token = token
                 });

            HttpContextAccessor.HttpContext.Response.Headers.Add("NGate", token);
        }

        public async Task<bool> CanLogin(User user, string password)
        {
            user = await Database.Of<User>().FirstOrDefaultAsync(c => c.Id == user.Id);
            if (user is null)
                return false;
            if (user.Password != password) return false;
            return true;
        }

        public async Task<bool> CanLogin(string userName, string password)
        {
            var user = await GetUserByName(userName);
            if (user is null)
                return false;
            if (user.Password != password) return false;
            return true;
        }

        public Task<User> GetUserByName(string userName)
        {
            return Database.Of<User>().FirstOrDefaultAsync(c => c.Name == userName);
        }

        public async Task<User?> GetUserByToken(string? token)
        {
            if (FindUser)
                return User;

            FindUser = true;

            if (token.None())
                return null;
            var userToken = await Database.Of<UserToken>().FirstOrDefaultAsync(c => c.Token == token);
            if (userToken is null)
                return null;
            
            User = await Database.Of<User>().FirstOrDefaultAsync(c => c.Id == userToken.UserId);
            return User;

        }
    }
}
