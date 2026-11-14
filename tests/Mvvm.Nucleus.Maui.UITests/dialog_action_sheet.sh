#!/usr/bin/env bash
#
# dialog_action_sheet — DisplayActionSheet: choose Option Two, dismiss the follow-up alert.
# Part of the tests/Mvvm.Nucleus.Maui.UITests/ suite (see README.md). Prereq: sample app running + agent connected.
set -euo pipefail

UI_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
# shellcheck source=lib/devflow.sh
source "${UI_DIR}/lib/devflow.sh"

flow_begin DialogTab
tap_text "Display Action Sheet"
wait_dialog "Option Two"
dismiss_dialog "Option Two"
wait_dialog "Okay"
dismiss_dialog "Okay"
wait_text "Display Alert"
