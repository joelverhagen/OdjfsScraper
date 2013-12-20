using System;
using System.Collections.Generic;
using System.Text;
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
                IEnumerable<ConsoleCommand> commands = ConsoleCommandDispatcher.FindCommandsInSameAssemblyAs(typeof (Program));
                return ConsoleCommandDispatcher.DispatchCommand(commands, args, Console.Out);
            }
            catch (Exception e)
            {
                Logger.ErrorException("An error occurred during the execution of OdjfsScraper.DataChecker.", e);
                var sb = new StringBuilder();
                TraceExceptionMessages(sb, e, 1);
                Logger.Error(sb.ToString().Trim());
                return 1;
            }
        }

        private static void TraceExceptionMessages(StringBuilder sb, Exception e, int level)
        {
            if (e == null)
            {
                return;
            }

            // limit depth of recursion, since this is being run in a catch statement (gotta be careful)
            sb.AppendFormat("{0} {1}", new string('=', level), level > 10 ? "..." : e.Message);
            sb.AppendLine();
            if (level > 10)
            {
                return;
            }

            // recurse on child exceptions
            var ae = e as AggregateException;
            if (ae != null)
            {
                foreach (Exception child in ae.InnerExceptions)
                {
                    TraceExceptionMessages(sb, child, level + 1);
                }
            }
            else
            {
                TraceExceptionMessages(sb, e.InnerException, level + 1);
            }
        }
    }
}