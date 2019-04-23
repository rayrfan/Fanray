using Fan.Blog.Models;
using Fan.Blog.Services.Interfaces;
using Fan.Exceptions;
using Fan.Settings;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using System.Threading.Tasks;

namespace Fan.WebApp.Manage.Admin
{
    public class CategoriesModel : PageModel
    {
        private readonly ICategoryService _catSvc;
        private readonly ISettingService _settingSvc;

        public CategoriesModel(ICategoryService catService,
            ISettingService settingService)
        {
            _catSvc = catService;
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

            var cats = await _catSvc.GetAllAsync();
            CategoryListJsonStr = JsonConvert.SerializeObject(cats);
        }

        /// <summary>
        /// DELETE a category by id.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task OnDeleteAsync(int id)
        {
            await _catSvc.DeleteAsync(id);
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
                var cat = await _catSvc.CreateAsync(category.Title, category.Description);
                return new JsonResult(cat);
            }
            catch (FanException ex)
            {
                return BadRequest(ex.Message);
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
                var cat = await _catSvc.UpdateAsync(category);
                return new JsonResult(cat);
            }
            catch (FanException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// POST to set default category.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task OnPostDefaultAsync(int id)
        {
            await _catSvc.SetDefaultAsync(id);
        }
    }
}