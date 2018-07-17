// ----------------------------------------------------------------------------
// <copyright file="XacmlInvalidSyntaxException.cs" company="ABC Software Ltd">
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

namespace Abc.Xacml.Runtime {
    using System;

#if !NETSTANDARD1_6
    [Serializable]
#endif
    public class XacmlInvalidSyntaxException : XacmlException {
        /// <summary>
        /// Initializes a new instance of the <see cref="XacmlInvalidSyntaxException"/> class.
        /// </summary>
        public XacmlInvalidSyntaxException()
            : base(XacmlConstants.StatusCodes.SyntaxError) {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="XacmlInvalidSyntaxException"/> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public XacmlInvalidSyntaxException(string message)
            : base(XacmlConstants.StatusCodes.SyntaxError, message) {
        }
    }
}
