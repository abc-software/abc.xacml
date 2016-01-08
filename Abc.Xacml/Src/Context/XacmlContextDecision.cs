// ----------------------------------------------------------------------------
// <copyright file="XacmlContextDecision.cs" company="ABC Software Ltd">
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
 
namespace Abc.Xacml.Context {
    /// <summary>
    /// The <c>XacmlContextDecision</c> enumeration contains the result of policy evaluation.
    /// </summary>
    /// <remarks>See the xacml-context:Decision element defined in [XacmlCore, 6.11] for more details.</remarks>
    public enum XacmlContextDecision {
        /// <summary>
        /// Represents a Permit
        /// </summary>
        Permit,

        /// <summary>
        /// Represents a Deny
        /// </summary>
        Deny,

        /// <summary>
        /// Represents a Indeterminate
        /// </summary>
        Indeterminate,

        /// <summary>
        /// Represents a NotApplicable
        /// </summary>
        NotApplicable
    }
}
