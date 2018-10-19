using Fan.Blog.Models;
using Fan.Blog.Tags;
using Fan.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using System.Threading.Tasks;

namespace Fan.Web.Pages.Admin
{
    public class TagsModel : PageModel
    {
        private readonly ITagService _tagSvc;
        public TagsModel(ITagService tagService)
        {
            _tagSvc = tagService;
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
            var tags = await _tagSvc.GetTagsAsync();
            TagListJsonStr = JsonConvert.SerializeObject(tags);
        }

        /// <summary>
        /// DELETE a tag by id.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task OnDeleteAsync(int id)
        {
            await _tagSvc.DeleteTagAsync(id);
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
                var tagNew = await _tagSvc.CreateTagAsync(new Tag { Title = tag.Title, Description = tag.Description });
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
                var cat = await _tagSvc.UpdateTagAsync(tag);
                return new JsonResult(cat);
            }
            catch (FanException ex)
            {
                return BadRequest(ex.ValidationFailures);
            }
        }
    }
}