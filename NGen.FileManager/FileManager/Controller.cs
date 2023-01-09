using Microsoft.AspNetCore.Mvc;
using NGen;

namespace Website.Controllers
{
    [ApiController]
    public partial class PublicController : SharedController
    {
        [HttpGet]
        [Route("download/{random}/{id}")]
        public async Task<IActionResult> Index(Guid random, Guid id)
        {
            var file = await Database.Of<NGen.File>().FirstOrDefaultAsync(c => c.RandomPath == random && c.Id == id);
            if (file is null)
                return NotFound("not found");

            return File(file.Source, file.Type, file.Name);
        }
    }
}
