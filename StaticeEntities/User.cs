
namespace Domain
{
    public class User : BaseEntity
    {
        public string Name { get; set; }
        public string Password { get; set; }
        public string Roles { get; set; }

    }
}
