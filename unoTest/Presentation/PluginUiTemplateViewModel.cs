namespace unoTest.Presentation;

public sealed class PluginUiTemplateViewModel : ObservableObject
{
    public IPluginUiActionDispatcher ActionDispatcher { get; }

    public PluginUiTemplateViewModel(
        IPluginUiActionDispatcher actionDispatcher,
        TitleBarStateService titleBarState)
    {
        ActionDispatcher = actionDispatcher;
        titleBarState.SetTabsMode(tabIndex: 0);
    }
}