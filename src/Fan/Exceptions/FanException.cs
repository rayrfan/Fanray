using System;

namespace Fan.Exceptions
{
    public class FanException : Exception
    {
        /// <summary>
        /// Thrown with a message.
        /// </summary>
        /// <param name="message"></param>
        public FanException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Thrown with a message and an exception.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="inner"></param>
        public FanException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
