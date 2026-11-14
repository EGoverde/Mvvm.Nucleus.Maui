using Xunit;

// Parallelization is disabled because two process-wide statics are shared across tests:
// NucleusMvvmCore._current (re-registered on every core init) and
// NavigationService._isNavigatingLastTriggeredUtc (debounce timing).
// The suite is small and sub-second per test, so serialization is the deterministic choice.
[assembly: CollectionBehavior(DisableTestParallelization = true)]
