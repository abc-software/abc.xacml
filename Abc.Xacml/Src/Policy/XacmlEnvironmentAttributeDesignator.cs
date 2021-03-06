﻿// ----------------------------------------------------------------------------
// <copyright file="XacmlEnvironmentAttributeDesignator.cs" company="ABC Software Ltd">
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
    /// The <c>XacmlEnvironmentAttributeDesignator</c> class element retrieves a bag of values for a named environment attribute from the request context.
    /// </summary>
    /// <remarks>
    /// See the xacml:EnvironmentAttributeDesignator element defined in [XacmlCore2, 5.41] for more details.
    /// Used only from XACML 1.0/1.1/2.0
    /// </remarks>
    public class XacmlEnvironmentAttributeDesignator : XacmlAttributeDesignator {
        /// <summary>
        /// Initializes a new instance of the <see cref="XacmlEnvironmentAttributeDesignator"/> class.
        /// </summary>
        /// <param name="attributeId">The AttributeId with which to match the attribute.</param>
        /// <param name="dataType">The attribute element data type.</param>
        public XacmlEnvironmentAttributeDesignator(Uri attributeId, Uri dataType)
            : base(attributeId, dataType) {
        }
    }
}