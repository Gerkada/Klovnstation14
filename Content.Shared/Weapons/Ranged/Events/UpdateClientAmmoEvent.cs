// SPDX-FileCopyrightText: 2024 BramvanZijp
// SPDX-FileCopyrightText: 2025 Alin
//
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Shared.Weapons.Ranged.Events;

[ByRefEvent]
public readonly record struct UpdateClientAmmoEvent(int AritifialIncrease = 0); //RMC14, added the parameter to update ammo count, when ammo is taken because of something happening serverside