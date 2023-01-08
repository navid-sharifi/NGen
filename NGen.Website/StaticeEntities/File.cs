using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace NGen
{
    public partial class File : BaseEntity
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
		public Folder? Folder { get; set; }
		public Guid? FolderId { get; set; }
        public Guid RandomPath { get; set; }
    }
}
