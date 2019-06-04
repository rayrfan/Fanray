namespace Fan.Settings
{
    /// <summary>
    /// A marker interface for any settings class that wants to persist through <see cref="ISettingService.GetSettingsAsync{T}"/> 
    /// and <see cref="ISettingService.UpsertSettingsAsync{T}(T)"/>.
    /// </summary>
    /// <remarks>
    /// <see cref="ISettingService"/> will persist it to Core_Setting table with each property name as Key, property value as Value.
    /// To add a new property, simply add the property and initialize it with a value, uninitialized property get the default value.
    /// To remove a property, simply remove it, even though the property still exists in database, it does not affect serializaiton.
    /// Property can be a complex type, the service will serialize anything that cannot be converted directly into a string into a
    /// json string.
    /// </remarks>
    public interface ISettings
    {
    }
}
