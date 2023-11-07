// ----------------------------------------------------------------------------
// <copyright file="XacmlIndeterminateException.cs" company="ABC Software Ltd">
//    Copyright © 2018 ABC Software Ltd. All rights reserved.
//
//    This library is free software; you can redistribute it and/or.
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

namespace Abc.Xacml.Runtime {
    using System;

#if !NETSTANDARD1_6
    [Serializable]
#endif
    public class XacmlIndeterminateException : XacmlException {
        /// <summary>
        /// Initializes a new instance of the <see cref="XacmlIndeterminateException"/> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public XacmlIndeterminateException(string message)
            : base(XacmlConstants.StatusCodes.SyntaxError, "Indeterminate exception: " + message) {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="XacmlIndeterminateException"/> class.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or a null reference (Nothing in Visual Basic) if no inner exception is specified.</param>
        public XacmlIndeterminateException(string message, Exception innerException)
            : base(XacmlConstants.StatusCodes.SyntaxError, "Indeterminate exception: " + message, innerException) {
        }
    }
}
