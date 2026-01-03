// SPDX-FileCopyrightText: 2024 Piras314
// SPDX-FileCopyrightText: 2024 username
// SPDX-FileCopyrightText: 2024 whateverusername0 <whateveremail>
// SPDX-FileCopyrightText: 2025 Aiden
// SPDX-FileCopyrightText: 2025 FrauzJ
// SPDX-FileCopyrightText: 2025 Misandry
// SPDX-FileCopyrightText: 2025 github_actions[bot]
// SPDX-FileCopyrightText: 2025 gus
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared._Goobstation.Emoting;
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
        // Raises the network event to clients so they play the animation
        RaiseNetworkEvent(new RequestEmoteAnimationEvent(GetNetEntity(uid), protoId));
    }
}
