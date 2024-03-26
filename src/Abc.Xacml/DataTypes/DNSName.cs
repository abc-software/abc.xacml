// ----------------------------------------------------------------------------
// <copyright file="DNSName.cs" company="ABC Software Ltd">
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
 
namespace Abc.Xacml.DataTypes {
    using System;
    using System.ComponentModel;

    /// <summary>
    /// The <see cref="DnsName"/> type is used to normalize and validate domain names and labels.
    /// </summary>
    /// <seealso cref="System.IEquatable{DnsName}" />
    [TypeConverter(typeof(DnsNameConverter))]
    public class DnsName : IEquatable<DnsName> {
        private readonly string value;

        /// <summary>
        /// Initializes a new instance of the <see cref="DnsName"/> class.
        /// </summary>
        /// <param name="value">The value.</param>
        public DnsName(string value) {
            this.value = value;
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>
        /// true if the current object is equal to the <paramref name="other" /> parameter; otherwise, false.
        /// </returns>
        public bool Equals(DnsName other) {
            return string.Equals(this.value, other.value, StringComparison.OrdinalIgnoreCase);
        }

        /// <inheritdoc/>
        public override bool Equals(object obj) {
            var t = obj as DnsName;
            if (t != null) {
                return this.Equals(t);
            }
            else {
                return false;
            }
        }

        /// <inheritdoc/>
        public override int GetHashCode() {
            return this.value.GetHashCode();
        }

        /// <inheritdoc/>
        public override string ToString() {
            return this.value;
        }
    }
}