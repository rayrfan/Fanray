using Fan.Blog.Models.Input;
using Fan.Blog.Services.Interfaces;
using Fan.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Fan.WebApp.Manage.Admin.Compose
{
    public class PageNavModel : Microsoft.AspNetCore.Mvc.RazorPages.PageModel
    {
        private readonly IPageService pageService;

        public PageNavModel(IPageService pageService)
        {
            this.pageService = pageService;
        }

        public string NavJson { get; set; }
        public string PagesJson { get; set; }
        public int PageId { get; set; }
        public string ParentTitle { get; set; }

        public async Task<IActionResult> OnGet(int pageId)
        {
            if (pageId <= 0)
            {
                return Redirect("/admin/pages");
            }

            var page = await pageService.GetAsync(pageId);
            if (!page.IsParent)
            {
                return Redirect("/admin/pages");
            }

            // id
            PageId = pageId;

            // title
            ParentTitle = page.Title;

            // nav
            NavJson = JsonConvert.SerializeObject(page.Nav ?? "");

            var list = new List<string>();
            foreach (var child in page.Children)
            {
                list.Add(child.Title);
            }
            PagesJson = JsonConvert.SerializeObject(list);

            return Page();
        }

        public async Task<IActionResult> OnPostSaveAsync([FromBody]PageNavIM nav)
        {
            try
            {
                await pageService.SaveNavAsync(nav.PageId, nav.NavMd);
                return new EmptyResult();
            }
            catch (FanException ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }

    public class PageNavIM
    {
        public int PageId { get; set; }
        public string NavMd { get; set; }
    }
}