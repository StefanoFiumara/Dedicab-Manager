using System.Configuration;

namespace DedicabUtility.Client.Core
{
    public static class AppSettings
    {
        private static readonly Configuration _config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

        public static string Get(Setting setting)
        {
            return _config.AppSettings.Settings[setting.ToString()]?.Value;
        }

        public static void Set(Setting setting, string value)
        {
            _config.AppSettings.Settings.Remove(setting.ToString());
            _config.AppSettings.Settings.Add(setting.ToString(), value);
            _config.Save();
        }
    }
}