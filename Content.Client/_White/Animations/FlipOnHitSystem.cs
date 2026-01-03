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
using Robust.Client.Player; // Needed to check Local Player
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

        // FIX 1: Prevent "Double Animation" (Prediction + Network).
        // If this event is for ME, and I am the one playing, I already predicted it via OnHit.
        // So ignore the server's "Echo".
        if (_player.LocalEntity == uid)
            return;

        PlayAnimation(uid);
    }

    private void OnAnimationComplete(Entity<FlippingComponent> ent, ref AnimationCompletedEvent args)
    {
        if (args.Key != FlippingComponent.AnimationKey)
            return;

        // Clean up component after animation
        RemComp<FlippingComponent>(ent);
    }

    protected override void PlayAnimation(EntityUid user)
    {
        if (TerminatingOrDeleted(user))
            return;

        // Ensure capability to play animations
        EnsureComp<AnimationPlayerComponent>(user);

        // Stop existing animation to restart it (allows spamming)
        if (_animationSystem.HasRunningAnimation(user, FlippingComponent.AnimationKey))
        {
            _animationSystem.Stop(user, FlippingComponent.AnimationKey);
        }

        // Add the component to mark us as flipping
        EnsureComp<FlippingComponent>(user);

        var baseAngle = Angle.Zero;
        if (EntityManager.TryGetComponent(user, out SpriteComponent? sprite))
            baseAngle = sprite.Rotation;

        var degrees = baseAngle.Degrees;

        // FIX 2: Correct Rotation Math.
        // We cannot use "360" or "720" because Angle wraps to 0.
        // We must use 120-degree steps (Shortest Path) to force a full rotation.
        // This loop generates 4 full spins (0.2s per spin).

        var keyFrames = new List<AnimationTrackProperty.KeyFrame>();
        var startTime = 0f;
        var spinDuration = 0.2f;

        // We do 4 spins
        for (var i = 0; i < 4; i++)
        {
            // Step 1: 0 -> 120
            keyFrames.Add(new AnimationTrackProperty.KeyFrame(Angle.FromDegrees(degrees + 120), startTime + (spinDuration * 0.33f)));
            // Step 2: 120 -> 240
            keyFrames.Add(new AnimationTrackProperty.KeyFrame(Angle.FromDegrees(degrees + 240), startTime + (spinDuration * 0.66f)));
            // Step 3: 240 -> 0 (360)
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
