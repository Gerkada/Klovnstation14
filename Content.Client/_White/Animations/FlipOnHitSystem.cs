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

        // 2. Calculate target (Always land Upright + 720 degrees spin)
        var targetDegrees = Math.Ceiling(startDegrees / 360) * 360 + 720;

        // 3. Setup Math variables
        var totalDist = targetDegrees - startDegrees;
        var totalDuration = 0.8f;
        var keyFrames = new List<AnimationTrackProperty.KeyFrame>();

        // Add Start Frame
        keyFrames.Add(new AnimationTrackProperty.KeyFrame(baseAngle, 0f));

        // 4. Generate Intermediate Frames
        // FIX: Use a while loop to ensure we cover the distance smoothly.
        // We calculate the time for each frame based on the percentage of distance covered.
        // This guarantees constant velocity, preventing the "slow down" effect.

        var current = startDegrees;
        var stepSize = 120; // Maximum step size for "Shortest Path" interpolation

        while (current + stepSize < targetDegrees)
        {
            current += stepSize;

            // Calculate Fraction: (Distance So Far / Total Distance)
            var fraction = (float)((current - startDegrees) / totalDist);

            keyFrames.Add(new AnimationTrackProperty.KeyFrame(Angle.FromDegrees(current), totalDuration * fraction));
        }

        // 5. Final Frame (The finish line)
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
