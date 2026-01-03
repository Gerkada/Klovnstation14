// SPDX-FileCopyrightText: 2024 Piras314
// SPDX-FileCopyrightText: 2025 FrauZj
// SPDX-FileCopyrightText: 2025 FrauzJ
// SPDX-FileCopyrightText: 2025 LaCumbiaDelCoronavirus
// SPDX-FileCopyrightText: 2025 github_actions[bot]
// SPDX-FileCopyrightText: 2026 Gerkada
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared._Goobstation.Emoting;
using Content.Server.Chat.Systems;
using Content.Shared.Chat.Prototypes;
using Content.Shared.Chat;
using Robust.Shared.Prototypes;

namespace Content.Server._Goobstation.Emoting;

public sealed partial class AnimatedEmotesSystem : SharedAnimatedEmotesSystem
{
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<AnimatedEmotesComponent, EmoteEvent>(OnEmote);
    }

    private void OnEmote(EntityUid uid, AnimatedEmotesComponent component, ref EmoteEvent args)
    {
        PlayEmoteAnimation(uid, args.Emote.ID);
    }

    public void PlayEmoteAnimation(EntityUid uid, ProtoId<EmotePrototype> protoId)
    {
        // FIX: Raise a Network Event.
        // This guarantees the client receives it every time, allowing repeated animations.
        RaiseNetworkEvent(new RequestEmoteAnimationEvent(GetNetEntity(uid), protoId));
    }
}
