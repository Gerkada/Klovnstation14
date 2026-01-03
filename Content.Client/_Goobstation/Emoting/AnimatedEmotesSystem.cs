// SPDX-FileCopyrightText: 2024 username
// SPDX-FileCopyrightText: 2025 FrauZj
// SPDX-FileCopyrightText: 2025 FrauzJ
// SPDX-FileCopyrightText: 2025 LaCumbiaDelCoronavirus
// SPDX-FileCopyrightText: 2026 Gerkada
// SPDX-FileCopyrightText: 2026 github_actions[bot]
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Numerics;
using Content.Client.Animations;
using Content.Shared._Goobstation.Emoting;
using Content.Shared.Chat.Prototypes;
using Robust.Client.Animations;
using Robust.Client.GameObjects;
using Robust.Shared.Animations;
using Robust.Shared.Prototypes;

namespace Content.Client._Goobstation.Emoting;

public sealed partial class AnimatedEmotesSystem : SharedAnimatedEmotesSystem
{
    [Dependency] private readonly AnimationPlayerSystem _anim = default!;
    [Dependency] private readonly IPrototypeManager _prot = default!;

    public override void Initialize()
    {
        base.Initialize();

        // FIX: Subscribe to Network Event instead of ComponentHandleState
        SubscribeNetworkEvent<RequestEmoteAnimationEvent>(OnEmoteEvent);

        SubscribeLocalEvent<AnimatedEmotesComponent, AnimationFlipEmoteEvent>(OnFlip);
        SubscribeLocalEvent<AnimatedEmotesComponent, AnimationSpinEmoteEvent>(OnSpin);
        SubscribeLocalEvent<AnimatedEmotesComponent, AnimationJumpEmoteEvent>(OnJump);
    }

    private void OnEmoteEvent(RequestEmoteAnimationEvent args)
    {
        var uid = GetEntity(args.User);

        // Basic validation
        if (!TryComp(uid, out AnimatedEmotesComponent? comp))
            return;

        if (!_prot.TryIndex(args.EmoteId, out var emote))
            return;

        // Ensure they can play animations
        EnsureComp<AnimationPlayerComponent>(uid);

        // FIX: Explicitly cast the event object.
        // The prototype stores 'Event' as an Object. We must cast it to
        // the specific type so the SubscribeLocalEvent handlers above can hear it.
        if (emote.Event != null)
        {
            switch (emote.Event)
            {
                case AnimationFlipEmoteEvent flip:
                    RaiseLocalEvent(uid, flip);
                    break;
                case AnimationSpinEmoteEvent spin:
                    RaiseLocalEvent(uid, spin);
                    break;
                case AnimationJumpEmoteEvent jump:
                    RaiseLocalEvent(uid, jump);
                    break;
            }
        }
    }

    public void PlayEmote(EntityUid uid, Animation anim, string animationKey = "emoteAnimKeyId")
    {
        // Stop existing animation to allow "spamming" the emote
        if (_anim.HasRunningAnimation(uid, animationKey))
            _anim.Stop(uid, animationKey);

        _anim.Play(uid, anim, animationKey);
    }

    // --- ANIMATION HANDLERS ---

    private void OnFlip(Entity<AnimatedEmotesComponent> ent, ref AnimationFlipEmoteEvent args)
    {
        var a = new Animation
        {
            Length = TimeSpan.FromMilliseconds(500),
            AnimationTracks =
            {
                new AnimationTrackComponentProperty
                {
                    ComponentType = typeof(SpriteComponent),
                    Property = nameof(SpriteComponent.Rotation),
                    InterpolationMode = AnimationInterpolationMode.Linear,
                    KeyFrames =
                    {
                        new AnimationTrackProperty.KeyFrame(Angle.Zero, 0f),
                        new AnimationTrackProperty.KeyFrame(Angle.FromDegrees(90), 0.125f),
                        new AnimationTrackProperty.KeyFrame(Angle.FromDegrees(180), 0.125f),
                        new AnimationTrackProperty.KeyFrame(Angle.FromDegrees(270), 0.125f),
                        new AnimationTrackProperty.KeyFrame(Angle.FromDegrees(360), 0.125f),
                    }
                }
            }
        };
        PlayEmote(ent, a);
    }

    private void OnSpin(Entity<AnimatedEmotesComponent> ent, ref AnimationSpinEmoteEvent args)
    {
        var a = new Animation
        {
            Length = TimeSpan.FromMilliseconds(600),
            AnimationTracks =
            {
                new AnimationTrackComponentProperty
                {
                    ComponentType = typeof(TransformComponent),
                    Property = nameof(TransformComponent.LocalRotation),
                    InterpolationMode = AnimationInterpolationMode.Linear,
                    KeyFrames =
                    {
                        new AnimationTrackProperty.KeyFrame(Angle.FromDegrees(0), 0f),
                        new AnimationTrackProperty.KeyFrame(Angle.FromDegrees(90), 0.075f),
                        new AnimationTrackProperty.KeyFrame(Angle.FromDegrees(180), 0.075f),
                        new AnimationTrackProperty.KeyFrame(Angle.FromDegrees(270), 0.075f),
                        new AnimationTrackProperty.KeyFrame(Angle.Zero, 0.075f),
                        new AnimationTrackProperty.KeyFrame(Angle.FromDegrees(90), 0.075f),
                        new AnimationTrackProperty.KeyFrame(Angle.FromDegrees(180), 0.075f),
                        new AnimationTrackProperty.KeyFrame(Angle.FromDegrees(270), 0.075f),
                        new AnimationTrackProperty.KeyFrame(Angle.Zero, 0.075f),
                    }
                }
            }
        };
        PlayEmote(ent, a, "emoteAnimSpin");
    }

    private void OnJump(Entity<AnimatedEmotesComponent> ent, ref AnimationJumpEmoteEvent args)
    {
        var a = new Animation
        {
            Length = TimeSpan.FromMilliseconds(250),
            AnimationTracks =
            {
                new AnimationTrackComponentProperty
                {
                    ComponentType = typeof(SpriteComponent),
                    Property = nameof(SpriteComponent.Offset),
                    InterpolationMode = AnimationInterpolationMode.Cubic,
                    KeyFrames =
                    {
                        new AnimationTrackProperty.KeyFrame(Vector2.Zero, 0f),
                        new AnimationTrackProperty.KeyFrame(new Vector2(0, .20f), 0.125f),
                        new AnimationTrackProperty.KeyFrame(Vector2.Zero, 0.125f),
                    }
                }
            }
        };
        PlayEmote(ent, a);
    }
}
