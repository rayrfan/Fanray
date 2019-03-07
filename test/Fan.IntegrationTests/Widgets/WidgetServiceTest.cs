using Fan.Data;
using Fan.IntegrationTests.Base;
using Fan.Settings;
using Fan.Themes;
using Fan.Widgets;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Fan.IntegrationTests.Widgets
{
    public class WidgetServiceTest : IntegrationTestBase
    {
        private const string MY_WIDGET_TYPE = "Fan.IntegrationTests.Widgets.MyWidget, Fan.IntegrationTests";
        private WidgetService _svc;
        private SqlMetaRepository _metaRepo;

        public WidgetServiceTest()
        {
            // meta repo
            _metaRepo = new SqlMetaRepository(_db);

            // setup CoreSettings
            var settingSvcMock = new Mock<ISettingService>();
            settingSvcMock.Setup(svc => svc.GetSettingsAsync<CoreSettings>()).Returns(Task.FromResult(new CoreSettings()));

            // set ContentRootPath to "Fan.IntegrationTests"
            var workingDirectory = Environment.CurrentDirectory;
            var projectDirectory = Directory.GetParent(workingDirectory).Parent.Parent.FullName;
            var env = new Mock<IHostingEnvironment>();
            env.Setup(m => m.ContentRootPath).Returns(projectDirectory);

            // logger
            var loggerWidgetSvc = _loggerFactory.CreateLogger<WidgetService>();
            var loggerThemeSvc = _loggerFactory.CreateLogger<ThemeService>();

            // theme service
            var themeSvc = new ThemeService(settingSvcMock.Object, env.Object, _cache, loggerThemeSvc);

            _svc = new WidgetService(_metaRepo, themeSvc, _cache, settingSvcMock.Object, env.Object, loggerWidgetSvc)
            {
                // set widget dir
                WidgetDirectoryName = "Widgets"
            };
        }

        /// <summary>
        /// During site setup pre-defined areas will be registered.
        /// </summary>
        [Fact]
        public async void Widget_areas_are_predefined_and_registered_during_setup()
        {
            // Given some pre-defined widget areas
            var blogSidebar1 = WidgetService.BlogSidebar1;
            var blogSidebar2 = WidgetService.BlogSidebar2;

            // When the system sets up, it would register the widget areas
            await _svc.RegisterAreaAsync(blogSidebar1);
            await _svc.RegisterAreaAsync(blogSidebar2);

            // Then the system would have widget areas avaiable for retrival
            var area = await _svc.GetAreaAsync(WidgetService.BlogSidebar1.Id);
            Assert.NotNull(area);
        }
       
        /// <summary>
        /// On the admin widgets page, you will see a list of all areas available in the current
        /// theme displayed on the right hand side.
        /// </summary>
        [Fact]
        public async void Admin_panel_widgets_page_displays_all_areas_in_the_current_theme()
        {
            // Given my current theme uses 2 widget areas
            await _svc.RegisterAreaAsync(WidgetService.BlogSidebar1);
            await _svc.RegisterAreaAsync(WidgetService.BlogSidebar2);

            // When the Admin Panel Widgets page is requested
            var areas = await _svc.GetCurrentThemeAreasAsync();

            // Then it will display all areas in the current theme
            Assert.Equal(2, areas.ToList().Count());
        }

        /// <summary>
        /// On the admin widgets page, you will see a list of all installed widgets information
        /// displayed on the left hand side.
        /// </summary>
        [Fact]
        public async void Admin_panel_widgets_page_displays_all_installed_widgets_information()
        {
            // Given I have MyWidget installed in this test project
            // When Admin Panel Widgets page is requested
            var widgetInfos = await _svc.GetInstalledWidgetsInfoAsync();

            // Then widget's info will displayed
            Assert.Single(widgetInfos);
            Assert.Equal("My Widget", widgetInfos.ToList()[0].Name);
            Assert.Equal("My testing widget.", widgetInfos.ToList()[0].Description);
        }

        /// <summary>
        /// When user drags a widget from the widget infos section on the left side to any of the
        /// widget areas on the right: 1) a widget instance is created; 2) then the widget instance 
        /// id is added to the area.
        /// </summary>
        [Fact]
        public async void User_can_drag_a_widget_from_widget_infos_section_to_an_area()
        {
            // Given a theme with 2 widget areas
            await _svc.RegisterAreaAsync(WidgetService.BlogSidebar1);
            await _svc.RegisterAreaAsync(WidgetService.BlogSidebar2);

            // When user drags a widget from the widget infos section to an area
            var widgetInst = await _svc.CreateWidgetAsync(MY_WIDGET_TYPE);
            await _svc.AddWidgetToAreaAsync(widgetInst.Id, WidgetService.BlogSidebar1.Id, 0);

            // Then the area would contain the widget
            var area = await _svc.GetAreaAsync(WidgetService.BlogSidebar1.Id);
            Assert.Contains(widgetInst.Id, area.WidgetIds);  
        }

        /// <summary>
        /// When user drags a widget from an area to another area: 1) the exist widget would be 
        /// removed from the current area; 2) the widget would be added to the new area.
        /// </summary>
        [Fact]
        public async void User_can_drag_a_widget_from_an_area_to_another_area()
        {
            // Given a theme with two widget areas
            await _svc.RegisterAreaAsync(WidgetService.BlogSidebar1);
            await _svc.RegisterAreaAsync(WidgetService.BlogSidebar2);
            // and a widget in area 1
            var widgetInst = await _svc.CreateWidgetAsync(MY_WIDGET_TYPE);
            await _svc.AddWidgetToAreaAsync(widgetInst.Id, WidgetService.BlogSidebar1.Id, 0);

            // When user drags the widget from area sidebar1 to area sidebar2
            await _svc.RemoveWidgetFromAreaAsync(widgetInst.Id, WidgetService.BlogSidebar1.Id);
            await _svc.AddWidgetToAreaAsync(widgetInst.Id, WidgetService.BlogSidebar2.Id, 0);

            // Then area sidebar1 would not have the widget anymore
            var area1 = await _svc.GetAreaAsync(WidgetService.BlogSidebar1.Id);
            Assert.DoesNotContain(widgetInst.Id, area1.WidgetIds);

            // and area sidebar2 would have the widget
            var area2 = await _svc.GetAreaAsync(WidgetService.BlogSidebar2.Id);
            Assert.Contains(widgetInst.Id, area2.WidgetIds);
        }

        /// <summary>
        /// When user drags and drops a widget from infos section to an area, a widget instance 
        /// is created with properties of default values.  The user can then click on edit to 
        /// change the props values.
        /// </summary>
        [Fact]
        public async void When_user_drops_a_widget_from_info_section_to_area_widget_has_initial_default_values()
        {
            // Given widget area "blog-sidebar1"
            await _svc.RegisterAreaAsync(WidgetService.BlogSidebar1);

            // When a widget is dropped to area from infos
            var widget = await _svc.CreateWidgetAsync(MY_WIDGET_TYPE);
            await _svc.AddWidgetToAreaAsync(widget.Id, WidgetService.BlogSidebar1.Id, 0);

            // Then widget instance has the default val
            Assert.Equal("My Widget", widget.Title);
        }

        /// <summary>
        /// User can drag and drop the same widget from infos section multiple time to an area.
        /// </summary>
        [Fact]
        public async void User_can_drop_same_widget_multiple_times_to_an_area()
        {
            // Given a theme with 2 widget areas
            await _svc.RegisterAreaAsync(WidgetService.BlogSidebar1);
            await _svc.RegisterAreaAsync(WidgetService.BlogSidebar2);
            // and two widget instances
            var w1 = await _svc.CreateWidgetAsync(MY_WIDGET_TYPE);
            await _svc.AddWidgetToAreaAsync(w1.Id, WidgetService.BlogSidebar1.Id, 0);
            var w2 = await _svc.CreateWidgetAsync(MY_WIDGET_TYPE);
            await _svc.AddWidgetToAreaAsync(w2.Id, WidgetService.BlogSidebar1.Id, 1);

            // When we retrieve a widget area
            var area = await _svc.GetAreaAsync(WidgetService.BlogSidebar1.Id);

            // Then area contains both instances
            Assert.Equal(2, area.WidgetInstances.Count);
            Assert.Equal(w1.Id, area.WidgetIds[0]);
            Assert.Equal(w2.Id, area.WidgetIds[1]);
        }

        /// <summary>
        /// A widget instance is stored as json and later it's instantiated back to object from json.
        /// </summary>
        [Fact]
        public async void A_widget_is_instantiated_from_json_and_type_info_strings()
        {
            // Given widget area and a widget in the area
            await _svc.RegisterAreaAsync(WidgetService.BlogSidebar1);
            var widgetVm = await _svc.CreateWidgetAsync(MY_WIDGET_TYPE);
            await _svc.AddWidgetToAreaAsync(widgetVm.Id, WidgetService.BlogSidebar1.Id, 0);

            // When the meta record is retrieved
            var widgetMeta = await _metaRepo.GetAsync(widgetVm.Id);

            // I'm able to get the widget type
            var widget = (Widget)JsonConvert.DeserializeObject(widgetMeta.Value, typeof(Widget));
            Assert.Equal(MY_WIDGET_TYPE, widget.Type);

            // Given a json string that represent an instance of MyWidget
            string json = @"{""age"":10,""title"":""Tags"",""id"":0, ""type"":""Fan.IntegrationTests.Widgets.MyWidget, Fan.IntegrationTests""}";
            // And the widget type I got from above
            var type = Type.GetType(widget.Type);

            // When I deserialize it
            var myWidget = (MyWidget) JsonConvert.DeserializeObject(json, type);

            // Then we get the actual instance
            Assert.Equal(10, myWidget.Age);
        }

        /// <summary>
        /// User can click on delete of a widget in an area to permanently delete the widget instance.
        /// The system would first remove the widget from the area, then delete the widget instance.
        /// </summary>
        [Fact]
        public async void User_can_delete_a_widget_from_an_area()
        {
            // Given a theme with two areas and a widget instance
            await _svc.RegisterAreaAsync(WidgetService.BlogSidebar1);
            await _svc.RegisterAreaAsync(WidgetService.BlogSidebar2);
            var widgetInst = await _svc.CreateWidgetAsync(MY_WIDGET_TYPE);
            await _svc.AddWidgetToAreaAsync(widgetInst.Id, WidgetService.BlogSidebar1.Id, 0);

            // When user deletes the widget
            await _svc.RemoveWidgetFromAreaAsync(widgetInst.Id, WidgetService.BlogSidebar1.Id);
            await _svc.DeleteWidgetAsync(widgetInst.Id);

            // Then the area does not have the widget anymore
            var area = await _svc.GetAreaAsync(WidgetService.BlogSidebar1.Id);
            Assert.DoesNotContain(widgetInst, area.WidgetInstances);
        }

        /// <summary>
        /// User can move a widget inside an area to a new index in the same area.
        /// </summary>
        [Fact]
        public async void User_can_order_widgets_in_an_area()
        {
            // Given two widgets w1 and w2 in blog-sidebar1 area
            await _svc.RegisterAreaAsync(WidgetService.BlogSidebar1);
            await _svc.RegisterAreaAsync(WidgetService.BlogSidebar2);
            var w1 = await _svc.CreateWidgetAsync(MY_WIDGET_TYPE);
            await _svc.AddWidgetToAreaAsync(w1.Id, WidgetService.BlogSidebar1.Id, 0);
            var w2 = await _svc.CreateWidgetAsync(MY_WIDGET_TYPE);
            await _svc.AddWidgetToAreaAsync(w2.Id, WidgetService.BlogSidebar1.Id, 1);

            // When user moves w1 below w2
            await _svc.OrderWidgetInAreaAsync(w1.Id, WidgetService.BlogSidebar1.Id, 1);

            // Then w1 is placed after w2
            var area = await _svc.GetAreaAsync(WidgetService.BlogSidebar1.Id);
            Assert.Equal(w1.Id, area.WidgetIds[1]);
            Assert.Equal(w2.Id, area.WidgetIds[0]);
        }

        [Fact]
        public async void User_can_update_instance_properties()
        {
            // Given a widget area "blog-sidebar1" and a widget
            await _svc.RegisterAreaAsync(WidgetService.BlogSidebar1);
            var widget = await _svc.CreateWidgetAsync(MY_WIDGET_TYPE);
            await _svc.AddWidgetToAreaAsync(widget.Id, WidgetService.BlogSidebar1.Id, 0);

            // When user udpates the widget instance
            MyWidget myWidget = (MyWidget) await _svc.GetWidgetAsync(widget.Id);
            myWidget.Age = 20;
            await _svc.UpdateWidgetAsync(widget.Id, myWidget);

            // Then the widget instance is updated
            var myWidgetAgain = (MyWidget)await _svc.GetWidgetAsync(widget.Id);
            Assert.Equal(20, myWidgetAgain.Age);
        }
    }
}
