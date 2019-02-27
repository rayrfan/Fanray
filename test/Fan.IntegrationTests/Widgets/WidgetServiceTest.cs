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
using System.Collections.Generic;
using System.IO;
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

        [Fact]
        public async void Widget_areas_are_predefined_and_registered_during_setup()
        {
            // Given some pre-defined widget areas
            var blogSidebar1 = WidgetService.BlogSidebar1;

            // When the system sets up, it would register the widget areas
            await _svc.RegisterAreaAsync(blogSidebar1);

            // Then the system would have widget areas avaiable for retrival
            var area = await _svc.GetAreaAsync(WidgetService.BlogSidebar1.Id);
            Assert.NotNull(area);
        }

        [Fact]
        public async void Widget_area_contains_a_list_of_widget_instances()
        {
            // Given a pre-defined widget area in system and two widgets
            await _svc.RegisterAreaAsync(WidgetService.BlogSidebar1);
            var w1 = await _svc.AddWidgetAsync(MY_WIDGET_TYPE, WidgetService.BlogSidebar1.Id, 0);
            var w2 = await _svc.AddWidgetAsync(MY_WIDGET_TYPE, WidgetService.BlogSidebar1.Id, 1);

            // When we retrieve a widget area
            var widgetArea = await _svc.GetAreaAsync(WidgetService.BlogSidebar1.Id);

            // Then it should have a list of widget instances
            Assert.NotNull(widgetArea.WidgetInstances);
            Assert.Equal(2, widgetArea.WidgetInstances.Count);
            Assert.Equal(w2.Id, widgetArea.WidgetIds[1]);
        }

        [Fact]
        public async void Admin_Panel_Widgets_page_displays_all_areas_in_the_current_theme()
        {
            // Arrange pre-defined widget areas in system
            await _svc.RegisterAreaAsync(WidgetService.BlogSidebar1);

            // Given my current theme has only 1 widget area
            // When the Admin Panel Widgets page is requested
            var areas = await _svc.GetCurrentThemeAreasAsync();

            // Then it will display all areas in the current theme
            Assert.Single(areas);
        }

        [Fact]
        public async void Admin_Panel_Widgets_page_will_display_all_installed_widgets_information()
        {
            // When Admin Panel Widgets page is requested
            var widgetInfos = await _svc.GetInstalledWidgetsInfoAsync();

            // Then widgets info will display
            Assert.NotNull(widgetInfos);
        }

        /// <summary>
        /// When a user drops a widget onto a widget area, an instance of the widget is created with
        /// its id returned, then the area is updated with the new widget instance's id added to its
        /// id list.
        /// </summary>
        [Fact]
        public async void When_widget_drops_to_area_a_widget_instance_is_created_and_inserted_and_area_is_updated_with_its_id()
        {
            // Given an emtpy widget area "blog-sidebar1"
            await _svc.RegisterAreaAsync(WidgetService.BlogSidebar1);

            // When a BlogTags widget is dropped into the area
            var widget = await _svc.AddWidgetAsync(MY_WIDGET_TYPE, WidgetService.BlogSidebar1.Id, 0);

            // Then widget area contains the new instance
            var area = await _svc.GetAreaAsync(WidgetService.BlogSidebar1.Id);
            Assert.Equal(WidgetService.BlogSidebar1.Id, area.Id);
            Assert.Single(area.WidgetIds);
            Assert.Equal(widget.Id, area.WidgetIds[0]);
        }

        /// <summary>
        /// When a widget instance is first created, its properties have default values, 
        /// then user updates them to be instance specific values.
        /// </summary>
        [Fact]
        public async void When_widget_drops_to_area_it_has_initial_default_values()
        {
            // Given an emtpy widget area "blog-sidebar1"
            await _svc.RegisterAreaAsync(WidgetService.BlogSidebar1);

            // When a widget is dropped to area
            var widget = await _svc.AddWidgetAsync(MY_WIDGET_TYPE, WidgetService.BlogSidebar1.Id, 0);

            // Then widget instance has the default val
            Assert.Equal("My Widget", widget.Title);
        }

        [Fact]
        public async void A_widget_is_instantiated_from_json_and_type_info_strings()
        {
            // Given widget area and a widget in the area
            await _svc.RegisterAreaAsync(WidgetService.BlogSidebar1);
            var widgetVm = await _svc.AddWidgetAsync(MY_WIDGET_TYPE, WidgetService.BlogSidebar1.Id, 0);

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

        [Fact]
        public async void A_widget_can_be_added_multiple_times_to_an_area()
        {
            // Given an emtpy widget area "blog-sidebar1"
            await _svc.RegisterAreaAsync(WidgetService.BlogSidebar1);

            // When a widget is dropped twice to area
            var w1 = await _svc.AddWidgetAsync(MY_WIDGET_TYPE, WidgetService.BlogSidebar1.Id, 0);
            var w2 = await _svc.AddWidgetAsync(MY_WIDGET_TYPE, WidgetService.BlogSidebar1.Id, 1);

            // Then area contains both instances
            var area = await _svc.GetAreaAsync(WidgetService.BlogSidebar1.Id);
            Assert.Equal(w1.Id, area.WidgetIds[0]);
            Assert.Equal(w2.Id, area.WidgetIds[1]);
        }

        [Fact]
        public async void A_widget_can_be_removed_from_an_area()
        {
            // Given area "blog-sidebar1" and 2 widget instances
            await _svc.RegisterAreaAsync(WidgetService.BlogSidebar1);
            var w1 = await _svc.AddWidgetAsync(MY_WIDGET_TYPE, WidgetService.BlogSidebar1.Id, 0);
            var w2 = await _svc.AddWidgetAsync(MY_WIDGET_TYPE, WidgetService.BlogSidebar1.Id, 1);

            // When user removes a widget
            await _svc.RemoveWidgetAsync(w1.Id, WidgetService.BlogSidebar1.Id);

            // Then there is only one widget left in the area
            var area = await _svc.GetAreaAsync(WidgetService.BlogSidebar1.Id);
            Assert.Single(area.WidgetInstances);
        }

        [Fact]
        public void User_can_sort_widgets_in_an_area()
        {

        }

        [Fact]
        public async void User_can_update_instance_properties()
        {
            // Given a widget area "blog-sidebar1" and a widget
            await _svc.RegisterAreaAsync(WidgetService.BlogSidebar1);
            var widget = await _svc.AddWidgetAsync(MY_WIDGET_TYPE, WidgetService.BlogSidebar1.Id, 0);

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
