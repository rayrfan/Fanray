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
            var tags = await _tagSvc.GetAllAsync();
            TagListJsonStr = JsonConvert.SerializeObject(tags);
        }

        /// <summary>
        /// DELETE a tag by id.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task OnDeleteAsync(int id)
        {
            await _tagSvc.DeleteAsync(id);
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
                var tagNew = await _tagSvc.CreateAsync(new Tag { Title = tag.Title, Description = tag.Description });
                return new JsonResult(tagNew);
            }
            catch (FanException ex)
            {
                return BadRequest(ex.Message);
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
                var tagUpdated = await _tagSvc.UpdateAsync(tag);
                return new JsonResult(tagUpdated);
            }
            catch (FanException ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}