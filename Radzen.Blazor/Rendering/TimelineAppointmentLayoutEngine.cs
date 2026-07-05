using System;
using System.Collections.Generic;
using System.Linq;

namespace Radzen.Blazor.Rendering
{
    /// <summary>
    /// Represents an appointment placed in the timeline view.
    /// </summary>
    public class RenderedTimelineAppointment
    {
        /// <summary>
        /// Gets or sets the target appointment data.
        /// </summary>
        public AppointmentData Appointment { get; set; } = null!;

        /// <summary>
        /// Gets or sets the left percentage position.
        /// </summary>
        public double Left { get; set; }

        /// <summary>
        /// Gets or sets the width percentage.
        /// </summary>
        public double Width { get; set; }

        /// <summary>
        /// Gets or sets the sub-lane index within the resource row.
        /// </summary>
        public int SubLane { get; set; }
    }

    /// <summary>
    /// Computes the horizontal and vertical positioning of appointments inside timeline lanes.
    /// </summary>
    public static class TimelineAppointmentLayoutEngine
    {
        /// <summary>
        /// Computes sub-lane stacking and coordinates for a list of appointments.
        /// </summary>
        public static List<RenderedTimelineAppointment> LayoutAppointments(
            IEnumerable<AppointmentData> appointments,
            DateTime viewStart,
            DateTime viewEnd,
            double minWidth = 0.0,
            bool snapToDays = false)
        {
            var list = new List<RenderedTimelineAppointment>();
            var sorted = appointments
                .OrderBy(a => a.Start)
                .ThenByDescending(a => a.End)
                .ToList();

            var subLaneEnds = new List<double>();

            foreach (var app in sorted)
            {
                var start = app.Start;
                var end = app.End;

                if (snapToDays)
                {
                    start = start.Date;
                    end = end.TimeOfDay == TimeSpan.Zero ? end.Date : end.Date.AddDays(1);
                }

                var coords = TimelineCoordinateCalculator.CalculateLeftAndWidth(
                    start,
                    end,
                    viewStart,
                    viewEnd);

                var left = coords.Left;
                var width = coords.Width;

                if (width < minWidth)
                {
                    width = minWidth;
                }

                // Prevent right-overflow by shifting left if needed
                if (left + width > 100.0)
                {
                    left = 100.0 - width;
                    if (left < 0.0) left = 0.0;
                }

                int subLaneIndex = -1;
                for (int i = 0; i < subLaneEnds.Count; i++)
                {
                    // Allow placement if start is after the sublane end (with safety margin)
                    if (left >= subLaneEnds[i] - 0.01)
                    {
                        subLaneIndex = i;
                        break;
                    }
                }

                if (subLaneIndex == -1)
                {
                    subLaneIndex = subLaneEnds.Count;
                    subLaneEnds.Add(left + width);
                }
                else
                {
                    subLaneEnds[subLaneIndex] = left + width;
                }

                list.Add(new RenderedTimelineAppointment
                {
                    Appointment = app,
                    Left = left,
                    Width = width,
                    SubLane = subLaneIndex
                });
            }

            return list;
        }
    }
}
