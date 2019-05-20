//using Fan.Data;
//using Fan.IntegrationTests.Base;
//using Fan.Settings;
//using Fan.Themes;
//using Fan.Widgets;
//using Microsoft.AspNetCore.Hosting;
//using Microsoft.Extensions.Logging;
//using Moq;
//using Newtonsoft.Json;
//using Serilog;
//using System;
//using System.IO;
//using System.Linq;
//using System.Threading.Tasks;
//using Xunit;
//using Xunit.Abstractions;

//namespace Fan.IntegrationTests.Widgets
//{
//    public class WidgetServiceTest : IntegrationTestBase, IAsyncLifetime
//    {
//        private const string MY_WIDGET_FOLDER = "MyWidget";
//        private readonly WidgetService _svc;
//        private readonly IThemeService themeService;
//        private readonly SqlMetaRepository _metaRepo;
//        private readonly Serilog.ILogger _output;

//        public WidgetServiceTest(ITestOutputHelper output)
//        {
//            // meta repo
//            _metaRepo = new SqlMetaRepository(_db);

//            _output = new LoggerConfiguration()
//               .MinimumLevel.Verbose()
//               .WriteTo.TestOutput(output, Serilog.Events.LogEventLevel.Verbose)
//               .CreateLogger()
//               .ForContext<WidgetServiceTest>();

//            // setup CoreSettings
//            var settingSvcMock = new Mock<ISettingService>();
//            settingSvcMock.Setup(svc => svc.GetSettingsAsync<CoreSettings>()).Returns(Task.FromResult(new CoreSettings()));

//            //// set ContentRootPath to "Fan.IntegrationTests"
//            //var workingDirectory = Environment.CurrentDirectory;
//            //var projectDirectory = Directory.GetParent(workingDirectory).Parent.Parent.FullName;
//            //_output.Information($"workingDirectory {workingDirectory}");
//            //_output.Information($"projectDirectory {projectDirectory}");

//            //var env = new Mock<IHostingEnvironment>();
//            //env.Setup(m => m.ContentRootPath).Returns(projectDirectory);

//            // logger
//            var loggerWidgetSvc = _loggerFactory.CreateLogger<WidgetService>();
//            var loggerThemeSvc = _loggerFactory.CreateLogger<ThemeService>();

//            //// theme service
//            //themeService = new ThemeService(settingSvcMock.Object, env.Object, _cache, _metaRepo, loggerThemeSvc);

//            //_svc = new WidgetService(_metaRepo, themeService, _cache, settingSvcMock.Object, env.Object, loggerWidgetSvc);
//        }

//        /// <summary>
//        /// Setup theme and 3 areas (1 theme defined, 2 system defined).
//        /// </summary>
//        /// <returns></returns>
//        public async Task InitializeAsync()
//        {
//            //await _svc.RegisterAreaAsync(WidgetService.BlogSidebar1.Id);
//            //await _svc.RegisterAreaAsync(WidgetService.BlogSidebar2.Id);
//            //await themeService.ActivateThemeAsync("Clarity");
//        }

//        public Task DisposeAsync() => Task.CompletedTask;

//        ///// <summary>
//        ///// During site setup system-defined areas will be registered.
//        ///// </summary>
//        //[Fact]
//        //public async void System_defined_widget_areas_are_registered_at_setup_thus_available_from_start()
//        //{
//        //    _output.Debug("System_defined_widget_areas_are_registered_at_setup_thus_available_from_start");

//        //    // Given system defined areas already exist
//        //    // Then the system would have widget areas avaiable for retrival
//        //    var area = await _svc.GetAreaAsync(WidgetService.BlogSidebar1.Id);
//        //    Assert.NotNull(area);
//        //}

//        ///// <summary>
//        ///// On the admin widgets page, you will see a list of all areas available in the current
//        ///// theme displayed on the right hand side.
//        ///// </summary>
//        //[Fact]
//        //public async void Admin_panel_widgets_page_displays_all_areas_in_the_current_theme()
//        //{
//        //    _output.Debug("Admin_panel_widgets_page_displays_all_areas_in_the_current_theme");

//        //    // When the Admin Panel Widgets page is requested
//        //    var areas = await _svc.GetCurrentThemeAreasAsync();

//        //    // Then it will display all areas in the current theme
//        //    Assert.Equal(3, areas.ToList().Count());
//        //}

//        ///// <summary>
//        ///// On the admin widgets page, you will see a list of all installed widgets information
//        ///// displayed on the left hand side.
//        ///// </summary>
//        //[Fact]
//        //public async void Admin_panel_widgets_page_displays_all_installed_widgets_information()
//        //{
//        //    _output.Debug("Admin_panel_widgets_page_displays_all_installed_widgets_information");

//        //    // Given I have MyWidget installed in this test project
//        //    // When Admin Panel Widgets page is requested
//        //    var widgetInfos = await _svc.GetManifestsAsync();

