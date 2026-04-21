namespace unoTest.Presentation;

/// <summary>
/// NodeLink 頁面的共用流程：初始化、載入、儲存與重置模板。
/// </summary>
public abstract partial class NodeLinkEditorViewModelBase : ObservableObject
{
    private readonly INodeGraphRepository _repository;
    private bool _isInitialized;

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private string _pageTitle;

    [ObservableProperty]
    private string _lastSavedText = "尚未儲存";

    public NodeLinkCanvasViewModel Canvas { get; } = new();

    public IAsyncRelayCommand InitializeCommand { get; }
    public IAsyncRelayCommand SaveCommand { get; }
    public IRelayCommand ResetTemplateCommand { get; }

    protected abstract string GraphKey { get; }

    protected NodeLinkEditorViewModelBase(INodeGraphRepository repository, string pageTitle)
    {
        _repository = repository;
        _pageTitle = pageTitle;

        InitializeCommand = new AsyncRelayCommand(InitializeAsync, () => !IsBusy);
        SaveCommand = new AsyncRelayCommand(SaveAsync, () => !IsBusy);
        ResetTemplateCommand = new RelayCommand(ResetTemplate, () => !IsBusy);
    }

    protected abstract NodeGraphDocument CreateTemplateDocument();

    private async Task InitializeAsync()
    {
        if (_isInitialized)
        {
            return;
        }

        _isInitialized = true;

        try
        {
            IsBusy = true;
            await _repository.EnsureCreatedAsync();

            var existingGraph = await _repository.LoadAsync(GraphKey);
            if (existingGraph is null)
            {
                Canvas.LoadDocument(CreateTemplateDocument());
                Canvas.SetStatus("已載入預設模板，請按儲存寫入 SQLite");
                LastSavedText = "尚未儲存";
            }
            else
            {
                Canvas.LoadDocument(existingGraph);
                LastSavedText = "已從 SQLite 載入";
            }
        }
        catch (Exception ex)
        {
            Canvas.SetStatus($"載入失敗：{ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task SaveAsync()
    {
        try
        {
            IsBusy = true;
            var graph = Canvas.ToDocument(GraphKey);
            await _repository.SaveAsync(graph);

            LastSavedText = $"最後儲存：{DateTime.Now:yyyy-MM-dd HH:mm:ss}";
            Canvas.SetStatus($"已儲存 {graph.Nodes.Count} 個節點 / {graph.Links.Count} 條連線");
        }
        catch (Exception ex)
        {
            Canvas.SetStatus($"儲存失敗：{ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void ResetTemplate()
    {
        Canvas.LoadDocument(CreateTemplateDocument());
        LastSavedText = "已重置為預設模板（尚未儲存）";
    }

    partial void OnIsBusyChanged(bool value)
    {
        InitializeCommand.NotifyCanExecuteChanged();
        SaveCommand.NotifyCanExecuteChanged();
        ResetTemplateCommand.NotifyCanExecuteChanged();
    }
}
