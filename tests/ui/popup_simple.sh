#!/usr/bin/env bash
#
# popup_simple — simple popup (no ViewModel): open, verify parameter text, close via IPopupService.
# Part of the tests/ui suite (see README.md). Prereq: sample app running + agent connected.
set -euo pipefail

UI_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
# shellcheck source=lib/devflow.sh
source "${UI_DIR}/lib/devflow.sh"

reset_to PopupTab
tap_text "Show Popup without ViewModel"
wait_text "Text from navigation parameters."
tap_text "Close using IPopupService"
wait_dialog "Okay"
dismiss_dialog "Okay"
wait_text "Show Popup without ViewModel"
