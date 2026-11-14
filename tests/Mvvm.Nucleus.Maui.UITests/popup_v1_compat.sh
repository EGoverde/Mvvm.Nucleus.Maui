#!/usr/bin/env bash
#
# popup_v1_compat — CommunityToolkit V1-compat popup: open, close via IPopupService.
# Part of the tests/ui suite (see README.md). Prereq: sample app running + agent connected.
set -euo pipefail

UI_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
# shellcheck source=lib/devflow.sh
source "${UI_DIR}/lib/devflow.sh"

flow_begin PopupTab
tap_text "Show Compatibility Popup"
wait_text "Close using IPopupService"
tap_text "Close using IPopupService"
wait_dialog "Okay"
dismiss_dialog "Okay"
wait_text "Show Compatibility Popup"
