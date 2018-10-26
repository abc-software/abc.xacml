// ----------------------------------------------------------------------------
// <copyright file="Xacml11Constants.cs" company="ABC Software Ltd">
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
    /// The XACML1.1 constants
    /// </summary>
    [SuppressMessage("Microsoft.StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented", Justification = "Reviewed. Constants is not commented.")]
#if !NETSTANDARD1_6
    [ExcludeFromCodeCoverage]
#endif
    public sealed class Xacml11Constants {
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
            public const string Assertion = "urn:oasis:names:tc:xacml:1.1:profile:saml2.0:v2:schema:assertion:wd-14";

            /// <summary>
            /// The namespace associated with the XACML schema that extends the SAML2.0 Protocol schema.
            /// </summary>
            public const string Protocol = "urn:oasis:names:tc:xacml:1.1:profile:saml2.0:v2:schema:protocol:wd-14";

            private Namespaces() {
            }
        }

        public sealed class PolicyCombiningAlgorithms {
            public static readonly Uri DenyOverrides = Xacml10Constants.PolicyCombiningAlgorithms.DenyOverrides;
            public static readonly Uri PermitOverrides = Xacml10Constants.PolicyCombiningAlgorithms.PermitOverrides;
            public static readonly Uri FirstApplicable = Xacml10Constants.PolicyCombiningAlgorithms.FirstApplicable;
            public static readonly Uri OnlyOneApplicable = Xacml10Constants.PolicyCombiningAlgorithms.OnlyOneApplicable;
            public static readonly Uri OrderedDenyOverrides = new Uri(OrderedDenyOverridesString);
            public static readonly Uri OrderedPermitOverrides = new Uri(OrderedPermitOverridesString);

            private const string OrderedDenyOverridesString = "urn:oasis:names:tc:xacml:1.1:policy-combining-algorithm:ordered-deny-overrides";
            private const string OrderedPermitOverridesString = "urn:oasis:names:tc:xacml:1.1:policy-combining-algorithm:ordered-permit-overrides";

            private PolicyCombiningAlgorithms() {
            }
        }

        public sealed class RuleCombiningAlgorithms {
            public const string OrderedDenyOverridesString = "urn:oasis:names:tc:xacml:1.1:rule-combining-algorithm:ordered-deny-overrides";
            public const string OrderedPermitOverridesString = "urn:oasis:names:tc:xacml:1.1:rule-combining-algorithm:ordered-permit-overrides";

            public static readonly Uri DenyOverrides = Xacml10Constants.RuleCombiningAlgorithms.DenyOverrides;
            public static readonly Uri PermitOverrides = Xacml10Constants.RuleCombiningAlgorithms.PermitOverrides;
            public static readonly Uri FirstApplicable = Xacml10Constants.RuleCombiningAlgorithms.FirstApplicable;
            public static readonly Uri OrderedDenyOverrides = new Uri(OrderedDenyOverridesString);
            public static readonly Uri OrderedPermitOverrides = new Uri(OrderedPermitOverridesString);

            private RuleCombiningAlgorithms() {
            }
        }
    }

#pragma warning restore 1591
}
