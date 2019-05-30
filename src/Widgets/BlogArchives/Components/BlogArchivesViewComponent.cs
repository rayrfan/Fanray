using Fan.Blog.Models;
using Fan.Blog.Services.Interfaces;
using Fan.Widgets;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BlogArchives.Components
{
    /// <summary>
    /// The BlogArchives view component.
    /// </summary>
    public class BlogArchivesViewComponent : ViewComponent
    {
        private readonly IStatsService _statsSvc;
        public BlogArchivesViewComponent(IStatsService statsService)
        {
            _statsSvc = statsService;
        }

        public async Task<IViewComponentResult> InvokeAsync(Widget widget)
        {
            var blogArchivesWidget = (BlogArchivesWidget)widget;
            var years = await _statsSvc.GetArchivesAsync();

            return View("~/Components/BlogArchives.cshtml", 
                new Tuple<Dictionary<int, List<MonthItem>>, BlogArchivesWidget>(years, blogArchivesWidget));
        }
    }
}