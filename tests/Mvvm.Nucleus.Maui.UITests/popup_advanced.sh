#!/usr/bin/env bash
#
# popup_advanced — popup with ViewModel: lifecycle states (Prepared/Opened) + parameter,
# close via IPopupService.
# Part of the tests/Mvvm.Nucleus.Maui.UITests/ suite (see README.md). Prereq: sample app running + agent connected.
set -euo pipefail

UI_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
# shellcheck source=lib/devflow.sh
source "${UI_DIR}/lib/devflow.sh"

flow_begin PopupTab
tap_text "Show Popup with ViewModel"
wait_text "State: Default, Prepared, Opened"
wait_text "Text from navigation parameters."
tap_text "Close using IPopupService"
wait_dialog "Okay"
dismiss_dialog "Okay"
wait_text "Show Popup with ViewModel"
