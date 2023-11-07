// ----------------------------------------------------------------------------
// <copyright file="XacmlApply.cs" company="ABC Software Ltd">
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

namespace Abc.Xacml.Policy {
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;

    /// <summary>
    /// The <c>XacmlApply</c> class element denotes application of a function to its arguments, thus encoding a function call.
    /// </summary>
    /// <remarks>See the xacml:Apply element defined in [XacmlCore, 5.25][XacmlCore2, 5.35][XacmlCore3, 5.27] for more details.</remarks>
    public class XacmlApply : IXacmlApply {
        private readonly ICollection<IXacmlApply> parameters = new Collection<IXacmlApply>();
        private Uri functionId;

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        /// <value>
        /// The description.
        /// </value>
        /// <remarks>
        /// Used only for XACML 3.0
        /// </remarks>
        public string Description { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="XacmlApply"/> class.
        /// </summary>
        /// <param name="functionId">The function identifier.</param>
        public XacmlApply(Uri functionId) {
            if (functionId == null) {
                throw new ArgumentNullException(nameof(functionId));
            }

            this.functionId = functionId;
        }

        /// <summary>
        /// Gets the parameters.
        /// </summary>
        /// <value>
        /// The parameters.
        /// </value>
        public ICollection<IXacmlApply> Parameters {
            get {
                return this.parameters;
            }
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
                if (value == null) {
                    throw new ArgumentNullException(nameof(value));
                }

                this.functionId = value;
            }
        }
    }
}