using System;
using System.Collections.Generic;
using ManyConsole;

namespace OdjfsScraper.DataChecker
{
    internal class Program
    {
        public static int Main(string[] args)
        {
            IEnumerable<ConsoleCommand> commands = ConsoleCommandDispatcher.FindCommandsInSameAssemblyAs(typeof (Program));
            return ConsoleCommandDispatcher.DispatchCommand(commands, args, Console.Out);
        }
    }
}