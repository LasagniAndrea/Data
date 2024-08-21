using System;

namespace Fonet {
    /// <summary>
    ///     This exception is thrown by FO.NET when an error occurs.
    /// </summary>
    // EG 20180425 Analyse du code Correction [CA2237]
    [Serializable]
    public class FonetException : ApplicationException {
        /// <summary>
        ///     Initialises a new instance of the FonetException class.
        /// </summary>
        /// <remarks>
        ///     The <see cref="Exception.Message"/> property will be initialised 
        ///     to <i>innerException.Message</i>
        /// </remarks>
        /// <param name="innerException">
        ///     The exception that is the cause of the current exception
        /// </param>
        public FonetException(Exception innerException)
            : base(innerException.Message, innerException) {}

        /// <summary>
        ///     Initialises a new instance of the FonetException class.
        /// </summary>
        /// <param name="message">
        ///     The error message that explains the reason for this exception
        /// </param>
        public FonetException(string message)
            : base(message) {}

        /// <summary>
        ///     Initialises a new instance of the FonetException class.
        /// </summary>
        /// <param name="message">
        ///     The error message that explains the reason for this exception
        /// </param>
        /// <param name="innerException">
        ///     The exception that is the cause of the current exception
        /// </param>
        public FonetException(string message, Exception innerException)
            : base(message, innerException) {}

    }
}