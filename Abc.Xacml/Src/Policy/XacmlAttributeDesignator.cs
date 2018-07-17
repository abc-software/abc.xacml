// ----------------------------------------------------------------------------
// <copyright file="XacmlAttributeDesignator.cs" company="ABC Software Ltd">
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

    /// <summary>
    /// class XacmlActionAttributeDesignator
    /// </summary>
    public class XacmlAttributeDesignator : IXacmlApply {
        private Uri attributeId;
        private Uri dataType;
        private string issuer;
        private bool? mustBePresent;
        private Uri category;

        /// <summary>
        /// Initializes a new instance of the <see cref="XacmlAttributeDesignator"/> class.
        /// </summary>
        /// <param name="attributeId">The AttributeId with which to match the attribute.</param>
        /// <param name="dataType">The attribute element data type.</param>
        /// <remarks>
        /// Used only from XACML 1.0/1.1/2.0
        /// </remarks>
        public XacmlAttributeDesignator(Uri attributeId, Uri dataType) {
            if (attributeId == null) {
                throw new ArgumentNullException(nameof(attributeId));
            }
            if (dataType == null) {
                throw new ArgumentNullException(nameof(dataType));
            }

            this.attributeId = attributeId;
            this.dataType = dataType;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="XacmlAttributeDesignator" /> class.
        /// </summary>
        /// <param name="category">The Category with which to match the attribute.</param>
        /// <param name="attributeId">The AttributeId with which to match the attribute.</param>
        /// <param name="dataType">The attribute element data type.</param>
        /// <param name="mustBePresent">The must be present.</param>
        /// <remarks>
        /// Used only from XACML 3.0
        /// </remarks>
        public XacmlAttributeDesignator(Uri category, Uri attributeId, Uri dataType, bool mustBePresent) {
            if (category == null) {
                throw new ArgumentNullException(nameof(category));
            }

            if (attributeId == null) {
                throw new ArgumentNullException(nameof(attributeId));
            }

            if (dataType == null) {
                throw new ArgumentNullException(nameof(dataType));
            }

            this.attributeId = attributeId;
            this.dataType = dataType;
            this.mustBePresent = mustBePresent;
            this.category = category;
        }

        /// <summary>
        /// Gets or sets the AttributeId with which to match the attribute
        /// </summary>
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
        /// Gets or sets the data type.
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
        /// Gets or sets the Issuer with which to match the attribute
        /// </summary>
        public string Issuer {
            get {
                return this.issuer;
            }

            set {
                this.issuer = value;
            }
        }

        /// <summary>
        /// Gets or sets the value indicating whether the item is mustBePresent.
        /// </summary>
        public bool? MustBePresent {
            get {
                return this.mustBePresent;
            }

            set {
                this.mustBePresent = value;
            }
        }

        /// <summary>
        /// Gets or sets the Category with which to match the attribute.
        /// </summary>
        public Uri Category {
            get {
                return this.category;
            }

            set {
                this.category = value;
            }
        }
    }
}