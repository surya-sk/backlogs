using System;

namespace Backlogs.Services
{
    /// <summary>
    /// Handles app settings set by the user
    /// </summary>
    public interface IUserSettings
    {
        /// <summary>
        /// Raised when a setting has been set
        /// </summary>
        event EventHandler<string> UserSettingsChanged;

        /// <summary>
        /// Saves a user setting 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="value"></param>
        void Set<T>(string key, T value);

        /// <summary>
        /// Gets a user setting
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        T Get <T>(string key);
    }
}
