// ----------------------------------------------------------------------------
// <copyright file="HexBinary.cs" company="ABC Software Ltd">
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
 
using System;
using System.ComponentModel;
using System.Linq;

namespace Abc.Xacml.DataTypes {

    [TypeConverter(typeof(HexBinaryConverter))]
    public class HexBinary : IEquatable<HexBinary> {
        private readonly byte[] name;
        private readonly string originalValue;

        public HexBinary(string name) {
            this.originalValue = name;
            this.name = name.ToLowerInvariant().Select(o => Convert.ToByte(o)).ToArray();
        }

        public bool Equals(HexBinary other) {
            return this.name.SequenceEqual(other.name);
        }

        public override bool Equals(object obj) {
            HexBinary t = obj as HexBinary;
            if (t != null) {
                return this.Equals(t);
            }
            else {
                return false;
            }
        }

        public override int GetHashCode() {
            unchecked // Overflow is fine, just wrap
            {
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

        public override string ToString() {
            return originalValue;
        }
    }

    public class HexBinaryConverter : TypeConverter {

        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType) {
            if (sourceType == typeof(string)) {
                return true;
            }

            return base.CanConvertFrom(context, sourceType);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value) {
            if (value is string) {
                return new HexBinary(value.ToString());
            }

            return base.ConvertFrom(context, culture, value);
        }
    }
}