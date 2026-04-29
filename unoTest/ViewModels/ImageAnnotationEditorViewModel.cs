using Color = Windows.UI.Color;
using IOPath = System.IO.Path;

namespace unoTest.ViewModels;

public sealed partial class ImageAnnotationEditorViewModel : ObservableObject
{
    public const double DefaultCanvasWidth = 960;
    public const double DefaultCanvasHeight = 540;
    public const float MinimumZoomFactor = 0.25f;
    public const float MaximumZoomFactor = 8f;

    private readonly IReadOnlyDictionary<string, Color> _colorMap = new Dictionary<string, Color>(StringComparer.Ordinal)
    {
        ["Red"] = Microsoft.UI.Colors.Red,
        ["Orange"] = Microsoft.UI.Colors.Orange,
        ["Gold"] = Microsoft.UI.Colors.Gold,
        ["LimeGreen"] = Microsoft.UI.Colors.LimeGreen,
        ["DodgerBlue"] = Microsoft.UI.Colors.DodgerBlue,
        ["MediumPurple"] = Microsoft.UI.Colors.MediumPurple,
        ["Black"] = Microsoft.UI.Colors.Black
    };

    [ObservableProperty]
    private AnnotationTool _activeTool = AnnotationTool.MoveImage;

    [ObservableProperty]
    private Color _activeColor = Microsoft.UI.Colors.Red;

    [ObservableProperty]
    private string _hintText = "先載入圖片，再選擇工具開始標註。";

    [ObservableProperty]
    private float _zoomFactor = 1f;

    [ObservableProperty]
    private string _zoomDisplayText = "100%";

    [ObservableProperty]
    private string _sourceFileName = "annotated-image";

    [ObservableProperty]
    private double _baseCanvasWidth = DefaultCanvasWidth;

    [ObservableProperty]
    private double _baseCanvasHeight = DefaultCanvasHeight;

    public bool TrySetTool(string rawTool, out AnnotationTool tool)
    {
        if (!Enum.TryParse(rawTool, ignoreCase: false, out tool))
        {
            return false;
        }

        ActiveTool = tool;
        return true;
    }

    public bool TrySetColor(string colorKey)
    {
        if (!_colorMap.TryGetValue(colorKey, out var color))
        {
            return false;
        }

        ActiveColor = color;
        return true;
    }

    public void SetCanvasSize(double width, double height)
    {
        BaseCanvasWidth = Math.Max(320, width);
        BaseCanvasHeight = Math.Max(240, height);
    }

    public void SetSourceFileName(string fileName)
    {
        SourceFileName = IOPath.GetFileNameWithoutExtension(fileName);
    }

    public float SetZoom(float nextZoom)
    {
        ZoomFactor = Math.Clamp(nextZoom, MinimumZoomFactor, MaximumZoomFactor);
        ZoomDisplayText = $"{Math.Round(ZoomFactor * 100, MidpointRounding.AwayFromZero)}%";
        return ZoomFactor;
    }

    public static string GetToolHint(AnnotationTool tool)
    {
        return tool switch
        {
            AnnotationTool.MoveImage => "移動模式：拖曳畫布平移檢視，縮放可用放大/縮小按鈕。",
            AnnotationTool.Rectangle => "方框線模式：拖曳建立方框，完成後可拖曳移動與拉角點調整。",
            AnnotationTool.Polygon => "多邊形模式：連續點擊建立節點，完成後可拖曳移動與調整頂點。",
            _ => "先載入圖片，再選擇工具開始標註。"
        };
    }
}

public enum AnnotationTool
{
    MoveImage,
    Rectangle,
    Polygon
}