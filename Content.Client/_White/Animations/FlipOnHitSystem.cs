// SPDX-FileCopyrightText: 2024 Aviu00
// SPDX-FileCopyrightText: 2024 Preston Smith
// SPDX-FileCopyrightText: 2024 Spatison
// SPDX-FileCopyrightText: 2025 Aiden
// SPDX-FileCopyrightText: 2025 FrauzJ
// SPDX-FileCopyrightText: 2025 github_actions[bot]
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared._White.Animations;
using Content.Shared.Popups;
using Robust.Client.Animations;
using Robust.Client.GameObjects;
using Robust.Client.Player;
using Robust.Shared.Animations;
using Robust.Shared.Timing;

namespace Content.Client._White.Animations;

public sealed class FlipOnHitSystem : SharedFlipOnHitSystem
{
    [Dependency] private readonly AnimationPlayerSystem _animationSystem = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly IPlayerManager _player = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<FlippingComponent, AnimationCompletedEvent>(OnAnimationComplete);
        SubscribeAllEvent<FlipOnHitEvent>(OnNetworkEvent);
    }

    private void OnNetworkEvent(FlipOnHitEvent ev)
    {
        var uid = GetEntity(ev.User);
        if (_player.LocalEntity == uid)
            return;

        PlayAnimation(uid);
    }

    private void OnAnimationComplete(Entity<FlippingComponent> ent, ref AnimationCompletedEvent args)
    {
        if (args.Key != FlippingComponent.AnimationKey)
            return;

        RemComp<FlippingComponent>(ent);

        // FIX: Force reset to Zero when done, just in case.
        if (TryComp<SpriteComponent>(ent, out var sprite))
        {
            sprite.Rotation = Angle.Zero;
        }
    }

    protected override void PlayAnimation(EntityUid user)
    {
        if (TerminatingOrDeleted(user))
            return;

        EnsureComp<AnimationPlayerComponent>(user);

        if (_animationSystem.HasRunningAnimation(user, FlippingComponent.AnimationKey))
        {
            _animationSystem.Stop(user, FlippingComponent.AnimationKey);
        }

        EnsureComp<FlippingComponent>(user);

        // FIX: Always use Zero as the base.
        // Do NOT read sprite.Rotation, or you will inherit the rotation
        // from a previous, interrupted animation (getting stuck sideways).
        var degrees = 0.0;

        var keyFrames = new List<AnimationTrackProperty.KeyFrame>();
        var startTime = 0f;
        var spinDuration = 0.2f;

        // 4 Spins
        for (var i = 0; i < 4; i++)
        {
            keyFrames.Add(new AnimationTrackProperty.KeyFrame(Angle.FromDegrees(degrees + 120), startTime + (spinDuration * 0.33f)));
            keyFrames.Add(new AnimationTrackProperty.KeyFrame(Angle.FromDegrees(degrees + 240), startTime + (spinDuration * 0.66f)));
            keyFrames.Add(new AnimationTrackProperty.KeyFrame(Angle.FromDegrees(degrees), startTime + spinDuration));

            startTime += spinDuration;
        }

        var animation = new Animation
        {
            Length = TimeSpan.FromMilliseconds(800),
            AnimationTracks =
            {
                new AnimationTrackComponentProperty
                {
                    ComponentType = typeof(SpriteComponent),
                    Property = nameof(SpriteComponent.Rotation),
                    InterpolationMode = AnimationInterpolationMode.Linear,
                    KeyFrames = keyFrames
                }
            }
        };

        _animationSystem.Play(user, animation, FlippingComponent.AnimationKey);
    }
}
