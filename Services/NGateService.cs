
using Domain;
using Microsoft.AspNetCore.Http;

namespace NSharp
{

    public class NGateService
    {
        public User User = null;
        private bool FindUser = false;

        private Database Database;
        private IHttpContextAccessor HttpContextAccessor;
        public NGateService(Database database, IHttpContextAccessor httpContextAccessor)
        {
            Database = database;
            this.HttpContextAccessor = httpContextAccessor;
        }

        public async Task Login(User user)
        {
            var datatime = DateTimeOffset.Now;
            var token = datatime.ToUnixTimeMilliseconds() + Guid.NewGuid().ToString().Replace("-", "");

            await Database.InsertAsync(
                 new UserToken
                 {
                     Date = datatime.LocalDateTime,
                     UserId = user.Id,
                     Token = token
                 });

            HttpContextAccessor.HttpContext.Response.Headers.Add("NGate", token);
        }

        public async Task<User?> GetUserByToken(string? token)
        {
            if (FindUser)
                return User;

            FindUser = true;

            if (token.None())
                return null;

            var userToken = await Database.FirstOrDefaultAsync<Domain.UserToken>(c => c.Token == token);

            if (userToken is null)
                return null;

            User = await Database.FirstOrDefaultAsync<Domain.User>(c => c.Id == userToken.UserId);

            return User;

        }

    }
}
