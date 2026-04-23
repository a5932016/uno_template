using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace unoTest.Presentation;

public sealed partial class Shell : UserControl, IContentControlProvider
{
    public Shell()
    {
        this.InitializeComponent();
        this.Loaded += OnLoaded;
    }
    public ContentControl ContentControl => Splash;

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        // 將拖曳區綁在 Shell 的獨立細條，避免 CustomTitleBar 內互動控件被 OS 吃掉點擊。
        ShellTitleBar.BindAsWindowTitleBar(WindowDragStrip);
    }
}
