#!/usr/bin/env bash
#
# nav_debounce — double navigation request is debounced: a single Details page is pushed,
# so one "Navigate Backwards" returns to the tab (proving the stack depth was 1).
# Part of the tests/ui suite (see README.md). Prereq: sample app running + agent connected.
set -euo pipefail

UI_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
# shellcheck source=lib/devflow.sh
source "${UI_DIR}/lib/devflow.sh"

reset_to NavigationTab
tap_text "Navigation ignoring multiple requests"
wait_text "Welcome to the details page."
tap_text "Navigate Backwards"
wait_text "Navigation by Type"
