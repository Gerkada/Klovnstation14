using Content.Shared.Emoting;
using Content.Shared.Chat.Prototypes; // Add this
using Robust.Shared.Prototypes;       // Add this
using Robust.Shared.Serialization;    // Add this

namespace Content.Shared._Goobstation.Emoting;

public abstract class SharedAnimatedEmotesSystem : EntitySystem
{
    // You can keep the old GetState logic if you want,
    // but we are bypassing it for the animation trigger now.
}

// FIX: New Network Event
[Serializable, NetSerializable]
public sealed class RequestEmoteAnimationEvent(NetEntity user, ProtoId<EmotePrototype> emoteId) : EntityEventArgs
{
    public NetEntity User = user;
    public ProtoId<EmotePrototype> EmoteId = emoteId;
}
