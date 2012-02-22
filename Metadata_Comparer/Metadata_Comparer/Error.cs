using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace Metadata_Comparer
{
    /// <summary>
    /// The type of error code to return to the parent process.
    /// </summary>
    public enum ErrorCode
    {
        Invalid_Parameters = -1
    }

    /// <summary>
    /// Package to gracefully handle errors.
    /// </summary>
    public static class Error
    {
        /// <summary>
        /// Prints a message and gracefully exits the process.
        /// </summary>
        /// <param name="code">The error code to return to the parent process.</param>
        /// <param name="message">The message to log.</param>
        public static void Throw(ErrorCode code, string message)
        {
            Console.WriteLine(message);
            Console.ReadLine();
            Environment.Exit((int)code);
        }
    }
}
