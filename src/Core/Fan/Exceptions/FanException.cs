using FluentValidation.Results;
using System;
using System.Collections.Generic;

namespace Fan.Exceptions
{
    /// <summary>
    /// The exception for the system.
    /// </summary>
    /// <remarks>
    /// Caller should log before throw an exception.  I choose not to pass in ILogger here for max flexibility.
    /// The message giving to this class will be displayed on Error.cshtml.
    /// </remarks>
    public class FanException : Exception
    {
        public FanException()
        {
        }

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
            ValidationErrors = validationFailures;
            ExceptionType = EExceptionType.ValidationError;
        }

        /// <summary>
        /// Thrown with a <see cref="EExceptionType"/>.
        /// </summary>
        /// <param name="exceptionType"></param>
        public FanException(EExceptionType exceptionType, string message = "")
            : base(message)
        {
            ExceptionType = exceptionType;
        }

        /// <summary>
        /// Thrown with a <see cref="EExceptionType"/> and inner exception.
        /// </summary>
        /// <param name="exceptionType"></param>
        /// <param name="inner"></param>
        public FanException(EExceptionType exceptionType, Exception inner)
            : base("", inner)
        {
            ExceptionType = exceptionType;
        }

        /// <summary>
        /// A list of <see cref="ValidationFailure"/>. Null if the exception thrown is not
        /// as a result of <see cref="ValidationResult.IsValid"/> being false.
        /// </summary>
        public IList<ValidationFailure> ValidationErrors { get; }

        public EExceptionType ExceptionType { get; }
    }
}