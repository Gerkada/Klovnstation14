// SPDX-FileCopyrightText: 2024 username
// SPDX-FileCopyrightText: 2025 Aiden
// SPDX-FileCopyrightText: 2025 FrauZj
// SPDX-FileCopyrightText: 2025 FrauzJ
// SPDX-FileCopyrightText: 2025 LaCumbiaDelCoronavirus
// SPDX-FileCopyrightText: 2026 Gerkada
// SPDX-FileCopyrightText: 2026 github_actions[bot]
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Chat.Prototypes;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared._Goobstation.Emoting;

public abstract class SharedAnimatedEmotesSystem : EntitySystem
{
    // We don't need OnGetState anymore if we are using Events for animations.
}

/// <summary>
/// Event sent from Server to Client to trigger an animation immediately.
/// </summary>
[Serializable, NetSerializable]
public sealed class RequestEmoteAnimationEvent(NetEntity user, ProtoId<EmotePrototype> emoteId) : EntityEventArgs
{
    public NetEntity User = user;
    public ProtoId<EmotePrototype> EmoteId = emoteId;
}
