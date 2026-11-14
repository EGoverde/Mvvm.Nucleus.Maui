#!/usr/bin/env bash
#
# popup_ct_compat — CommunityToolkit 17 IPopupService popup: open, verify lifecycle states, close.
# Part of the tests/Mvvm.Nucleus.Maui.UnitTests/ suite (see README.md). Prereq: sample app running + agent connected.
set -euo pipefail

UI_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
# shellcheck source=lib/devflow.sh
source "${UI_DIR}/lib/devflow.sh"

flow_begin PopupTab
tap_text "Use CommunityToolkit.PopupService"
wait_text "Text from navigation parameters."
# CT 17's IPopupService path fires OnOpened() BEFORE Prepare() (see
# NucleusMvvmPopupBehavior.OnPopupOpened), so the state order is "Opened,
# Prepared" — the reverse of the nucleus-service path pinned in
# popup_advanced.sh ("Prepared, Opened"). The order is deterministic library
# behavior, so it is asserted exactly.
wait_text "State: Default, Opened, Prepared"
tap_text "Close using IPopupService"
# The closing alert's message body is not queryable on Catalyst (spike #1);
# assert on the button instead.
wait_dialog "Okay"
dismiss_dialog "Okay"
wait_text "Use CommunityToolkit.PopupService"
