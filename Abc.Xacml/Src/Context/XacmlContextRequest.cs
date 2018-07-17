// ----------------------------------------------------------------------------
// <copyright file="XacmlContextRequest.cs" company="ABC Software Ltd">
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
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;

    /// <summary>
    /// The <c>XacmlContextRequest</c> enumeration contains the request of policy evaluation.
    /// </summary>
    /// <remarks>See the xacml-context:Request element defined in [XacmlCore, 6.1] for more details.</remarks>
    public class XacmlContextRequest {
        private readonly ICollection<XacmlContextSubject> subjects = new Collection<XacmlContextSubject>();
        private readonly ICollection<XacmlContextResource> resources = new Collection<XacmlContextResource>();
        private readonly ICollection<XacmlContextAttributes> attributes = new Collection<XacmlContextAttributes>();
        private readonly ICollection<XacmlContextRequestReference> requestReferences = new Collection<XacmlContextRequestReference>();
        private XacmlContextAction action;

        /// <summary>
        /// Constructor user only for XACML 1.0/1.1/2.0
        /// </summary>
        /// <param name="resource">The resource.</param>
        /// <param name="action">The action.</param>
        /// <param name="subjects">The subjects.</param>
        public XacmlContextRequest(XacmlContextResource resource, XacmlContextAction action, XacmlContextSubject subject)
            : this(resource, action, new List<XacmlContextSubject> { subject }) {
        }

        /// <summary>
        /// Constructor user only for XACML 1.0/1.1/2.0
        /// </summary>
        /// <param name="resource">The resource.</param>
        /// <param name="action">The action.</param>
        /// <param name="subjects">The subjects.</param>
        public XacmlContextRequest(XacmlContextResource resource, XacmlContextAction action, IEnumerable<XacmlContextSubject> subjects) {
            if (resource == null) {
                throw new ArgumentNullException(nameof(resource));
            }

            if (action == null) {
                throw new ArgumentNullException(nameof(action));
            }

            if (subjects == null) {
                throw new ArgumentNullException(nameof(subjects));
            }

            this.resources.Add(resource);
            this.action = action;

            foreach (var item in subjects) {
                this.subjects.Add(item);
            }
        }

        /// <summary>
        /// Constructor user only for XACML 2.0
        /// </summary>
        /// <param name="resources">The resource.</param>
        /// <param name="action">The action.</param>
        /// <param name="subjects">The subject.</param>
        /// <param name="environment">The environment.</param>
        public XacmlContextRequest(XacmlContextResource resource, XacmlContextAction action, XacmlContextSubject subject, XacmlContextEnvironment environment) :
            this(new List<XacmlContextResource> { resource }, action, new List<XacmlContextSubject> { subject }, environment) {
        }

        /// <summary>
        /// Constructor user only for XACML 2.0
        /// </summary>
        /// <param name="resources">The resources.</param>
        /// <param name="action">The action.</param>
        /// <param name="subjects">The subjects.</param>
        /// <param name="environment">The environment.</param>
        public XacmlContextRequest(IEnumerable<XacmlContextResource> resources, XacmlContextAction action, IEnumerable<XacmlContextSubject> subjects, XacmlContextEnvironment environment) {
            if (resources == null) {
                throw new ArgumentNullException(nameof(resources));
            }

            if (action == null) {
                throw new ArgumentNullException(nameof(action));
            }

            if (subjects == null) {
                throw new ArgumentNullException(nameof(subjects));
            }

            if (environment == null) {
                throw new ArgumentNullException(nameof(environment));
            }

            foreach (var item in resources) {
                this.resources.Add(item);
            }

            this.action = action;

            foreach (var item in subjects) {
                this.subjects.Add(item);
            }

            this.Environment = environment;
        }

        /// <summary>
        /// Constructor user only for XACML 3.0
        /// </summary>
        /// <param name="returnPolicyIdList">if set to <c>true</c> [return policy identifier list].</param>
        /// <param name="combinedDecision">if set to <c>true</c> [combined decision].</param>
        /// <param name="attributes">The attributes.</param>
        public XacmlContextRequest(bool returnPolicyIdList, bool combinedDecision, IEnumerable<XacmlContextAttributes> attributes) {
            if (attributes == null) {
                throw new ArgumentNullException(nameof(attributes));
            }

            this.ReturnPolicyIdList = returnPolicyIdList;
            this.CombinedDecision = combinedDecision;

            foreach (var item in attributes) {
                this.attributes.Add(item);
            }
        }

        /// <summary>
        /// Gets the subjects of request context.
        /// </summary>
        /// <value>
        /// The subjects of request context.
        /// </value>
        /// <remarks>See [XacmlCore, 6.2] for more details.</remarks>
        public ICollection<XacmlContextSubject> Subjects {
            get {
                return this.subjects;
            }
        }

        /// <summary>
        /// Gets the resource or resources for which access is being requested.
        /// </summary>
        /// <value>
        /// The resource or resources for which access is being requested.
        /// </value>
        public ICollection<XacmlContextResource> Resources {
            get {
                return this.resources;
            }
        }

        /// <summary>
        /// Gets or sets the requested action to be performed on the resource.
        /// </summary>
        /// <value>
        /// The requested action to be performed on the resource.
        /// </value>
        public XacmlContextAction Action {
            get {
                return this.action;
            }

            set {
                if (value == null) {
                    throw new ArgumentNullException(nameof(value));
                }

                this.action = value;
            }
        }

        /// <summary>
        /// Gets or sets the environment.
        /// </summary>
        /// <value>
        /// The environment.
        /// </value>
        /// <remarks>
        /// Used only from XACML V2.0
        /// </remarks>
        public XacmlContextEnvironment Environment { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether return list of all fully applicable policies and policy sets which were used.
        /// </summary>
        /// <value>
        /// <c>true</c> if [return list of all fully applicable policies and policy sets which were used; otherwise, <c>false</c>.
        /// </value>
        /// <remarks>
        /// Used only from XACML V3.0
        /// See [XacmlCore, 5.42] for more details.
        /// </remarks>
        public bool ReturnPolicyIdList { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether PDP combines multiple decisions into a single decision.
        /// </summary>
        /// <value>
        /// <c>true</c> if PDP combines multiple decisions into a single decision; otherwise, <c>false</c>.
        /// </value>
        /// <remarks>
        /// Used only from XACML V3.0
        /// See [XacmlCore, 5.42] for more details.
        /// </remarks>
        public bool CombinedDecision { get; set; }

        /// <summary>
        /// Gets or sets the XPathVersion
        /// </summary>
        /// <remarks>
        /// Used only from XACML V3.0
        /// See [XacmlCore, 5.42] for more details.
        /// </remarks>
        public Uri XPathVersion { get; set; }

        /// <summary>
        /// Gets the attributes.
        /// </summary>
        /// <remarks>
        /// Used only from XACML V3.0
        /// See [XacmlCore, 5.42] for more details.
        /// </remarks>
        public ICollection<XacmlContextAttributes> Attributes {
            get {
                return this.attributes;
            }
        }

        /// <summary>
        /// Gets the request references.
        /// </summary>
        /// <remarks>
        /// Used only from XACML V3.0
        /// See [XacmlCore, 5.51] for more details.
        /// </remarks>
        public ICollection<XacmlContextRequestReference> RequestReferences {
            get {
                return this.requestReferences;
            }
        }
    }
}
