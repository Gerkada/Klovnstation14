// SPDX-FileCopyrightText: 2025 Gerkada
// SPDX-FileCopyrightText: 2025 github_actions[bot]
//
// SPDX-License-Identifier: MIT

namespace Content.Shared._KS14.Anomaly.Events;

[ByRefEvent]
public record struct AnomalyShoveEvent(EntityUid User, EntityUid Target, EntityUid Gauntlet);
