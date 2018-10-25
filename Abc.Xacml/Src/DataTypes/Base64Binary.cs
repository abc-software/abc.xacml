// ----------------------------------------------------------------------------
// <copyright file="Base64Binary.cs" company="ABC Software Ltd">
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

    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="System.IEquatable{Base64Binary}" />
    [TypeConverter(typeof(Base64BinaryConverter))]
    public class Base64Binary : IEquatable<Base64Binary> {
        private readonly byte[] value;
        private readonly string originalValue;

        /// <summary>
        /// Initializes a new instance of the <see cref="Base64Binary"/> class.
        /// </summary>
        /// <param name="value">The value.</param>
        public Base64Binary(string value) {
            if (value == null) {
                throw new ArgumentNullException(nameof(value));
            }

            this.originalValue = value;
            this.value = value.Select(o => Convert.ToByte(o)).ToArray();
        }

        /// <inheritdoc/>
        public bool Equals(Base64Binary other) {
            return this.value.SequenceEqual(other.value);
        }

        /// <inheritdoc/>
        public override bool Equals(object obj) {
            Base64Binary t = obj as Base64Binary;
            if (t == null) {
                return false;
            }

            return this.Equals(t);
        }

        /// <inheritdoc/>
        public override int GetHashCode() {
            // Overflow is fine, just wrap
            unchecked {
                if (this.value == null) {
                    return 0;
                }

                int hash = 17;
                foreach (byte element in this.value) {
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
