using Microsoft.AspNetCore.Components;
using System;
using System.Collections;
using System.Globalization;

namespace Radzen.Blazor
{
    /// <summary>
    /// Displays the appointments in a day timeline view in <see cref="RadzenScheduler{TItem}" />
    /// </summary>
    public partial class RadzenDayTimelineView : SchedulerViewBase
    {
        /// <inheritdoc />
        public override string Icon => "view_timeline";

        /// <inheritdoc />
        [Parameter]
        public override string Text { get; set; } = "Day Timeline";

        /// <summary>
        /// Gets or sets the time format.
        /// </summary>
        [Parameter]
        public string TimeFormat { get; set; } = "h tt";

        /// <summary>
        /// Gets or sets the start time.
        /// </summary>
        [Parameter]
        public TimeSpan StartTime { get; set; } = TimeSpan.FromHours(8);

        /// <summary>
        /// Gets or sets the end time.
        /// </summary>
        [Parameter]
        public TimeSpan EndTime { get; set; } = TimeSpan.FromHours(24);

        /// <summary>
        /// Gets or sets slot size in minutes.
        /// </summary>
        [Parameter]
        public int MinutesPerSlot { get; set; } = 30;

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
            get
            {
                var culture = Scheduler?.Culture ?? CultureInfo.CurrentCulture;
                return Scheduler?.CurrentDate.ToString(Scheduler.Culture.DateTimeFormat.ShortDatePattern ?? "d", culture) ?? "";
            }
        }

        /// <inheritdoc />
        public override DateTime StartDate
        {
            get
            {
                return Scheduler?.CurrentDate.Date.Add(StartTime) ?? DateTime.Today.Add(StartTime);
            }
        }

        /// <inheritdoc />
        public override DateTime EndDate
        {
            get
            {
                return Scheduler?.CurrentDate.Date.Add(EndTime) ?? DateTime.Today.Add(EndTime);
            }
        }

        /// <inheritdoc />
        public override DateTime Next()
        {
            return Scheduler?.CurrentDate.Date.AddDays(1) ?? DateTime.Today.AddDays(1);
        }

        /// <inheritdoc />
        public override DateTime Prev()
        {
            return Scheduler?.CurrentDate.Date.AddDays(-1) ?? DateTime.Today.AddDays(-1);
        }
    }
}
