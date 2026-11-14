#!/usr/bin/env bash
#
# lib/devflow.sh — shared DevFlow driver helpers for the Mvvm.Nucleus.Maui UI
# regression suite (tests/ui). Sourced by run-all.sh and every flow script.
#
# Every `maui devflow` invocation goes through `df` with `--ci --json` (the exact
# mode a CI job would use). The helpers encapsulate the operational caveats found
# in spike #1 (research-testingOptions.md):
#
#   1. Sequential execution: a MUTATING DevFlow CLI call acquires a driving lease
#      that is held for ~10-15s; another CLI session's mutating call inside that
#      window gets "Another DevFlow session is driving this app (MAUI CLI)".
#      Read-only calls (query/tree/element/agent) are exempt. A conflicting
#      mutating call is NOT guaranteed undelivered — so the driver spaces
#      mutating calls far apart (DF_GAP_SECS, default 10s) and treats conflicts
#      as wait + registry refresh + retry (DF_RETRY_WAIT_SECS).
#   2. Broker lease authority hiccups (reported non-retryable) are recovered with
#      `maui devflow broker stop && start`; the app's agent auto re-registers.
#   3. Element IDs are epoch-scoped and go stale across navigation/state changes;
#      refreshing with `ui tree` (or re-querying) recovers.
#   4. NO-OP SHELL NAVIGATION WEDGES THE APP (reproduced 2026-11-13): a
#      `ui navigate` to the CURRENT location fires Shell's `Navigating` (the
#      library's NavigationService sets IsNavigating=true) but never fires
#      `Navigated`, so IsNavigating is never reset and every subsequent
#      INavigationService request is silently ignored ("Ignoring this navigation
#      request as another request is already in progress"). Any REAL navigation
#      (e.g. a tab switch) fires `Navigated` and unsticks it. reset_to therefore
#      never issues a no-op navigate and self-heals via a real tab switch.
#   5. The app must be launched via LaunchServices (`open`): a bundle binary run
#      directly from a terminal ends up with a non-key window that silently
#      swallows DevFlow-synthesized touches (the CLI reports success, the button
#      command never runs).
#
# Catalyst native-dialog findings (spike #1, re-confirmed 2026-11-13):
#   - `ui alert detect`/`ui alert dismiss` report false for Catalyst native NSAlerts,
#     but the alert's buttons ARE registered as native `DialogAction` elements and
#     `ui tap <id>` dismisses them reliably. The alert message BODY is not queryable
#     (flows assert on the button set / follow-up state instead).
#   - Prompts: buttons are native "OK"/"Cancel" DialogActions; the prompt text field
#     is a native `UITextField` (role "dialog") fillable via `ui fill`.
#
# Tunables (overridable via environment):
#   DF_GAP_SECS         spacing after a MUTATING CLI call (default 12s; keeps the
#                       next mutating call outside the ~10-15s driving-lease window)
#   DF_RO_GAP_SECS      spacing after a read-only CLI call (default 1s)
#   DF_RETRY_WAIT_SECS  wait before retrying after a driving-lease conflict (12s)
#   DF_MAX_ATTEMPTS     attempts per call, including the first (4)
#   DF_WAIT_TIMEOUT     default wait-until timeout in seconds (15)
DF_GAP_SECS="${DF_GAP_SECS:-12}"
DF_RO_GAP_SECS="${DF_RO_GAP_SECS:-1}"
DF_RETRY_WAIT_SECS="${DF_RETRY_WAIT_SECS:-12}"
DF_MAX_ATTEMPTS="${DF_MAX_ATTEMPTS:-4}"
DF_WAIT_TIMEOUT="${DF_WAIT_TIMEOUT:-15}"

# --- diagnostics ---------------------------------------------------------------

_df_log()  { printf '[devflow] %s\n' "$*" >&2; }

_df_fail() {
    local msg="$1"; shift
    printf '[FAIL] %s\n' "$msg" >&2
    while IFS= read -r line; do
        printf '       %s\n' "$line" >&2
    done <<< "$*"
    return 1
}

# --- core CLI wrapper ----------------------------------------------------------

