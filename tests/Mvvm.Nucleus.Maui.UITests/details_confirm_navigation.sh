#!/usr/bin/env bash
#
# details_confirm_navigation — IConfirmNavigationAsync: Cancel keeps us on Details,
# Confirm proceeds with the pop back to the tab.
# Part of the tests/Mvvm.Nucleus.Maui.UITests/ suite (see README.md). Prereq: sample app running + agent connected.
set -euo pipefail

UI_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
# shellcheck source=lib/devflow.sh
source "${UI_DIR}/lib/devflow.sh"

flow_begin NavigationTab
tap_text "Navigation by Type"
wait_text "Welcome to the details page."
tap_text "Navigate Backwards (confirm)"
wait_dialog "Cancel"
dismiss_dialog "Cancel"
wait_text "Welcome to the details page."
tap_text "Navigate Backwards (confirm)"
wait_dialog "Confirm"
dismiss_dialog "Confirm"
wait_text "Navigation by Type"
