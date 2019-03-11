using System.Collections.Generic;
using System.Threading.Tasks;

namespace Fan.Widgets
{
    public interface IWidgetService
    {
        /// <summary>
        /// Register a pre-defined area into the system.
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
        /// Returns all the installed widget infos.
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
        /// Returns a <see cref="Widget"/> by id for update.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<Widget> GetWidgetAsync(int id);
        /// <summary>
        /// Updates a widget instance.
        /// </summary>
        /// <param name="id">The id of the widget instance.</param>
        /// <param name="widget"></param>
        /// <returns></returns>
        Task UpdateWidgetAsync(int id, Widget widget);
        /// <summary>
        /// Adds a widget instance to a widget area.
        /// </summary>
        /// <param name="widgetId"></param>
        /// <param name="areaId">The id of the area the widget is added to.</param>
        /// <param name="index">The index of the added widget in the id array.</param>
        /// <returns>A <see cref="WidgetInstance"/>.</returns>
        /// <remarks>
        /// This is used when user drops a widget in a widget area, an instance of the widget 
        /// will be created then the area is updated with the new widget instance's id added 
        /// to its id list.
        /// </remarks>
        Task<WidgetInstance> AddWidgetToAreaAsync(int widgetId, string areaId, int index);
        /// <summary>
        /// Removes a widget instance from a widget area. The widget instance is not deleted.
        /// This is used when user moves a widget instance from one area to another.
        /// </summary>
        /// <param name="widgetId"></param>
        /// <param name="areaId"></param>
        /// <returns></returns>
        Task RemoveWidgetFromAreaAsync(int widgetId, string areaId);
        /// <summary>
        /// Moves a widget in an area to a new position.
        /// </summary>
        /// <param name="widgetId"></param>
        /// <param name="areaId"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        Task OrderWidgetInAreaAsync(int widgetId, string areaId, int index);
        /// <summary>
        /// Creates a widget instance by type. 
        /// </summary>
        /// <param name="widgetType"></param>
        /// <returns></returns>
        /// <remarks>
        /// The newly created instance has the widget's default values.
        /// </remarks>
        Task<int> CreateWidgetAsync(string widgetType);
        /// <summary>
        /// Creates a widget instance by a given widget object.
        /// </summary>
        /// <param name="widget"></param>
        /// <param name="widgetType"></param>
        /// <remarks>
        /// The widget object has the widget's seed values.
        /// </remarks>
        Task<int> CreateWidgetAsync(Widget widget, string widgetType);
        /// <summary>
        /// Deletes a widget instance.
        /// </summary>
        /// <param name="widgetId"></param>
        /// <returns></returns>
        Task DeleteWidgetAsync(int widgetId);        
    }
}
