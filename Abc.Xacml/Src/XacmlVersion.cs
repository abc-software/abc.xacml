// ----------------------------------------------------------------------------
// <copyright file="XacmlVersion.cs" company="ABC Software Ltd">
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

namespace Abc.Xacml {
    using System;

    /// <summary>
    /// The XACML version
    /// </summary>
    public abstract class XacmlVersion {
        private static XacmlVersion xacml10 = new XacmlVersion10();
        private static XacmlVersion xacml11 = new XacmlVersion11();
        private static XacmlVersion xacml20 = new XacmlVersion20();
        private static XacmlVersion xacml30 = new XacmlVersion30();

        /// <summary>
        /// Initializes a new instance of the <see cref="XacmlVersion"/> class.
        /// </summary>
        /// <param name="policyNs">The policy namespace.</param>
        /// <param name="contextNs">The context namespace.</param>
        /// <param name="protocolNs">The protocol namespace.</param>
        /// <param name="assertionNs">The assertion namespace.</param>
        /// <param name="contextPrefix">The context prefix.</param>
        protected XacmlVersion(string policyNs, string contextNs, string protocolNs, string assertionNs, string contextPrefix) {
            this.NamespacePolicy = policyNs;
            this.NamespaceContext = contextNs;
            this.NamespaceProtocol = protocolNs;
            this.NamespaceAssertion = assertionNs;
            this.PrefixContext = contextPrefix;
        }

        /// <summary>
        /// Gets the XACML V1.0.
        /// </summary>
        public static XacmlVersion Xacml10 {
            get { return xacml10; }
        }

        /// <summary>
        /// Gets the XACML V1.1.
        /// </summary>
        public static XacmlVersion Xacml11 {
            get { return xacml11; }
        }

        /// <summary>
        /// Gets the XACML V2.0.
        /// </summary>
        public static XacmlVersion Xacml20 {
            get { return xacml20; }
        }

        /// <summary>
        /// Gets the XACML V3.0 WD17.
        /// </summary>
        public static XacmlVersion Xacml30 {
            get { return xacml30; }
        }

        /// <summary>
        /// Gets the protocol namespace.
        /// </summary>
        public string NamespaceProtocol { get; private set; }

        /// <summary>
        /// Gets the policy namespace.
        /// </summary>
        public string NamespacePolicy { get; private set; }

        /// <summary>
        /// Gets the context namespace.
        /// </summary>
        public string NamespaceContext { get; private set; }

        /// <summary>
        /// Gets the assertion namespace.
        /// </summary>
        public string NamespaceAssertion { get; private set; }

        /// <summary>
        /// Gets the context prefix.
        /// </summary>
        public string PrefixContext { get; private set; }

        internal class XacmlVersion10 : XacmlVersion {
            public XacmlVersion10()
                : base(Xacml10Constants.Namespaces.Policy, Xacml10Constants.Namespaces.Context, Xacml10Constants.Namespaces.Protocol, Xacml10Constants.Namespaces.Assertion, XacmlConstants.Prefixes.Context) {
            }
        }

        internal class XacmlVersion11 : XacmlVersion {
            public XacmlVersion11()
                : base(Xacml11Constants.Namespaces.Policy, Xacml11Constants.Namespaces.Context, Xacml11Constants.Namespaces.Protocol, Xacml11Constants.Namespaces.Assertion, XacmlConstants.Prefixes.Context) {
            }
        }

        internal class XacmlVersion20 : XacmlVersion {
            public XacmlVersion20()
                : base(Xacml20Constants.Namespaces.Policy, Xacml20Constants.Namespaces.Context, Xacml20Constants.Namespaces.Protocol, Xacml20Constants.Namespaces.Assertion, XacmlConstants.Prefixes.Context) {
            }
        }

        internal class XacmlVersion30 : XacmlVersion {
            public XacmlVersion30()
                : base(Xacml30Constants.Namespaces.Policy, Xacml30Constants.Namespaces.Policy, Xacml30Constants.Namespaces.Protocol, Xacml30Constants.Namespaces.Assertion, XacmlConstants.Prefixes.Policy) {
            }
        }
    }
}
