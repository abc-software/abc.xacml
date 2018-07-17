// ----------------------------------------------------------------------------
// <copyright file="Xacml10Constants.cs" company="ABC Software Ltd">
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
 
namespace Abc.Xacml {
    using System;
    using System.Diagnostics.CodeAnalysis;

#pragma warning disable 1591
    /// <summary>
    /// The XACML1.0 constants
    /// </summary>
    [SuppressMessage("Microsoft.StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented", Justification = "Reviewed. Constants is not commented.")]
#if !NETSTANDARD
    [ExcludeFromCodeCoverage]
#endif
    public sealed class Xacml10Constants {
        public sealed class Namespaces {
            /// <summary>
            /// The namespace associated with the XACML policy.
            /// </summary>
            public const string Policy = "urn:oasis:names:tc:xacml:1.0:policy";

            /// <summary>
            /// The namespace associated with the XACML context.
            /// </summary>
            public const string Context = "urn:oasis:names:tc:xacml:1.0:context";

            /// <summary>
            /// The namespace associated with the XACML schema that extends the SAML2.0 Assertion schema.
            /// </summary>
            public const string Assertion = "urn:oasis:names:tc:xacml:1.0:profile:saml2.0:v2:schema:assertion:wd-14";

            /// <summary>
            /// The namespace associated with the XACML schema that extends the SAML2.0 Protocol schema.
            /// </summary>
            public const string Protocol = "urn:oasis:names:tc:xacml:1.0:profile:saml2.0:v2:schema:protocol:wd-14";

            private Namespaces() {
            }
        }

        public sealed class PolicyCombiningAlgorithms {
            public static readonly Uri DenyOverrides = new Uri(DenyOverridesString);
            public static readonly Uri PermitOverrides = new Uri(PermitOverridesString);
            public static readonly Uri FirstApplicable = new Uri(FirstApplicableString);
            public static readonly Uri OnlyOneApplicable = new Uri(OnlyOneApplicableString);

            private const string DenyOverridesString = "urn:oasis:names:tc:xacml:1.0:policy-combining-algorithm:deny-overrides";
            private const string PermitOverridesString = "urn:oasis:names:tc:xacml:1.0:policy-combining-algorithm:permit-overrides";
            private const string FirstApplicableString = "urn:oasis:names:tc:xacml:1.0:policy-combining-algorithm:first-applicable";
            private const string OnlyOneApplicableString = "urn:oasis:names:tc:xacml:1.0:policy-combining-algorithm:only-one-applicable";

            private PolicyCombiningAlgorithms() {
            }
        }

        public sealed class RuleCombiningAlgorithms {
            public static readonly Uri DenyOverrides = new Uri(DenyOverridesString);
            public static readonly Uri PermitOverrides = new Uri(PermitOverridesString);
            public static readonly Uri FirstApplicable = new Uri(FirstApplicableString);

            private const string DenyOverridesString = "urn:oasis:names:tc:xacml:1.0:rule-combining-algorithm:deny-overrides";
            private const string PermitOverridesString = "urn:oasis:names:tc:xacml:1.0:rule-combining-algorithm:permit-overrides";
            private const string FirstApplicableString = "urn:oasis:names:tc:xacml:1.0:rule-combining-algorithm:first-applicable";

            private RuleCombiningAlgorithms() {
            }
        }

        public sealed class XPathVersions {
            public static readonly Uri Xpath10 = new Uri(Xpath10String);

            internal const string Xpath10String = "http://www.w3.org/TR/1999/Rec-xpath-19991116";

            private XPathVersions() {
            }
        }
    }

#pragma warning restore 1591
}
