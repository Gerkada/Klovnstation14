// SPDX-FileCopyrightText: 2025 Gerkada
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Projectiles;

namespace Content.Shared._KS14.Execution;

/// <summary>
/// Handles the GunExecutedEvent for projectile-based ammunition,
/// such as bullets provided by a revolver.
/// </summary>
public sealed class ProjectileExecutionSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<ProjectileComponent, GunExecutedEvent>(OnProjectileExecuted);
    }

    private void OnProjectileExecuted(EntityUid uid, ProjectileComponent component, ref GunExecutedEvent args)
    {
        args.Damage = component.Damage;

        // The projectile entity is temporary and should be deleted.
        Del(uid);
    }
}
