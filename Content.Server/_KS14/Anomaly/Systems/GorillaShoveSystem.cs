// SPDX-FileCopyrightText: 2025 Gerkada
// SPDX-FileCopyrightText: 2025 github_actions[bot]
//
// SPDX-License-Identifier: MIT

using Content.Server.Disposal.Unit;
using Content.Shared.Disposal.Components;
using Content.Server.Popups;
using Content.Server.Power.EntitySystems;
using Content.Server.Power.Components;
using Content.Shared.Anomaly.Components;
using Content.Shared.Containers.ItemSlots;
using Content.Shared.Weapons.Melee.Components;
using Content.Shared._KS14.Anomaly.Components;
using System.Linq;

namespace Content.Server._KS14.Anomaly.Systems;

public sealed class GorillaShoveSystem : EntitySystem
{
    [Dependency] private readonly BatterySystem _battery = default!;
    [Dependency] private readonly DisposalUnitSystem _disposals = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly PopupSystem _popup = default!;
    [Dependency] private readonly ItemSlotsSystem _itemSlots = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<CorePoweredThrowerComponent, AttemptMeleeThrowOnHitEvent>(OnAttemptGorillaShove);
    }

    private void OnAttemptGorillaShove(Entity<CorePoweredThrowerComponent> ent, ref AttemptMeleeThrowOnHitEvent args)
    {
        // Basic validation
        if (args.Handled || args.User is not { } user)
            return;

        // Check target
        if (!TryComp<GorillaDisposalComponent>(args.Target, out _))
            return;

        // Check for Core in Gauntlet
        if (!_itemSlots.TryGetSlot(ent.Owner, ent.Comp.CoreSlotId, out var slot) || slot.Item is not { } coreItem)
        {
            _popup.PopupEntity(Loc.GetString("gorilla-shove-gauntlet-not-active"), user, user);
            return;
        }

        // Try to use charge FIRST.
        // If this fails, the gauntlet is dead, so we stop everything immediately.
        if (!_battery.TryUseCharge(coreItem, 100))
        {
            _popup.PopupEntity(Loc.GetString("gorilla-shove-gauntlet-not-active"), user, user);
            return;
        }

        // Find nearby disposal unit
        var disposals = _lookup.GetEntitiesInRange<DisposalUnitComponent>(Transform(args.Target).Coordinates, 1.5f);
        var targetDisposal = disposals.FirstOrDefault();

        if (targetDisposal == null)
        {
            // Refund the charge because we couldn't even try the action
            RefundCharge(coreItem, 100);
            _popup.PopupEntity(Loc.GetString("gorilla-shove-not-disposals"), user, user);
            return;
        }

        // Try shove
        if (!_disposals.TryInsert(targetDisposal.Owner, args.Target, user))
        {
            // Refund the charge if the insertion failed (e.g. unit full/pressurized)
            RefundCharge(coreItem, 100);
            return;
        }

        // Success
        args.Handled = true;
        args.Cancelled = true;
    }

    // Helper method to add charge back safely
    private void RefundCharge(EntityUid uid, float amount)
    {
        if (TryComp<BatteryComponent>(uid, out var component))
        {
            _battery.SetCharge(uid, component.CurrentCharge + amount, component);
        }
    }
}
