namespace Mvvm.Nucleus.Maui;

/// <summary>
/// The <see cref="ElementExtensions"/> contains extensions for <see cref="Element"/>.
/// </summary>
internal static class ElementExtensions
{
    /// <summary>
    /// Listen to parent changes on the given <see cref="Element"/>. When the parent changes the root is passed to the
    /// given <see cref="Func{T, TResult}"/>, which should return a value indicating if the listener should continue.
    /// A target <see cref="Type"/> can be given that will limit the depth of the root to that <see cref="Type"/>.
    /// </summary>
    /// <param name="element">The <see cref="Element"/> to listen to parent changes.</param>
    /// <param name="targetType">The <see cref="Type"/> that if found will be seen as the root (set to <see langword="null"/> to not check <see cref="Type"/>).</param>
    /// <param name="shouldStopListening">A <see cref="Func{T, TResult}"/> indicating whether to continue listening for parent changes.</param>
    internal static void ListenToParentChanges(this Element element, Type? targetType, Func<Element, bool> shouldStopListening)
    {
        void OnParentChanged(object sender, EventArgs args)
        {
            if (sender is not Element element)
            {
                return;
            }

            var rootElement = element.Parent.GetRootElement(targetType);
            if (rootElement == null)
            {
                return;
            }

            element.ParentChanged -= OnParentChanged!;

            if (shouldStopListening(rootElement))
            {
                return;
            }

            rootElement.ParentChanged += OnParentChanged!;
        }

        element.ParentChanged += OnParentChanged!;
    }

    /// <summary>
    /// Gets the root element. If given a <see cref="Type"/> the first match will be returned.
    /// </summary>
    /// <param name="element">The <see cref="Element"/>.</param>
    /// <param name="targetType">The <see cref="Type"/> that if found should be returned. If <see langword="null"/> no type checking takes place.</param>
    /// <returns>The root <see cref="Element"/></returns>
    internal static Element? GetRootElement(this Element element, Type? targetType)
    {
        var rootElement = element?.Parent ?? element;
        var parentElement = element?.Parent;

        while (parentElement != null)
        {
            rootElement = parentElement;

            if (targetType != null && rootElement.GetType().IsAssignableTo(targetType))
            {
                break;
            }

            parentElement = rootElement.Parent;
        }

        return rootElement;
    }
}