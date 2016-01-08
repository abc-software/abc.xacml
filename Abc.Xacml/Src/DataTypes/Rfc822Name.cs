// ----------------------------------------------------------------------------
// <copyright file="Rfc822Name.cs" company="ABC Software Ltd">
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

    /// <summary>
    /// WARNING
    /// TODO: https://www.ietf.org/rfc/rfc2821.txt 4.1.2 Command Argument Syntax
    /// </summary>
    [TypeConverter(typeof(Rfc822NameConverter))]
    public class Rfc822Name : IEquatable<Rfc822Name> {
        private string name;
        private string namePart;
        private string domainPart;

        public Rfc822Name(string name) {
            if (string.IsNullOrWhiteSpace(name)) {
                throw new FormatException("Wrong Rfc822 format");
            }

            string[] parts = name.Split('@');

            if (parts.Count() > 2) {
                throw new FormatException("Wrong Rfc822 format");
            }

            if (parts.Count() == 2) {
                this.namePart = parts[0];
                this.domainPart = parts[1].ToLowerInvariant();
                this.name = this.namePart + "@" + this.domainPart;
            }
            else {
                this.namePart = string.Empty;
                this.domainPart = name.ToLowerInvariant();
                this.name = this.domainPart;
            }
        }

        public bool Equals(Rfc822Name other) {
            return string.Equals(this.name, other.name);
        }

        public override bool Equals(object obj) {
            Rfc822Name t = obj as Rfc822Name;
            if (t != null) {
                return this.Equals(t);
            }
            else {
                return false;
            }
        }

        public override int GetHashCode() {
            return this.name.GetHashCode();
        }

        public override string ToString() {
            return name.ToLower();
        }

        public bool Match(string other) {
            if (string.IsNullOrEmpty(this.namePart)) {
                return false;
            }

            string[] parts = other.Split('@');
            if (parts.Count() > 2) {
                return false;
            }
            else if (parts.Count() == 2) {
                return string.Equals(this.namePart, parts[0]) && string.Equals(this.domainPart, parts[1].ToLowerInvariant());
            }
            else {
                if (other.StartsWith(".")) {
                    string matchDomain = other.Substring(1, other.Length - 1).ToLowerInvariant();
                    return System.Text.RegularExpressions.Regex.IsMatch(this.domainPart, string.Format("[@,.]{0}$", matchDomain));
                }
                else {
                    return string.Equals(this.domainPart, parts[0].ToLowerInvariant());
                }
            }
        }
    }

    public class Rfc822NameConverter : TypeConverter {

        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType) {
            if (sourceType == typeof(string)) {
                return true;
            }

            return base.CanConvertFrom(context, sourceType);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value) {
            if (value is string) {
                return new Rfc822Name(value.ToString());
            }

            return base.ConvertFrom(context, culture, value);
        }
    }
}