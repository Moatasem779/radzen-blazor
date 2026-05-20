namespace Radzen.Blazor;
/// <summary>
/// Represents a scheduler resource.
/// </summary>
public class SchedulerResource
{
    /// <summary>
    /// Gets or sets the resource identifier.
    /// </summary>
    public string? Id { get; set; }
    /// <summary>
    /// Gets or sets the resource title.
    /// </summary>
    public string? End { get; set; }
    /// <summary>
    /// Gets or sets the resource title.
    /// </summary>
    public string? Text { get; set; }

    /// <summary>
    /// Gets or sets the resource color used by timeline views.
    /// </summary>
    public string? Color { get; set; }

    /// <summary>
    /// Gets or sets the resource value used to match appointments (see appointment resource property).
    /// </summary>
    /// <value><c>null</c> or a sentinel value for non-schedulable rows such as group headers.</value>
    public object? Value { get; set; }

    /// <summary>
    /// Gets or sets the original resource item (passed to resource templates).
    /// </summary>
    public object? Data { get; set; }

    /// <summary>
    /// When <c>true</c>, this row is a timeline group header: no appointment slots are rendered next to it.
    /// </summary>
    /// <seealso cref="GroupKey"/>
    public bool IsGroupHeader { get; set; }

    /// <summary>
    /// Stable key for group expand/collapse. Required for group headers that should toggle.
    /// </summary>
    public object? GroupKey { get; set; }

    /// <summary>
    /// When set on a leaf resource row, the row is omitted from the timeline while its parent group's <see cref="GroupKey"/> is collapsed.
    /// </summary>
    public object? ParentGroupKey { get; set; }
}
