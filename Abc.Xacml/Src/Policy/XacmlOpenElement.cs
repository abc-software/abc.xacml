// ----------------------------------------------------------------------------
// <copyright file="XacmlOpenElement.cs" company="ABC Software Ltd">
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
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Xml.Linq;

    /// <summary>
    /// Represent <c>##any</c> Xml type.
    /// </summary>
    public abstract class XacmlOpenElement {
        private readonly ICollection<XAttribute> attributes = new Collection<XAttribute>();
        private readonly ICollection<XElement> elements = new Collection<XElement>();

        /// <summary>
        /// Initializes a new instance of the <see cref="XacmlOpenElement"/> class.
        /// </summary>
        protected XacmlOpenElement() {
        }

        /// <summary>
        /// Gets the XML attributes.
        /// </summary>
        public ICollection<XAttribute> Attributes { 
            get { return this.attributes; } 
        }

        /// <summary>
        /// Gets the XML elements.
        /// </summary>
        public ICollection<XElement> Elements { 
            get { return this.elements; } 
        }
    }
}