using System.Collections.Generic;
using System.Threading.Tasks;

namespace Fan.Widgets
{
    public interface IWidgetService
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="widgetArea"></param>
        /// <returns></returns>
        Task RegisterAreaAsync(WidgetArea widgetArea);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="areaId"></param>
        /// <returns></returns>
        Task<WidgetAreaViewModel> GetAreaAsync(string areaId);
        /// <summary>
        /// Returns the current theme's widget areas.
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<WidgetAreaViewModel>> GetCurrentThemeAreasAsync();
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<WidgetInfo>> GetInstalledWidgetsInfoAsync();
        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<Widget> GetWidgetAsync(int id);
        /// <summary>
        /// Adds a widget instance to widget area.
        /// </summary>
        /// <param name="widgetType"></param>
        /// <param name="areaId"></param>
        /// <returns></returns>
        Task<Widget> AddWidgetAsync(string widgetType, string areaId, int index); // int
        /// <summary>
        /// Updates a widget instance.
        /// </summary>
        /// <param name="widget"></param>
        /// <returns></returns>
        Task UpdateWidgetAsync(Widget widget);
        /// <summary>
        /// Removes a widget instance from a widget area.
        /// </summary>
        /// <param name="widgetId"></param>
        /// <param name="areaId"></param>
        /// <returns></returns>
        Task RemoveWidgetAsync(int widgetId, string areaId);
    }
}
