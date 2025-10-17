using System;
using System.Collections.Generic;

namespace Neolant.PFEI.SyncModule.H2.Tests
{
    public static class ExceptionHelper
    {
        /// <summary>
        /// Expands all nested exceptions to a list of strings
        /// </summary>
        /// <param name="exception">The original exception</param>
        /// <returns>A list of lines describing all exceptions</returns>
        private static IReadOnlyList<string> UnwrapException(Exception exception)
        {
            var result = new List<string>();

            if (exception == null)
                return result.AsReadOnly();

            // Adding the main exception
            result.Add(exception.ToString());

            // Expand nested exceptions
            var inner = exception.InnerException;
            int depth = 1;
            while (inner != null)
            {
                var prefix = new string('-', depth * 2);
                result.Add($"{prefix} Inner[{depth}] {prefix}>");
                result.Add(inner.ToString());
                inner = inner.InnerException;
                depth++;
            }

            return result.AsReadOnly();
        }

        public static void WriteAllErrorsToConsole(string message, Exception exception)
        {
            if (!string.IsNullOrEmpty(message)) {
                Console.WriteLine("[ERR] " + message);
            }

            var messages = ExceptionHelper.UnwrapException(exception);

            for (int i = 0; i < messages.Count; i++)
            {
                Console.WriteLine("[ERR] " + messages[i]);
            }
        }
    }
}
