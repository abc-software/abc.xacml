// ----------------------------------------------------------------------------
// <copyright file="x500Name.cs" company="ABC Software Ltd">
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
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Security.Cryptography.X509Certificates;

namespace Abc.Xacml.DataTypes {

    [TypeConverter(typeof(x500NameConverter))]
    public class x500Name : IEquatable<x500Name> {
        private readonly X500DistinguishedName name;

        private readonly IDictionary<string, string> x500rdns;

        public x500Name(string name) {
            this.name = new X500DistinguishedName(name);
            x500rdns = name.Split(',').ToDictionary(o => o.Split('=').First().Trim().ToUpperInvariant(), o => o.Split('=').Last().Trim());
        }

        public bool Equals(x500Name other) {
            return this.name.RawData.SequenceEqual(other.name.RawData);
        }

        public override bool Equals(object obj) {
            x500Name t = obj as x500Name;
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
                if (this.name.RawData == null) {
                    return 0;
                }
                int hash = 17;
                foreach (byte element in this.name.RawData) {
                    hash = hash * 31 + element.GetHashCode();
                }
                return hash;
            }
        }

        public override string ToString() {
            return this.name.Name;
        }

        public bool Match(x500Name other) {
            foreach (var rdn in this.x500rdns) {
                string value;
                if (other.x500rdns.TryGetValue(rdn.Key, out value)) {
                    if (!string.Equals(rdn.Value, value, StringComparison.InvariantCultureIgnoreCase)) {
                        return false;
                    }
                }
                else {
                    return false;
                }
            }

            return true;
        }
    }

    public class x500NameConverter : TypeConverter {

        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType) {
            if (sourceType == typeof(string)) {
                return true;
            }

            return base.CanConvertFrom(context, sourceType);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value) {
            if (value is string) {
                return new x500Name(value.ToString());
            }

            return base.ConvertFrom(context, culture, value);
        }
    }
}