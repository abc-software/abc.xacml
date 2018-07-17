// ----------------------------------------------------------------------------
// <copyright file="XacmlVersionMatchType.cs" company="ABC Software Ltd">
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
 
namespace Abc.Xacml.Policy {
    using System;
    using System.Text.RegularExpressions;

    public class XacmlVersionMatchType {
        private readonly string value;

        public XacmlVersionMatchType(string value) {
            if (!Regex.IsMatch(value, @"((\d+|\*)\.)*(\d+|\*|\+)")) {
                throw new ArgumentException("Wrong VersionMatch format.", nameof(value));
            }

            this.value = value;

        }

        public override string ToString() {
            return this.value;
        }
    }
}
