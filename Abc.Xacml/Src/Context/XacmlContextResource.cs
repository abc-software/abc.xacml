// ----------------------------------------------------------------------------
// <copyright file="XacmlContextResource.cs" company="ABC Software Ltd">
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
 
namespace Abc.Xacml.Context {
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// The <c>XacmlContextResource</c> class associated with the resource.
    /// </summary>
    /// <remarks>See the xacml-context:AttributeValue element defined in [XacmlCore, 6.3] for more details.</remarks>
    public class XacmlContextResource : XacmlContextBase {
        /// <summary>
        /// Initializes a new instance of the <see cref="XacmlContextResource"/> class.
        /// </summary>
        public XacmlContextResource()
            : base() {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="XacmlContextResource"/> class.
        /// </summary>
        /// <param name="attributes">The attribute.</param>
        public XacmlContextResource(XacmlContextAttribute attribute)
            : this(new XacmlContextAttribute[] { attribute }) {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="XacmlContextResource"/> class.
        /// </summary>
        /// <param name="attributes">The attributes.</param>
        public XacmlContextResource(IEnumerable<XacmlContextAttribute> attributes)
            : base(attributes) {
        }

        /// <summary>
        /// Gets or sets the content of the resource.
        /// </summary>
        /// <remarks>See [XacmlCore, 6.4] for more details.</remarks>
        public XacmlContextResourceContent ResourceContent { get; set; }
    }
}
