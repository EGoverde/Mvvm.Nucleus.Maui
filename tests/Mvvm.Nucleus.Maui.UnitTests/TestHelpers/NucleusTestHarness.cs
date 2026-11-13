using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Mvvm.Nucleus.Maui.UnitTests.TestHelpers;

/// <summary>
/// Base class for tests that need an initialized <see cref="NucleusMvvmCore"/>.
///
/// <see cref="NucleusMvvmCore.Current"/> is process-wide (a static), so each test gets its own
/// core instance and (re-)registers it as <c>Current</c> in the constructor. Test parallelization
/// is disabled (see <c>AssemblyInfo.cs</c>), so a test's assertions always observe its own core.
///
/// The constructor mirrors how a test drives the library in a bare <c>net11.0</c> host without a
/// <c>MauiApp</c> bootstrap: construct the core directly (public ctor), call the internal
/// <c>Initialize(IServiceProvider)</c> (exposed through <c>InternalsVisibleTo</c>), and register a
/// <c>Shell</c> through the internal <c>Shell</c> setter.
/// </summary>
public abstract class NucleusTestHarness
{
    /// <summary>
    /// Initializes a new instance of the <see cref="NucleusTestHarness"/> class.
    /// </summary>
    /// <param name="options">The <see cref="NucleusMvvmOptions"/> (a fresh default instance when not set).</param>
    /// <param name="viewFactory">The <see cref="IViewFactory"/> (a <see cref="NoopViewFactory"/> when not set).</param>
    /// <param name="coreLogger">The <see cref="ILogger{NucleusMvvmCore}"/> (a <see cref="NullLogger{T}"/> when not set).</param>
    /// <param name="serviceProvider">The <see cref="IServiceProvider"/> (a <see cref="NoopServiceProvider"/> when not set).</param>
    protected NucleusTestHarness(
        NucleusMvvmOptions? options = null,
        IViewFactory? viewFactory = null,
        ILogger<NucleusMvvmCore>? coreLogger = null,
        IServiceProvider? serviceProvider = null)
    {
        Options = options ?? new NucleusMvvmOptions();
        Core = new NucleusMvvmCore(
            new Application(),
            viewFactory ?? new NoopViewFactory(),
            Options,
            coreLogger ?? NullLogger<NucleusMvvmCore>.Instance);

        Core.Initialize(serviceProvider ?? new NoopServiceProvider());

        Shell = new Shell();
        Core.Shell = Shell;
    }

    /// <summary>
    /// The initialized <see cref="NucleusMvvmCore"/> for this test.
    /// </summary>
    public NucleusMvvmCore Core { get; }

    /// <summary>
    /// The <see cref="NucleusMvvmOptions"/> in use (mutable per test).
    /// </summary>
    public NucleusMvvmOptions Options { get; }

    /// <summary>
    /// The registered <see cref="Shell"/>.
    /// </summary>
    public Shell Shell { get; }

    /// <summary>
    /// Creates a <see cref="Shell"/> that contains <paramref name="page"/> as its current page, and
    /// registers it with <see cref="Core"/>. In the bare host <c>Shell.CurrentPage</c> only resolves
    /// once the shell actually contains a <c>ShellItem</c>/<c>ShellContent</c> structure — this mirrors
    /// a minimally configured <c>AppShell</c> and is what the confirm-navigation logic reads
    /// (<c>Shell.CurrentPage.BindingContext</c>).
    /// </summary>
    protected Shell CreateShellWithCurrentPage(Page page)
    {
        var shell = new Shell();

        var shellContent = new ShellContent
        {
            Content = page,
        };

        var shellItem = new ShellItem();
        shellItem.Items.Add(shellContent);

        shell.Items.Add(shellItem);

        Core.Shell = shell;

        return shell;
    }

    /// <summary>
    /// Creates a new <see cref="ContentPage"/> with the given <paramref name="bindingContext"/>
    /// and a fresh attached <see cref="NucleusMvvmPageBehavior"/> (the state a
    /// <see cref="ViewFactory"/> would leave the page in after creation).
    /// </summary>
    protected (Page Page, NucleusMvvmPageBehavior Behavior) CreatePage(object? bindingContext = null)
    {
        var page = new ContentPage
        {
            BindingContext = bindingContext,
        };

        var behavior = new NucleusMvvmPageBehavior
        {
            Page = page,
        };

        page.Behaviors.Add(behavior);

        return (page, behavior);
    }
}
