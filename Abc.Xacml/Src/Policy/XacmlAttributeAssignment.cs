// ----------------------------------------------------------------------------
// <copyright file="XacmlAttributeAssignment.cs" company="ABC Software Ltd">
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
    using System.Diagnostics.Contracts;

    /// <summary>
    /// class XacmlAttributeAssignment
    /// </summary>
    public class XacmlAttributeAssignment : XacmlAttributeValue {
        private Uri attributeId;

        public XacmlAttributeAssignment(Uri attributeId, Uri dataType)
            : base(dataType) {
            Contract.Requires<ArgumentNullException>(attributeId != null);
            Contract.Requires<ArgumentNullException>(dataType != null);

            this.attributeId = attributeId;
        }

        public XacmlAttributeAssignment(Uri attributeId, Uri dataType, string value)
            : base(dataType) {
            Contract.Requires<ArgumentNullException>(attributeId != null);
            Contract.Requires<ArgumentNullException>(dataType != null);
            Contract.Requires<ArgumentNullException>(value != null);

            this.attributeId = attributeId;
            base.Value = value;
        }

        // v 3.0
        public Uri Category { get; set; }
        public string Issuer { get; set; }

        /// <summary>
        /// Gets or sets the AttributeId
        /// </summary>
        public Uri AttributeId {
            get {
                return this.attributeId;
            }

            set {
                Contract.Requires<ArgumentNullException>(value != null);
                this.attributeId = value;
            }
        }

        /// <summary>
        /// Gets or sets the value
        /// </summary>
        public override string Value {
            get {
                return base.Value;
            }

            set {
                Contract.Requires<ArgumentNullException>(value != null);
                base.Value = value;
            }
        }
    }
}