#!/usr/bin/env bash
#
# popup_simple — simple popup (no ViewModel): open, verify parameter text, close via IPopupService.
# Part of the tests/Mvvm.Nucleus.Maui.UnitTests/ suite (see README.md). Prereq: sample app running + agent connected.
set -euo pipefail

UI_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
# shellcheck source=lib/devflow.sh
source "${UI_DIR}/lib/devflow.sh"

flow_begin PopupTab
tap_text "Show Popup without ViewModel"
wait_text "Text from navigation parameters."
tap_text "Close using IPopupService"
wait_dialog "Okay"
dismiss_dialog "Okay"
wait_text "Show Popup without ViewModel"
