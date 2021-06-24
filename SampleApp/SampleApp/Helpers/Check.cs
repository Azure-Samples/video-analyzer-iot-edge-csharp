// -----------------------------------------------------------------------
//  <copyright company="Microsoft Corporation">
//      Copyright (C) Microsoft Corporation. All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

using System;
using System.Diagnostics.CodeAnalysis;

namespace SampleApp.Helpers
{
    /// <summary>
    /// Represents a utility class for parameter validation.
    /// </summary>
    public static class Check
    {
        /// <summary>
        /// Checks that the specified string parameter is not null, empty or whitespace.
        /// </summary>
        /// <param name="parameter">Parameter to be checked.</param>
        /// <param name="parameterName">Parameter name.</param>
        /// <param name="message">Optional error message.</param>
        public static void NotNullOrWhiteSpace(
            string parameter,
            string parameterName,
            string message = null)
        {
            CheckParameterName(parameterName);

            if (string.IsNullOrWhiteSpace(parameter))
            {
                throw new ArgumentException(
                    message ?? $"Parameter '{parameterName}' cannot be null, empty or whitespace.",
                    parameterName);
            }
        }

        /// <summary>
        /// Checks that the specified parameter is not null.
        /// </summary>
        /// <param name="parameter">Parameter to be checked.</param>
        /// <param name="parameterName">Parameter name.</param>
        /// <param name="message">Optional error message.</param>
        public static void NotNull(
            [NotNull] object? parameter,
            string parameterName,
            string? message = null)
        {
            CheckParameterName(parameterName);

            if (parameter == null)
            {
                throw CreateArgumentNullException(parameterName, message);
            }
        }

        /// <summary>
        /// Checks that the parameter value is within the specified range.
        /// </summary>
        /// <param name="parameterValue">Value being checked.</param>
        /// <param name="min">Minimum allowed value. Whether or not inclusive is determined by <paramref name="inclusive"/>.</param>
        /// <param name="max">Maximum allowed value. Whether or not inclusive is determined by <paramref name="inclusive"/>.</param>
        /// <param name="parameterName">Parameter name.</param>
        /// <param name="message">Error message.</param>
        /// <param name="inclusive">When true, the min and max values are included in the range of allowed values. Otherwise, excluded.</param>
        /// <typeparam name="T">Parameter type.</typeparam>
        public static void InRange<T>(
            T parameterValue,
            T min,
            T max,
            string parameterName,
            string? message = null,
            bool inclusive = true)
            where T : IComparable<T>
        {
            CheckParameterName(parameterName);

            if (parameterValue.CompareTo(min) >= (inclusive ? 0 : 1) &&
                parameterValue.CompareTo(max) <= (inclusive ? 0 : -1))
            {
                return;
            }

            if (message == null)
            {
                message = $"The value '{parameterValue}' of parameter '{parameterName}' must be in range: {min}, {max}. Inclusive: {inclusive}";
            }

            throw new ArgumentException(message, parameterName);
        }

        private static void CheckParameterName(string? parameterName)
        {
            if (string.IsNullOrWhiteSpace(parameterName))
            {
                throw new ArgumentException(
                    "Argument cannot be null, empty or whitespace.",
                    nameof(parameterName));
            }
        }

        private static ArgumentNullException CreateArgumentNullException(string? parameterName, string? message)
        {
            return message != null
                ? new ArgumentNullException(parameterName, message)
                : new ArgumentNullException(parameterName);
        }
    }
}