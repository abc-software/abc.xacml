// ----------------------------------------------------------------------------
// <copyright file="XacmlAttributeSelector.cs" company="ABC Software Ltd">
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
    /// The <c>XacmlAttributeSelector</c> class identifies attributes by their location in the request context.
    /// </summary>
    /// <remarks>See the xacml:AttributeSelector element defined in [XacmlCore2, 5.42][XacmlCore3, 5.30] for more details.</remarks>
    public class XacmlAttributeSelector : IXacmlApply {
        private Uri dataType;
        private bool? mustBePresent;
        private Uri category;
        private string path;

        /// <summary>
        /// Initializes a new instance of the <see cref="XacmlAttributeSelector"/> class.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="dataType">Type of the data.</param>
        /// <param name="mustBePresent">The must be present.</param>
        /// <remarks>
        /// Used only from XACML 1.0/1.1/2.0
        /// </remarks>
        public XacmlAttributeSelector(string path, Uri dataType) {
            Contract.Requires<ArgumentNullException>(path != null);
            Contract.Requires<ArgumentNullException>(dataType != null);

            this.path = path;
            this.dataType = dataType;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="XacmlAttributeSelector"/> class.
        /// </summary>
        /// <param name="category">The category.</param>
        /// <param name="path">The path.</param>
        /// <param name="dataType">Type of the data.</param>
        /// <param name="mustBePresent">if set to <c>true</c> [must be present].</param>
        /// <remarks>
        /// Used only from XACML 3.0
        /// </remarks>
        public XacmlAttributeSelector(Uri category, string path, Uri dataType, bool mustBePresent) {
            Contract.Requires<ArgumentNullException>(category != null);
            Contract.Requires<ArgumentNullException>(path != null);
            Contract.Requires<ArgumentNullException>(dataType != null);

            this.dataType = dataType;
            this.category = category;
            this.path = path;
            this.mustBePresent = mustBePresent;
        }

        /// <summary>
        /// Gets or sets the category.
        /// </summary>
        /// <value>
        /// The category.
        /// </value>
        public Uri Category {
            get {
                return this.category;
            }

            set {
                Contract.Requires<ArgumentNullException>(value != null);
                this.category = value;
            }
        }

        /// <summary>
        /// Gets or sets the context selector identifier.
        /// </summary>
        /// <value>
        /// The context selector identifier.
        /// </value>
        /// <remarks>
        /// Used only from XACML 3.0
        /// </remarks>
        public Uri ContextSelectorId { get; set; }

        /// <summary>
        /// Gets or sets the (RequestContextPath|Path)
        /// </summary>
        public string Path {
            get {
                return this.path;
            }

            set {
                Contract.Requires<ArgumentNullException>(!string.IsNullOrEmpty(value));
                this.path = value;
            }
        }

        /// <summary>
        /// Gets or sets the DataType
        /// </summary>
        public Uri DataType {
            get {
                return this.dataType;
            }

            set {
                Contract.Requires<ArgumentNullException>(value != null);
                this.dataType = value;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the item is mustBePresent.
        /// </summary>
        public bool? MustBePresent {
            get {
                return this.mustBePresent;
            }

            set {
                this.mustBePresent = value;
            }
        }
    }
}