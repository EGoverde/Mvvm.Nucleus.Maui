#!/usr/bin/env bash
#
# dialog_alert_background — DisplayAlert triggered from a background thread: alert appears, dismiss it.
# Part of the tests/ui suite (see README.md). Prereq: sample app running + agent connected.
set -euo pipefail

UI_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
# shellcheck source=lib/devflow.sh
source "${UI_DIR}/lib/devflow.sh"

flow_begin DialogTab
tap_text "Display Alert (background thread)"
wait_dialog "Okay"
dismiss_dialog "Okay"
wait_text "Display Alert"
