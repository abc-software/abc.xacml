// ----------------------------------------------------------------------------
// <copyright file="XacmlPolicySet.cs" company="ABC Software Ltd">
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
    using Abc.Xacml.Context;
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Xml;

    /// <summary>
    /// The <c>XacmlPolicySet</c> class is an aggregation of other policy sets and policies.
    /// </summary>
    /// <remarks>See the xacml:XacmlPolicySet element defined in [XacmlCore, 5.1] for more details.</remarks>
    public class XacmlPolicySet {
        public IDictionary<string, string> Namespaces { get; set; }
        private readonly ICollection<XacmlPolicy> policies = new Collection<XacmlPolicy>();
        private readonly ICollection<XacmlPolicySet> policySets = new Collection<XacmlPolicySet>();
        private readonly ICollection<Uri> policyIdReferences = new Collection<Uri>();
        private readonly ICollection<Uri> policySetIdReferences = new Collection<Uri>();
        private readonly ICollection<XacmlObligation> obligations = new Collection<XacmlObligation>();

        private readonly ICollection<XacmlCombinerParameter> combinerParameters = new Collection<XacmlCombinerParameter>();
        private readonly ICollection<XacmlPolicyCombinerParameters> policyCombinerParameters = new Collection<XacmlPolicyCombinerParameters>();
        private readonly ICollection<XacmlPolicySetCombinerParameters> policySetCombinerParameters = new Collection<XacmlPolicySetCombinerParameters>();

        private XacmlTarget target;
        private Uri policySetId;
        private Uri policyCombiningAlgId;
        private string version = "1.0";

        // v 3.0
        public XacmlPolicyIssuer PolicyIssuer { get; set; }
        public int? MaxDelegationDepth { get; set; }
        private readonly ICollection<XacmlObligationExpression> obligations_3_0 = new Collection<XacmlObligationExpression>();
        private readonly ICollection<XacmlAdviceExpression> advices = new Collection<XacmlAdviceExpression>();

        private readonly ICollection<XacmlContextPolicyIdReference> policyIdReferences_3_0 = new Collection<XacmlContextPolicyIdReference>();
        private readonly ICollection<XacmlContextPolicySetIdReference> policySetIdReferences_3_0 = new Collection<XacmlContextPolicySetIdReference>();

        /// <summary>
        /// Initializes a new instance of the <see cref="XacmlPolicySet"/> class.
        /// </summary>
        /// <param name="policyCombiningAlgId">The policy combining alg identifier.</param>
        /// <param name="target">The target.</param>
        public XacmlPolicySet(Uri policyCombiningAlgId, XacmlTarget target)
            : this(XacmlUtils.GeneratePolicySetId(), policyCombiningAlgId, target) {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="XacmlPolicySet"/> class.
        /// </summary>
        /// <param name="policySetId">The policy set identifier.</param>
        /// <param name="policyCombiningAlgId">The policy combining alg identifier.</param>
        /// <param name="target">The target.</param>
        public XacmlPolicySet(Uri policySetId, Uri policyCombiningAlgId, XacmlTarget target) {
            if (policySetId == null) {
                throw new ArgumentNullException(nameof(policySetId));
            }

            if (policyCombiningAlgId == null) {
                throw new ArgumentNullException(nameof(policyCombiningAlgId));
            }

            if (target == null) {
                throw new ArgumentNullException(nameof(target));
            }

            this.policySetId = policySetId;
            this.policyCombiningAlgId = policyCombiningAlgId;
            this.target = target;
        }

        /// <summary>
        /// Gets or sets the Description. 
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the XPath version.
        /// </summary>
        public Uri XPathVersion { get; set; }

        /// <summary>
        /// Gets or sets the Target. 
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
        /// Gets Obligations. 
        /// </summary>
        public ICollection<XacmlObligation> Obligations {
            get {
                return this.obligations;
            }
        }

        /// <summary>
        /// Gets or sets the PolicySetId. 
        /// </summary>
        public Uri PolicySetId {
            get {
                return this.policySetId;
            }

            set {
                if (value == null) {
                    throw new ArgumentNullException(nameof(value));
                }
                this.policySetId = value;
            }
        }

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
        /// Gets or sets the PolicyCombiningAlgId. 
        /// </summary>
        public Uri PolicyCombiningAlgId {
            get {
                return this.policyCombiningAlgId;
            }

            set {
                if (value == null) {
                    throw new ArgumentNullException(nameof(value));
                }

                this.policyCombiningAlgId = value;
            }
        }

        /// <summary>
        /// Gets PolicySets. 
        /// </summary>
        public ICollection<XacmlPolicySet> PolicySets {
            get {
                return this.policySets;
            }
        }

        /// <summary>
        /// Gets Policies.
        /// </summary>
        public ICollection<XacmlPolicy> Policies {
            get {
                return this.policies;
            }
        }

        /// <summary>
        /// Gets PolicySetIdReferences
        /// </summary>
        public ICollection<Uri> PolicySetIdReferences {
            get {
                return this.policySetIdReferences;
            }
        }

        /// <summary>
        /// Gets PolicyIdReferences
        /// </summary>
        public ICollection<Uri> PolicyIdReferences {
            get { return this.policyIdReferences; }
        }

        public ICollection<XacmlContextPolicyIdReference> PolicyIdReferences_3_0 {
            get { return this.policyIdReferences_3_0; }
        }

        public ICollection<XacmlContextPolicySetIdReference> PolicySetIdReferences_3_0 {
            get { return this.policySetIdReferences_3_0; }
        }

        public ICollection<XacmlObligationExpression> Obligations_3_0 {
            get {
                return this.obligations_3_0;
            }
        }

        public ICollection<XacmlAdviceExpression> Advices {
            get {
                return this.advices;
            }
        }

        public ICollection<XacmlCombinerParameter> CombinerParameters {
            get {
                return this.combinerParameters;
            }
        }

        public ICollection<XacmlPolicyCombinerParameters> PolicyCombinerParameters {
            get {
                return this.policyCombinerParameters;
            }
        }

        public ICollection<XacmlPolicySetCombinerParameters> PolicySetCombinerParameters {
            get {
                return this.policySetCombinerParameters;
            }
        }
    }
}