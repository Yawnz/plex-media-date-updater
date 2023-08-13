using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlexDateAddedUpdater.Helpers
{
    public static class ConsoleHelpers
    {
        public static bool IsExitInput(string input)
        {
            return string.Equals(input, "q", StringComparison.OrdinalIgnoreCase)
                || string.Equals(input, "quit", StringComparison.OrdinalIgnoreCase)
                || string.Equals(input, "exit", StringComparison.OrdinalIgnoreCase);
        }
    }
}
