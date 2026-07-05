using System;

namespace Radzen.Blazor.Rendering
{
    /// <summary>
    /// Computes relative coordinates (Left/Width percentages) for timeline views.
    /// </summary>
    public static class TimelineCoordinateCalculator
    {
        /// <summary>
        /// Calculates the percentage Left offset and Width for an appointment in the timeline.
        /// </summary>
        public static (double Left, double Width) CalculateLeftAndWidth(
            DateTime start,
            DateTime end,
            DateTime viewStart,
            DateTime viewEnd)
        {
            var totalMs = (viewEnd - viewStart).TotalMilliseconds;
            if (totalMs <= 0)
            {
                return (0.0, 0.0);
            }

            var startMs = (start - viewStart).TotalMilliseconds;
            var endMs = (end - viewStart).TotalMilliseconds;

            // Clamp coordinates to view boundaries
            if (startMs < 0) startMs = 0;
            if (endMs > totalMs) endMs = totalMs;
            if (endMs < startMs) endMs = startMs;

            var left = (startMs / totalMs) * 100.0;
            var width = ((endMs - startMs) / totalMs) * 100.0;

            // Minimum visual width
            if (width < 1.0)
            {
                width = 1.0;
            }

            return (left, width);
        }
    }
}
