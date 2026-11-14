#!/usr/bin/env bash
#
# nav_switch_tabs — Shell tab switching: "Switch Tabs" lands on the Popup tab.
# Part of the tests/ui suite (see README.md). Prereq: sample app running + agent connected.
set -euo pipefail

UI_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
# shellcheck source=lib/devflow.sh
source "${UI_DIR}/lib/devflow.sh"

reset_to NavigationTab
tap_text "Switch Tabs"
wait_text "Show Popup without ViewModel"
