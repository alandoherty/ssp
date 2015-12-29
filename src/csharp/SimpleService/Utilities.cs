using System;

namespace SimpleService
{
    internal class Utilities
    {
        #region Methods
        /// <summary>
        /// Logs a debug message.
        /// </summary>
        /// <param name="msg">The message.</param>
        public static void DebugLog(string msg) {
#if DEBUG
            Console.WriteLine("[ssp:log] " + msg);
#endif
        }
        #endregion
    }
}
