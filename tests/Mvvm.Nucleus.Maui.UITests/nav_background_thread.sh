#!/usr/bin/env bash
#
# nav_background_thread — Navigation triggered from a background thread; verify the parameter arrived.
# Part of the tests/ui suite (see README.md). Prereq: sample app running + agent connected.
set -euo pipefail

UI_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
# shellcheck source=lib/devflow.sh
source "${UI_DIR}/lib/devflow.sh"

flow_begin NavigationTab
tap_text "Navigation from background thread"
wait_text "Parameters: Sample : IsMainThread = False"
tap_text "Navigate Backwards"
wait_text "Navigation by Type"
