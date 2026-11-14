#!/usr/bin/env bash
#
# dialog_confirm — DisplayConfirm: choose Confirm, then dismiss the follow-up alert.
# Part of the tests/Mvvm.Nucleus.Maui.UITests/ suite (see README.md). Prereq: sample app running + agent connected.
set -euo pipefail

UI_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
# shellcheck source=lib/devflow.sh
source "${UI_DIR}/lib/devflow.sh"

flow_begin DialogTab
tap_text "Display Confirm"
wait_dialog "Confirm"
dismiss_dialog "Confirm"
wait_dialog "Okay"
dismiss_dialog "Okay"
wait_text "Display Alert"
