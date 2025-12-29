// SPDX-FileCopyrightText: 2024 Celene <4323352+CuteMoonGod@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Celene <maurice_riepert94@web.de>
// SPDX-FileCopyrightText: 2024 Mervill <mervills.email@gmail.com>
// SPDX-FileCopyrightText: 2024 Plykiya <58439124+Plykiya@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Scribbles0 <91828755+Scribbles0@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aviu00 <93730715+Aviu00@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2025 Solstice <solsticeofthewinter@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.ActionBlocker;
using Content.Shared.Chat;
using Content.Shared.CombatMode;
using Content.Shared.Damage;
using Content.Shared.Database;
using Content.Shared.DoAfter;
using Content.Shared.IdentityManagement;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.Popups;
using Content.Shared.Verbs;
using Content.Shared.Entry;
using Content.Shared.Interaction.Events;
using Content.Shared.Weapons.Ranged.Components;
using Content.Shared.Weapons.Ranged.Systems;
using Content.Shared.Weapons.Ranged.Events;
using Content.Shared.Weapons.Ranged;
using Content.Shared.CombatMode.Pacification;
using Content.Shared.Projectiles;
using Content.Shared.Execution;
using Content.Shared.Camera;
using Robust.Shared.Player;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;
using Robust.Shared.Containers;
using Content.Shared.Containers.ItemSlots;
using System.Diagnostics;
using System.Linq;
using System.Net.Http.Headers;
using System.Numerics;

namespace Content.Shared._KS14.Execution;

/// <summary>
///     verb for executing with guns
/// </summary>
public sealed class SharedGunExecutionSystem : EntitySystem
{
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly SharedSuicideSystem _suicide = default!;
    [Dependency] private readonly SharedCombatModeSystem _combat = default!;
    [Dependency] private readonly SharedExecutionSystem _execution = default!;
    [Dependency] private readonly SharedGunSystem _gunSystem = default!;
    [Dependency] private readonly IComponentFactory _componentFactory = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly SharedAppearanceSystem _appearanceSystem = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly SharedCameraRecoilSystem _recoil = default!;
    [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly SharedContainerSystem _container = default!;
    [Dependency] private readonly ItemSlotsSystem _itemSlots = default!;

    private const float GunExecutionTime = 4.0f;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<GunComponent, GetVerbsEvent<UtilityVerb>>(OnGetInteractionVerbsGun);
        SubscribeLocalEvent<GunComponent, ExecutionDoAfterEvent>(OnDoafterGun);

    }

    private void OnGetInteractionVerbsGun(EntityUid uid, GunComponent component, GetVerbsEvent<UtilityVerb> args)
    {
        if (args.Hands == null || args.Using == null || !args.CanAccess || !args.CanInteract)
            return;

        var attacker = args.User;
        var weapon = args.Using!.Value;
        var victim = args.Target;
        var gunexecutiontime = component.GunExecutionTime;

        if (!HasComp<GunExecutionWhitelistComponent>(weapon)
            || !CanExecuteWithGun(weapon, victim, attacker))
            return;

        UtilityVerb verb = new()
        {
            Act = () => TryStartGunExecutionDoafter(weapon, victim, attacker, gunexecutiontime),
            Impact = LogImpact.High,
            Text = Loc.GetString("execution-verb-name"),
            Message = Loc.GetString("execution-verb-message"),
        };

        args.Verbs.Add(verb);
    }

    private bool CanExecuteWithGun(EntityUid weapon, EntityUid victim, EntityUid user)
    {
        if (!_execution.CanBeExecuted(victim, user)
            || TryComp<GunComponent>(weapon, out var gun)
            && !_gunSystem.CanShoot(gun))
            return false;

        return true;
    }

