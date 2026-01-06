// SPDX-FileCopyrightText: 2026 Gerkada
//
// SPDX-License-Identifier: MPL-2.0

using Content.Server.GameTicking;
using Content.Server.GameTicking.Rules;
using Content.Server.GameTicking.Rules.Components;
using Content.Shared.GameTicking.Components;
using Robust.Server.GameObjects;
using Robust.Shared.Map;
using Robust.Shared.EntitySerialization.Systems;
using Robust.Shared.Utility;
using System;
using Content.Server.Chat.Systems;
using Robust.Shared.Localization;
using Robust.Shared.Maths;

namespace Content.Server._KS14.GameTicking.Rules;

[RegisterComponent]
public sealed partial class LoneERTRuleComponent : Component
{
}

public sealed class LoneERTSystem : GameRuleSystem<LoneERTRuleComponent>
{
    [Dependency] private readonly MapLoaderSystem _mapLoader = default!;
    [Dependency] private readonly IMapManager _mapManager = default!;
    [Dependency] private readonly MapSystem _mapSystem = default!;
    [Dependency] private readonly ChatSystem _chat = default!;

    protected override void Added(EntityUid uid, LoneERTRuleComponent component, GameRuleComponent gameRule, GameRuleAddedEvent args)
    {
        base.Added(uid, component, gameRule, args);

        if (!TryComp<LoadMapRuleComponent>(uid, out var mapRule)) return;

        if (mapRule.GridPath == null)
        {
            Log.Error($"LoneERTRule {uid} started but GridPath is null!");
            return;
        }
        var path = mapRule.GridPath.Value;

        try
        {
            if (_mapLoader.TryLoadMap(path, out var mapEntity, out var roots))
            {
                if (mapEntity.HasValue)
                {
                    var mapId = mapEntity.Value.Comp.MapId;

                    // 1. Initialize (Gravity/Atmos)
                    _mapSystem.InitializeMap(mapId);

                    // 2. Unpause (Time)
                    _mapManager.SetMapPaused(mapId, false);

                    Log.Info($"LoneERT Map {mapId} initialized and unpaused.");

                    // 3. Make an announcement
                    _chat.DispatchGlobalAnnouncement(
                        Loc.GetString("station-event-lone-ert-shuttle-incoming"),
                        playSound: true,
                        colorOverride: Color.Gold
                    );
                }
            }
            else
            {
                Log.Error($"Failed to load LoneERT map: {path}");
            }
        }
        catch (Exception e)
        {
            Log.Error($"Exception loading map: {e.Message}");
        }
    }
}
