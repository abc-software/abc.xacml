// ----------------------------------------------------------------------------
// <copyright file="HexBinary.cs" company="ABC Software Ltd">
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
    using System.Linq;

    [TypeConverter(typeof(HexBinaryConverter))]
    public class HexBinary : IEquatable<HexBinary> {
        private readonly byte[] name;
        private readonly string originalValue;

        /// <summary>
        /// Initializes a new instance of the <see cref="HexBinary"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        public HexBinary(string name) {
            if (name == null) {
                throw new ArgumentNullException(nameof(name));
            }

            this.originalValue = name;
            this.name = name.ToLowerInvariant().Select(o => Convert.ToByte(o)).ToArray();
        }

        /// <inheritdoc/>
        public bool Equals(HexBinary other) {
            return this.name.SequenceEqual(other.name);
        }

        /// <inheritdoc/>
        public override bool Equals(object obj) {
            HexBinary t = obj as HexBinary;
            if (t != null) {
                return this.Equals(t);
            }
            else {
                return false;
            }
        }

        /// <inheritdoc/>
        public override int GetHashCode() {
            // Overflow is fine, just wrap
            unchecked {
                if (this.name == null) {
                    return 0;
                }

                int hash = 17;
                foreach (byte element in this.name) {
                    hash = hash * 31 + element.GetHashCode();
                }

                return hash;
            }
        }

        /// <inheritdoc/>
        public override string ToString() {
            return originalValue;
        }
    }
}