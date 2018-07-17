// ----------------------------------------------------------------------------
// <copyright file="DNSName.cs" company="ABC Software Ltd">
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
using System.Globalization;

namespace Abc.Xacml.DataTypes {

    [TypeConverter(typeof(DNSNameConverter))]
    public class DNSName : IEquatable<DNSName> {
        private readonly string value;

        public DNSName(string value) {
            this.value = value;
        }

        public bool Equals(DNSName other) {
            return string.Equals(this.value, other.value, StringComparison.OrdinalIgnoreCase);
        }

        public override bool Equals(object obj) {
            var t = obj as DNSName;
            if (t != null) {
                return this.Equals(t);
            }
            else {
                return false;
            }
        }

        public override int GetHashCode() {
            return this.value.GetHashCode();
        }

        public override string ToString() {
            return this.value;
        }
    }

    public class DNSNameConverter : TypeConverter {
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
                return new DNSName(value.ToString());
            }

            return base.ConvertFrom(context, culture, value);
        }
    }
}