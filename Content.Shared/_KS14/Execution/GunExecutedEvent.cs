// SPDX-FileCopyrightText: 2025 Gerkada
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Damage;

namespace Content.Shared._KS14.Execution;

/// <summary>
/// Raised on an ammo entity when it is consumed by the GunExecutionSystem.
/// Systems that manage specific ammo types should subscribe to this and
/// populate the Damage field.
/// </summary>
[ByRefEvent]
public record struct GunExecutedEvent(
    EntityUid User,
    EntityUid Target,
    DamageSpecifier? Damage = null,
    string? MainDamageType = null);
