using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Radzen.Blazor;
using Radzen.Blazor.Rendering;
using Radzen;
using System;
using System.Collections.Generic;
using System.Globalization;
using Xunit;

namespace Radzen.Blazor.Tests
{
    /// <summary>
    /// Test suite for Scheduler Resource Timeline views, coordinate layout engines, and resource managers.
    /// </summary>
    public class SchedulerTimelineTests
    {
        class TestAppointment
        {
            public DateTime Start { get; set; }
            public DateTime End { get; set; }
            public string Text { get; set; } = string.Empty;
            public int RoomId { get; set; }
        }

        class Resource
        {
            public int Id { get; set; }
            public string Name { get; set; } = string.Empty;
        }

        /// <summary>
        /// Validates coordinate percentage mapping inside the Timeline Coordinate Calculator.
        /// </summary>
        [Fact]
        public void TimelineCoordinateCalculator_CalculatesCorrectPercentages()
        {
            var start = new DateTime(2024, 7, 5, 10, 0, 0);
            var end = new DateTime(2024, 7, 5, 12, 0, 0);
            var viewStart = new DateTime(2024, 7, 5, 8, 0, 0);
            var viewEnd = new DateTime(2024, 7, 5, 16, 0, 0);

            var (left, width) = TimelineCoordinateCalculator.CalculateLeftAndWidth(start, end, viewStart, viewEnd);

            // Total view span: 8 hours. Start offset: 2 hours (25%). Duration: 2 hours (25%).
            Assert.Equal(25.0, left);
            Assert.Equal(25.0, width);
        }

        /// <summary>
        /// Validates appointment overlap resolving inside the Timeline Appointment Layout Engine.
        /// </summary>
        [Fact]
        public void TimelineAppointmentLayoutEngine_SolvesOverlapsIntoSubLanes()
        {
            var viewStart = new DateTime(2024, 7, 5, 8, 0, 0);
            var viewEnd = new DateTime(2024, 7, 5, 16, 0, 0);

            var appointments = new List<AppointmentData>
            {
                new AppointmentData { Start = new DateTime(2024, 7, 5, 9, 0, 0), End = new DateTime(2024, 7, 5, 11, 0, 0) },
                new AppointmentData { Start = new DateTime(2024, 7, 5, 10, 0, 0), End = new DateTime(2024, 7, 5, 12, 0, 0) },
                new AppointmentData { Start = new DateTime(2024, 7, 5, 13, 0, 0), End = new DateTime(2024, 7, 5, 15, 0, 0) }
            };

            var layout = TimelineAppointmentLayoutEngine.LayoutAppointments(appointments, viewStart, viewEnd);

            Assert.Equal(3, layout.Count);
            // The first two overlap, so they should be on different sub-lanes
            Assert.NotEqual(layout[0].SubLane, layout[1].SubLane);
            // The third is after the first two, so it should be able to reuse SubLane 0
            Assert.Equal(0, layout[2].SubLane);
        }

        /// <summary>
        /// Validates resource model parsing inside the Timeline Resource Manager.
        /// </summary>
        [Fact]
        public void TimelineResourceManager_ResolvesResourceGroups()
        {
            var resources = new List<Resource>
            {
                new Resource { Id = 1, Name = "Room A" },
                new Resource { Id = 2, Name = "Room B" }
            };

            var groups = TimelineResourceManager.ResolveResourceGroups(resources, nameof(Resource.Name), nameof(Resource.Id));

            Assert.Equal(2, groups.Count);
            Assert.Equal(1, groups[0].Key);
            Assert.Equal("Room A", groups[0].Text);
        }

        /// <summary>
        /// Validates linking of appointments to target resource keys inside the Timeline Resource Manager.
        /// </summary>
        [Fact]
        public void TimelineResourceManager_MatchesAppointmentsToResources()
        {
            var app = new AppointmentData
            {
                Data = new TestAppointment { RoomId = 2, Text = "Design Sprint" }
            };

            var matches = TimelineResourceManager.AppBelongsToResource(app, nameof(TestAppointment.RoomId), 2);
            var misses = TimelineResourceManager.AppBelongsToResource(app, nameof(TestAppointment.RoomId), 1);

            Assert.True(matches);
            Assert.False(misses);
        }

        /// <summary>
        /// Validates component rendering and view type selection under the cascading context.
        /// </summary>
        [Fact]
        public void SchedulerTimelineViews_RenderCorrectly()
        {
            using var ctx = new TestContext();
            ctx.JSInterop.Mode = JSRuntimeMode.Loose;
            ctx.Services.AddScoped<DialogService>();
            ctx.JSInterop.Setup<Rect>("Radzen.createScheduler", _ => true)
                .SetResult(new Rect { Left = 0, Top = 0, Width = 800, Height = 600 });
            ctx.JSInterop.Setup<Rect>("Radzen.createResizable", _ => true)
                .SetResult(new Rect { Left = 0, Top = 0, Width = 800, Height = 600 });

            var appointments = new List<TestAppointment>
            {
                new() { Start = new DateTime(2024, 7, 5, 9, 0, 0), End = new DateTime(2024, 7, 5, 11, 0, 0), Text = "Meeting A", RoomId = 1 }
            };

            var resources = new List<Resource>
            {
                new Resource { Id = 1, Name = "Room 1" }
            };

            var cut = ctx.RenderComponent<RadzenScheduler<TestAppointment>>(p =>
            {
                p.Add(x => x.Date, new DateTime(2024, 7, 5));
                p.Add(x => x.Data, appointments);
                p.Add(x => x.StartProperty, nameof(TestAppointment.Start));
                p.Add(x => x.EndProperty, nameof(TestAppointment.End));
                p.Add(x => x.TextProperty, nameof(TestAppointment.Text));
                p.Add(x => x.SelectedIndex, 0);
                p.AddChildContent<RadzenDayTimelineView>(v =>
                {
                    v.Add(x => x.Resources, resources);
                    v.Add(x => x.ResourceProperty, nameof(TestAppointment.RoomId));
                    v.Add(x => x.ResourceTextProperty, nameof(Resource.Name));
                    v.Add(x => x.ResourceValueProperty, nameof(Resource.Id));
                });
            });

            var view = Assert.IsType<RadzenDayTimelineView>(cut.Instance.SelectedView);
            Assert.NotNull(view);
            Assert.Equal("Day Timeline", view.Text);
        }
    }
}
