using System.Configuration;
using System.Linq;

namespace OdjfsScraper.Tool
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

        public static string AzureBlobStorageConnectionString
        {
            get { return GetAppSettingOrNull("AzureBlobStorageConnectionString"); }
        }

        public static string AzureBlobStorageContainer
        {
            get { return GetAppSettingOrNull("AzureBlobStorageContainer"); }
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