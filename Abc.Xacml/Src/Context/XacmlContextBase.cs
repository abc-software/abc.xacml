// ----------------------------------------------------------------------------
// <copyright file="XacmlContextBase.cs" company="ABC Software Ltd">
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
    using System.Collections.Generic;
    using System.Collections.ObjectModel;

    /// <summary>
    /// The <c>XacmlContextBase</c> abstract class for request.
    /// </summary>
    public abstract class XacmlContextBase {
        private readonly ICollection<XacmlContextAttribute> attributes = new Collection<XacmlContextAttribute>();

        /// <summary>
        /// Initializes a new instance of the <see cref="XacmlContextBase"/> class.
        /// </summary>
        protected XacmlContextBase() {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="XacmlContextBase"/> class.
        /// </summary>
        /// <param name="attributes">The attributes.</param>
        protected XacmlContextBase(IEnumerable<XacmlContextAttribute> attributes) {
            if (attributes == null) {
                throw new ArgumentNullException(nameof(attributes));
            }

            foreach (var item in attributes) {
                this.attributes.Add(item);
            }
        }

        /// <summary>
        /// Gets the attributes.
        /// </summary>
        public ICollection<XacmlContextAttribute> Attributes {
            get {
                return this.attributes;
            }
        }
    }
}
