#!/usr/bin/env bash
#
# nav_switch_tabs — Shell tab switching: "Switch Tabs" lands on the Popup tab.
# Part of the tests/Mvvm.Nucleus.Maui.UnitTests/ suite (see README.md). Prereq: sample app running + agent connected.
# NOTE: this flow ends on the Popup tab (declared below), so the assembler inserts
# no tab switch before the following popup_* flows.
set -euo pipefail

UI_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
# shellcheck source=lib/devflow.sh
source "${UI_DIR}/lib/devflow.sh"

flow_begin NavigationTab
tap_text "Switch Tabs"
wait_text "Show Popup without ViewModel"
flow_end PopupTab
