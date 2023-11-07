// ----------------------------------------------------------------------------
// <copyright file="XacmlContextResourceContent.cs" company="ABC Software Ltd">
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
    /// The <c>XacmlContextResourceContent</c> class is a notional placeholder for the content of the resource.
    /// </summary>
    /// <remarks>See the xacml-context:ResourceContent element defined in [XacmlCore, 6.6] for more details.</remarks>
    public class XacmlContextResourceContent {
        // UNDONE: xs:any, xs:anyAttribute

        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        public string Value { get; set; }
    }
}
