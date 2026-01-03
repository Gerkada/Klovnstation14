// SPDX-FileCopyrightText: 2024 Aviu00
// SPDX-FileCopyrightText: 2024 Preston Smith
// SPDX-FileCopyrightText: 2024 Spatison
// SPDX-FileCopyrightText: 2025 Aiden
// SPDX-FileCopyrightText: 2025 FrauZj
// SPDX-FileCopyrightText: 2025 FrauzJ
// SPDX-FileCopyrightText: 2026 Gerkada
// SPDX-FileCopyrightText: 2026 github_actions[bot]
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System;
using System.Collections.Generic;
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
        // Prevent double-playing for the local player (Prediction vs Server)
        if (_player.LocalEntity == uid)
            return;

        PlayAnimation(uid);
    }

    private void OnAnimationComplete(Entity<FlippingComponent> ent, ref AnimationCompletedEvent args)
    {
        if (args.Key != FlippingComponent.AnimationKey)
            return;

        RemComp<FlippingComponent>(ent);

        // Reset rotation to exactly zero to prevent slight offsets
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

        // FIX: Check if the animation is already running.
        // The engine crashes if we try to Play() while the key "flip" is active.
        if (_animationSystem.HasRunningAnimation(user, FlippingComponent.AnimationKey))
        {
            _animationSystem.Stop(user, FlippingComponent.AnimationKey);
        }

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

        // 4. Generate Intermediate Frames (While loop for constant speed)
        var current = startDegrees;
        var stepSize = 120; // 120 degree steps for shortest-path interpolation

        while (current + stepSize < targetDegrees)
        {
            current += stepSize;
            var fraction = (float)((current - startDegrees) / totalDist);
            keyFrames.Add(new AnimationTrackProperty.KeyFrame(Angle.FromDegrees(current), totalDuration * fraction));
        }

        // 5. Final Frame
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
