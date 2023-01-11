namespace Domain
{
    public partial class FileManager : BaseEntity
    {
        public string Name { get; set; }
		public long CreateDateTime { get; set; }
		public User Creator { get; set; }
		public Guid CreatorId { get; set; }
		public Folder? Father { get; set; }
		public Guid? FatherId { get; set; }
	}
}
