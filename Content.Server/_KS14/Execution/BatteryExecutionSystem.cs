// SPDX-FileCopyrightText: 2025 Gerkada
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared._KS14.Execution;
using Content.Shared.Weapons.Ranged.Components;
using Content.Server.Power.Components;
using Content.Server.Power.EntitySystems;
using Content.Shared.Damage;
using Content.Shared.Damage.Prototypes;
using Content.Shared.Projectiles;
using Robust.Shared.Prototypes;

namespace Content.Server._KS14.Execution;

/// <summary>
/// Server-side handler for GunExecutedEvent on battery-powered weapons.
/// </summary>
public sealed class BatteryExecutionSystem : SharedBatteryExecutionSystem
{
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly BatterySystem _batterySystem = default!;
    [Dependency] private readonly IComponentFactory _componentFactory = default!;

    private const string HeatDamageType = "Heat";

    public override void Initialize()
    {
        base.Initialize();
        // We subscribe on the weapon, not the battery, because the damage info is on the weapon's provider.
        SubscribeLocalEvent<HitscanBatteryAmmoProviderComponent, GunExecutedEvent>(OnHitscanBatteryExecuted);
        SubscribeLocalEvent<ProjectileBatteryAmmoProviderComponent, GunExecutedEvent>(OnProjectileBatteryExecuted);
    }

    private void OnHitscanBatteryExecuted(EntityUid uid, HitscanBatteryAmmoProviderComponent component, ref GunExecutedEvent args)
    {
        if (_batterySystem.TryUseCharge(uid, component.FireCost))
        {
            args.Damage = new DamageSpecifier(_prototypeManager.Index<DamageTypePrototype>(HeatDamageType), 100);
            args.MainDamageType = HeatDamageType;
        }
    }

    private void OnProjectileBatteryExecuted(EntityUid uid, ProjectileBatteryAmmoProviderComponent component, ref GunExecutedEvent args)
    {
        if (_batterySystem.TryUseCharge(uid, component.FireCost))
        {
            if (_prototypeManager.TryIndex(component.Prototype, out var proto) &&
                proto.TryGetComponent<ProjectileComponent>(out var projectile, _componentFactory))
            {
                args.Damage = projectile.Damage;
            }
        }
    }
}
