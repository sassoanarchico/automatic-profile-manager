using Playnite.SDK;

namespace AutomationProfileManager.Services
{
    /// <summary>
    /// Helper class for retrieving localized strings.
    /// Uses Playnite's ResourceProvider which automatically loads the correct language file.
    /// </summary>
    public static class LocalizationService
    {
        /// <summary>
        /// Gets a localized string by key.
        /// </summary>
        /// <param name="key">The resource key (e.g., "LOC_APM_AddAction")</param>
        /// <returns>The localized string, or the key itself if not found</returns>
        public static string GetString(string key)
        {
            var result = ResourceProvider.GetString(key);
            // If the key is not found, ResourceProvider returns the key itself
            return result ?? key;
        }

        /// <summary>
        /// Gets a localized string with format arguments.
        /// </summary>
        /// <param name="key">The resource key</param>
        /// <param name="args">Format arguments</param>
        /// <returns>The formatted localized string</returns>
        public static string GetString(string key, params object[] args)
        {
            var format = GetString(key);
            try
            {
                return string.Format(format, args);
            }
            catch
            {
                return format;
            }
        }
    }
}
