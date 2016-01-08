// ----------------------------------------------------------------------------
// <copyright file="XacmlFunction.cs" company="ABC Software Ltd">
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
 
namespace Abc.Xacml.Policy {
    using System;
    using System.Diagnostics.Contracts;

    /// <summary>
    /// The <c>XacmlEnvironmentAttributeDesignator</c> class element retrieves a bag of values for a named environment attribute from the request context.
    /// </summary>
    /// <remarks>
    /// See the xacml:EnvironmentAttributeDesignator element defined in [XacmlCore2, 5.36][XacmlCore3, 5.28] for more details.
    /// Used only for XACML 2.0/3.0
    /// </remarks>
    public class XacmlFunction : IXacmlApply {
        private Uri functionId;

        /// <summary>
        /// Initializes a new instance of the <see cref="XacmlFunction"/> class.
        /// </summary>
        /// <param name="functionId">The function identifier.</param>
        public XacmlFunction(Uri functionId) {
            Contract.Requires<ArgumentNullException>(functionId != null);
            this.functionId = functionId;
        }

        /// <summary>
        /// Gets or sets the function identifier.
        /// </summary>
        /// <value>
        /// The function identifier.
        /// </value>
        public Uri FunctionId {
            get {
                return this.functionId;
            }

            set {
                Contract.Requires<ArgumentNullException>(value != null);
                this.functionId = value;
            }
        }
    }
}