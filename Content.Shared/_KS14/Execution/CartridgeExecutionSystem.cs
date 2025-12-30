// SPDX-FileCopyrightText: 2025 Gerkada
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Weapons.Ranged.Components;
using Content.Shared.Projectiles;
using Content.Shared.Weapons.Ranged;
using Content.Shared.Weapons.Ranged.Systems;
using Robust.Shared.Prototypes;

namespace Content.Shared._KS14.Execution;

/// <summary>
/// Handles the GunExecutedEvent for cartridge-based ammunition.
/// Populates the damage specifier and marks the cartridge as spent.
/// </summary>
public sealed class CartridgeExecutionSystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly IComponentFactory _componentFactory = default!;
    [Dependency] private readonly SharedAppearanceSystem _appearanceSystem = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<CartridgeAmmoComponent, GunExecutedEvent>(OnCartridgeExecuted);
    }

    private void OnCartridgeExecuted(EntityUid uid, CartridgeAmmoComponent component, ref GunExecutedEvent args)
    {
        if (component.Spent)
            return;

        if (_prototypeManager.TryIndex(component.Prototype, out EntityPrototype? proto) &&
            proto.TryGetComponent<ProjectileComponent>(out var projectile, _componentFactory))
        {
            args.Damage = projectile.Damage;
        }

        component.Spent = true;
        _appearanceSystem.SetData(uid, AmmoVisuals.Spent, true);
        Dirty(uid, component);
    }
}
