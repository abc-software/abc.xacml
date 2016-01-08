// ----------------------------------------------------------------------------
// <copyright file="IXacmlPolicyRepository.cs" company="ABC Software Ltd">
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

namespace Abc.Xacml.Runtime {
    using System;
    using Abc.Xacml.Policy;

    /// <summary>
    /// The exteranl XACML policy repository.
    /// </summary>
    public interface IXacmlPolicyRepository {
        /// <summary>
        /// Requests the policy.
        /// </summary>
        /// <param name="policyId">The policy identifier.</param>
        /// <returns>The <see cref="XacmlPolicy"/>.</returns>
        XacmlPolicy RequestPolicy(Uri policyId);

        /// <summary>
        /// Requests the policy set.
        /// </summary>
        /// <param name="policySetId">The policy set identifier.</param>
        /// <returns>The <see cref="XacmlPolicySet"/>.</returns>
        XacmlPolicySet RequestPolicySet(Uri policySetId);
    }
}
