using System.Collections.ObjectModel;
using System.Text.Json;
using CommunityToolkit.Mvvm.ComponentModel;
using unoTest.Models;
using Color = Windows.UI.Color;
using IOPath = System.IO.Path;

namespace uIP.UI.Uno.ViewModels;

public sealed partial class ImageAnnotationEditorViewModel : ObservableObject
{
    public const double DefaultCanvasWidth = 960;
    public const double DefaultCanvasHeight = 540;
    public const float MinimumZoomFactor = 0.25f;
    public const float MaximumZoomFactor = 8f;
    private const double MinimumBlockSize = 6;

    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };

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

    private readonly Dictionary<string, AnnotationBlock> _blockLookup = new(StringComparer.Ordinal);

    public ObservableCollection<AnnotationBlock> Blocks { get; } = [];

    public ObservableCollection<AnnotationBlockEventRecord> BlockEventRecords { get; } = [];

    public event EventHandler? BlocksReloadRequested;

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

    [ObservableProperty]
    private bool _isDirty;

    [ObservableProperty]
    private string _documentVersion = AnnotationDocument.CurrentVersion;

    [ObservableProperty]
    private string _sourceIdentifier = string.Empty;

    [ObservableProperty]
    private string _lastSavedPath = string.Empty;

    [ObservableProperty]
    private DateTimeOffset _lastUpdatedAtUtc = DateTimeOffset.UtcNow;

    [ObservableProperty]
    private string? _selectedBlockId;

    public bool TrySetTool(string rawTool, out AnnotationTool tool)
    {
        if (!Enum.TryParse(rawTool, ignoreCase: false, out tool))
        {
            return false;
        }

        ActiveTool = tool;
        return true;
    }

    public bool TryGetColor(string colorKey, out Color color)
    {
        return _colorMap.TryGetValue(colorKey, out color);
    }

    public bool TrySetColor(string colorKey)
    {
        if (!TryGetColor(colorKey, out var color))
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

    public AnnotationDocument CreateDocument()
    {
        return new AnnotationDocument
        {
            Version = DocumentVersion,
            SourceFileName = SourceFileName,
            SourceIdentifier = SourceIdentifier,
            CanvasWidth = BaseCanvasWidth,
            CanvasHeight = BaseCanvasHeight,
            UpdatedAtUtc = LastUpdatedAtUtc,
            Blocks = Blocks.Select(CloneBlock).ToList()
        };
    }

    public string SaveDocumentAsJson(string? targetPath = null)
    {
        LastUpdatedAtUtc = DateTimeOffset.UtcNow;
        var json = JsonSerializer.Serialize(CreateDocument(), SerializerOptions);
        if (!string.IsNullOrWhiteSpace(targetPath))
        {
            File.WriteAllText(targetPath, json);
            LastSavedPath = targetPath;
        }

        IsDirty = false;
        return json;
    }

    public bool TryLoadDocumentFromJson(string json, out string? errorMessage)
    {
        try
        {
            var document = JsonSerializer.Deserialize<AnnotationDocument>(json, SerializerOptions);
            if (document is null)
            {
                errorMessage = "標註文件內容為空。";
                return false;
            }

            LoadDocument(document);
            errorMessage = null;
            return true;
        }
        catch (Exception ex)
        {
            errorMessage = ex.Message;
            return false;
        }
    }

    public void LoadDocument(AnnotationDocument? document)
    {
        if (document is null)
        {
            ReplaceBlocks([], markDirty: false, requestControlSync: true, clearLog: true);
            DocumentVersion = AnnotationDocument.CurrentVersion;
            SourceIdentifier = string.Empty;
            LastSavedPath = string.Empty;
            LastUpdatedAtUtc = DateTimeOffset.UtcNow;
            SelectedBlockId = null;
            IsDirty = false;
            return;
        }

        DocumentVersion = string.IsNullOrWhiteSpace(document.Version) ? AnnotationDocument.CurrentVersion : document.Version;
        SourceFileName = string.IsNullOrWhiteSpace(document.SourceFileName) ? "annotated-image" : document.SourceFileName;
        SourceIdentifier = document.SourceIdentifier ?? string.Empty;
        SetCanvasSize(document.CanvasWidth, document.CanvasHeight);
        LastUpdatedAtUtc = document.UpdatedAtUtc == default ? DateTimeOffset.UtcNow : document.UpdatedAtUtc;
        SelectedBlockId = null;

        ReplaceBlocks(document.Blocks, markDirty: false, requestControlSync: true, clearLog: true);

        foreach (var block in Blocks)
        {
            RecordBlockEvent(AnnotationBlockEventType.Loaded, block.Id, before: null, after: block);
        }

        IsDirty = false;
    }

    public void ReplaceBlocks(IEnumerable<AnnotationBlock> blocks, bool markDirty, bool requestControlSync, bool clearLog = false)
    {
        if (clearLog)
        {
            BlockEventRecords.Clear();
        }

        Blocks.Clear();
        _blockLookup.Clear();

        foreach (var normalized in blocks.Select(NormalizeBlock).Where(static block => block is not null).Cast<AnnotationBlock>())
        {
            Blocks.Add(normalized);
            _blockLookup[normalized.Id] = normalized;
        }

        LastUpdatedAtUtc = DateTimeOffset.UtcNow;
        IsDirty = markDirty;

        if (requestControlSync)
        {
            BlocksReloadRequested?.Invoke(this, EventArgs.Empty);
        }
    }

    public void UpsertBlock(AnnotationBlock block, AnnotationBlockEventType eventType, bool markDirty, bool requestControlSync = false)
    {
        var normalized = NormalizeBlock(block);
        if (normalized is null)
        {
            return;
        }

        var before = _blockLookup.TryGetValue(normalized.Id, out var existing) ? CloneBlock(existing) : null;
        if (existing is not null)
        {
            var index = Blocks.IndexOf(existing);
            if (index >= 0)
            {
                Blocks[index] = normalized;
            }
            else
            {
                Blocks.Add(normalized);
            }
        }
        else
        {
            Blocks.Add(normalized);
        }

        _blockLookup[normalized.Id] = normalized;
        LastUpdatedAtUtc = DateTimeOffset.UtcNow;
        IsDirty = markDirty;
        RecordBlockEvent(eventType, normalized.Id, before, normalized);

        if (requestControlSync)
        {
            BlocksReloadRequested?.Invoke(this, EventArgs.Empty);
        }
    }

    public bool RemoveBlock(string blockId, bool markDirty, bool requestControlSync = false)
    {
        if (!_blockLookup.TryGetValue(blockId, out var existing))
        {
            return false;
        }

        Blocks.Remove(existing);
        _blockLookup.Remove(blockId);
        if (SelectedBlockId == blockId)
        {
            SelectedBlockId = null;
        }

        LastUpdatedAtUtc = DateTimeOffset.UtcNow;
        IsDirty = markDirty;
        RecordBlockEvent(AnnotationBlockEventType.Deleted, blockId, CloneBlock(existing), after: null);

        if (requestControlSync)
        {
            BlocksReloadRequested?.Invoke(this, EventArgs.Empty);
        }

        return true;
    }

    public void SelectBlock(string? blockId)
    {
        if (string.Equals(SelectedBlockId, blockId, StringComparison.Ordinal))
        {
            return;
        }

        SelectedBlockId = blockId;
        if (!string.IsNullOrWhiteSpace(blockId) && _blockLookup.TryGetValue(blockId, out var block))
        {
            RecordBlockEvent(AnnotationBlockEventType.Selected, blockId, before: null, after: block);
        }
    }

    public void ClearBlocks(bool markDirty, bool requestControlSync = false)
    {
        if (Blocks.Count == 0)
        {
            return;
        }

        var removedBlocks = Blocks.Select(CloneBlock).ToList();
        Blocks.Clear();
        _blockLookup.Clear();
        SelectedBlockId = null;
        LastUpdatedAtUtc = DateTimeOffset.UtcNow;
        IsDirty = markDirty;

        foreach (var removed in removedBlocks)
        {
            RecordBlockEvent(AnnotationBlockEventType.Cleared, removed.Id, removed, after: null);
        }

        if (requestControlSync)
        {
            BlocksReloadRequested?.Invoke(this, EventArgs.Empty);
        }
    }

    public bool TryReadDocumentFromPath(string filePath, out string? errorMessage)
    {
        try
        {
            var json = File.ReadAllText(filePath);
            if (!TryLoadDocumentFromJson(json, out errorMessage))
            {
                return false;
            }

            LastSavedPath = filePath;
            IsDirty = false;
            return true;
        }
        catch (Exception ex)
        {
            errorMessage = ex.Message;
            return false;
        }
    }

    private void RecordBlockEvent(AnnotationBlockEventType eventType, string blockId, AnnotationBlock? before, AnnotationBlock? after)
    {
        BlockEventRecords.Add(new AnnotationBlockEventRecord
        {
            BlockId = blockId,
            EventType = eventType,
            OccurredAtUtc = DateTimeOffset.UtcNow,
            Before = before is null ? null : CloneBlock(before),
            After = after is null ? null : CloneBlock(after)
        });
    }

    private AnnotationBlock? NormalizeBlock(AnnotationBlock block)
    {
        if (block is null)
        {
            return null;
        }

        var createdAt = block.CreatedAtUtc == default ? DateTimeOffset.UtcNow : block.CreatedAtUtc;
        var updatedAt = DateTimeOffset.UtcNow;
        var id = string.IsNullOrWhiteSpace(block.Id) ? Guid.NewGuid().ToString("N") : block.Id;
        var stroke = string.IsNullOrWhiteSpace(block.StrokeColor) ? ColorToHex(ActiveColor) : block.StrokeColor;

        if (block.Type == AnnotationBlockType.Rectangle)
        {
            var x = Clamp(block.Bounds.X, 0, BaseCanvasWidth);
            var y = Clamp(block.Bounds.Y, 0, BaseCanvasHeight);
            var maxWidth = Math.Max(MinimumBlockSize, BaseCanvasWidth - x);
            var maxHeight = Math.Max(MinimumBlockSize, BaseCanvasHeight - y);
            var width = Clamp(block.Bounds.Width, MinimumBlockSize, maxWidth);
            var height = Clamp(block.Bounds.Height, MinimumBlockSize, maxHeight);

            return new AnnotationBlock
            {
                Id = id,
                Type = AnnotationBlockType.Rectangle,
                StrokeColor = stroke,
                Bounds = new AnnotationRect { X = x, Y = y, Width = width, Height = height },
                Points = [],
                Label = block.Label ?? string.Empty,
                Note = block.Note ?? string.Empty,
                CreatedAtUtc = createdAt,
                UpdatedAtUtc = updatedAt
            };
        }

        var points = block.Points
            .Select(point => new AnnotationPoint
            {
                X = Clamp(point.X, 0, BaseCanvasWidth),
                Y = Clamp(point.Y, 0, BaseCanvasHeight)
            })
            .ToList();

        if (points.Count < 3)
        {
            return null;
        }

        var minX = points.Min(point => point.X);
        var maxX = points.Max(point => point.X);
        var minY = points.Min(point => point.Y);
        var maxY = points.Max(point => point.Y);

        return new AnnotationBlock
        {
            Id = id,
            Type = AnnotationBlockType.Polygon,
            StrokeColor = stroke,
            Bounds = new AnnotationRect { X = minX, Y = minY, Width = Math.Max(MinimumBlockSize, maxX - minX), Height = Math.Max(MinimumBlockSize, maxY - minY) },
            Points = points,
            Label = block.Label ?? string.Empty,
            Note = block.Note ?? string.Empty,
            CreatedAtUtc = createdAt,
            UpdatedAtUtc = updatedAt
        };
    }

    private static AnnotationBlock CloneBlock(AnnotationBlock block)
    {
        return new AnnotationBlock
        {
            Id = block.Id,
            Type = block.Type,
            StrokeColor = block.StrokeColor,
            Bounds = new AnnotationRect
            {
                X = block.Bounds.X,
                Y = block.Bounds.Y,
                Width = block.Bounds.Width,
                Height = block.Bounds.Height
            },
            Points = block.Points
                .Select(point => new AnnotationPoint
                {
                    X = point.X,
                    Y = point.Y
                })
                .ToList(),
            Label = block.Label,
            Note = block.Note,
            CreatedAtUtc = block.CreatedAtUtc,
            UpdatedAtUtc = block.UpdatedAtUtc
        };
    }

    private static string ColorToHex(Color color)
    {
        return $"#{color.A:X2}{color.R:X2}{color.G:X2}{color.B:X2}";
    }

    private static double Clamp(double value, double min, double max)
    {
        return Math.Max(min, Math.Min(max, value));
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
