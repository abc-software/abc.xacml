// ----------------------------------------------------------------------------
// <copyright file="XacmlAttribute.cs" company="ABC Software Ltd">
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

    public class XacmlAttribute {
        private readonly ICollection<XacmlAttributeValue> attributeValues = new Collection<XacmlAttributeValue>();
        private Uri attributeId;

        public XacmlAttribute(Uri attributeId, bool includeInResult) {
            if (attributeId == null) {
                throw new ArgumentNullException(nameof(attributeId));
            }

            this.attributeId = attributeId;
            this.IncludeInResult = includeInResult;
        }

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

        /// <summary>
        /// Gets or sets a value indicating whether [include in result].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [include in result]; otherwise, <c>false</c>.
        /// </value>
        public bool IncludeInResult { get; set; }

        /// <summary>
        /// Gets the attribute values.
        /// </summary>
        /// <value>
        /// The attribute values.
        /// </value>
        public ICollection<XacmlAttributeValue> AttributeValues {
            get {
                return this.attributeValues;
            }
        }
    }
}
