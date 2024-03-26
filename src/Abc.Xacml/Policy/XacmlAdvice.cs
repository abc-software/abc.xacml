// ----------------------------------------------------------------------------
// <copyright file="XacmlAdvice.cs" company="ABC Software Ltd">
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
    /// class XacmlAdvice
    /// </summary>
    public class XacmlAdvice {
        private readonly ICollection<XacmlAttributeAssignment> attributeAssignment = new Collection<XacmlAttributeAssignment>();
        private Uri adviceId;

        /// <summary>
        /// Initializes a new instance of the <see cref="XacmlAdvice"/> class.
        /// </summary>
        /// <param name="adviceId">The advice identifier.</param>
        /// <param name="attributeAssigments">The attribute assigments.</param>
        public XacmlAdvice(Uri adviceId, IEnumerable<XacmlAttributeAssignment> attributeAssigments) {
            if (adviceId == null) {
                throw new ArgumentNullException(nameof(adviceId));
            }

            this.adviceId = adviceId;

            foreach (var item in attributeAssigments) {
                if (item == null) {
                    throw new ArgumentException("Enumeration cannot contains null values.", nameof(attributeAssigments));
                }

                this.attributeAssignment.Add(item);
            }
        }

        /// <summary>
        /// Gets the attribute assignment.
        /// </summary>
        /// <value>
        /// The attribute assignment.
        /// </value>
        public ICollection<XacmlAttributeAssignment> AttributeAssignment {
            get {
                return this.attributeAssignment;
            }
        }

        /// <summary>
        /// Gets or sets the advice identifier.
        /// </summary>
        /// <value>
        /// The advice identifier.
        /// </value>
        public Uri AdviceId {
            get {
                return this.adviceId;
            }

            set {
                if (value == null) {
                    throw new ArgumentNullException(nameof(value));
                }

                this.adviceId = value;
            }
        }
    }
}