# _df_gap <subcommand...> — seconds to wait after a successful call: read-only
# calls don't take the driving lease (1s is fine); mutating calls do (DF_GAP_SECS).
_df_gap() {
    local sub="${1:-}" obj="${2:-}" verb="${3:-}"
    case "${sub} ${obj}" in
        "ui query" | "ui tree" | "ui element" | "ui hit-test" | "ui status" | \
        "ui diagnostics" | "agent" | "diagnose" | "broker")
            echo "$DF_RO_GAP_SECS" ;;
        "ui alert")
            [[ "$verb" == "detect" ]] && echo "$DF_RO_GAP_SECS" || echo "$DF_GAP_SECS" ;;
        *)
            echo "$DF_GAP_SECS" ;;
    esac
}

# df <step> <devflow subcommand + args...>
# Runs `maui devflow <args> --ci --json`, applying the recovery rules:
#   - driving-lease conflict ("Another DevFlow session...") -> wait + registry refresh + retry
#   - broker lease authority unavailable                     -> broker stop/start + agent wait + retry
#   - stale element/epoch/registry errors                    -> `ui tree` refresh + retry
#   - action reported success:false (tap/fill not accepted)  -> reactivate app window + retry
#   - anything else                                          -> fail with the step name
# stdout carries the command's JSON output; diagnostics go to stderr.
df() {
    local step="${1:-devflow call}"; shift
    local attempt=1 out rc recoverable gap
    while :; do
        if out="$(maui devflow "$@" --ci --json 2>&1)"; then
            printf '%s\n' "$out"
            gap="$(_df_gap "$@")"
            sleep "$gap"
            return 0
        fi
        rc=$?
        recoverable=0
        case "$out" in
            *"Another DevFlow session"*)
                _df_log "${step}: driving-lease conflict (attempt ${attempt}/${DF_MAX_ATTEMPTS}); waiting ${DF_RETRY_WAIT_SECS}s + registry refresh before retry"
                sleep "$DF_RETRY_WAIT_SECS"
                maui devflow ui tree --depth 1 --ci --json >/dev/null 2>&1 || true
                recoverable=1
                ;;
            *"lease authority"*)
                _df_log "${step}: broker lease authority unavailable (attempt ${attempt}/${DF_MAX_ATTEMPTS}); restarting broker"
                maui devflow broker stop >/dev/null 2>&1
                maui devflow broker start >/dev/null 2>&1
                maui devflow wait --timeout 30 >/dev/null 2>&1
                recoverable=1
                ;;
            *"stale"* | *"epoch"* | *"registry"*)
                _df_log "${step}: stale element/epoch suspected (attempt ${attempt}/${DF_MAX_ATTEMPTS}); refreshing tree"
                maui devflow ui tree --depth 1 --ci --json >/dev/null 2>&1 || true
                sleep "$DF_RETRY_WAIT_SECS"
                recoverable=1
                ;;
            *'"success":false'*)
                # The action was synthesized but the app did not act on it. The known
                # cause is a non-key window (the app launched in the background, or
                # lost key status), which silently swallows DevFlow-synthesized
                # touches (README "Operational caveats" #1, run-2 failure 2026-11-14).
                # Reactivating the app makes its window key again; then retry.
                _df_log "${step}: action reported success:false (attempt ${attempt}/${DF_MAX_ATTEMPTS}); reactivating app window before retry"
                osascript -e 'tell application "Mvvm.Nucleus.Maui.Sample" to activate' >/dev/null 2>&1 || true
                sleep 2
                recoverable=1
                ;;
        esac
        if (( recoverable == 0 || attempt >= DF_MAX_ATTEMPTS )); then
            _df_fail "${step}: 'maui devflow $*' failed (attempt ${attempt}/${DF_MAX_ATTEMPTS})" "$out"
            return 1
        fi
        attempt=$(( attempt + 1 ))
    done
}

# --- broker / agent ------------------------------------------------------------

# ensure_broker — start the broker daemon if it is not already running (idempotent).
ensure_broker() {
    local out running
    out="$(maui devflow broker status --json 2>&1)" || true
    running="$(printf '%s' "$out" | jq -r '.running // false' 2>/dev/null)" || running="false"
    if [[ "$running" != "true" ]]; then
        _df_log "broker not running; starting"
        maui devflow broker start || return 1
    fi
    return 0
}