//        //    // Then widget's info will displayed
//        //    Assert.Single(widgetInfos);
//        //    Assert.Equal("My Widget", widgetInfos.ToList()[0].Name);
//        //    Assert.Equal("My testing widget.", widgetInfos.ToList()[0].Description);
//        //}

//        ///// <summary>
//        ///// When user drags a widget from the widget infos section on the left side to any of the
//        ///// widget areas on the right: 1) a widget instance is created; 2) then the widget instance 
//        ///// id is added to the area.
//        ///// </summary>
//        //[Fact]
//        //public async void User_can_drag_a_widget_from_widget_infos_section_to_an_area()
//        //{
//        //    _output.Debug("User_can_drag_a_widget_from_widget_infos_section_to_an_area");

//        //    // When user drags a widget from the widget infos section to an area
//        //    var widgetId = await _svc.CreateWidgetAsync(MY_WIDGET_FOLDER);
//        //    await _svc.AddWidgetToAreaAsync(widgetId, WidgetService.BlogSidebar1.Id, 0);

//        //    // Then the area would contain the widget
//        //    var area = await _svc.GetAreaAsync(WidgetService.BlogSidebar1.Id);
//        //    Assert.Contains(widgetId, area.WidgetIds);
//        //}

//        ///// <summary>
//        ///// When user drags a widget from an area to another area: 1) the exist widget would be 
//        ///// removed from the current area; 2) the widget would be added to the new area.
//        ///// </summary>
//        //[Fact]
//        //public async void User_can_drag_a_widget_from_an_area_to_another_area()
//        //{
//        //    _output.Debug("User_can_drag_a_widget_from_an_area_to_another_area");

//        //    // Given a widget in area blog sidebar1
//        //    var widgetId = await _svc.CreateWidgetAsync(MY_WIDGET_FOLDER);
//        //    await _svc.AddWidgetToAreaAsync(widgetId, WidgetService.BlogSidebar1.Id, 0);

//        //    // When user drags the widget from area sidebar1 to area sidebar2
//        //    await _svc.RemoveWidgetFromAreaAsync(widgetId, WidgetService.BlogSidebar1.Id);
//        //    await _svc.AddWidgetToAreaAsync(widgetId, WidgetService.BlogSidebar2.Id, 0);

//        //    // Then area sidebar1 would not have the widget anymore
//        //    var area1 = await _svc.GetAreaAsync(WidgetService.BlogSidebar1.Id);
//        //    Assert.DoesNotContain(widgetId, area1.WidgetIds);

//        //    // and area sidebar2 would have the widget
//        //    var area2 = await _svc.GetAreaAsync(WidgetService.BlogSidebar2.Id);
//        //    Assert.Contains(widgetId, area2.WidgetIds);

//        //    // and widget's areaId will be updated too
//        //    var widgetAgain = await _svc.GetExtensionAsync(widgetId);
//        //    Assert.Equal(WidgetService.BlogSidebar2.Id, widgetAgain.AreaId);
//        //}

//        ///// <summary>
//        ///// When user drags and drops a widget from infos section to an area, a widget instance 
//        ///// is created with properties of default values.  The user can then click on edit to 
//        ///// change the props values.
//        ///// </summary>
//        //[Fact]
//        //public async void When_user_drops_a_widget_from_info_section_to_area_widget_has_initial_default_values()
//        //{
//        //    _output.Debug("When_user_drops_a_widget_from_info_section_to_area_widget_has_initial_default_values");

//        //    // When a widget is dropped to area from infos
//        //    var widgetId = await _svc.CreateWidgetAsync(MY_WIDGET_FOLDER);
//        //    var widget = await _svc.AddWidgetToAreaAsync(widgetId, WidgetService.BlogSidebar1.Id, 0);

//        //    // Then widget instance has the default val
//        //    Assert.Equal("My Widget", widget.Title);
//        //}

//        ///// <summary>
//        ///// User can drag and drop the same widget from infos section multiple time to an area.
//        ///// </summary>
//        //[Fact]
//        //public async void User_can_drop_same_widget_multiple_times_to_an_area()
//        //{
//        //    _output.Debug("User_can_drop_same_widget_multiple_times_to_an_area");

//        //    // Given two widget instances in blog sidebar1
//        //    var w1Id = await _svc.CreateWidgetAsync(MY_WIDGET_FOLDER);
//        //    await _svc.AddWidgetToAreaAsync(w1Id, WidgetService.BlogSidebar1.Id, 0);
//        //    var w2Id = await _svc.CreateWidgetAsync(MY_WIDGET_FOLDER);
//        //    await _svc.AddWidgetToAreaAsync(w2Id, WidgetService.BlogSidebar1.Id, 1);

//        //    // When we retrieve a widget area
//        //    var area = await _svc.GetAreaAsync(WidgetService.BlogSidebar1.Id);

//        //    // Then area contains both instances
//        //    Assert.Equal(2, area.WidgetInstances.Count);
//        //    Assert.Equal(w1Id, area.WidgetIds[0]);
//        //    Assert.Equal(w2Id, area.WidgetIds[1]);
//        //}

