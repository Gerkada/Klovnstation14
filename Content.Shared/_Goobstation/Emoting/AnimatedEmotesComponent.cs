// SPDX-FileCopyrightText: 2024 username
// SPDX-FileCopyrightText: 2025 FrauZj
// SPDX-FileCopyrightText: 2025 FrauzJ
// SPDX-FileCopyrightText: 2025 LaCumbiaDelCoronavirus
// SPDX-FileCopyrightText: 2026 Gerkada
// SPDX-FileCopyrightText: 2026 github_actions[bot]
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

namespace Content.Shared._Goobstation.Emoting;

// --- EVENT DEFINITIONS (Used by Prototypes) ---
[Serializable, NetSerializable, DataDefinition] public sealed partial class AnimationFlipEmoteEvent : EntityEventArgs { }
[Serializable, NetSerializable, DataDefinition] public sealed partial class AnimationSpinEmoteEvent : EntityEventArgs { }
[Serializable, NetSerializable, DataDefinition] public sealed partial class AnimationJumpEmoteEvent : EntityEventArgs { }
[Serializable, NetSerializable, DataDefinition] public sealed partial class AnimationTweakEmoteEvent : EntityEventArgs { }
[Serializable, NetSerializable, DataDefinition] public sealed partial class AnimationFlexEmoteEvent : EntityEventArgs { }

// --- COMPONENT DEFINITION ---
/// <summary>
///     Marks entities that can use animated emotes.
///     The actual animation logic is handled via Network Events, not component state.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class AnimatedEmotesComponent : Component
{
    // We don't need to store the 'Emote' here anymore.
    // The Server sends a transient event, and the Client plays it immediately.
}
