using System.ComponentModel;
using System.Runtime.CompilerServices;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Mvvm.Nucleus.Maui.Compatibility;

/// <summary>
/// The <see cref="BindableBase"/> is a compatibility class containing additional functions for bindings.
/// It matches most of the functionality of Prism's BindableBase class.
/// </summary>
public class BindableBase : ObservableObject
{
    /// <summary>
    /// Sets a bindable property with an additional <see cref="Action"/> to call after it changes.
    /// </summary>
    /// <typeparam name="T">The <see cref="Type"/> of the property.</typeparam>
    /// <param name="storage">The reference to the storage property.</param>
    /// <param name="value">The value to set.</param>
    /// <param name="onChanged">A <see cref="Action"/> to call if the value is different from the previous value.</param>
    /// <param name="propertyName">The name of the bindable property, automatically set.</param>
    /// <returns></returns>
    protected virtual bool SetProperty<T>(ref T storage, T value, Action onChanged, [CallerMemberName] string? propertyName = null)
    {
        if (SetProperty(ref storage, value, propertyName))
        {
            onChanged?.Invoke();

            return true;
        }

        return false;
    }

    /// <summary>
    /// Triggers an OnPropertyChanged event for the given property.
    /// </summary>
    /// <param name="propertyName"></param>
    protected void RaisePropertyChanged([CallerMemberName] string? propertyName = null)
    {
        OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
    }
}