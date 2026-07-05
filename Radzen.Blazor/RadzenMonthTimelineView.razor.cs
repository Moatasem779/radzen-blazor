using Microsoft.AspNetCore.Components;
using Radzen.Blazor.Rendering;
using System;
using System.Collections;
using System.Globalization;

namespace Radzen.Blazor
{
    /// <summary>
    /// Displays the appointments in a month timeline view in <see cref="RadzenScheduler{TItem}" />
    /// </summary>
    public partial class RadzenMonthTimelineView : SchedulerViewBase
    {
        /// <inheritdoc />
        public override string Icon => "view_comfy";

        /// <inheritdoc />
        [Parameter]
        public override string Text { get; set; } = "Month Timeline";

        /// <summary>
        /// Gets or sets the header format.
        /// </summary>
        [Parameter]
        public string HeaderFormat { get; set; } = "d";

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
        public override string Title
        {
            get => Scheduler?.CurrentDate.ToString("MMMM yyyy", Scheduler.Culture ?? CultureInfo.CurrentCulture) ?? "";
        }

        /// <inheritdoc />
        public override DateTime StartDate
        {
            get
            {
                return Scheduler?.CurrentDate.Date.StartOfMonth() ?? DateTime.Today.StartOfMonth();
            }
        }

        /// <inheritdoc />
        public override DateTime EndDate
        {
            get
            {
                return StartDate.AddMonths(1);
            }
        }

        /// <inheritdoc />
        public override DateTime Next()
        {
            return Scheduler?.CurrentDate.Date.StartOfMonth().AddMonths(1) ?? DateTime.Today.StartOfMonth().AddMonths(1);
        }

        /// <inheritdoc />
        public override DateTime Prev()
        {
            return Scheduler?.CurrentDate.Date.StartOfMonth().AddMonths(-1) ?? DateTime.Today.StartOfMonth().AddMonths(-1);
        }
    }
}
