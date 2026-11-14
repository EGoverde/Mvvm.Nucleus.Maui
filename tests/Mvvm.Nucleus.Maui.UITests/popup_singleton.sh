#!/usr/bin/env bash
#
# popup_singleton — singleton popup: set a value, close, reopen and verify the value persisted
# (the same instance was reused).
# Part of the tests/Mvvm.Nucleus.Maui.UITests/ suite (see README.md). Prereq: sample app running + agent connected.
set -euo pipefail

UI_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
# shellcheck source=lib/devflow.sh
source "${UI_DIR}/lib/devflow.sh"

flow_begin PopupTab
tap_text "Show Popup as Singleton"
wait_text "Value: Unset"
tap_text "Set Value"
wait_dialog "OK"
enter_prompt "Persist-42"
wait_text "Value: Persist-42"
tap_text "Close Popup"
wait_dialog "Okay"
dismiss_dialog "Okay"
tap_text "Show Popup as Singleton"
wait_text "Value: Persist-42"
tap_text "Close Popup"
wait_dialog "Okay"
dismiss_dialog "Okay"
wait_text "Show Popup as Singleton"
