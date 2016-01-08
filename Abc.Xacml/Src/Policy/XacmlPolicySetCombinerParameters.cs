// ----------------------------------------------------------------------------
// <copyright file="XacmlPolicySetCombinerParameters.cs" company="ABC Software Ltd">
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
    /// The <c>XacmlPolicySetCombinerParameters</c> class element conveys parameters associated with a particular policy set within a policy set for a policy-combining algorithm.
    /// </summary>
    /// <remarks>See the xacml:PolicySetCombinerParameters element defined in [XacmlCore3, 5.20] for more details.</remarks>
    /// <remarks>
    /// Used only from XACML 3.0
    /// </remarks>
    public class XacmlPolicySetCombinerParameters : XacmlCombinerParameters {
        private Uri idRef;

        /// <summary>
        /// Initializes a new instance of the <see cref="XacmlPolicySetCombinerParameters"/> class.
        /// </summary>
        /// <param name="policySetIdRef">The policy set identifier reference.</param>
        public XacmlPolicySetCombinerParameters(Uri policySetIdRef)
            : base() {
            Contract.Requires<ArgumentNullException>(policySetIdRef != null);
            this.idRef = policySetIdRef;
        }

        /// <summary>
        /// Gets or sets the policy set identifier reference.
        /// </summary>
        /// <value>
        /// The policy set identifier reference.
        /// </value>
        public Uri PolicySetIdRef {
            get {
                return this.idRef;
            }

            set {
                Contract.Requires<ArgumentNullException>(value != null);
                this.idRef = value;
            }
        }
    }
}
