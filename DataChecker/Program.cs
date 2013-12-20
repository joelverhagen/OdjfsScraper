using System;
using System.Collections.Generic;
using ManyConsole;
using NLog;

namespace OdjfsScraper.DataChecker
{
    internal class Program
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public static int Main(string[] args)
        {
            try
            {
                IEnumerable<ConsoleCommand> commands = ConsoleCommandDispatcher.FindCommandsInSameAssemblyAs(typeof(Program));
                return ConsoleCommandDispatcher.DispatchCommand(commands, args, Console.Out);
            }
            catch (Exception e)
            {
                Logger.Trace(e.Message);
                Logger.ErrorException("An error occurred during the execution of OdjfsScraper.DataChecker.", e);
                return 1;
            }
        }
    }
}