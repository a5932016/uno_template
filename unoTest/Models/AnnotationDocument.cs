namespace unoTest.Models;

/// <summary>
/// 可序列化與持久化的標註文件資料。
/// </summary>
public sealed class AnnotationDocument
{
    public const string CurrentVersion = "1.0";

    public string Version { get; init; } = CurrentVersion;

    public string SourceFileName { get; init; } = string.Empty;

    public string SourceIdentifier { get; init; } = string.Empty;

    public double CanvasWidth { get; init; }

    public double CanvasHeight { get; init; }

    public DateTimeOffset UpdatedAtUtc { get; init; } = DateTimeOffset.UtcNow;

    public List<AnnotationBlock> Blocks { get; init; } = [];
}

public enum AnnotationBlockType
{
    Rectangle,
    Polygon
}

public sealed class AnnotationBlock
{
    public string Id { get; init; } = Guid.NewGuid().ToString("N");

    public AnnotationBlockType Type { get; init; }

    public string StrokeColor { get; init; } = "#FFFF0000";

    public AnnotationRect Bounds { get; init; } = new();

    public List<AnnotationPoint> Points { get; init; } = [];

    public string Label { get; init; } = string.Empty;

    public string Note { get; init; } = string.Empty;

    public DateTimeOffset CreatedAtUtc { get; init; } = DateTimeOffset.UtcNow;

    public DateTimeOffset UpdatedAtUtc { get; init; } = DateTimeOffset.UtcNow;
}

public sealed class AnnotationRect
{
    public double X { get; init; }
    public double Y { get; init; }
    public double Width { get; init; }
    public double Height { get; init; }
}

public sealed class AnnotationPoint
{
    public double X { get; init; }
    public double Y { get; init; }
}

public enum AnnotationBlockEventType
{
    Created,
    Updated,
    Deleted,
    Selected,
    Loaded,
    Cleared
}

public sealed class AnnotationBlockEventRecord
{
    public string BlockId { get; init; } = string.Empty;

    public AnnotationBlockEventType EventType { get; init; }

    public DateTimeOffset OccurredAtUtc { get; init; } = DateTimeOffset.UtcNow;

    public AnnotationBlock? Before { get; init; }

    public AnnotationBlock? After { get; init; }
}
