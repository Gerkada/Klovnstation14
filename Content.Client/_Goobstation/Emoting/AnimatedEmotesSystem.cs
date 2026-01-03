using System.Linq;
using System.Numerics;
using Content.Client.Animations;
using Content.Client.DamageState;
using Content.Shared._Goobstation.Emoting;
using Content.Shared.Chat.Prototypes;
using Robust.Client.Animations;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Shared.Animations;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.Client._Goobstation.Emoting;

public sealed partial class AnimatedEmotesSystem : SharedAnimatedEmotesSystem
{
    [Dependency] private readonly AnimationPlayerSystem _anim = default!;
    [Dependency] private readonly IPrototypeManager _prot = default!;

    public override void Initialize()
    {
        base.Initialize();
        // Subscribe to the new Network Event
        SubscribeNetworkEvent<RequestEmoteAnimationEvent>(OnEmoteEvent);
    }

    private void OnEmoteEvent(RequestEmoteAnimationEvent args)
    {
        var uid = GetEntity(args.User);
        if (!_prot.TryIndex(args.EmoteId, out var emote))
            return;

        // FIX: Ensure the entity has the capability to play animations
        EnsureComp<AnimationPlayerComponent>(uid);

        // Identify and raise the specific local event for the animation logic
        if (emote.Event != null)
        {
            switch (emote.Event)
            {
                case AnimationFlipEmoteEvent flip:
                    OnFlip(uid, flip); // Call handler directly or RaiseLocal
                    break;
                case AnimationSpinEmoteEvent spin:
                    OnSpin(uid, spin);
                    break;
                case AnimationJumpEmoteEvent jump:
                    OnJump(uid, jump);
                    break;
            }
        }
    }

    public void PlayEmote(EntityUid uid, Animation anim, string animationKey = "emoteAnimKeyId")
    {
        if (_anim.HasRunningAnimation(uid, animationKey))
            _anim.Stop(uid, animationKey);

        _anim.Play(uid, anim, animationKey);
    }

    // --- Animation Logic (Keyframes fixed for full rotation) ---

    private void OnFlip(EntityUid uid, AnimationFlipEmoteEvent args)
    {
        var a = new Animation
        {
            Length = TimeSpan.FromMilliseconds(500),
            AnimationTracks = {
                new AnimationTrackComponentProperty {
                    ComponentType = typeof(SpriteComponent),
                    Property = nameof(SpriteComponent.Rotation),
                    InterpolationMode = AnimationInterpolationMode.Linear,
                    KeyFrames = {
                        new AnimationTrackProperty.KeyFrame(Angle.Zero, 0f),
                        new AnimationTrackProperty.KeyFrame(Angle.FromDegrees(90), 0.125f),
                        new AnimationTrackProperty.KeyFrame(Angle.FromDegrees(180), 0.125f),
                        new AnimationTrackProperty.KeyFrame(Angle.FromDegrees(270), 0.125f),
                        new AnimationTrackProperty.KeyFrame(Angle.FromDegrees(360), 0.125f),
                    }
                }
            }
        };
        PlayEmote(uid, a);
    }

    private void OnSpin(EntityUid uid, AnimationSpinEmoteEvent args)
    {
        var a = new Animation
        {
            Length = TimeSpan.FromMilliseconds(600),
            AnimationTracks = {
                new AnimationTrackComponentProperty {
                    ComponentType = typeof(TransformComponent),
                    Property = nameof(TransformComponent.LocalRotation),
                    InterpolationMode = AnimationInterpolationMode.Linear,
                    KeyFrames = {
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
        PlayEmote(uid, a, "emoteAnimSpin");
    }

    private void OnJump(EntityUid uid, AnimationJumpEmoteEvent args)
    {
        var a = new Animation
        {
            Length = TimeSpan.FromMilliseconds(250),
            AnimationTracks = {
                new AnimationTrackComponentProperty {
                    ComponentType = typeof(SpriteComponent),
                    Property = nameof(SpriteComponent.Offset),
                    InterpolationMode = AnimationInterpolationMode.Cubic,
                    KeyFrames = {
                        new AnimationTrackProperty.KeyFrame(Vector2.Zero, 0f),
                        new AnimationTrackProperty.KeyFrame(new Vector2(0, 0.5f), 0.125f),
                        new AnimationTrackProperty.KeyFrame(Vector2.Zero, 0.125f),
                    }
                }
            }
        };
        PlayEmote(uid, a);
    }
}
