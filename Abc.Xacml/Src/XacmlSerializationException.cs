// ----------------------------------------------------------------------------
// <copyright file="XacmlSerializationException.cs" company="ABC Software Ltd">
//    Copyright © 2015 ABC Software Ltd. All rights reserved.
//
//    This library is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Lesser General Public
//    License  as published by the Free Software Foundation, either 
//    version 3 of the License, or (at your option) any later version. 
//
//    This library is distributed in the hope that it will be useful, 
//    but WITHOUT ANY WARRANTY; without even the implied warranty of 
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU 
//    Lesser General Public License for more details.
//
//    You should have received a copy of the GNU Lesser General Public 
//    License along with the library. If not, see http://www.gnu.org/licenses/.
// </copyright>
// ----------------------------------------------------------------------------
 
namespace Abc.Xacml {
    using System;
    using System.Runtime.Serialization;

    /// <summary>
    /// The exception that is thrown when an error occurs while serializing or deserializing a XACML message. 
    /// </summary
    [Serializable]
    public class XacmlSerializationException : Exception {
        /// <summary>
        /// Initializes a new instance of the <see cref="XacmlSerializationException"/> class.
        /// </summary>
        public XacmlSerializationException() {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="XacmlSerializationException"/> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public XacmlSerializationException(string message)
            : base(message) { 
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="XacmlSerializationException"/> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="innerException">The inner exception.</param>
        public XacmlSerializationException(string message, Exception innerException)
            : base(message, innerException) {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="XacmlSerializationException"/> class.
        /// </summary>
        /// <param name="info">The <see cref="T:System.Runtime.Serialization.SerializationInfo" /> that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="T:System.Runtime.Serialization.StreamingContext" /> that contains contextual information about the source or destination.</param>
        protected XacmlSerializationException(SerializationInfo info, StreamingContext context)
            : base(info, context) {
        }
    }
}
