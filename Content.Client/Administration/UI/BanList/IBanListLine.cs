// SPDX-FileCopyrightText: 2023 DrSmugleaf
// SPDX-FileCopyrightText: 2026 Pieter-Jan Briers
// SPDX-FileCopyrightText: 2026 github_actions[bot]
//
// SPDX-License-Identifier: MIT

using Content.Shared.Administration.BanList;
using Robust.Client.UserInterface.Controls;

namespace Content.Client.Administration.UI.BanList;

public interface IBanListLine<T> where T : SharedBan
{
    T Ban { get; }
    Label Reason { get; }
    Label BanTime { get; }
    Label Expires { get; }
    Label BanningAdmin { get; }
}
