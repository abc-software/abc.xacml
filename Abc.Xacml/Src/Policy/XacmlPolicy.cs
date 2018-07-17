// ----------------------------------------------------------------------------
// <copyright file="XacmlPolicy.cs" company="ABC Software Ltd">
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
    using System.Collections.Generic;
    using System.Collections.ObjectModel;

    /// <summary>
    /// The <c>XacmlPolicy</c> class element retrieves a bag of values for a named resource attribute from the request context.
    /// </summary>
    /// <remarks>See the xacml:ResourceAttributeDesignator element defined in [XacmlCore2, 5.22][XacmlCore3, 5.14] for more details.</remarks>
    public class XacmlPolicy {
        private readonly ICollection<XacmlRule> rule = new Collection<XacmlRule>();
        private readonly ICollection<XacmlObligation> obligations = new Collection<XacmlObligation>();
        private readonly ICollection<XacmlCombinerParameter> combinerParameters = new Collection<XacmlCombinerParameter>();

        private readonly ICollection<XacmlRuleCombinerParameters> ruleCombinerParameters = new Collection<XacmlRuleCombinerParameters>();
        private readonly ICollection<XacmlVariableDefinition> variableDefinitions = new Collection<XacmlVariableDefinition>();
        private readonly ICollection<XacmlCombinerParameter> choiseCombinerParameters = new Collection<XacmlCombinerParameter>();
        private readonly ICollection<XacmlObligationExpression> obligationExpressions = new Collection<XacmlObligationExpression>();
        private readonly ICollection<XacmlAdviceExpression> adviceExpressions = new Collection<XacmlAdviceExpression>();

        private XacmlTarget target;
        private Uri policyId;
        private Uri ruleCombiningAlgId;
        private string version = "1.0";

        /// <summary>
        /// Initializes a new instance of the <see cref="XacmlPolicy"/> class.
        /// </summary>
        /// <param name="ruleCombiningAlgId">The rule combining algorithm identifier.</param>
        /// <param name="target">The target.</param>
        public XacmlPolicy(Uri ruleCombiningAlgId, XacmlTarget target)
            : this(XacmlUtils.GeneratePolicyId(), ruleCombiningAlgId, target) {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="XacmlPolicy"/> class.
        /// </summary>
        /// <param name="policyId">The policy identifier.</param>
        /// <param name="ruleCombiningAlgId">The rule combining algorithm identifier.</param>
        /// <param name="target">The target.</param>
        public XacmlPolicy(Uri policyId, Uri ruleCombiningAlgId, XacmlTarget target) {
            if (policyId == null) {
                throw new ArgumentNullException(nameof(policyId));
            }

            if (ruleCombiningAlgId == null) {
                throw new ArgumentNullException(nameof(ruleCombiningAlgId));
            }

            if (target == null) {
                throw new ArgumentNullException(nameof(target));
            }

            this.policyId = policyId;
            this.ruleCombiningAlgId = ruleCombiningAlgId;
            this.target = target;
        }

        /// <summary>
        /// Gets or sets the name of the description.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the policy issuer.
        /// </summary>
        /// <value>
        /// The policy issuer.
        /// </value>
        /// <remarks>
        /// Used only for XACML 3.0
        /// </remarks>
        public XacmlPolicyIssuer PolicyIssuer { get; set; }

        /// <summary>
        /// Gets or sets the XPath version.
        /// </summary>
        /// <value>
        /// The XPath version.
        /// </value>
        public Uri XPathVersion { get; set; }

        /// <summary>
        /// Gets or sets the name of the target.
        /// </summary>
        public XacmlTarget Target {
            get {
                return this.target;
            }

            set {
                if (value == null) {
                    throw new ArgumentNullException(nameof(value));
                }

                this.target = value;
            }
        }

        /// <summary>
        /// Gets the combiner parameters.
        /// </summary>
        /// <value>
        /// The combiner parameters.
        /// </value>
        public ICollection<XacmlCombinerParameter> CombinerParameters {
            get {
                return this.combinerParameters;
            }
        }

        /// <summary>
        /// Gets the rule combiner parameters.
        /// </summary>
        /// <value>
        /// The rule combiner parameters.
        /// </value>
        public ICollection<XacmlRuleCombinerParameters> RuleCombinerParameters {
            get {
                return this.ruleCombinerParameters;
            }
        }

        /// <summary>
        /// Gets the variable definitions.
        /// </summary>
        /// <value>
        /// The variable definitions.
        /// </value>
        public ICollection<XacmlVariableDefinition> VariableDefinitions {
            get {
                return this.variableDefinitions;
            }
        }

        public ICollection<XacmlCombinerParameter> ChoiceCombinerParameters {
            get {
                return this.choiseCombinerParameters;
            }
        }

        /// <summary>
        /// Gets the rules.
        /// </summary>
        /// <value>
        /// The rules.
        /// </value>
        public ICollection<XacmlRule> Rules {
            get {
                return this.rule;
            }
        }

        /// <summary>
        /// Gets the obligations.
        /// </summary>
        /// <value>
        /// The obligations.
        /// </value>
        /// <remarks>
        /// Used only for XACML 1.0/1.1/2.0
        /// </remarks>
        public ICollection<XacmlObligation> Obligations {
            get {
                return this.obligations;
            }
        }

        /// <summary>
        /// Gets or sets the name of the PolicyId.
        /// </summary>
        public Uri PolicyId {
            get {
                return this.policyId;
            }

            set {
                if (value == null) {
                    throw new ArgumentNullException(nameof(value));
                }

                this.policyId = value;
            }
        }

        /// <summary>
        /// Gets or sets the version.
        /// </summary>
        public string Version {
            get {
                return this.version;
            }
            set {
                if (value != null) {
                    if (System.Text.RegularExpressions.Regex.IsMatch(value, @"(\d+\.)*\d+")) {
                        this.version = value;
                    }
                    else {
                        throw new ArgumentException("Wrong VersionType format", nameof(value));
                    }
                }
            }
        }

        /// <summary>
        /// Gets or sets the rule combining algorithm.
        /// </summary>
        /// <value>
        /// The rule combining algorithm.
        /// </value>
        public Uri RuleCombiningAlgId {
            get {
                return this.ruleCombiningAlgId;
            }

            set {
                if (value == null) {
                    throw new ArgumentNullException(nameof(value));
                }

                this.ruleCombiningAlgId = value;
            }
        }

        /// <summary>
        /// Gets or sets the maximum delegation depth.
        /// </summary>
        /// <value>
        /// The maximum delegation depth.
        /// </value>
        /// <remarks>
        /// Used only for XACML 3.0
        /// </remarks>
        public int? MaxDelegationDepth { get; set; }

        /// <summary>
        /// Gets the obligation expressions
        /// </summary>
        /// <value>
        /// The obligation expressions.
        /// </value>
        /// <remarks>
        /// Used only for XACML 3.0
        /// </remarks>
        public ICollection<XacmlObligationExpression> ObligationExpressions {
            get {
                return this.obligationExpressions;
            }
        }

        /// <summary>
        /// Gets the advice expressions.
        /// </summary>
        /// <value>
        /// The advice expressions.
        /// </value>
        /// <remarks>
        /// Used only for XACML 3.0
        /// </remarks>
        public ICollection<XacmlAdviceExpression> AdviceExpressions {
            get {
                return this.adviceExpressions;
            }
        }

        internal IDictionary<string, string> Namespaces { get; set; }
    }
}