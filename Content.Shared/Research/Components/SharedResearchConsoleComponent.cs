// SPDX-FileCopyrightText: 2019 Víctor Aguilera Puerto
// SPDX-FileCopyrightText: 2021 Acruid
// SPDX-FileCopyrightText: 2021 DrSmugleaf
// SPDX-FileCopyrightText: 2021 Visne
// SPDX-FileCopyrightText: 2022 metalgearsloth
// SPDX-FileCopyrightText: 2022 mirrorcult
// SPDX-FileCopyrightText: 2022 wrexbe
// SPDX-FileCopyrightText: 2023 Nemanja
// SPDX-FileCopyrightText: 2025 Gerkada
// SPDX-FileCopyrightText: 2025 github_actions[bot]
//
// SPDX-License-Identifier: MIT

using Robust.Shared.Serialization;
using Content.Shared._KS14.Research;

namespace Content.Shared._KS14.Research.Components
{
    [NetSerializable, Serializable]
    public enum ResearchConsoleUiKey : byte
    {
        Key,
    }

    [Serializable, NetSerializable]
    public sealed class ConsoleUnlockTechnologyMessage(string id) : BoundUserInterfaceMessage
    {
        public string Id = id;
    }

    [Serializable, NetSerializable]
    public sealed class ConsoleRediscoverTechnologyMessage : BoundUserInterfaceMessage;

    [Serializable, NetSerializable]
    public sealed class ConsoleServerSelectionMessage : BoundUserInterfaceMessage
    {

    }

    [Serializable, NetSerializable]
    public sealed class ResearchConsoleBoundInterfaceState(int points, TimeSpan nextRediscover, int rediscoverCost) : BoundUserInterfaceState
    {
<<<<<<< HEAD
        public int Points;

        /// <summary>
        /// Goobstation field - all researches and their availablities
        /// </summary>
        public Dictionary<string, ResearchAvailability> Researches;

        public ResearchConsoleBoundInterfaceState(int points, Dictionary<string, ResearchAvailability> researches) // Goobstation R&D console rework = researches field
        {
            Points = points;
            Researches = researches; // Goobstation R&D console rework
        }
=======
        public int Points = points;

        public TimeSpan NextRediscover = nextRediscover;

        public int RediscoverCost = rediscoverCost;
>>>>>>> upstream/master
    }
}
