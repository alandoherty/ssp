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

        /// <summary>
        /// Gets the current unix timestamp.
        /// </summary>
        /// <returns>The current unix timestamp.</returns>
        public static long Timestamp() {
            var timeSpan = (DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0));
            return (long)timeSpan.TotalSeconds;
        }
        #endregion
    }
}
