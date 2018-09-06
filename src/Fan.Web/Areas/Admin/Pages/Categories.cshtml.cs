using Fan.Blog.Models;
using Fan.Blog.Services;
using Fan.Exceptions;
using Fan.Settings;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using System.Threading.Tasks;

namespace Fan.Web.Areas.Admin.Pages
{
    public class CategoriesModel : PageModel
    {
        private readonly IBlogService _blogSvc;
        private readonly ISettingService _settingSvc;
        public CategoriesModel(IBlogService blogService,
            ISettingService settingService)
        {
            _blogSvc = blogService;
            _settingSvc = settingService;
        }

        public string CategoryListJsonStr { get; private set; }
        public int DefaultCategoryId { get; private set; }

        /// <summary>
        /// GET
        /// </summary>
        /// <returns></returns>
        public async Task OnGetAsync()
        {
            var blogSettings = await _settingSvc.GetSettingsAsync<BlogSettings>();
            DefaultCategoryId = blogSettings.DefaultCategoryId;

            var cats = await _blogSvc.GetCategoriesAsync();
            CategoryListJsonStr = JsonConvert.SerializeObject(cats);
        }

        /// <summary>
        /// DELETE a category by id.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task OnDeleteAsync(int id)
        {
            await _blogSvc.DeleteCategoryAsync(id);
        }

        /// <summary>
        /// POST to create a new category.
        /// </summary>
        /// <param name="category"></param>
        /// <returns></returns>
        public async Task<IActionResult> OnPostAsync([FromBody]Category category)
        {
            try
            {
                var cat = await _blogSvc.CreateCategoryAsync(category.Title, category.Description);
                return new JsonResult(cat);
            }
            catch (FanException ex)
            {
                return BadRequest(ex.ValidationFailures);
            }
        }

        /// <summary>
        /// POST to udpate an existing category.
        /// </summary>
        /// <param name="category"></param>
        /// <returns></returns>
        public async Task<IActionResult> OnPostUpdateAsync([FromBody]Category category)
        {
            try
            {
                var cat = await _blogSvc.UpdateCategoryAsync(category);
                return new JsonResult(cat);
            }
            catch (FanException ex)
            {
                return BadRequest(ex.ValidationFailures);
            }
        }

        /// <summary>
        /// POST to set default category.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task OnPostDefaultAsync(int id)
        {
            await _blogSvc.SetDefaultCategoryAsync(id);
        }
    }
}