# wait_agent [timeout_s] — wait until a DevFlow agent (the sample app) is registered.
wait_agent() {
    local timeout="${1:-60}"
    local deadline=$(( $(date +%s) + timeout ))
    while :; do
        if maui devflow wait --timeout 5 --json >/dev/null 2>&1; then
            return 0
        fi
        if (( $(date +%s) >= deadline )); then
            _df_fail "wait_agent: no DevFlow agent registered within ${timeout}s"
            return 1
        fi
    done
}

# --- navigation / state --------------------------------------------------------

# _dismiss_stray_dialogs — dismiss any dialog left over from an interrupted
# (killed/timed-out) flow. A leftover Catalyst NSAlert is app-modal and would
# swallow the next flow's first tap. Strategy per dialog:
#   - prompt (UITextField present)        -> tap Cancel (cancel, no side effects)
#   - multi-button alert (e.g. Confirm)   -> tap Cancel (safe choice)
#   - single-button alert (e.g. "Okay")   -> tap that button
_dismiss_stray_dialogs() {
    local actions count label fields i
    for i in 1 2 3; do
        actions="$(df "stray-dialog scan" ui query --type DialogAction --json)" || actions="[]"
        count="$(printf '%s' "$actions" | jq -r 'if type == "array" then length else 0 end' 2>/dev/null)" || count=0
        [[ "$count" =~ ^[0-9]+$ ]] || count=0
        [[ "$count" == "0" ]] && return 0
        fields="$(df "stray-field scan" ui query --type UITextField --json)" || fields="[]"
        if [[ "$(printf '%s' "$fields" | jq -r 'if type == "array" then length else 0 end' 2>/dev/null)" != "0" || count -gt 1 ]]; then
            tap_text "Cancel" || _df_log "stray dialog: 'Cancel' not found"
        else
            label="$(printf '%s' "$actions" | jq -r 'if type == "array" and length > 0 then .[0].text else "" end' 2>/dev/null)"
            tap_text "$label" || _df_log "stray dialog: could not tap '${label}'"
        fi
    done
    return 0
}

# _current_route — the agent-reported current shell route ("" if unavailable).
# Read-only (agent status) — does not take the driving lease.
# Shape: "//Tabs/<Tab>" on a tab; "//Tabs/<Tab>/<Page>[/...]" with pushed pages.
# (Modal pages are overlays and do NOT appear in the route.)
_current_route() {
    local out
    out="$(maui devflow agent status --json 2>/dev/null)" || out=""
    printf '%s' "$out" | jq -r '.route // ""' 2>/dev/null
}

# _tap_tab <tab> — switch Shell tabs via the app's OWN tab bar (DevFlow's stable
# Tab_<name> elements). This is a real Shell navigation driven through the app UI —
# the only reset mechanism that cannot wedge the library's IsNavigating flag
# (agent-driven `ui navigate` can — see caveat 4 in the header).
# A tap on the ALREADY-ACTIVE tab is a no-op that DevFlow reports as
# {"success":false} and the CLI exits non-zero (observed 2026-11-13, which failed
# an entire reset). The agent's route is authoritative, so: skip the tap when the
# route already shows the target tab, and tolerate the reported failure when the
# route confirms we are there anyway.
_tap_tab() {
    local tab="$1"
    if [[ "$(_current_route)" == "//Tabs/${tab}" ]]; then
        _df_log "tab switch to ${tab}: already here; no-op tap skipped"
        return 0
    fi
    if df "tab switch to ${tab}" ui tap "Tab_${tab}" >/dev/null; then
        return 0
    fi
    if [[ "$(_current_route)" == "//Tabs/${tab}" ]]; then
        _df_log "tab switch to ${tab}: tap reported not-OK, but the route confirms we are here"
        return 0
    fi
    return 1
}

