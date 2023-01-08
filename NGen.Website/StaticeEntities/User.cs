namespace NGen
{
    public partial class User : BaseEntity
    {
        public string Name { get; set; }
        public string Password { get; set; }
        public string Roles { get; set; }
		public string Family { get; set; }
		public string Description { get; set; }
	}
}
