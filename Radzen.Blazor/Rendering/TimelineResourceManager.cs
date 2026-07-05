using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Radzen.Blazor.Rendering
{
    /// <summary>
    /// Represents a grouped resource item.
    /// </summary>
    public class TimelineResourceGroup
    {
        /// <summary>
        /// Gets or sets the unique key of the resource.
        /// </summary>
        public object Key { get; set; } = null!;

        /// <summary>
        /// Gets or sets the text representation of the resource.
        /// </summary>
        public string Text { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the original raw resource object.
        /// </summary>
        public object RawResource { get; set; } = null!;

        /// <summary>
        /// Gets or sets child resources (for nested groups).
        /// </summary>
        public List<TimelineResourceGroup> Children { get; set; } = new();

        /// <summary>
        /// Gets or sets whether this group is expanded.
        /// </summary>
        public bool IsExpanded { get; set; } = true;
    }

    /// <summary>
    /// Handles resource structure resolution and matches appointments to resource lanes.
    /// </summary>
    public static class TimelineResourceManager
    {
        /// <summary>
        /// Resolves flat resources list into a unified list of resource groups.
        /// </summary>
        public static List<TimelineResourceGroup> ResolveResourceGroups(
            IEnumerable? resources,
            string? textProperty,
            string? valueProperty)
        {
            var groups = new List<TimelineResourceGroup>();
            if (resources == null)
            {
                return groups;
            }

            foreach (var item in resources)
            {
                if (item == null) continue;

                object key = item;
                if (!string.IsNullOrEmpty(valueProperty))
                {
                    key = PropertyAccess.GetItemOrValueFromProperty(item, valueProperty) ?? item;
                }

                string text = item.ToString() ?? string.Empty;
                if (!string.IsNullOrEmpty(textProperty))
                {
                    text = PropertyAccess.GetItemOrValueFromProperty(item, textProperty)?.ToString() ?? text;
                }

                groups.Add(new TimelineResourceGroup
                {
                    Key = key,
                    Text = text,
                    RawResource = item
                });
            }

            return groups;
        }

        /// <summary>
        /// Determines if the specified appointment matches the target resource key.
        /// </summary>
        public static bool AppBelongsToResource(
            AppointmentData appointment,
            string? resourceProperty,
            object? resourceKey)
        {
            if (resourceKey == null) return false;
            if (appointment?.Data == null || string.IsNullOrEmpty(resourceProperty)) return false;

            var val = PropertyAccess.GetItemOrValueFromProperty(appointment.Data, resourceProperty);
            if (val == null) return false;

            if (val.Equals(resourceKey)) return true;

            // Handle enumerable collection values (e.g., RoomIds = [1, 2])
            if (val is IEnumerable enumerable && val is not string)
            {
                foreach (var item in enumerable)
                {
                    if (item != null && item.Equals(resourceKey))
                        return true;
                }
            }

            return false;
        }
    }
}
