// SPDX-FileCopyrightText: 2024 Aiden
// SPDX-FileCopyrightText: 2024 DrSmugleaf
// SPDX-FileCopyrightText: 2025 Toaster
// SPDX-FileCopyrightText: 2025 Toastermeister
// SPDX-FileCopyrightText: 2025 github_actions[bot]
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameStates;

namespace Content.Shared._RMC14.Weapons.Ranged.Prediction;

[RegisterComponent, NetworkedComponent]
[Access(typeof(SharedGunPredictionSystem))]
public sealed partial class IgnorePredictionHideComponent : Component;
