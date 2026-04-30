using Microsoft.UI.Xaml.Controls;
using unoTest.ViewModels;

namespace unoTest.Presentation;

public sealed partial class ImageAnnotationTemplatePage : Page
{
    public ImageAnnotationTemplatePage()
    {
        this.InitializeComponent();
        Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        Editor.SetTool(AnnotationTool.Rectangle);
        Editor.TrySetColor("Red");
        Editor.SetZoom(1f);
    }
}