    private void TryStartGunExecutionDoafter(EntityUid weapon, EntityUid victim, EntityUid attacker, float gunexecutiontime)
    {
        if (!CanExecuteWithGun(weapon, victim, attacker))
            return;

        if (attacker == victim)
        {
            _execution.ShowExecutionInternalPopup("suicide-popup-gun-initial-internal", attacker, victim, weapon);
            _execution.ShowExecutionExternalPopup("suicide-popup-gun-initial-external", attacker, victim, weapon);
        }
        else
        {
            _execution.ShowExecutionInternalPopup("execution-popup-gun-initial-internal", attacker, victim, weapon);
            _execution.ShowExecutionExternalPopup("execution-popup-gun-initial-external", attacker, victim, weapon);
        }

        var doAfter =
            new DoAfterArgs(EntityManager, attacker, gunexecutiontime, new ExecutionDoAfterEvent(), weapon, target: victim, used: weapon)
            {
                BreakOnMove = true,
                BreakOnDamage = true,
                NeedHand = true,
            };

        _doAfter.TryStartDoAfter(doAfter);
    }

    private string GetDamage(DamageSpecifier damage, string? mainDamageType)
    {
        // Default fallback if nothing valid found
        mainDamageType ??= "Blunt";

        if (damage == null || damage.DamageDict.Count == 0)
            return mainDamageType;

        var filtered = damage.DamageDict
            .Where(kv => !string.Equals(kv.Key, "Structural", StringComparison.OrdinalIgnoreCase));

        if (filtered.Any())
        {
            mainDamageType = filtered.Aggregate((a, b) => a.Value > b.Value ? a : b).Key;
        }

        return mainDamageType ?? "Blunt";
    }

    private void OnDoafterGun(EntityUid uid, GunComponent component, DoAfterEvent args)
    {
        if (args.Handled
            || args.Cancelled
            || args.Used == null
            || args.Target == null
            || !TryComp<GunComponent>(uid, out var guncomp))
            return;

        var attacker = args.User;
        var victim = args.Target.Value;
        var weapon = args.Used.Value;

        // Get the direction for the recoil
        var direction = Vector2.Zero;
        var attackerXform = Transform(attacker);
        var victimXform = Transform(victim);

        // Use SharedTransformSystem instead of obsolete WorldPosition
        var diff = _transform.GetWorldPosition(victimXform) - _transform.GetWorldPosition(attackerXform);

        if (diff != Vector2.Zero)
            direction = -diff.Normalized(); // recoil opposite of shot


        if (!CanExecuteWithGun(weapon, victim, attacker)
            || !TryComp<DamageableComponent>(victim, out var damageableComponent))
            return;

        // Server only block

        // We only want to process the ammo removal and dirtying on the server.
        // If the client runs this, it predicts the ammo is still there (since it can't delete entities),
        // causing a visual desync where the ammo bar refuses to go down.
        if (_net.IsServer)
        {
            // Take some ammunition for the shot (one bullet)
            var fromCoordinates = Transform(attacker).Coordinates;
            var ev = new TakeAmmoEvent(1, new List<(EntityUid? Entity, IShootable Shootable)>(), fromCoordinates, attacker);
            RaiseLocalEvent(weapon, ev);

            // Dirty everything involved in ammo counting.

            // Dirty the Gun
            Dirty(weapon, guncomp);

            // Dirty the Magazine (if applicable)
            if (TryComp<MagazineAmmoProviderComponent>(weapon, out var magProv))
            {
                Dirty(weapon, magProv);

                // Use ItemSlots to find the mag reliably instead of guessing container strings
                if (_itemSlots.TryGetSlot(weapon, "gun_magazine", out var slot) && slot.Item is { } magazine)
                {
                    if (TryComp<BallisticAmmoProviderComponent>(magazine, out var magBallistic))
                        Dirty(magazine, magBallistic);
                }
            }
            // Dirty Ballistic/Battery Providers
            else if (TryComp<BallisticAmmoProviderComponent>(weapon, out var ballisticComp))
            {
                Dirty(weapon, ballisticComp);
            }
            else if (TryComp<HitscanBatteryAmmoProviderComponent>(weapon, out var batteryComp))
            {
                Dirty(weapon, batteryComp);
            }

            // Empty check (server side)
            if (ev.Ammo.Count <= 0)
            {
                _audio.PlayPredicted(component.SoundEmpty, uid, attacker);
                _execution.ShowExecutionInternalPopup("execution-popup-gun-empty", attacker, victim, weapon);
                _execution.ShowExecutionExternalPopup("execution-popup-gun-empty", attacker, victim, weapon);
                return;
            }

            // Process Damage Logic (Still on Server)
            ProcessExecutionEffects(ev, component, uid, attacker, victim, damageableComponent, weapon);
        }
        else
        {
            // Client side

            // On the client, we just play the recoil prediction if applicable, but we do NOT touch ammo.
            // The ammo update will arrive via the Server's Dirty() call.
            if (direction != Vector2.Zero && _timing.IsFirstTimePredicted)
                _recoil.KickCamera(attacker, direction);
        }

        args.Handled = true;
    }

