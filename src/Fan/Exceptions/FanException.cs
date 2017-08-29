using FluentValidation.Results;
using System;
using System.Collections.Generic;

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

        /// <summary>
        /// Thrown when <see cref="ValidationResult.IsValid"/> is false. 
        /// </summary>
        /// <param name="message">Summary of what's happening</param>
        /// <param name="result">Individual errors inside the result</param>
        public FanException(string message, IList<ValidationFailure> validationFailures)
            : base(message)
        {
            ValidationFailures = validationFailures;
        }

        /// <summary>
        /// A list of <see cref="ValidationFailure"/>. Null if the exception thrown is not
        /// as a result of <see cref="ValidationResult.IsValid"/> being false.
        /// </summary>
        public IList<ValidationFailure> ValidationFailures { get; }
    }
}