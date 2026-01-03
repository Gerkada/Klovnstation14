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

        // SAFETY: Force them to stand upright when done,
        // just in case the animation math was off by 0.001 degrees.
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
        EnsureComp<FlippingComponent>(user);

        // 1. Get current rotation
        var baseAngle = Angle.Zero;
        if (EntityManager.TryGetComponent(user, out SpriteComponent? sprite))
            baseAngle = sprite.Rotation;

        var startDegrees = baseAngle.Degrees;

        // 2. Calculate the target "Upright" angle.
        // We want to land on a multiple of 360 (which is 0 degrees visual).
        // We add 720 to ensure we do at least 2 full flips.
        // Math.Ceiling ensures that if we are at 10 degrees, we go forward to 360, not back to 0.
        var targetDegrees = Math.Ceiling(startDegrees / 360) * 360 + 720;

        // If we are exactly at 0, the math above gives 720 (2 spins).
        // If we are at 90, math gives 360 + 720 = 1080 (2.75 spins).
        // This ensures we ALWAYS land upright.

        // 3. Generate intermediate keyframes
        // We need points every 120 degrees so the engine knows which way to spin.
        var keyFrames = new List<AnimationTrackProperty.KeyFrame>();
        var totalDuration = 0.8f;

        keyFrames.Add(new AnimationTrackProperty.KeyFrame(baseAngle, 0f));

        var current = startDegrees;
        var stepSize = 120; // 120 degree steps
        var steps = (int)((targetDegrees - startDegrees) / stepSize);

        // Interpolate time based on how far we have to go
        for (var i = 1; i <= steps; i++)
        {
            current += stepSize;
            var timeRatio = (float)i / steps; // 0.1, 0.2 ... 1.0

            // If the last step overshoots, clamp it (though math shouldn't allow it)
            if (current > targetDegrees) current = targetDegrees;

            keyFrames.Add(new AnimationTrackProperty.KeyFrame(Angle.FromDegrees(current), totalDuration * timeRatio));
        }

        // Add final frame to be perfectly sure
        keyFrames.Add(new AnimationTrackProperty.KeyFrame(Angle.FromDegrees(targetDegrees), totalDuration));

        var animation = new Animation
        {
            Length = TimeSpan.FromSeconds(totalDuration),
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
