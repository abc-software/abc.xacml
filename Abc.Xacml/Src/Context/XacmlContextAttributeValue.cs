﻿// ----------------------------------------------------------------------------
// <copyright file="XacmlContextAttributeValue.cs" company="ABC Software Ltd">
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
    /// <summary>
    /// The <c>XacmlContextAttributeValue</c> class contains the value of an attribute.
    /// </summary>
    /// <remarks>See the xacml-context:AttributeValue element defined in [XacmlCore, 6.8] for more details.</remarks>
    public class XacmlContextAttributeValue {
        // UNDONE: xs:any, xs:anyAttribute

        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        public string Value { get; set; }
    }
}