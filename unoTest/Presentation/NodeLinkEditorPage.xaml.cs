using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace unoTest.Presentation;

public sealed partial class NodeLinkEditorPage : Page
{
    public NodeLinkEditorPage()
    {
        this.InitializeComponent();
        this.Loaded += OnPageLoaded;
    }

    private async void OnPageLoaded(object sender, RoutedEventArgs e)
    {
        if (DataContext is NodeLinkEditorViewModelBase vm && vm.InitializeCommand.CanExecute(null))
        {
            await vm.InitializeCommand.ExecuteAsync(null);
        }
    }
}
