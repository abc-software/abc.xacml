// ----------------------------------------------------------------------------
// <copyright file="XacmlAnyOf.cs" company="ABC Software Ltd">
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
    using System.Collections.Generic;
    using System.Collections.ObjectModel;

    /// <summary>
    /// A disjunctive sequence of &lt;AllOf&gt; elements
    /// </summary>
    /// <remarks>
    /// See the xacml:EnvironmentAttributeDesignator element defined in [XacmlCore3, 5.7] for more details.
    /// Used only from XACML 3.0
    /// </remarks>
    public class XacmlAnyOf {
        private readonly ICollection<XacmlAllOf> allOf = new NoNullCollection<XacmlAllOf>();

        /// <summary>
        /// Initializes a new instance of the <see cref="XacmlAnyOf"/> class.
        /// </summary>
        /// <param name="allOf">All of.</param>
        public XacmlAnyOf(IEnumerable<XacmlAllOf> allOf) {
            if (allOf == null) {
                throw new ArgumentNullException(nameof(allOf));
            }

            foreach (var item in allOf) {
                this.allOf.Add(item);
            }
        }

        /// <summary>
        /// Gets a value AllOf.
        /// </summary>
        public ICollection<XacmlAllOf> AllOf {
            get {
                return this.allOf;
            }
        }

        private class NoNullCollection<T> : Collection<T> {
            protected override void InsertItem(int index, T item) {
                if (item == null) {
                    throw new ArgumentNullException(nameof(item));
                }

                base.InsertItem(index, item);
            }
        }
    }
}

