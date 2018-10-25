// ----------------------------------------------------------------------------
// <copyright file="Rfc822Name.cs" company="ABC Software Ltd">
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

/// <summary>
/// 
/// </summary>
namespace Abc.Xacml.DataTypes {
    using System;
    using System.ComponentModel;
    using System.Linq;

    /// <summary>
    /// WARNING
    /// TODO: https://www.ietf.org/rfc/rfc2821.txt 4.1.2 Command Argument Syntax
    /// </summary>
    [TypeConverter(typeof(Rfc822NameConverter))]
    public class Rfc822Name : IEquatable<Rfc822Name> {
        private readonly string name;
        private readonly string namePart;
        private readonly string domainPart;

        /// <summary>
        /// Initializes a new instance of the <see cref="Rfc822Name"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <exception cref="System.FormatException">
        /// Wrong Rfc822 format
        /// </exception>
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
            return name.ToLowerInvariant();
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
}