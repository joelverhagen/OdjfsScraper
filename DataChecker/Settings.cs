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