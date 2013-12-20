using System;
using System.IO;
using ManyConsole;
using Ninject;
using Ninject.Extensions.Conventions;
using Ninject.Parameters;
using OdjfsScraper.DataChecker.Commands;
using OdjfsScraper.Scraper.Scrapers;
using OdjfsScraper.Scraper.Support;

namespace OdjfsScraper.DataChecker
{
    public static class ExtensionMethods
    {
        public static string GetMapQuestKey(this Command command)
        {
            string mapQuestKey = Settings.MapQuestKey;
            if (string.IsNullOrWhiteSpace(mapQuestKey))
            {
                throw new ConsoleHelpAsException(string.Format("No MapQuest API key has been specified in {0}.",
                    Path.GetFileName(AppDomain.CurrentDomain.SetupInformation.ConfigurationFile)));
            }

            return mapQuestKey;
        }

        public static Odjfs GetOdjfs(this Command command)
        {
            IKernel kernel = new StandardKernel();
            kernel.Bind(c => c
                .FromAssemblyContaining(typeof (IChildCareStubListScraper))
                .SelectAllClasses()
                .BindAllInterfaces());

            var parameter = new ConstructorArgument("odjfsClient", new DownloadingOdjfsClient(@"Logs\HTML"));
            var odjfs = new Odjfs(kernel.Get<IChildCareStubListScraper>(parameter), kernel.Get<IChildCareScraper>(parameter));

            return odjfs;
        }
    }
}