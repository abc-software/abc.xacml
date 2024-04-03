// ----------------------------------------------------------------------------
// <copyright file="XacmlAttributeAssignmentExpression.cs" company="ABC Software Ltd">
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

    public class XacmlAttributeAssignmentExpression : XacmlExpression {
        private Uri attributeId;

        /// <summary>
        /// Initializes a new instance of the <see cref="XacmlAttributeAssignmentExpression"/> class.
        /// </summary>
        /// <param name="attributeId">The attribute identifier.</param>
        /// <param name="expressionElement">The expression element.</param>
        public XacmlAttributeAssignmentExpression(Uri attributeId, IXacmlApply expressionElement) {
            if (attributeId == null) {
                throw new ArgumentNullException(nameof(attributeId));
            }

            if (expressionElement == null) {
                throw new ArgumentNullException(nameof(expressionElement));
            }

            this.attributeId = attributeId;
            this.Property = expressionElement;
        }

        /// <summary>
        /// Gets or sets the category.
        /// </summary>
        /// <value>
        /// The category.
        /// </value>
        public Uri Category { get; set; }

        /// <summary>
        /// Gets or sets the issuer.
        /// </summary>
        /// <value>
        /// The issuer.
        /// </value>
        public string Issuer { get; set; }

        /// <summary>
        /// Gets or sets the attribute identifier.
        /// </summary>
        /// <value>
        /// The attribute identifier.
        /// </value>
        public Uri AttributeId {
            get {
                return this.attributeId;
            }

            set {
                if (value == null) {
                    throw new ArgumentNullException(nameof(value));
                }

                this.attributeId = value;
            }
        }
    }
}
