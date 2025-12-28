namespace Content.Shared._KS14.Anomaly.Events;

[ByRefEvent]
public record struct AnomalyShoveEvent(EntityUid User, EntityUid Target, EntityUid Gauntlet);
