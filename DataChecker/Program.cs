using System;
using System.Collections.Generic;
using ManyConsole;
using Ninject;
using Ninject.Extensions.Conventions;
using Ninject.Parameters;
using OdjfsScraper.Scraper.Scrapers;
using OdjfsScraper.Scraper.Support;

namespace OdjfsScraper.DataChecker
{
    internal class Program
    {
        public static int Main(string[] args)
        {
            IEnumerable<ConsoleCommand> commands = ConsoleCommandDispatcher.FindCommandsInSameAssemblyAs(typeof (Program));
            return ConsoleCommandDispatcher.DispatchCommand(commands, args, Console.Out);
        }

        public static Odjfs GetOdjfs()
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