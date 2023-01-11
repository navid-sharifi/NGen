namespace Domain
{
    using NGen;

    public partial class FileManagerFolder : BaseEntity
    {
        public string Name { get; set; }
		public long CreateDateTime { get; set; }
		public User Creator { get; set; }
		public Guid CreatorId { get; set; }
		public FileManagerFolder? Father { get; set; }
		public Guid? FatherId { get; set; }
	}

    public partial class FileManagerFile : BaseEntity
    {
        public override Task OnSaving()
        {
            RandomPath = Guid.NewGuid();
            return base.OnSaving();
        }

        [BindProperty]
        public byte[] Source { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public long CreateDateTime { get; set; }
        public User Creator { get; set; }
        public Guid CreatorId { get; set; }
        public FileManagerFolder? Folder { get; set; }
        public Guid? FolderId { get; set; }
        public Guid RandomPath { get; set; }
    }
}
