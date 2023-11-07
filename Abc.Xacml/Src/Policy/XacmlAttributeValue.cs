// ----------------------------------------------------------------------------
// <copyright file="XacmlAttributeValue.cs" company="ABC Software Ltd">
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

    /// <summary>
    /// The <c>XacmlAttributeValue</c> class contain a literal attribute value.
    /// </summary>
    /// <remarks>See the xacml:AttributeValue element defined in [XacmlCore2, 5.43][XacmlCore3, 5.31] for more details.</remarks>
    public class XacmlAttributeValue : XacmlOpenElement, IXacmlApply {
        private Uri dataType;
        private string value;

        /// <summary>
        /// Initializes a new instance of the <see cref="XacmlAttributeValue"/> class.
        /// </summary>
        /// <param name="dataType">Type of the data.</param>
        public XacmlAttributeValue(Uri dataType) {
            if (dataType == null) {
                throw new ArgumentNullException(nameof(dataType));
            }

            this.dataType = dataType;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="XacmlAttributeValue"/> class.
        /// </summary>
        /// <param name="dataType">Type of the data.</param>
        /// <param name="value">The value.</param>
        public XacmlAttributeValue(Uri dataType, string value) {
            if (dataType == null) {
                throw new ArgumentNullException(nameof(dataType));
            }

            if (value == null) {
                throw new ArgumentNullException(nameof(value));
            }

            this.dataType = dataType;
            this.value = value;
        }

        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        public virtual string Value {
            get {
                return this.value;
            }

            set {
                this.value = value;
            }
        }

        /// <summary>
        /// Gets or sets the type of the data.
        /// </summary>
        /// <value>
        /// The type of the data.
        /// </value>
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
    }
}