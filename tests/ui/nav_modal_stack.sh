#!/usr/bin/env bash
#
# nav_modal_stack — Navigation by Modal + Stack: modal Details inside a NavigationPage, close all.
# Part of the tests/ui suite (see README.md). Prereq: sample app running + agent connected.
set -euo pipefail

UI_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
# shellcheck source=lib/devflow.sh
source "${UI_DIR}/lib/devflow.sh"

flow_begin NavigationTab
tap_text "Navigation by Modal + Stack"
wait_text "Welcome to the details page."
tap_text "Close All Modals (if any)"
wait_text "Navigation by Type"