    // Helper method to keep OnDoafter clean and avoid duplicate logic
    private void ProcessExecutionEffects(TakeAmmoEvent ev, GunComponent component, EntityUid uid,
        EntityUid attacker, EntityUid victim, DamageableComponent damageableComponent, EntityUid weapon)
    {
        var damage = new DamageSpecifier();
        string? mainDamageType = null;
        var ammoUid = ev.Ammo[0].Entity;

        switch (ev.Ammo[0].Shootable)
        {
            case CartridgeAmmoComponent cartridge:
                {
                    if (cartridge.Spent)
                    {
                        _audio.PlayPredicted(component.SoundEmpty, uid, attacker);
                        _execution.ShowExecutionInternalPopup("execution-popup-gun-empty", attacker, victim, weapon);
                        _execution.ShowExecutionExternalPopup("execution-popup-gun-empty", attacker, victim, weapon);
                        return;
                    }

                    var prototype = _prototypeManager.Index<EntityPrototype>(cartridge.Prototype);
                    prototype.TryGetComponent<ProjectileComponent>(out var projectileA, _componentFactory);

                    if (projectileA != null)
                    {
                        damage = projectileA.Damage;
                        mainDamageType = GetDamage(damage, mainDamageType);
                    }

                    if (damage.GetTotal() < 5)
                    {
                        _audio.PlayPredicted(component.SoundEmpty, uid, attacker);
                        _execution.ShowExecutionInternalPopup("execution-popup-gun-empty", attacker, victim, weapon);
                        _execution.ShowExecutionExternalPopup("execution-popup-gun-empty", attacker, victim, weapon);
                        return;
                    }

                    cartridge.Spent = true;
                    _appearanceSystem.SetData(ammoUid!.Value, AmmoVisuals.Spent, true);
                    Dirty(ammoUid.Value, cartridge);
                    break;
                }
            case AmmoComponent:
                TryComp<ProjectileComponent>(ammoUid, out var projectileB);

                if (projectileB != null)
                {
                    damage = projectileB.Damage;
                    mainDamageType = GetDamage(damage, mainDamageType);
                }

                if (damage.GetTotal() < 5)
                {
                    _audio.PlayPredicted(component.SoundEmpty, uid, attacker);
                    _execution.ShowExecutionInternalPopup("execution-popup-gun-empty", attacker, victim, weapon);
                    _execution.ShowExecutionExternalPopup("execution-popup-gun-empty", attacker, victim, weapon);
                    return;
                }

                if (ammoUid != null)
                    Del(ammoUid);
                break;
        }

        if (HasComp<HitscanBatteryAmmoProviderComponent>(weapon))
            mainDamageType = "Heat";

        var prev = _combat.IsInCombatMode(attacker);
        _combat.SetInCombatMode(attacker, true);

        if (attacker == victim)
        {
            _execution.ShowExecutionInternalPopup("suicide-popup-gun-complete-internal", attacker, victim, weapon);
            _execution.ShowExecutionExternalPopup("suicide-popup-gun-complete-external", attacker, victim, weapon);
            _audio.PlayPredicted(component.SoundGunshot, uid, attacker);
            _suicide.ApplyLethalDamage((victim, damageableComponent), mainDamageType);
        }
        else
        {
            // Recoil is handled client-side in the else block of the main function,
            // but we can trigger it here for server-authoritative setups if needed.
            _execution.ShowExecutionInternalPopup("execution-popup-gun-complete-internal", attacker, victim, weapon);
            _execution.ShowExecutionExternalPopup("execution-popup-gun-complete-external", attacker, victim, weapon);
            _audio.PlayPredicted(component.SoundGunshot, uid, attacker);
            _suicide.ApplyLethalDamage((victim, damageableComponent), mainDamageType);
        }

        _combat.SetInCombatMode(attacker, prev);
    }
}
