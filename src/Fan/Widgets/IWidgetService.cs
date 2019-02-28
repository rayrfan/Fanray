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
        /// Returns an <see cref="WidgetAreaInstance"/> by id.
        /// </summary>
        /// <param name="areaId"></param>
        /// <returns></returns>
        Task<WidgetAreaInstance> GetAreaAsync(string areaId);
        /// <summary>
        /// Returns the current theme's widget areas.
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<WidgetAreaInstance>> GetCurrentThemeAreasAsync();
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<WidgetInfo>> GetInstalledWidgetsInfoAsync();
        /// <summary>
        /// Returns a <see cref="WidgetInfo"/> based on a given widget type.
        /// </summary>
        /// <param name="widgetType"></param>
        /// <returns></returns>
        Task<WidgetInfo> GetWidgetInfoAsync(string widgetType);
        /// <summary>
        /// Returns a <see cref="Widget"/> by id.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<Widget> GetWidgetAsync(int id);
        /// <summary>
        /// Creates a widget instance.
        /// </summary>
        /// <param name="widgetType">The .NET type of the widget to add.</param>
        /// <param name="areaId">The id of the area the widget is added to.</param>
        /// <param name="index">The index of the added widget in the id array.</param>
        /// <returns>A <see cref="WidgetInstance"/>.</returns>
        /// <remarks>
        /// This is used when user drops a widget in a widget area, an instance of the widget 
        /// will be created then the area is updated with the new widget instance's id added 
        /// to its id list.
        /// </remarks>
        Task<WidgetInstance> AddWidgetAsync(string widgetType, string areaId, int index);
        /// <summary>
        /// Creates a widget instance.
        /// </summary>
        /// <param name="widget">Widget object to be added.</param>
        /// <param name="widgetType">The .NET type of the widget to add.</param>
        /// <param name="areaId">The id of the area the widget is added to.</param>
        /// <param name="index">The index of the added widget in the id array.</param>
        /// <returns>A <see cref="WidgetInstance"/>.</returns>
        /// <remarks>
        /// This is used when initializing widget areas with widget instances.
        /// </remarks>
        Task<WidgetInstance> AddWidgetAsync(Widget widget, string widgetType, string areaId, int index);
        /// <summary>
        /// Updates a widget instance.
        /// </summary>
        /// <param name="id">The id of the widget instance.</param>
        /// <param name="widget"></param>
        /// <returns></returns>
        Task UpdateWidgetAsync(int id, Widget widget);
        /// <summary>
        /// Removes a widget instance from a widget area.
        /// </summary>
        /// <param name="widgetId"></param>
        /// <param name="areaId"></param>
        /// <returns></returns>
        Task RemoveWidgetAsync(int widgetId, string areaId);
    }
}
