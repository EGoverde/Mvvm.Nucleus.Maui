#!/usr/bin/env bash
#
# nav_query — Navigation with Query: prompt for a value, verify it arrives as a query parameter.
# Part of the tests/ui suite (see README.md). Prereq: sample app running + agent connected.
set -euo pipefail

UI_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
# shellcheck source=lib/devflow.sh
source "${UI_DIR}/lib/devflow.sh"

reset_to NavigationTab
tap_text "Navigation with Query"
enter_prompt "UI-Query-77"
assert_text "Parameters: Sample : UI-Query-77"
tap_text "Navigate Backwards"
wait_text "Navigation by Type"
