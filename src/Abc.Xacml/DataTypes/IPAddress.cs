// ----------------------------------------------------------------------------
// <copyright file="IPAddress.cs" company="ABC Software Ltd">
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

    [TypeConverter(typeof(IPAddressConverter))]
    public class IPAddress : IEquatable<IPAddress> {
        private readonly string value;

        public IPAddress(string value) {
            this.value = value;
        }

        public bool Equals(IPAddress other) {
            return string.Equals(this.value, other.value, StringComparison.OrdinalIgnoreCase);
        }

        /// <inheritdoc/>
        public override bool Equals(object obj) {
            IPAddress t = obj as IPAddress;
            if (t == null) {
                return false;
            }

            return this.Equals(t);
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