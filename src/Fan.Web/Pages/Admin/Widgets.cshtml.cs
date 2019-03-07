using Fan.Widgets;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

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
        /// When user drags a widget from info to an area or from an area to anther area.
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> OnPostAddAsync([FromBody]AddWidgetDto dto)
        {
            WidgetInstance widgetInst = null;

            // user drags a widget from infos to an area
            if (dto.AreaFromId.IsNullOrEmpty())
            {
                widgetInst = await _widgetService.CreateWidgetAsync(dto.WidgetType);
                await _widgetService.AddWidgetToAreaAsync(widgetInst.Id, dto.AreaToId, dto.Index);
            }
            else // user drags a widget from area to another
            {
                await _widgetService.RemoveWidgetFromAreaAsync(dto.WidgetId, dto.AreaFromId);
                await _widgetService.AddWidgetToAreaAsync(dto.WidgetId, dto.AreaToId, dto.Index);
            }

            return new JsonResult(widgetInst);
        }

        public async Task OnPostReorderAsync([FromBody]OrderWidgetDto dto)
        {
            await _widgetService.OrderWidgetInAreaAsync(dto.WidgetId, dto.AreaId, dto.Index);
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
        /// DELETE a widget instance from an area.
        /// </summary>
        /// <param name="widgetId"></param>
        /// <param name="areaId"></param>
        /// <returns></returns>
        public async Task OnDeleteAsync(int widgetId, string areaId)
        {
            await _widgetService.RemoveWidgetFromAreaAsync(widgetId, areaId);
            await _widgetService.DeleteWidgetAsync(widgetId);
        }
    }

    public class AddWidgetDto
    {
        /// <summary>
        /// The new index in the area to insert a widget.
        /// </summary>
        public int Index { get; set; }
        /// <summary>
        /// The type of the widget user drags.
        /// </summary>
        public string WidgetType { get; set; }
        /// <summary>
        /// Id of the area user drags widget to.
        /// </summary>
        public string AreaToId { get; set; }
        /// <summary>
        /// Id of the widget user drags from an area, 0 if user drags widget from infos.
        /// </summary>
        public int WidgetId { get; set; }
        /// <summary>
        /// Id of the area user drags widget from, null if user drags widget from infos. 
        /// </summary>
        public string AreaFromId { get; set; }

        public string Name { get; set; }
        public string Title { get; set; }
    }

    public class OrderWidgetDto
    {
        /// <summary>
        /// The new index to insert the widget.
        /// </summary>
        public int Index { get; set; }
        public int WidgetId { get; set; }
        public string AreaId { get; set; }
    }
}