# reset_to <NavigationTab|PopupTab|DialogTab> — land on a known, healthy tab using
# ONLY the app's own UI (never `ui navigate` — caveat 4):
#   1. dismiss any dialog a killed flow may have left open (app-modal, swallows taps);
#   2. close any open popup ("Close using IPopupService" / "Close Popup");
#   3. pop any pushed Details page (plain "Navigate Backwards" — first match);
#   4. close any open modal Details ("Close All Modals (if any)");
#   5. switch to the target tab via the tab bar; if we are already on it and
#      nothing had to be unwound, perform a 2-step tab dance (away and back) to
#      clear a possibly wedged IsNavigating flag left by an interrupted flow.
# Every action here is a real navigation through the app UI, which fires Shell's
# Navigated event and therefore keeps the library's navigation state machine sane.
reset_to() {
    local tab="${1:?reset_to: tab name required}"
    local i did_real=0 current other
    for i in 1 2 3 4; do
        _dismiss_stray_dialogs
        # 2. a popup is open?
        if [[ -n "$(query_id "Close using IPopupService" Button)" ]]; then
            tap_text "Close using IPopupService"; did_real=1; sleep 2; continue
        fi
        if [[ -n "$(query_id "Close Popup" Button)" ]]; then
            tap_text "Close Popup"; did_real=1; sleep 2; continue
        fi
        current="$(_current_route)"
        # 3. a page is pushed on the current tab's stack? pop one level.
        if [[ "$current" == "//Tabs/"*/* ]]; then
            if [[ -n "$(query_id_first "Navigate Backwards" Button)" ]]; then
                tap_text_first "Navigate Backwards" Button; did_real=1; sleep 2; continue
            fi
        fi
        # 4. a modal Details is open (route stays at tab level)? close all modals.
        if [[ -n "$(query_id "Close All Modals (if any)" Button)" ]]; then
            tap_text "Close All Modals (if any)"; did_real=1; sleep 2; continue
        fi
        break
    done
    _dismiss_stray_dialogs # follow-up alerts ("Okay") from the closes above
    current="$(_current_route)"
    if [[ "$current" == "//Tabs/${tab}" && "$did_real" == "0" ]]; then
        # Already on the target tab with nothing to unwind: a killed flow may have
        # left the IsNavigating flag wedged — a pair of REAL tab switches
        # (Navigating + Navigated) clears it.
        if [[ "$tab" == "NavigationTab" ]]; then other="PopupTab"; else other="NavigationTab"; fi
        _df_log "reset to ${tab}: nothing to unwind; un-wedge dance via real tab switches"
        _tap_tab "$other"
        _tap_tab "$tab"
        if [[ "$(_current_route)" != "//Tabs/${tab}" ]]; then
            _df_fail "reset to ${tab}: un-wedge dance did not land on the target tab"
            return 1
        fi
    elif [[ "$current" != "//Tabs/${tab}" ]]; then
        _tap_tab "$tab"
    fi
}

# --- element lookup ------------------------------------------------------------

# query_id <text> [type] — id of the TOPMOST element with the exact text (last match
# in tree order), optionally filtered by element type. Empty if none.
# Overlay rule (plan §4): popups, pushed pages, and native dialogs are appended AFTER
# the underlying content in the tree, so the last match is the topmost interactive one
# (e.g. the popup's "Close using IPopupService" over any same-named element behind it).
query_id() {
    local text="$1" type="${2:-}" out
    if [[ -n "$type" ]]; then
        out="$(df "query text '${text}' type ${type}" ui query --text "$text" --type "$type" --json)" || out="[]"
    else
        out="$(df "query text '${text}'" ui query --text "$text" --json)" || out="[]"
    fi
    printf '%s' "$out" | jq -r 'if type == "array" and length > 0 then .[-1].id else "" end' 2>/dev/null
}

# query_id_first <text> [type] — id of the FIRST (earliest in tree order) element
# matching the text, optionally filtered by type. Use when the FIRST match is the
# intended one (e.g. the plain "Navigate Backwards" button, which is the first of
# the three "Navigate Backwards*" buttons on the Details page). Empty if none.
query_id_first() {
    local text="$1" type="${2:-}" out
    if [[ -n "$type" ]]; then
        out="$(df "query first '${text}' type ${type}" ui query --text "$text" --type "$type" --json)" || out="[]"
    else
        out="$(df "query first '${text}'" ui query --text "$text" --json)" || out="[]"
    fi
    printf '%s' "$out" | jq -r 'if type == "array" and length > 0 then .[0].id else "" end' 2>/dev/null
}

# query_type_id <type> — id of the topmost element of the given type (empty if none).
query_type_id() {
    local type="$1" out
    out="$(df "query type ${type}" ui query --type "$type" --json)" || out="[]"
    printf '%s' "$out" | jq -r 'if type == "array" and length > 0 then .[-1].id else "" end' 2>/dev/null
}

# --- actions -------------------------------------------------------------------

# tap_text <label> [kind] — tap the topmost element with the exact label, optionally
# filtered by element kind (e.g. "Button").
tap_text() {
    local text="$1" kind="${2:-}" id
    id="$(query_id "$text" "$kind")"
    if [[ -z "$id" ]]; then
        _df_fail "tap_text: no element with text '${text}'${kind:+ (kind ${kind})}"
        return 1
    fi
    df "tap '${text}'" ui tap "$id" >/dev/null
}

# tap_text_first <label> [kind] — tap the FIRST tree-order match (see query_id_first).
tap_text_first() {
    local text="$1" kind="${2:-}" id
    id="$(query_id_first "$text" "$kind")"
    if [[ -z "$id" ]]; then
        _df_fail "tap_text_first: no element with text '${text}'${kind:+ (kind ${kind})}"
        return 1
    fi
    df "tap first '${text}'" ui tap "$id" >/dev/null
}

# wait_text <text> [timeout_s] — wait until an element with the exact text exists.
wait_text() {
    local text="$1" timeout="${2:-$DF_WAIT_TIMEOUT}"
    df "wait text '${text}'" ui query --text "$text" --wait-until exists --timeout "$timeout" --json \
        | jq -e 'type == "array" and length > 0' >/dev/null
}

# assert_text <text> — an element with the exact text is present (waits up to the
# default timeout, so in-flight navigation/popup transitions settle first).
assert_text() { wait_text "$1"; }

# assert_absent <text> — no element with the exact text is present.
# (1s grace so in-flight state changes settle before the negative check.)
assert_absent() {
    local text="$1" out
    sleep 1
    out="$(df "assert absent '${text}'" ui query --text "$text" --json)" || out="[]"
    if [[ "$(printf '%s' "$out" | jq -r 'if type == "array" then length else 0 end' 2>/dev/null)" != "0" ]]; then
        _df_fail "assert_absent: '${text}' is present" "$out"
        return 1
    fi
}

# wait_dialog <button_label> [timeout_s] — wait until a native DialogAction with the
# label is registered (i.e. the alert/prompt/action sheet is up).
wait_dialog() {
    local button="$1" timeout="${2:-$DF_WAIT_TIMEOUT}"
    df "wait dialog button '${button}'" ui query --text "$button" --wait-until exists --timeout "$timeout" --json \
        | jq -e 'type == "array" and (map(select(.role == "dialog-action")) | length > 0)' >/dev/null
}

# assert_dialog <button_label> — a dialog with the given button is up (default timeout).
assert_dialog() { wait_dialog "$1"; }

# dismiss_dialog <button_label> — dismiss the current dialog.
# Tries the native alert API first (`ui alert detect` -> `ui alert dismiss`, which
# works for in-app dialogs and off-Catalyst); on Catalyst the native NSAlert is
# invisible to the alert API, so fall back to tapping the button, which is
# registered as a native DialogAction element (spike #1 pattern).
dismiss_dialog() {
    local button="$1" detected out
    out="$(df "alert detect" ui alert detect)" || out="{}"
    detected="$(printf '%s' "$out" | jq -r '.detected // false' 2>/dev/null)"
    if [[ "$detected" == "true" ]]; then
        if df "alert dismiss '${button}'" ui alert dismiss "$button" >/dev/null; then
            return 0
        fi
        _df_log "alert dismiss '${button}' failed; falling back to DialogAction tap"
    fi
    tap_text "$button"
}

# enter_prompt <value> [ok_label] — fill the prompt's text field and confirm.
# The prompt's field is a native UITextField on Catalyst (spike-confirmed); ok_label
# defaults to "OK" (Catalyst native label, confirmed 2026-11-13).
enter_prompt() {
    local value="$1" ok_label="${2:-OK}" id
    if ! df "wait prompt field" ui query --type UITextField --wait-until exists --timeout "$DF_WAIT_TIMEOUT" --json \
        | jq -e 'type == "array" and length > 0' >/dev/null; then
        _df_fail "enter_prompt: prompt text field (UITextField) never appeared"
        return 1
    fi
    id="$(query_type_id UITextField)"
    if [[ -z "$id" ]]; then
        _df_fail "enter_prompt: UITextField not found"
        return 1
    fi
    df "prompt fill '${value}'" ui fill "$id" "$value" >/dev/null || return 1
    dismiss_dialog "$ok_label"
}
