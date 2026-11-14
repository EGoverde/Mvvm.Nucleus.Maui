#!/usr/bin/env bash
#
# details_back_twice — push a second Details page, then one "Navigate Backwards (twice)"
# pops both levels back to the tab.
# Part of the tests/ui suite (see README.md). Prereq: sample app running + agent connected.
set -euo pipefail

UI_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
# shellcheck source=lib/devflow.sh
source "${UI_DIR}/lib/devflow.sh"

reset_to NavigationTab
tap_text "Navigation by Type"
wait_text "Welcome to the details page."
tap_text "Show More Details (Regular)"
wait_text "Navigate Backwards (twice)"
tap_text "Navigate Backwards (twice)"
wait_text "Navigation by Type"
