﻿// ----------------------------------------------------------------------------
// <copyright file="XacmlContextAttributes.cs" company="ABC Software Ltd">
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

namespace Abc.Xacml.Context {
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using Abc.Xacml.Policy;

    public class XacmlContextAttributes {
        private readonly ICollection<XacmlAttribute> attributes = new Collection<XacmlAttribute>();
        private Uri category;

        /// <summary>
        /// Initializes a new instance of the <see cref="XacmlContextAttributes"/> class.
        /// </summary>
        /// <param name="category">The category.</param>
        public XacmlContextAttributes(Uri category)
            : this(category, null) {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="XacmlContextAttributes" /> class.
        /// </summary>
        /// <param name="category">The category.</param>
        /// <param name="attributes">The attributes.</param>
        /// <exception cref="System.ArgumentNullException">category</exception>
        public XacmlContextAttributes(Uri category, IEnumerable<XacmlAttribute> attributes) {
            if (category == null) {
                throw new ArgumentNullException(nameof(category));
            }

            this.Category = category;
            if (attributes != null) {
                foreach (var item in attributes) {
                    this.attributes.Add(item);
                }
            }
        }

        /// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        /// <value>
        /// The identifier.
        /// </value>
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the content.
        /// </summary>
        /// <value>
        /// The content.
        /// </value>
        public string Content { get; set; }

        /// <summary>
        /// Gets or sets the attributes.
        /// </summary>
        /// <value>
        /// The attributes.
        /// </value>
        public ICollection<XacmlAttribute> Attributes {
            get {
                return this.attributes;
            }
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
                if (value == null) {
                    throw new ArgumentNullException(nameof(value));
                }

                this.category = value;
            }
        }
    }
}
