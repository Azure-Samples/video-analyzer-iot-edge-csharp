// -----------------------------------------------------------------------
//  <copyright company="Microsoft Corporation">
//      Copyright (C) Microsoft Corporation. All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

namespace Tools.JwtTokenIssuer
{
    /// <summary>
    /// Key information.
    /// </summary>
    public class KeyInfo
    {
        /// <summary>
        /// Gets or sets the key type.
        /// </summary>
        public string KeyType { get; set; }

        /// <summary>
        /// Gets or sets the key algorithm.
        /// </summary>
        public string KeyAlgorithm { get; set; }

        /// <summary>
        /// Gets or sets the key id.
        /// </summary>
        public string KeyId { get; set; }

        /// <summary>
        /// Gets or sets the key modulus.
        /// </summary>
        public string Modulus { get; set; }

        /// <summary>
        /// Gets or sets the exponent.
        /// </summary>
        public string Exponent { get; set; }
    }
}
