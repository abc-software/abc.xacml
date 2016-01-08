// ----------------------------------------------------------------------------
// <copyright file="XacmlRuleCombinerParameters.cs" company="ABC Software Ltd">
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
    /// The <c>XacmlRuleCombinerParameters</c> class element conveys parameters associated with a particular rule within a policy for a rule-combining algorithm.
    /// </summary>
    /// <remarks>See the xacml:RuleCombinerParameters element defined in [XacmlCore3, 5.18] for more details.</remarks>
    /// <remarks>
    /// Used only from XACML 3.0
    /// </remarks>
    public class XacmlRuleCombinerParameters : XacmlCombinerParameters {
        private string idRef;

        /// <summary>
        /// Initializes a new instance of the <see cref="XacmlRuleCombinerParameters"/> class.
        /// </summary>
        /// <param name="ruleIdRef">The rule identifier reference.</param>
        public XacmlRuleCombinerParameters(string ruleIdRef)
            : base() {
            Contract.Requires<ArgumentNullException>(ruleIdRef != null);
            this.idRef = ruleIdRef;
        }

        /// <summary>
        /// Gets or sets the rule identifier reference.
        /// </summary>
        /// <value>
        /// The rule identifier reference.
        /// </value>
        public string RuleIdRef {
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
