// ----------------------------------------------------------------------------
// <copyright file="Xacml30Constants.cs" company="ABC Software Ltd">
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
    using System.Diagnostics.CodeAnalysis;

#pragma warning disable 1591
    /// <summary>
    /// The XACML3.0 constants
    /// </summary>
    [SuppressMessage("Microsoft.StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented", Justification = "Reviewed. Constants is not commented.")]
#if !NETSTANDARD1_6
    [ExcludeFromCodeCoverage]
#endif
    public sealed class Xacml30Constants {
        public sealed class Namespaces {
            /// <summary>
            /// The namespace associated with the XACML policy.
            /// </summary>
            public const string Policy = "urn:oasis:names:tc:xacml:3.0:core:schema:wd-17";

            /// <summary>
            /// The namespace associated with the XACML schema that extends the SAML2.0 Assertion schema.
            /// </summary>
            public const string Assertion = "urn:oasis:names:tc:xacml:3.0:profile:saml2.0:v2:schema:assertion:wd-14";

            /// <summary>
            /// The namespace associated with the XACML schema that extends the SAML2.0 Protocol schema.
            /// </summary>
            public const string Protocol = "urn:oasis:names:tc:xacml:3.0:profile:saml2.0:v2:schema:protocol:wd-14";

            private Namespaces() {
            }
        }

        public sealed class PolicyCombiningAlgorithms {
            public static readonly Uri DenyOverrides = new Uri(DenyOverridesString);
            public static readonly Uri OrderedDenyOverrides = new Uri(OrderedDenyOverridesString);
            public static readonly Uri PermitOverrides = new Uri(PermitOverridesString);
            public static readonly Uri OrderedPermitOverrides = new Uri(OrderedPermitOverridesString);
            public static readonly Uri DenyUnlessPermit = new Uri(DenyUnlessPermitString);
            public static readonly Uri PermitUnlessDeny = new Uri(PermitUnlessDenyString);
            public static readonly Uri FirstApplicable = Xacml10Constants.PolicyCombiningAlgorithms.FirstApplicable;
            public static readonly Uri OnlyOneApplicable = Xacml10Constants.PolicyCombiningAlgorithms.OnlyOneApplicable;
            public static readonly Uri LegacyDenyOverrides = Xacml10Constants.PolicyCombiningAlgorithms.DenyOverrides;
            public static readonly Uri LegacyOrderedDenyOverrides = Xacml11Constants.PolicyCombiningAlgorithms.OrderedDenyOverrides;
            public static readonly Uri LegacyPermitOverrides = Xacml10Constants.PolicyCombiningAlgorithms.PermitOverrides;
            public static readonly Uri LegacyOrderedPermitOverrides = Xacml11Constants.PolicyCombiningAlgorithms.OrderedPermitOverrides;

            private const string DenyOverridesString = "urn:oasis:names:tc:xacml:3.0:policy-combining-algorithm:deny-overrides";
            private const string OrderedDenyOverridesString = "urn:oasis:names:tc:xacml:3.0:policy-combining-algorithm:ordered-deny-overrides";
            private const string PermitOverridesString = "urn:oasis:names:tc:xacml:3.0:policy-combining-algorithm:permit-overrides";
            private const string OrderedPermitOverridesString = "urn:oasis:names:tc:xacml:3.0:policy-combining-algorithm:ordered-permit-overrides";
            private const string DenyUnlessPermitString = "urn:oasis:names:tc:xacml:3.0:rule-combining-algorithm:deny-unless-permit";
            private const string PermitUnlessDenyString = "urn:oasis:names:tc:xacml:3.0:rule-combining-algorithm:permit-unless-deny";

            private PolicyCombiningAlgorithms() {
            }
        }

        public sealed class RuleCombiningAlgorithms {
            public static readonly Uri DenyOverrides = new Uri(DenyOverridesString);
            public static readonly Uri OrderedDenyOverrides = new Uri(OrderedDenyOverridesString);
            public static readonly Uri PermitOverrides = new Uri(PermitOverridesString);
            public static readonly Uri OrderedPermitOverrides = new Uri(OrderedPermitOverridesString);
            public static readonly Uri DenyUnlessPermit = new Uri(DenyUnlessPermitString);
            public static readonly Uri PermitUnlessDeny = new Uri(PermitUnlessDenyString);
            public static readonly Uri FirstApplicable = Xacml10Constants.RuleCombiningAlgorithms.FirstApplicable;
            public static readonly Uri LegacyDenyOverrides = Xacml10Constants.RuleCombiningAlgorithms.DenyOverrides;
            public static readonly Uri LegacyOrderedDenyOverrides = Xacml11Constants.RuleCombiningAlgorithms.OrderedDenyOverrides;
            public static readonly Uri LegacyPermitOverrides = Xacml10Constants.RuleCombiningAlgorithms.PermitOverrides;
            public static readonly Uri LegacyOrderedPermitOverrides = Xacml11Constants.RuleCombiningAlgorithms.OrderedPermitOverrides;

            private const string DenyOverridesString = "urn:oasis:names:tc:xacml:3.0:rule-combining-algorithm:deny-overrides";
            private const string OrderedDenyOverridesString = "urn:oasis:names:tc:xacml:3.0:rule-combining-algorithm:ordered-deny-overrides";
            private const string PermitOverridesString = "urn:oasis:names:tc:xacml:3.0:rule-combining-algorithm:permit-overrides";
            private const string OrderedPermitOverridesString = "urn:oasis:names:tc:xacml:3.0:rule-combining-algorithm:ordered-permit-overrides";
            private const string DenyUnlessPermitString = "urn:oasis:names:tc:xacml:3.0:policy-combining-algorithm:deny-unless-permit";
            private const string PermitUnlessDenyString = "urn:oasis:names:tc:xacml:3.0:policy-combining-algorithm:permit-unless-deny";

            private RuleCombiningAlgorithms() {
            }
        }

        public sealed class XPathVersions {
            public static readonly Uri Xpath10 = Xacml10Constants.XPathVersions.Xpath10;
            public static readonly Uri Xpath20 = new Uri(Xpath20String);

            internal const string Xpath20String = "http://www.w3.org/TR/2007/REC-xpath20-20070123";

            private XPathVersions() {
            }
        }
    }
#pragma warning restore 1591
}
