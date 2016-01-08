// ----------------------------------------------------------------------------
// <copyright file="XacmlEffectType.cs" company="ABC Software Ltd">
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
    /// <summary>
    /// The <c>XacmlEffectType</c> enum.
    /// </summary>
    /// <remarks>See the xacml:EffectType simple type defined in [XacmlCore2, 5.30][XacmlCore3, 5.22] for more details.</remarks>
    public enum XacmlEffectType {
        /// <summary>
        /// Represents a Permit
        /// </summary>
        Permit,

        /// <summary>
        /// Represents a Deny
        /// </summary>
        Deny,
    }
}