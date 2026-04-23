using System.Diagnostics.CodeAnalysis;
using Microsoft.UI.Windowing;
using Uno.Resizetizer;

namespace unoTest;

public partial class App : Application
{
    /// <summary>
    /// Initializes the singleton application object. This is the first line of authored code
    /// executed, and as such is the logical equivalent of main() or WinMain().
    /// </summary>
    public App()
    {
        this.InitializeComponent();
    }

    protected Window? MainWindow { get; private set; }
    protected IHost? Host { get; private set; }

    /// <summary>
    /// 靜態視窗參照，供 Controls（CustomTitleBar）呼叫 SetTitleBar / AppWindow API 使用
    /// </summary>
    internal static Window? CurrentMainWindow { get; private set; }

    [SuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "Uno.Extensions APIs are used in a way that is safe for trimming in this template context.")]
    protected async override void OnLaunched(LaunchActivatedEventArgs args)
    {
        var builder = this.CreateBuilder(args)
            // Add navigation support for toolkit controls such as TabBar and NavigationView
            .UseToolkitNavigation()
            .Configure(host => host
#if DEBUG
                // Switch to Development environment when running in DEBUG
                .UseEnvironment(Environments.Development)
#endif
                .UseLogging(configure: (context, logBuilder) =>
                {
                    // Configure log levels for different categories of logging
                    logBuilder
                        .SetMinimumLevel(
                            context.HostingEnvironment.IsDevelopment() ?
                                LogLevel.Information :
                                LogLevel.Warning)

                        // Default filters for core Uno Platform namespaces
                        .CoreLogLevel(LogLevel.Warning);

                    // Uno Platform namespace filter groups
                    // Uncomment individual methods to see more detailed logging
                    //// Generic Xaml events
                    //logBuilder.XamlLogLevel(LogLevel.Debug);
                    //// Layout specific messages
                    //logBuilder.XamlLayoutLogLevel(LogLevel.Debug);
                    //// Storage messages
                    //logBuilder.StorageLogLevel(LogLevel.Debug);
                    //// Binding related messages
                    //logBuilder.XamlBindingLogLevel(LogLevel.Debug);
                    //// Binder memory references tracking
                    //logBuilder.BinderMemoryReferenceLogLevel(LogLevel.Debug);
                    //// DevServer and HotReload related
                    //logBuilder.HotReloadCoreLogLevel(LogLevel.Information);
                    //// Debug JS interop
                    //logBuilder.WebAssemblyLogLevel(LogLevel.Debug);

                }, enableUnoLogging: true)
                .UseConfiguration(configure: configBuilder =>
                    configBuilder
                        .EmbeddedSource<App>()
                        .Section<AppConfig>()
                )
                // Enable localization (see appsettings.json for supported languages)
                .UseLocalization()
                .UseHttp((context, services) =>
                {
#if DEBUG
                // DelegatingHandler will be automatically injected
                services.AddTransient<DelegatingHandler, DebugHttpHandler>();
#endif

                })
                .ConfigureServices((context, services) =>
                {
                    // 註冊服務
                    services.AddSingleton<IAuthService, MockAuthService>();
                    services.AddSingleton<IProductService, MockProductService>();
                    // ★ TitleBarStateService：Singleton，跨頁面共享 TitleBar 狀態
                    services.AddSingleton<TitleBarStateService>();
#if __WASM__
                    services.AddSingleton<INodeGraphRepository, InMemoryNodeGraphRepository>();
#else
                    services.AddSingleton<ISqliteDbConnectionFactory, SqliteDbConnectionFactory>();
                    services.AddSingleton<INodeGraphRepository, SqliteNodeGraphRepository>();
#endif
                })
                .UseNavigation(RegisterRoutes)
            );
        MainWindow = builder.Window;
        CurrentMainWindow = MainWindow;  // ★ 供 Controls 層存取

#if DEBUG
        MainWindow.UseStudio();
#endif
        MainWindow.SetWindowIcon();

        // ★ Desktop only：移除作業系統原生標題列，讓 Shell 的 CustomTitleBar（Row 0）
        //   延伸至視窗最頂端，成為真正的 OS-level 標題列。
        //
        //   底層機制（Uno Platform Win32 後端）：
        //   ExtendsContentIntoTitleBar = true
        //     → Win32WindowWrapper 訂閱 AppWindowTitleBar.Changed 事件
        //     → 呼叫 DwmExtendFrameIntoClientArea（Win32 DWM API）移除原生標題列
        //     → Skia 渲染層從視窗頂端（y=0）開始繪製，覆蓋整個視窗
        //
        //   PreferredHeightOption = Tall（48px）：
        //     → 讓 DwmExtendFrameIntoClientArea 的 cyTopHeight = 48px，與 CustomTitleBar 高度一致
        //     → 讓 WindowChrome 的 XAML 視窗控制按鈕（Min/Max/Close）高度也是 48px
        //     → 確保 Windows 11 拖曳快照功能在正確位置觸發
        //
        //   Shell.xaml.cs 會呼叫 CustomTitleBar.BindAsWindowTitleBar(WindowDragStrip)：
        //     → 只把頂部 8px strip 註冊成可拖曳 Caption Region
        //     → Tab / Button 保持在 Client 區域，確保可點擊
#if !__WASM__ && !__ANDROID__ && !__IOS__
        // Uno Platform 目前僅 Windows Desktop 支援 OS-level TitleBar 客製化
        if (OperatingSystem.IsWindows())
        {
            MainWindow.ExtendsContentIntoTitleBar = true;
            // 告訴 Uno Platform 標題列高度為 48px（Tall），與 CustomTitleBar 的 Height="48" 一致
            MainWindow.AppWindow.TitleBar.PreferredHeightOption = TitleBarHeightOption.Tall;
        }
#endif

        Host = await builder.NavigateAsync<Shell>();
    }

    private static void RegisterRoutes(IViewRegistry views, IRouteRegistry routes)
    {
        views.Register(
            new ViewMap(ViewModel: typeof(ShellViewModel)),
            new ViewMap<MainPage, MainViewModel>(),
            new DataViewMap<SecondPage, SecondViewModel, Entity>(),
            // 新增頁面註冊
            new ViewMap<DemoIndexPage, DemoIndexViewModel>(),
            new ViewMap<LoginPage, LoginViewModel>(),
            new ViewMap<LayoutDemoPage, LayoutDemoViewModel>(),
            new ViewMap<CrudDemoPage, CrudDemoViewModel>(),
            new ViewMap<NodeLinkDemoPage, NodeLinkDemoViewModel>(),
            new ViewMap<NodeLinkTemplatePage, NodeLinkTemplateViewModel>(),
            new ViewMap<NodeLinkEditorPage, NodeLinkEditorViewModel>(),
            new ViewMap<SettingsPage, SettingsViewModel>(),
            // 新功能頁面
            new ViewMap<ImageGalleryPage>(),
            new ViewMap<LinkedListDemoPage>(),
            new ViewMap<TabDemoPage>(),
            new ViewMap<DialogDemoPage>(),
            new ViewMap<LocalizationDemoPage>(),
            // ★ 動態 UI + 資料傳遞示範頁面
            new ViewMap<ProductListPage, ProductListViewModel>(),
            new DataViewMap<ProductDetailPage, ProductDetailViewModel, ProductNavData>()
        );

        routes.Register(
            new RouteMap("", View: views.FindByViewModel<ShellViewModel>(),
                Nested:
                [
                    new ("DemoIndex", View: views.FindByViewModel<DemoIndexViewModel>(), IsDefault:true),
                    new ("Main", View: views.FindByViewModel<MainViewModel>()),
                    new ("Second", View: views.FindByViewModel<SecondViewModel>()),
                    new ("Login", View: views.FindByViewModel<LoginViewModel>()),
                    new ("LayoutDemo", View: views.FindByViewModel<LayoutDemoViewModel>()),
                    new ("CrudDemo", View: views.FindByViewModel<CrudDemoViewModel>()),
                    new ("NodeLinkDemo", View: views.FindByViewModel<NodeLinkDemoViewModel>()),
                    new ("NodeLinkTemplate", View: views.FindByViewModel<NodeLinkTemplateViewModel>()),
                    new ("NodeLinkEditor", View: views.FindByViewModel<NodeLinkEditorViewModel>()),
                    new ("Settings", View: views.FindByViewModel<SettingsViewModel>()),
                    // 新功能路由
                    new ("ImageGallery", View: views.FindByView<ImageGalleryPage>()),
                    new ("LinkedListDemo", View: views.FindByView<LinkedListDemoPage>()),
                    new ("TabDemo", View: views.FindByView<TabDemoPage>()),
                    new ("DialogDemo", View: views.FindByView<DialogDemoPage>()),
                    new ("LocalizationDemo", View: views.FindByView<LocalizationDemoPage>()),
                    // ★ 動態 UI + 資料傳遞示範
                    new ("ProductList", View: views.FindByViewModel<ProductListViewModel>()),
                    new ("ProductDetail", View: views.FindByViewModel<ProductDetailViewModel>()),
                ]
            )
        );
    }
}
