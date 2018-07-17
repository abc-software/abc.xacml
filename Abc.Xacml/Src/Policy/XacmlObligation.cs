// ----------------------------------------------------------------------------
// <copyright file="XacmlObligation.cs" company="ABC Software Ltd">
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
    /// The <c>XacmlObligation</c> class contain an identifier for the obligation and a set of attributes that form arguments of the action defined by the obligation.
    /// </summary>
    /// <remarks>See the xacml:Obligation element defined in [XacmlCore1, 5.35][XacmlCore2, 5.44][XacmlCore3, 5.34] for more details.</remarks>
    public class XacmlObligation {
        private readonly ICollection<XacmlAttributeAssignment> attributeAssignment = new Collection<XacmlAttributeAssignment>();
        private Uri obligationId;

        /// <summary>
        /// Initializes a new instance of the <see cref="XacmlObligation"/> class.
        /// </summary>
        /// <param name="fulfillOn">The fulfill on.</param>
        /// <param name="attributeAssigments">The attribute assigments.</param>
        /// <remarks>
        /// Used only for XACML 1.0/1.1/2.0
        /// </remarks>
        public XacmlObligation(XacmlEffectType fulfillOn, IEnumerable<XacmlAttributeAssignment> attributeAssigments)
            : this(XacmlUtils.GenerateObligationId(), fulfillOn, attributeAssigments) {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="XacmlObligation"/> class.
        /// </summary>
        /// <param name="obligationId">The obligation identifier.</param>
        /// <param name="fulfillOn">The fulfill on.</param>
        /// <param name="attributeAssigments">The attribute assigments.</param>
        /// <remarks>
        /// Used only for XACML 1.0/1.1/2.0
        /// </remarks>
        public XacmlObligation(Uri obligationId, XacmlEffectType fulfillOn, IEnumerable<XacmlAttributeAssignment> attributeAssigments) {
            if (obligationId == null) {
                throw new ArgumentNullException(nameof(obligationId));
            }

            this.obligationId = obligationId;
            this.FulfillOn = fulfillOn;

            foreach (var item in attributeAssigments) {
                if (item == null) {
                    throw new ArgumentException("Enumeration cannot contains null values.", nameof(attributeAssigments));
                }

                this.attributeAssignment.Add(item);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="XacmlObligation"/> class.
        /// </summary>
        /// <param name="obligationId">The obligation identifier.</param>
        /// <param name="attributeAssigments">The attribute assigments.</param>
        /// <remarks>
        /// Used only for XACML 3.0
        /// </remarks>
        public XacmlObligation(Uri obligationId, IEnumerable<XacmlAttributeAssignment> attributeAssigments)
            : this(obligationId, XacmlEffectType.Deny, attributeAssigments) {
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
        /// Gets or sets the obligation identifier.
        /// </summary>
        /// <value>
        /// The obligation identifier.
        /// </value>
        public Uri ObligationId {
            get {
                return this.obligationId;
            }

            set {
                if (value == null) {
                    throw new ArgumentNullException(nameof(value));
                }

                this.obligationId = value;
            }
        }

        /// <summary>
        /// Gets or sets the fulfill on.
        /// </summary>
        /// <value>
        /// The fulfill on.
        /// </value>
        public XacmlEffectType FulfillOn { get; set; }
    }
}