// ----------------------------------------------------------------------------
// <copyright file="XacmlUtils.cs" company="ABC Software Ltd">
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
 
namespace Abc.Xacml.Policy {
    using System;

    /// <summary>
    /// Xacml utilities.
    /// </summary>
    internal static class XacmlUtils {
        /// <summary>
        /// Generates the policy identifier.
        /// </summary>
        /// <returns>The policy identifier</returns>
        public static Uri GeneratePolicyId() {
            return new Uri(GenerateId("policy"));
        }

        /// <summary>
        /// Generates the policy identifier.
        /// </summary>
        /// <returns>The policy identifier</returns>
        public static string GenerateRuleId() {
            return GenerateId("rule");
        }

        /// <summary>
        /// Generates the policy set identifier.
        /// </summary>
        /// <returns>the policy set identifier</returns>
        public static Uri GeneratePolicySetId() {
            return new Uri(GenerateId("policyset"));
        }

        /// <summary>
        /// Generates the obligation identifier.
        /// </summary>
        /// <returns>the obligation identifier</returns>
        public static Uri GenerateObligationId() {
            return new Uri(GenerateId("obligation"));
        }

        private static string GenerateId(string prefix) {
            return string.Concat("urn:", prefix, ":" + Guid.NewGuid().ToString("N"));
        }
    }
}