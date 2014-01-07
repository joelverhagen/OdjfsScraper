using System.Configuration;
using System.Linq;

namespace OdjfsScraper.DataChecker
{
    public static class Settings
    {
        public static string MapQuestKey
        {
            get { return GetAppSettingOrNull("MapQuestKey"); }
        }

        public static string LogsDirectory
        {
            get { return GetAppSettingOrNull("LogsDirectory"); }
        }

        public static string HtmlDirectory
        {
            get { return GetAppSettingOrNull("HtmlDirectory"); }
        }

        private static string GetAppSettingOrNull(string key)
        {
            if (!ConfigurationManager.AppSettings.AllKeys.Contains(key))
            {
                return null;
            }

            return ConfigurationManager.AppSettings[key];
        }
    }
}