using System;

namespace SimpleService.Protocol
{
    public class ProtocolException : Exception
    {
        #region Fields
        private ProtocolError error;
        #endregion

        #region Properties
        /// <summary>
        /// Gets the error code.
        /// </summary>
        public ProtocolError Error {
            get {
                return error;
            }
        }
        #endregion

        #region Constructors
        /// <summary>
        /// Creates a new protocol exception with the specified message and error code.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="error">The error code.</param>
        public ProtocolException(string message, ProtocolError error) 
            : base(message)  {
            this.error = error;
        }

        /// <summary>
        /// Creates a new protocol exception with the specified message and error code.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="error"></param>
        /// <param name="innerException"></param>
        public ProtocolException(string message, ProtocolError error, Exception innerException) 
            : base(message, innerException) {
            this.error = error;
        }
        #endregion
    }
}
