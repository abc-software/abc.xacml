// ----------------------------------------------------------------------------
// <copyright file="XacmlContextAttribute.cs" company="ABC Software Ltd">
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

    /// <summary>
    /// The <c>XacmlContextAttribute</c> class contains an attribute value and attribute meta-data.
    /// </summary>
    /// <remarks>See the xacml-context:Attribute element defined in [XacmlCore, 6.7] for more details.</remarks>
    public class XacmlContextAttribute {
        private Uri attributeId;
        private Uri dataType;
        private readonly ICollection<XacmlContextAttributeValue> attributeValues;

        /// <summary>
        /// Initializes a new instance of the <see cref="XacmlContextAttribute"/> class.
        /// </summary>
        /// <param name="attributeId">The attribute identifier.</param>
        /// <param name="dataType">Type of the data.</param>
        /// <param name="attributeValue">The attribute value.</param>
        /// <remarks>Used only for XACML1.0/1.1</remarks>
        public XacmlContextAttribute(Uri attributeId, Uri dataType, XacmlContextAttributeValue attributeValue) {
            if (attributeId == null) {
                throw new ArgumentNullException(nameof(attributeId));
            }

            if (dataType == null) {
                throw new ArgumentNullException(nameof(dataType));
            }

            if (attributeValue == null) {
                throw new ArgumentNullException(nameof(attributeValue));
            }

            this.attributeId = attributeId;
            this.dataType = dataType;
            this.attributeValues = new List<XacmlContextAttributeValue>() { attributeValue };
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="XacmlContextAttribute"/> class.
        /// </summary>
        /// <param name="attributeId">The attribute identifier.</param>
        /// <param name="dataType">Type of the data.</param>
        /// <param name="attributeValues">The attribute values.</param>
        /// <remarks>Used only for XACML2.0/3.0</remarks>
        public XacmlContextAttribute(Uri attributeId, Uri dataType, ICollection<XacmlContextAttributeValue> attributeValues) {
            if (attributeId == null) {
                throw new ArgumentNullException(nameof(attributeId));
            }

            if (dataType == null) {
                throw new ArgumentNullException(nameof(dataType));
            }

            if (attributeValues == null) {
                throw new ArgumentNullException(nameof(attributeValues));
            }

            this.attributeId = attributeId;
            this.dataType = dataType;
            this.attributeValues = attributeValues;
        }

        /// <summary>
        /// Gets or sets the Issuer
        /// </summary>
        public string Issuer { get; set; }

        /// <summary>
        /// Gets or sets the issue instant.
        /// </summary>
        /// <remarks>Used obly for XACML1.0/1.1</remarks>
        public DateTime? IssueInstant { get; set; }

        /// <summary>
        /// Gets the attribute identifier.
        /// </summary>
        public Uri AttributeId {
            get {
                return this.attributeId;
            }

            private set {
                if (value == null) {
                    throw new ArgumentNullException(nameof(value));
                }

                this.attributeId = value;
            }
        }

        /// <summary>
        /// Gets or sets the DataType.
        /// </summary>
        public Uri DataType {
            get {
                return this.dataType;
            }

            set {
                if (value == null) {
                    throw new ArgumentNullException(nameof(value));
                }

                this.dataType = value;
            }
        }

        /// <summary>
        /// Gets the attribute values.
        /// </summary>
        /// <remarks>Used only first element for XACML1.0/1.1</remarks>
        public ICollection<XacmlContextAttributeValue> AttributeValues {
            get {
                return this.attributeValues;
            }
        }
    }
}