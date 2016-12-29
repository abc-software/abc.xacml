// ----------------------------------------------------------------------------
// <copyright file="Base64Binary.cs" company="ABC Software Ltd">
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

namespace Abc.Xacml.DataTypes {
    using System;
    using System.ComponentModel;
    using System.Linq;

    [TypeConverter(typeof(Base64BinaryConverter))]
    public class Base64Binary : IEquatable<Base64Binary> {
        private readonly byte[] value;
        private readonly string originalValue;

        public Base64Binary(string value) {
            this.originalValue = value;
            this.value = value.Select(o => Convert.ToByte(o)).ToArray();
        }

        public bool Equals(Base64Binary other) {
            return this.value.SequenceEqual(other.value);
        }

        public override bool Equals(object obj) {
            Base64Binary t = obj as Base64Binary;
            if (t == null) {
                return false;
            }

            return this.Equals(t);
        }

        public override int GetHashCode() {
            unchecked // Overflow is fine, just wrap
            {
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

        public override string ToString() {
            return originalValue;
        }
    }

    public class Base64BinaryConverter : TypeConverter {
        /// <inheritdoc/>
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType) {
            if (sourceType == typeof(string)) {
                return true;
            }

            return base.CanConvertFrom(context, sourceType);
        }

        /// <inheritdoc/>
        public override object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value) {
            if (value is string) {
                return new Base64Binary(value.ToString());
            }

            return base.ConvertFrom(context, culture, value);
        }
    }
}
