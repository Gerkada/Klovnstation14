// SPDX-FileCopyrightText: 2025 Alin
// SPDX-FileCopyrightText: 2025 github_actions[bot]
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameStates;

namespace Content.Shared._RMC14.Weapons.Ranged.Prediction;

[RegisterComponent, NetworkedComponent]
[Access(typeof(SharedGunPredictionSystem))]
public sealed partial class GunIgnorePredictionComponent : Component;