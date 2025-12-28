using Content.Server.Disposal.Unit;
using Content.Shared.Disposal.Components;
using Content.Server.Popups;
using Content.Server.Power.EntitySystems;
using Content.Server.Power.Components;
using Content.Shared.Anomaly.Components;
using Content.Shared.Containers.ItemSlots;
using Content.Shared.Weapons.Melee.Events;
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
        // Subscribe to MeleeHitEvent
        SubscribeLocalEvent<GorillaGauntletComponent, MeleeHitEvent>(OnMeleeHit);
    }

    private void OnMeleeHit(Entity<GorillaGauntletComponent> ent, ref MeleeHitEvent args)
    {
        // Basic Validation
        if (args.Handled || args.HitEntities.Count == 0)
            return;

        var user = args.User;
        // We only care about the first thing hit (you can't shove 2 anomalies at once)
        var target = args.HitEntities.First();

        // Check target
        if (!TryComp<GorillaDisposalComponent>(target, out _))
            return;

        // Check for Core in Gauntlet
        var slotId = "core_slot";
        if (!_itemSlots.TryGetSlot(ent.Owner, slotId, out var slot) || slot.Item is not { } coreItem)
        {
            _popup.PopupEntity(Loc.GetString("gorilla-shove-gauntlet-not-active"), user, user);
            return;
        }

        // Try to use charge
        if (!_battery.TryUseCharge(coreItem, 100))
        {
            _popup.PopupEntity(Loc.GetString("gorilla-shove-gauntlet-not-active"), user, user);
            return;
        }

        // Find nearby disposal unit
        var disposals = _lookup.GetEntitiesInRange<DisposalUnitComponent>(Transform(target).Coordinates, 1.5f);
        var targetDisposal = disposals.FirstOrDefault();

        if (targetDisposal == null)
        {
            RefundCharge(coreItem, 100);
            _popup.PopupEntity(Loc.GetString("gorilla-shove-not-disposals"), user, user);
            return;
        }

        // Try shove
        if (!_disposals.TryInsert(targetDisposal.Owner, target, user))
        {
            RefundCharge(coreItem, 100);
            return;
        }

        // Success
        args.Handled = true;
    }

    private void RefundCharge(EntityUid uid, float amount)
    {
        if (TryComp<BatteryComponent>(uid, out var component))
        {
            _battery.SetCharge(uid, component.CurrentCharge + amount, component);
        }
    }
}