//        ///// <summary>
//        ///// A widget instance is stored as json and later it's instantiated back to object from json.
//        ///// </summary>
//        //[Fact]
//        //public async void A_widget_is_instantiated_from_json_and_type_info_strings()
//        //{
//        //    _output.Debug("A_widget_is_instantiated_from_json_and_type_info_strings");

//        //    // Given a widget in the area blog sidebar1
//        //    var widgetId = await _svc.CreateWidgetAsync(MY_WIDGET_FOLDER);
//        //    await _svc.AddWidgetToAreaAsync(widgetId, WidgetService.BlogSidebar1.Id, 0);

//        //    // When the meta record is retrieved
//        //    var widgetMeta = await _metaRepo.GetAsync(widgetId);

//        //    // I'm able to get the widget folder
//        //    var widget = (Widget)JsonConvert.DeserializeObject(widgetMeta.Value, typeof(Widget));
//        //    Assert.Equal(MY_WIDGET_FOLDER, widget.Folder);

//        //    // Given a json string that represent an instance of MyWidget
//        //    string json = @"{""age"":10,""title"":""Tags"",""id"":0, ""folder"":""MyWidget""}";
//        //    // And the widget type I got from above
//        //    var type = await _svc.GetManifestTypeByFolderAsync(widget.Folder);

//        //    // When I deserialize it
//        //    var myWidget = (MyWidget)JsonConvert.DeserializeObject(json, type);

//        //    // Then we get the actual instance
//        //    Assert.Equal(10, myWidget.Age);
//        //}

//        ///// <summary>
//        ///// User can click on delete of a widget in an area to permanently delete the widget instance.
//        ///// The system would first remove the widget from the area, then delete the widget instance.
//        ///// </summary>
//        //[Fact]
//        //public async void User_can_delete_a_widget_from_an_area()
//        //{
//        //    _output.Debug("User_can_delete_a_widget_from_an_area");

//        //    // Given a widget in blog sidebar1
//        //    var widgetId = await _svc.CreateWidgetAsync(MY_WIDGET_FOLDER);
//        //    var widgetInst = await _svc.AddWidgetToAreaAsync(widgetId, WidgetService.BlogSidebar1.Id, 0);

//        //    // When user deletes the widget
//        //    await _svc.RemoveWidgetFromAreaAsync(widgetId, WidgetService.BlogSidebar1.Id);
//        //    await _svc.DeleteWidgetAsync(widgetId);

//        //    // Then the area does not have the widget anymore
//        //    var area = await _svc.GetAreaAsync(WidgetService.BlogSidebar1.Id);
//        //    Assert.DoesNotContain(widgetInst, area.WidgetInstances);
//        //}

//        ///// <summary>
//        ///// User can move a widget inside an area to a new index in the same area.
//        ///// </summary>
//        //[Fact]
//        //public async void User_can_order_widgets_in_an_area()
//        //{
//        //    _output.Debug("User_can_order_widgets_in_an_area");

//        //    // Given two widgets w1 and w2 in blog-sidebar1 area
//        //    var w1Id = await _svc.CreateWidgetAsync(MY_WIDGET_FOLDER);
//        //    await _svc.AddWidgetToAreaAsync(w1Id, WidgetService.BlogSidebar1.Id, 0);
//        //    var w2Id = await _svc.CreateWidgetAsync(MY_WIDGET_FOLDER);
//        //    await _svc.AddWidgetToAreaAsync(w2Id, WidgetService.BlogSidebar1.Id, 1);

//        //    // When user moves w1 below w2
//        //    await _svc.OrderWidgetInAreaAsync(w1Id, WidgetService.BlogSidebar1.Id, 1);

//        //    // Then w1 is placed after w2
//        //    var area = await _svc.GetAreaAsync(WidgetService.BlogSidebar1.Id);
//        //    Assert.Equal(w1Id, area.WidgetIds[1]);
//        //    Assert.Equal(w2Id, area.WidgetIds[0]);
//        //}

//        //[Fact]
//        //public async void User_can_update_instance_properties()
//        //{
//        //    _output.Debug("User_can_update_instance_properties");

//        //    // Given a widget in blog sidebar1
//        //    var widgetId = await _svc.CreateWidgetAsync(MY_WIDGET_FOLDER);
//        //    await _svc.AddWidgetToAreaAsync(widgetId, WidgetService.BlogSidebar1.Id, 0);

//        //    // When user udpates the widget instance
//        //    MyWidget myWidget = (MyWidget)await _svc.GetExtensionAsync(widgetId);
//        //    myWidget.Age = 20;
//        //    await _svc.UpdateWidgetAsync(widgetId, myWidget);

//        //    // Then the widget instance is updated
//        //    var myWidgetAgain = (MyWidget)await _svc.GetExtensionAsync(widgetId);
//        //    Assert.Equal(20, myWidgetAgain.Age);
//        //}
//    }
//}
