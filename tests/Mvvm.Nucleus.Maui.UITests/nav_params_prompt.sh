#!/usr/bin/env bash
#
# nav_params_prompt — Navigation with Parameters: prompt for a value, verify it arrives via Prepare().
# Part of the tests/ui suite (see README.md). Prereq: sample app running + agent connected.
set -euo pipefail

UI_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
# shellcheck source=lib/devflow.sh
source "${UI_DIR}/lib/devflow.sh"

flow_begin NavigationTab
tap_text "Navigation with Parameters"
wait_dialog "OK"
enter_prompt "UI-Param-42"
assert_text "Parameters: Sample : UI-Param-42"
tap_text "Navigate Backwards"
wait_text "Navigation by Type"
