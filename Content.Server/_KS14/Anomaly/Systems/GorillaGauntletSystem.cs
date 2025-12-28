using Content.Server.Disposal.Unit;
using Content.Shared.Disposal.Components;
using Content.Server.Popups;
using Content.Server.Power.EntitySystems;
using Content.Shared.Anomaly.Components;
using Content.Shared.Containers.ItemSlots;
using Content.Shared.Weapons.Melee.Components;
using Content.Shared._KS14.Anomaly.Components;
using System.Linq;

namespace Content.Server._KS14.Anomaly.Systems;

public sealed class GorillaGauntletSystem : EntitySystem
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

        // Is target a Gorilla Disposal Anomaly?
        if (!TryComp<GorillaDisposalComponent>(args.Target, out _))
            return;

        // Check for Core in Gauntlet
        if (!_itemSlots.TryGetSlot(ent.Owner, ent.Comp.CoreSlotId, out var slot) || !slot.HasItem)
        {
            _popup.PopupEntity(Loc.GetString("gorilla-shove-gauntlet-not-active"), user, user);
            return;
        }

        // Find nearby disposal unit
        // FIXED: Now uses the standard DisposalUnitComponent from Shared
        var disposals = _lookup.GetEntitiesInRange<DisposalUnitComponent>(Transform(args.Target).Coordinates, 1.5f);
        var targetDisposal = disposals.FirstOrDefault();

        if (targetDisposal == null)
        {
            _popup.PopupEntity(Loc.GetString("gorilla-shove-not-disposals"), user, user);
            return;
        }

        // Try shove
        // Note: Owner check is redundant if targetDisposal is not null, but harmless.
        if (!_disposals.TryInsert(targetDisposal.Owner, args.Target, user))
            return;

        // Success
        args.Handled = true;
        args.Cancelled = true; // Stop the throw

        // Deduct charge
        if (slot.Item is { } item)
            _battery.TryUseCharge(item, 100);
    }
}
