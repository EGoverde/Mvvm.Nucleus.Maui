namespace Mvvm.Nucleus.Maui.UnitTests.TestHelpers;

/// <summary>
/// Awaits work that the library routes through <c>async void</c> (
/// <see cref="NucleusMvvmCore.RunTaskInVoidAndTrackException"/>) where no directly awaitable
/// <see cref="Task"/> is available: polls a <see cref="Task"/> (typically the
/// <see cref="RecordingViewModel.AsyncCallbackCompleted"/> TCS) or a boolean condition with a
/// timeout, so tests fail fast and deterministically instead of hanging.
/// </summary>
internal static class TestAwaiter
{
    /// <summary>
    /// Waits until <paramref name="task"/> completes, or the timeout elapses.
    /// </summary>
    /// <returns><c>true</c> when the task completed within the timeout; otherwise <c>false</c>.</returns>
    public static async Task<bool> CompletedWithin(Task task, TimeSpan timeout = default)
    {
        if (timeout == TimeSpan.Zero)
        {
            timeout = TimeSpan.FromSeconds(5);
        }

        var winner = await Task.WhenAny(task, Task.Delay(timeout));
        if (winner != task)
        {
            return false;
        }

        await task;

        return true;
    }

    /// <summary>
    /// Polls <paramref name="condition"/> until it returns <c>true</c> or the timeout elapses.
    /// </summary>
    /// <returns><c>true</c> when the condition became true within the timeout; otherwise <c>false</c>.</returns>
    public static async Task<bool> CompletedWithin(Func<bool> condition, TimeSpan timeout = default)
    {
        if (timeout == TimeSpan.Zero)
        {
            timeout = TimeSpan.FromSeconds(5);
        }

        var deadline = DateTime.UtcNow + timeout;
        while (DateTime.UtcNow < deadline)
        {
            if (condition())
            {
                return true;
            }

            await Task.Delay(10);
        }

        return condition();
    }
}
