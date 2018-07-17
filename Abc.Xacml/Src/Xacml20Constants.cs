// ----------------------------------------------------------------------------
// <copyright file="Xacml20Constants.cs" company="ABC Software Ltd">
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
    using System.Diagnostics.CodeAnalysis;

#pragma warning disable 1591
    /// <summary>
    /// The XACML2.0 constants
    /// </summary>
    [SuppressMessage("Microsoft.StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented", Justification = "Reviewed. Constants is not commented.")]
#if !NETSTANDARD
    [ExcludeFromCodeCoverage]
#endif
    public sealed class Xacml20Constants {
        public sealed class Namespaces {
            /// <summary>
            /// The namespace associated with the XACML policy.
            /// </summary>
            public const string Policy = "urn:oasis:names:tc:xacml:2.0:policy:schema:os";

            /// <summary>
            /// The namespace associated with the XACML context.
            /// </summary>
            public const string Context = "urn:oasis:names:tc:xacml:2.0:context:schema:os";

            /// <summary>
            /// The namespace associated with the XACML schema that extends the SAML2.0 Assertion schema.
            /// </summary>
            public const string Assertion = "urn:oasis:names:tc:xacml:2.0:profile:saml2.0:v2:schema:assertion:wd-14";

            /// <summary>
            /// The namespace associated with the XACML schema that extends the SAML2.0 Protocol schema.
            /// </summary>
            public const string Protocol = "urn:oasis:names:tc:xacml:2.0:profile:saml2.0:v2:schema:protocol:wd-14";

            private Namespaces() {
            }
        }
    }
#pragma warning restore 1591
}
