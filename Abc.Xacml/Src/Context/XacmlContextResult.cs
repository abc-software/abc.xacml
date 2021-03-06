﻿// ----------------------------------------------------------------------------
// <copyright file="XacmlContextResult.cs" company="ABC Software Ltd">
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
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using Abc.Xacml.Policy;

    /// <summary>
    /// The <c>XacmlContextResult</c> class represents an authorization decision result for the resource.
    /// </summary>
    /// <remarks>See the xacml-context:Result element defined in [XacmlCore, 6.10] for more details.</remarks>
    public class XacmlContextResult {
        private readonly ICollection<XacmlObligation> obligations = new Collection<XacmlObligation>();

#region XACML3.0
        private readonly ICollection<XacmlAdvice> advices = new Collection<XacmlAdvice>();
        private readonly ICollection<XacmlContextAttributes> attributes = new Collection<XacmlContextAttributes>();
        private readonly ICollection<XacmlContextPolicyIdReference> policyIdReferences = new Collection<XacmlContextPolicyIdReference>();
        private readonly ICollection<XacmlContextPolicySetIdReference> policySetIdReferences = new Collection<XacmlContextPolicySetIdReference>();
#endregion

        private XacmlContextDecision decision;
        private XacmlContextStatus status;

        /// <summary>
        /// Initializes a new instance of the <see cref="XacmlContextResult"/> class.
        /// </summary>
        /// <param name="decision">The authorization decision.</param>
        /// <param name="status">The status.</param>
        public XacmlContextResult(XacmlContextDecision decision, XacmlContextStatus status) {
            if (status == null) {
                throw new ArgumentNullException(nameof(status));
            }

            this.decision = decision;
            this.status = status;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="XacmlContextResult"/> class.
        /// </summary>
        /// <param name="decision">The authorization decision.</param>
        /// <remarks>
        /// Used only from XACML 2.0/3.0
        /// </remarks>
        public XacmlContextResult(XacmlContextDecision decision) {
            this.decision = decision;
        }

        /// <summary>
        /// Gets or sets the authorization decision.
        /// </summary>
        /// <remarks>See [XacmlCore, 6.11] for more details.</remarks>
        public XacmlContextDecision Decision {
            get {
                return this.decision;
            }

            set {
                this.decision = value;
            }
        }

        /// <summary>
        /// Gets or sets the status.
        /// </summary>
        /// <remarks>
        /// See [XacmlCore, 6.12] for more details.
        /// Optional in XACML2.0/3.0
        /// </remarks>
        public XacmlContextStatus Status {
            get {
                return this.status;
            }

            set {
                this.status = value;
            }
        }

        /// <summary>
        /// Gets the obligations.
        /// </summary>
        public ICollection<XacmlObligation> Obligations {
            get {
                return this.obligations;
            }
        }

        /// <summary>
        /// Gets or sets the resource identifier.
        /// </summary>
        public string ResourceId { get; set; }

        /// <summary>
        /// Gets the advices.
        /// </summary>
        /// <remarks>Used only for XACML V3.0</remarks>
        public ICollection<XacmlAdvice> Advices {
            get {
                return this.advices;
            }
        }

        /// <summary>
        /// Gets the attributes.
        /// </summary>
        /// <remarks>Used only for XACML V3.0</remarks>
        public ICollection<XacmlContextAttributes> Attributes {
            get {
                return this.attributes;
            }
        }

        /// <summary>
        /// Gets the policy identifier references.
        /// </summary>
        /// <remarks>Used only for XACML V3.0</remarks>
        public ICollection<XacmlContextPolicyIdReference> PolicyIdReferences {
            get {
                return this.policyIdReferences;
            }
        }

        /// <summary>
        /// Gets the policy set identifier references.
        /// </summary>
        /// <remarks>Used only for XACML V3.0</remarks>
        public ICollection<XacmlContextPolicySetIdReference> PolicySetIdReferences {
            get {
                return this.policySetIdReferences;
            }
        }
    }
}