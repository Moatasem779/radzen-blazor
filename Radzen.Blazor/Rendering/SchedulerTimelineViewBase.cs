using Microsoft.AspNetCore.Components;
using Radzen.Blazor;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace Radzen.Blazor.Rendering;

/// <summary>
/// Base class for all timeline views.
/// </summary>
public abstract class SchedulerTimelineViewBase : SchedulerViewBase
{
    /// <summary>
    /// Gets or sets the collection of resources that will be displayed in the timeline view.
    /// Each resource represents a row in the timeline.
    /// </summary>
    [Parameter]
    public IEnumerable<SchedulerResource> Resources { get; set; } = Enumerable.Empty<SchedulerResource>();

    /// <summary>
    /// Gets or sets the name of the resource property used to build timeline groups.
    /// When set, resources are rendered under generated group header rows.
    /// </summary>
    [Parameter]
    public string? ResourceGroupProperty { get; set; }

    /// <summary>
    /// Gets or sets the name of the resource property used as the group header text.
    /// If omitted, <see cref="ResourceGroupProperty"/> is used for both key and text.
    /// </summary>
    [Parameter]
    public string? ResourceGroupTextProperty { get; set; }

    /// <summary>
    /// Gets or sets whether generated timeline resource groups can be collapsed.
    /// </summary>
    [Parameter]
    public bool AllowResourceGroupCollapse { get; set; } = true;

    /// <summary>
    /// Gets or sets the initially collapsed resource group keys.
    /// </summary>
    [Parameter]
    public IEnumerable<object?>? CollapsedResourceGroups { get; set; }

    /// <summary>
    /// Gets or sets the name of the property on the appointment data object that identifies the resource.
    /// This property is used to match appointments with their corresponding resources for display.
    /// For example, if appointments have a "ResourceId" property, set this to "ResourceId".
    /// </summary>
    [Parameter]
    public string? ResourceProperty { get; set; }

    /// <summary>
    /// Gets or sets the duration of each time slot in the timeline view.
    /// This determines how the timeline is divided horizontally.
    /// Default value is 30 minutes.
    /// </summary>
    [Parameter]
    public TimeSpan SlotDuration { get; set; } = TimeSpan.FromMinutes(30);

    private readonly HashSet<object?> collapsedGroups = new();
    private IEnumerable<object?>? previousCollapsedResourceGroups;

    /// <inheritdoc />
    protected override void OnParametersSet()
    {
        base.OnParametersSet();

        if (!ReferenceEquals(previousCollapsedResourceGroups, CollapsedResourceGroups))
        {
            collapsedGroups.Clear();

            if (CollapsedResourceGroups != null)
            {
                foreach (var group in CollapsedResourceGroups)
                {
                    collapsedGroups.Add(group);
                }
            }

            previousCollapsedResourceGroups = CollapsedResourceGroups;
        }
    }

    /// <summary>
    /// Gets resources with generated group headers and collapsed children removed.
    /// </summary>
    protected IEnumerable<SchedulerResource> VisibleResources => GetVisibleResources();

    /// <summary>
    /// Toggles a resource group between expanded and collapsed states.
    /// </summary>
    /// <param name="groupKey">The group key to toggle.</param>
    protected void ToggleResourceGroup(object? groupKey)
    {
        if (!AllowResourceGroupCollapse)
        {
            return;
        }

        if (!collapsedGroups.Add(groupKey))
        {
            collapsedGroups.Remove(groupKey);
        }
    }

    /// <summary>
    /// Returns true when the specified resource group is collapsed.
    /// </summary>
    /// <param name="groupKey">The group key.</param>
    /// <returns><c>true</c> if collapsed; otherwise, <c>false</c>.</returns>
    protected bool IsResourceGroupCollapsed(object? groupKey) => collapsedGroups.Contains(groupKey);

