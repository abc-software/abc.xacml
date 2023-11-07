// ----------------------------------------------------------------------------
// <copyright file="Time.cs" company="ABC Software Ltd">
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
 
using System;
using System.ComponentModel;
using System.Xml;

namespace Abc.Xacml.DataTypes {
    [TypeConverter(typeof(TimeConverter))]
    [ImmutableObject(true)]
    public class Time : IComparable<Time>, IEquatable<Time> {
        private readonly TimeSpan time;
        private readonly string value;

        public Time(string time) {
            this.value = time;
            DateTime date = System.Xml.XmlConvert.ToDateTime(time, XmlDateTimeSerializationMode.Utc);
            this.time = date.ToUniversalTime().TimeOfDay;
        }

        public int CompareTo(Time other) {
            return this.time.CompareTo(other.time);
        }

        public bool Equals(Time other) {
            return this.time.Equals(other.time);
        }

        /// <inheritdoc/>
        public override bool Equals(object obj) {
            Time t = obj as Time;
            if (t != null) {
                return this.Equals(t);
            }
            else {
                return false;
            }
        }

        /// <inheritdoc/>
        public override int GetHashCode() {
            return this.time.GetHashCode();
        }

        /// <inheritdoc/>
        public override string ToString() {
            return this.value;
        }
    }
}