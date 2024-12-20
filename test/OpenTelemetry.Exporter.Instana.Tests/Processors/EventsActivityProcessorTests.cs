// Copyright The OpenTelemetry Authors
// SPDX-License-Identifier: Apache-2.0

using System.Diagnostics;
using OpenTelemetry.Exporter.Instana.Implementation;
using OpenTelemetry.Exporter.Instana.Implementation.Processors;
using Xunit;

namespace OpenTelemetry.Exporter.Instana.Tests.Processors;

public class EventsActivityProcessorTests
{
    private readonly EventsActivityProcessor eventsActivityProcessor = new();

    [Fact]
    public async Task ProcessAsync()
    {
        var activityTagsCollection = new ActivityTagsCollection { new KeyValuePair<string, object?>("eventTagKey", "eventTagValue") };
        var activityEvent = new ActivityEvent(
            "testActivityEvent",
            DateTimeOffset.MinValue,
            activityTagsCollection);

        var activityTagsCollection2 = new ActivityTagsCollection { new KeyValuePair<string, object?>("eventTagKey2", "eventTagValue2") };
        var activityEvent2 = new ActivityEvent(
            "testActivityEvent2",
            DateTimeOffset.MaxValue,
            activityTagsCollection2);

        var activity = new Activity("testOperationName");
        activity.AddEvent(activityEvent);
        activity.AddEvent(activityEvent2);
        var instanaSpan = new InstanaSpan() { TransformInfo = new Implementation.InstanaSpanTransformInfo() };
        if (this.eventsActivityProcessor != null)
        {
            await this.eventsActivityProcessor.ProcessAsync(activity, instanaSpan);
        }

        Assert.NotNull(instanaSpan.Data?.Events);
        Assert.True(instanaSpan.Ec == 0);
        Assert.True(instanaSpan.Data.Events.Count == 2);
        Assert.True(instanaSpan.Data.Events[0].Name == "testActivityEvent");
        Assert.True(instanaSpan.Data.Events[0].Ts > 0);
        Assert.NotNull(instanaSpan.Data?.Events[0]?.Tags);
        var eventTagValue = string.Empty;
        _ = instanaSpan.Data.Events[0].Tags?.TryGetValue("eventTagKey", out eventTagValue);
        Assert.True(eventTagValue == "eventTagValue");
        Assert.True(instanaSpan.Data.Events[1].Name == "testActivityEvent2");
        Assert.True(instanaSpan.Data.Events[1].Ts > 0);
        Assert.NotNull(instanaSpan.Data?.Events[1]?.Tags);
        eventTagValue = string.Empty;
        _ = instanaSpan.Data.Events[1].Tags?.TryGetValue("eventTagKey2", out eventTagValue);
        Assert.True(eventTagValue == "eventTagValue2");
    }
}
