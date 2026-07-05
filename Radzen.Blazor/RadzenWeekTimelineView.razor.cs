using Microsoft.AspNetCore.Components;
using Radzen.Blazor.Rendering;
using System;
using System.Collections;
using System.Globalization;

namespace Radzen.Blazor
{
    /// <summary>
    /// Displays the appointments in a week timeline view in <see cref="RadzenScheduler{TItem}" />
    /// </summary>
    public partial class RadzenWeekTimelineView : SchedulerViewBase
    {
        /// <inheritdoc />
        public override string Icon => "view_week";

        /// <inheritdoc />
        [Parameter]
        public override string Text { get; set; } = "Week Timeline";

        /// <summary>
        /// Gets or sets the header format.
        /// </summary>
        [Parameter]
        public string HeaderFormat { get; set; } = "ddd d/M";

        /// <summary>
        /// The list of resources.
        /// </summary>
        [Parameter]
        public IEnumerable? Resources { get; set; }

        /// <summary>
        /// The property name on the appointment data representing its resource ID.
        /// </summary>
        [Parameter]
        public string? ResourceProperty { get; set; }

        /// <summary>
        /// The property name on the resource representing its display text.
        /// </summary>
        [Parameter]
        public string? ResourceTextProperty { get; set; }

        /// <summary>
        /// The property name on the resource representing its unique value.
        /// </summary>
        [Parameter]
        public string? ResourceValueProperty { get; set; }

        /// <summary>
        /// The orientation of the grouping.
        /// </summary>
        [Parameter]
        public GroupOrientation GroupOrientation { get; set; } = GroupOrientation.Vertical;

        /// <inheritdoc />
        public override DateTime StartDate
        {
            get
            {
                var culture = Scheduler?.Culture ?? CultureInfo.CurrentCulture;
                return Scheduler?.CurrentDate.Date.StartOfWeek(culture) ?? DateTime.Today.StartOfWeek(culture);
            }
        }

        /// <inheritdoc />
        public override DateTime EndDate
        {
            get
            {
                var culture = Scheduler?.Culture ?? CultureInfo.CurrentCulture;
                return StartDate.EndOfWeek(culture).AddDays(1);
            }
        }

        /// <inheritdoc />
        public override string Title
        {
            get
            {
                var culture = Scheduler?.Culture ?? CultureInfo.CurrentCulture;
                return $"{StartDate.ToString(culture.DateTimeFormat.ShortDatePattern, culture)} - {StartDate.EndOfWeek(culture).ToString(culture.DateTimeFormat.ShortDatePattern, culture)}";
            }
        }

        /// <inheritdoc />
        public override DateTime Next()
        {
            return Scheduler?.CurrentDate.Date.AddDays(7) ?? DateTime.Today.AddDays(7);
        }

        /// <inheritdoc />
        public override DateTime Prev()
        {
            return Scheduler?.CurrentDate.Date.AddDays(-7) ?? DateTime.Today.AddDays(-7);
        }
    }
}
