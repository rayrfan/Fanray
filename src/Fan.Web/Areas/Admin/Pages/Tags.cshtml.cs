using Fan.Blog.Models;
using Fan.Blog.Services;
using Fan.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using System.Threading.Tasks;

namespace Fan.Web.Areas.Admin.Pages
{
    public class TagsModel : PageModel
    {
        private readonly IBlogService _blogSvc;
        public TagsModel(IBlogService blogService)
        {
            _blogSvc = blogService;
        }

        // -------------------------------------------------------------------- consts & properties

        public string TagListJsonStr { get; private set; }

        // -------------------------------------------------------------------- public methods

        /// <summary>
        /// GE Tbootstrap page with json data.
        /// </summary>
        /// <returns></returns>
        public async Task OnGetAsync()
        {
            var tags = await _blogSvc.GetTagsAsync();
            TagListJsonStr = JsonConvert.SerializeObject(tags);
        }

        /// <summary>
        /// DELETE a tag by id.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task OnDeleteAsync(int id)
        {
            await _blogSvc.DeleteTagAsync(id);
        }

        /// <summary>
        /// POST to create a new tag.
        /// </summary>
        /// <param name="tag"></param>
        /// <returns></returns>
        public async Task<IActionResult> OnPostAsync([FromBody]Tag tag)
        {
            try
            {
                var tagNew = await _blogSvc.CreateTagAsync(new Tag { Title = tag.Title, Description = tag.Description });
                return new JsonResult(tagNew);
            }
            catch (FanException ex)
            {
                return BadRequest(ex.ValidationFailures);
            }
        }

        /// <summary>
        /// POST to udpate an existing tag.
        /// </summary>
        /// <param name="tag"></param>
        /// <returns></returns>
        public async Task<IActionResult> OnPostUpdateAsync([FromBody]Tag tag)
        {
            try
            {
                var cat = await _blogSvc.UpdateTagAsync(tag);
                return new JsonResult(cat);
            }
            catch (FanException ex)
            {
                return BadRequest(ex.ValidationFailures);
            }
        }
    }
}