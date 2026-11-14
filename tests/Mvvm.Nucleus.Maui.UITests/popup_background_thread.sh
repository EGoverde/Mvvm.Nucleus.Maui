#!/usr/bin/env bash
#
# popup_background_thread — popup shown from a background thread: open, verify, close.
# Part of the tests/Mvvm.Nucleus.Maui.UnitTests/ suite (see README.md). Prereq: sample app running + agent connected.
set -euo pipefail

UI_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
# shellcheck source=lib/devflow.sh
source "${UI_DIR}/lib/devflow.sh"

flow_begin PopupTab
tap_text "Show Popup from background thread"
wait_text "IsMainThread = False"
tap_text "Close using IPopupService"
wait_text "Show Popup from background thread"
