using Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NGen;

namespace Website.Controllers
{
    public partial class PublicController : SharedController
    {
        [HttpGet]
        [Route("download/{random}/{id}")]
        public async Task<IActionResult> Index(Guid random, Guid id)
        {
            var file = await Database.Of<FileManagerFile>().FirstOrDefaultAsync(c => c.RandomPath == random && c.Id == id);
            if (file is null)
                return NotFound("not found");

            return File(file.Source, file.Type, file.Name);
        }

        [HttpPost]
        [Route("[action]")]
        public async Task<IActionResult> FileManagerAddFolder(AddPostFileManagerAddNewFolderVM data)
        {
            var alreadyFolderExist = await Database.Of<FileManagerFolder>().Table.AnyAsync(c => c.Name == data.Name.Trim());

            if (alreadyFolderExist)
                return BadRequest("this folder already exist.");

            await Database.Of<FileManagerFolder>().InsertAsync(new FileManagerFolder
            {
                Name = data.Name,
                CreateDateTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                CreatorId = NGate.User.Id,
                FatherId = data.FatherId,
            });

            return Ok();
        }


        [HttpPost]
        [Route("[action]")]
        public async Task<IActionResult> FileManagerAddFile([FromForm] AddPostFileManagerAddFileVM data)
        {
            var file = new FileManagerFile
            {
                FolderId = data.Folder,
                CreatorId = NGate.User.Id,
                Name = data.Name.HasValue() ? data.Name + '.' + data.File.FileName.Split(".").Last() : data.File.FileName,
                Type = data.File.ContentType,
                Source = await data.File.ToArray()
            };
            await Database.InsertAsync(file);

            return Ok();
        }


        [HttpPost]
        [Route("[action]")]
        public async Task<IActionResult> FileManagerGetFilesAndFolders(AddPostFileManagerGetFilesAndFoldersVM? data)
        {
            var filesQuery = Database.Of<FileManagerFile>().Table;
            var foldersQuery = Database.Of<FileManagerFolder>().Table;

            if (data.Find.HasValue())
            {
                filesQuery = filesQuery.Where(c => c.Name.Contains(data.Find));
                foldersQuery = foldersQuery.Where(c => c.Name.Contains(data.Find));
            }
            if (data.FolderId.HasValue())
            {
                filesQuery = filesQuery.Where(c => c.FolderId == data.FolderId);
                foldersQuery = foldersQuery.Where(c => c.FatherId == data.FolderId);
            }

            if (data.Find.None() && !data.FolderId.HasValue())
            {
                filesQuery = filesQuery.Where(c => c.FolderId == null);
                foldersQuery = foldersQuery.Where(c => c.FatherId == null);
            }

            var files = await filesQuery.ToListAsync();
            var folders = await foldersQuery.ToListAsync();

            return Ok(folders.Select(c => new { type = "folder", name = c.Name, id = c.Id, fatherId = c.FatherId?.ToString(), random = Guid.Empty }).Append(files.Select(c => new { type = "file", name = c.Name, id = c.Id, fatherId = "", random = c.RandomPath })));
        }


        [HttpPost]
        [Route("[action]/{id}")]
        public async Task<IActionResult> FileManagerDownload(Guid id)
        {
            var file = await Database.Of<FileManagerFile>().FirstOrDefaultAsync();
            if (file == null)
                return Ok("not found");

            return File(file.Source, file.Type, file.Name);
        }
    }
    public class AddPostFileManagerAddNewFolderVM
    {
        public string Name { get; set; }
        public Guid? FatherId { get; set; }
    }

    public class AddPostFileManagerGetFilesAndFoldersVM
    {
        public string? Find { get; set; }

        public Guid? FolderId { get; set; }
    }
    public class AddPostFileManagerAddFileVM
    {
        public Guid? Folder { get; set; }
        public string? Name { get; set; }
        public IFormFile File { get; set; }
    }
}
