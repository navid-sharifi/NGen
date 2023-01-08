namespace NGen
{
    public class UserToken : BaseEntity
    {
        public Guid UserId { get; set; }
        public string Token { get; set; }
        public DateTime Date { get; set; }
    }
}