    /// <summary>
    /// Retrieves appointments for a specific resource within the specified time range.
    /// Filters appointments that fall within the time boundary and match the resource ID.
    /// </summary>
    /// <param name="resource">The resource for which to retrieve appointments.</param>
    /// <param name="start">The start of the time range to query.</param>
    /// <param name="end">The end of the time range to query.</param>
    /// <returns>A collection of appointments that match both the time range and resource criteria.</returns>
    protected IEnumerable<AppointmentData> GetAppointmentsForResource(SchedulerResource resource, DateTime start, DateTime end)
    {
        ArgumentNullException.ThrowIfNull(resource);

        if (Scheduler == null || resource.IsGroupHeader)
        {
            return Enumerable.Empty<AppointmentData>();
        }

        var appointments = Scheduler.GetAppointmentsInRange(start, end)
            .Where(appointment => Scheduler.IsAppointmentInRange(appointment, start, end));

        if (string.IsNullOrEmpty(ResourceProperty))
        {
            return appointments;
        }

        var resourceValue = resource.Value ?? resource.Id;

        return appointments.Where(appointment => ResourceMatches(appointment.Data, resourceValue));
    }

    /// <summary>
    /// Retrieves the resource property value from an appointment data item using reflection.
    /// </summary>
    /// <param name="item">The appointment data item to extract the resource value from.</param>
    /// <returns>The value of the resource property, or null if the property doesn't exist or the item is null.</returns>
    private object? GetResourceValue(object? item)
    {
        return GetPropertyValue(item, ResourceProperty);
    }

    private bool ResourceMatches(object? item, object? resourceValue)
    {
        var appointmentResourceValue = GetResourceValue(item);

        if (appointmentResourceValue is System.Collections.IEnumerable values && appointmentResourceValue is not string)
        {
            foreach (var value in values)
            {
                if (ResourceValueEquals(value, resourceValue))
                {
                    return true;
                }
            }

            return false;
        }

        return ResourceValueEquals(appointmentResourceValue, resourceValue);
    }

    private static bool ResourceValueEquals(object? value, object? resourceValue)
    {
        if (Equals(value, resourceValue))
        {
            return true;
        }

        return string.Equals(Convert.ToString(value, CultureInfo.InvariantCulture), Convert.ToString(resourceValue, CultureInfo.InvariantCulture), StringComparison.Ordinal);
    }

    private IEnumerable<SchedulerResource> GetVisibleResources()
    {
        foreach (var resource in GetGroupedResources())
        {
            if (!resource.IsGroupHeader && resource.ParentGroupKey != null && collapsedGroups.Contains(resource.ParentGroupKey))
            {
                continue;
            }

            yield return resource;
        }
    }

    private IEnumerable<SchedulerResource> GetGroupedResources()
    {
        if (string.IsNullOrEmpty(ResourceGroupProperty))
        {
            return Resources ?? Enumerable.Empty<SchedulerResource>();
        }

        return (Resources ?? Enumerable.Empty<SchedulerResource>())
            .GroupBy(GetResourceGroupKey)
            .SelectMany(group =>
            {
                var groupKey = group.Key;
                var first = group.FirstOrDefault();
                var groupText = Convert.ToString(GetPropertyValue(first?.Data ?? first, ResourceGroupTextProperty ?? ResourceGroupProperty), CultureInfo.InvariantCulture) ?? Convert.ToString(groupKey, CultureInfo.InvariantCulture);

                var header = new SchedulerResource
                {
                    Text = groupText,
                    Value = groupKey,
                    GroupKey = groupKey,
                    IsGroupHeader = true
                };

                return new[] { header }.Concat(group.Select(resource =>
                {
                    resource.ParentGroupKey = groupKey;
                    return resource;
                }));
            });
    }

    private object? GetResourceGroupKey(SchedulerResource resource)
    {
        return GetPropertyValue(resource.Data ?? resource, ResourceGroupProperty);
    }

    private static object? GetPropertyValue(object? item, string? propertyName)
    {
        if (item == null || string.IsNullOrEmpty(propertyName))
        {
            return null;
        }

        return item.GetType().GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase)?.GetValue(item, null);
    }
}
