// ----------------------------------------------------------------------------
// <copyright file="XacmlContextStatusCode.cs" company="ABC Software Ltd">
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

namespace Abc.Xacml.Context {
    using System;
    
    /// <summary>
    /// The <c>XacmlContextStatusCode</c> class contains a major status code value and an optional sequence of minor status codes.
    /// </summary>
    /// <remarks>See the xacml-context:StatusCode element defined in [XacmlCore, 6.13] for more details.</remarks>
    public class XacmlContextStatusCode {
        private Uri value;
        private XacmlContextStatusCode statusCode = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="XacmlContextStatusCode"/> class.
        /// </summary>
        /// <param name="value">The status value.</param>
        public XacmlContextStatusCode(Uri value) {
            if (value == null) {
                throw new ArgumentNullException(nameof(value));
            }

            this.Value = value;
        }

        #region Public Static Properties

        /// <summary>
        /// Gets the success status code.
        /// </summary>
        public static XacmlContextStatusCode Success {
            get {
                return new XacmlContextStatusCode(XacmlConstants.StatusCodes.Success);
            }
        }

        /// <summary>
        /// Gets the missing attribute code.
        /// </summary>
        public static XacmlContextStatusCode MissingAttribute {
            get {
                return new XacmlContextStatusCode(XacmlConstants.StatusCodes.MissingAttribute);
            }
        }

        /// <summary>
        /// Gets the syntax error code.
        /// </summary>
        public static XacmlContextStatusCode SyntaxError {
            get {
                return new XacmlContextStatusCode(XacmlConstants.StatusCodes.SyntaxError);
            }
        }

        /// <summary>
        /// Gets the parsing error status code.
        /// </summary>
        public static XacmlContextStatusCode ProcessingError {
            get {
                return new XacmlContextStatusCode(XacmlConstants.StatusCodes.ProcessingError);
            }
        }

        #endregion Public Static Properties

        /// <summary>
        /// Gets or sets the status value.
        /// </summary>
        public Uri Value {
            get {
                return this.value;
            }

            set {
                if (value == null) {
                    throw new ArgumentNullException(nameof(value));
                }

                this.value = value;
            }
        }

        /// <summary>
        /// Gets or sets the status code.
        /// </summary>
        public XacmlContextStatusCode StatusCode {
            get {
                return this.statusCode;
            }

            set {
                this.statusCode = value;
            }
        }
    }
}