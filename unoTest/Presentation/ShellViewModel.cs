namespace unoTest.Presentation;

public class ShellViewModel
{
    /// <summary>
    /// TitleBar ViewModel - CustomTitleBar 的 DataContext
    /// 由 ShellViewModel 建立，使用 Shell 層級的 INavigator（根導航器）
    /// </summary>
    public TitleBarViewModel TitleBar { get; }

    public ShellViewModel(
        INavigator navigator,
        TitleBarStateService titleBarStateService,
        IThemeService? themeService = null)
    {
        // 建立 TitleBarViewModel，傳入根層級 INavigator
        // 這樣 Tab 切換可以控制主 Frame 的導航
        TitleBar = new TitleBarViewModel(navigator, titleBarStateService, themeService);
    }
}
