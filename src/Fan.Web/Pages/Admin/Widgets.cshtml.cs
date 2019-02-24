using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fan.Widgets;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;

namespace Fan.Web.Pages.Admin
{
    public class WidgetsModel : PageModel
    {
        private readonly IWidgetService _widgetService;

        public WidgetsModel(IWidgetService widgetService)
        {
            _widgetService = widgetService;
        }

        // -------------------------------------------------------------------- properties

        public string WidgetInfosJson { get; private set; }
        public string WidgetAreasJson { get; private set; }

        public async Task OnGet()
        {
            // widget infos
            var widgetInfos = await _widgetService.GetInstalledWidgetsInfoAsync();
            WidgetInfosJson = JsonConvert.SerializeObject(widgetInfos);

            // areas
            var widgetAreas = await _widgetService.GetCurrentThemeAreasAsync();
            WidgetAreasJson = JsonConvert.SerializeObject(widgetAreas);
        }

        /// <summary>
        /// string widgetType, int index, string areaId
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> OnPostAddAsync([FromBody]AddWidgetDto dto)
        {
            // insert to db a new widget instance based on type
            var widgetVm = await _widgetService.AddWidgetAsync(dto.WidgetType, dto.AreaId, dto.Index);
            return new JsonResult(widgetVm);
        }

        /// <summary>
        /// Returns the widget edit page url.
        /// </summary>
        /// <param name="widgetId"></param>
        /// <returns></returns>
        public async Task<JsonResult> OnGetEditAsync(int widgetId)
        {
            var widget = await _widgetService.GetWidgetAsync(widgetId);
            var widgetInfo = await _widgetService.GetWidgetInfoAsync(widget.Type);
            return new JsonResult($"{Request.Scheme}://{Request.Host}/widgets/{widgetInfo.Folder}/edit");
        }

        /// <summary>
        /// DELETE a widget instance by id.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task OnDeleteAsync(int widgetId, string areaId)
        {
            //await _widgetService.RemoveWidgetAsync(widgetId, areaId);
        }
    }

    public class AddWidgetDto
    {
        public int Index { get; set; }
        public string WidgetType { get; set; }
        public string AreaId { get; set; }
    }
}