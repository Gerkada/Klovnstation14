// SPDX-FileCopyrightText: 2025 FrauZj
// SPDX-FileCopyrightText: 2026 Gerkada
// SPDX-FileCopyrightText: 2026 github_actions[bot]
//
// SPDX-License-Identifier: MIT

using Content.Shared._White.Animations;
using Robust.Shared.Player;

namespace Content.Server._White.Animations;

public sealed class FlipOnHitSystem : SharedFlipOnHitSystem
{
    protected override void PlayAnimation(EntityUid user)
    {
        var filter = Filter.Pvs(user, entityManager: EntityManager);

        // FIX: Do NOT remove the player.
        // We want the server to force the animation on the client to guarantee it plays,
        // even if client-side prediction failed.
        // if (TryComp<ActorComponent>(user, out var actor))
        //    filter.RemovePlayer(actor.PlayerSession);

        RaiseNetworkEvent(new FlipOnHitEvent(GetNetEntity(user)), filter);
    }
}
