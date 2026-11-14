#!/usr/bin/env bash
#
# nav_by_type — Navigation by Type: push Details, verify render, go back.
# Part of the tests/Mvvm.Nucleus.Maui.UnitTests/ suite (see README.md). Prereq: sample app running + agent connected.
set -euo pipefail

UI_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
# shellcheck source=lib/devflow.sh
source "${UI_DIR}/lib/devflow.sh"

flow_begin NavigationTab
tap_text "Navigation by Type"
wait_text "Welcome to the details page."
tap_text "Navigate Backwards"
wait_text "Navigation by Type"
